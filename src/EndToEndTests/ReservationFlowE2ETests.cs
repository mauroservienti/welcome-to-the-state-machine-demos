using System.Net;
using System.Text.RegularExpressions;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace EndToEndTests;

/// <summary>
/// End-to-end tests for the reservation flow.
///
/// The test starts the whole system as described by the AppHost, with Aspire's testing
/// support: RabbitMQ and PostgreSQL containers, the databases, and every service as a process.
/// The Particular platform isn't started. The test drives the website through real HTTP,
/// following the same steps as a user:
///   1. GET /                              lists the available tickets
///   2. POST /reservations/reserve/{id}    Reservations and Finance composition handlers send their commands,
///                                         then the controller redirects to /reservations/added
///   3. GET /reservations                  polls until the reservation shows up, then reads the payment
///                                         methods and the delivery options
///   4. POST /reservations/finalize        Finance and Shipping composition handlers store the choices,
///                                         then the controller redirects to /reservations/review
///   5. POST /reservations/checkout        Reservations, Finance and Shipping composition handlers send their
///                                         commands, then the controller redirects to /reservations/checkedout
///   6. The services' logs show that the payment succeeded and that the order is shipped.
///
/// The POST steps are composition handlers composing over controller actions that redirect,
/// so the test fails if those handlers don't run.
///
/// Because the test requires Docker it runs only on Linux in CI.
/// </summary>
[Trait("Category", "EndToEnd")]
public class ReservationFlowE2ETests
{
    static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(5);
    static readonly TimeSpan FlowTimeout = TimeSpan.FromMinutes(3);

    [Fact]
    public async Task Reserving_and_checking_out_a_ticket_eventually_pays_and_ships_the_order()
    {
        using var startupCts = new CancellationTokenSource(StartupTimeout);

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>(
            ["--ParticularPlatform:Enabled=false"], startupCts.Token);
        await using var app = await appHost.BuildAsync(startupCts.Token);
        await app.StartAsync(startupCts.Token);

        // Watch the logs before the flow starts, so that no line is missed
        var paymentGatewayLogs = WatchLogs(app, "finance-paymentgateway");
        var shippingLogs = WatchLogs(app, "shipping-service");

        await app.ResourceNotifications.WaitForResourceAsync("website", KnownResourceStates.Running, startupCts.Token);

        using var handler = new HttpClientHandler { CookieContainer = new CookieContainer(), AllowAutoRedirect = false };
        using var client = new HttpClient(handler) { BaseAddress = app.GetEndpoint("website", "http") };

        using var cts = new CancellationTokenSource(FlowTimeout);

        // 1. The home page lists the available tickets. It also sets the reservation-id cookie.
        var homePage = await PollUntil(client, "/", "the available tickets", page => Regex.IsMatch(page, @"/reservations/reserve/\d+"), cts.Token);
        var ticketId = Regex.Match(homePage, @"/reservations/reserve/(\d+)").Groups[1].Value;
        var reservationId = handler.CookieContainer.GetCookies(client.BaseAddress!)["reservation-id"]?.Value;
        Assert.False(string.IsNullOrEmpty(reservationId), "The home page didn't set the reservation-id cookie.");

        // 2. Reserve the ticket
        var reserveResponse = await client.PostAsync($"/reservations/reserve/{ticketId}", null, cts.Token);
        AssertRedirectsTo("/reservations/added", reserveResponse);

        // 3. The reservation eventually shows up, with the payment methods and delivery options to choose from
        var reservationPage = await PollUntil(client, "/reservations", "the reserved ticket", page => page.Contains("Total due:"), cts.Token);
        var paymentMethod = Regex.Match(reservationPage, @"name=""PaymentMethod"" type=""radio"" value=""([^""]+)""").Groups[1].Value;
        var deliveryOption = Regex.Match(reservationPage, @"name=""DeliveryOption"" type=""radio"" value=""([^""]+)""").Groups[1].Value;
        Assert.False(string.IsNullOrEmpty(paymentMethod), "The reservation page lists no payment methods.");
        Assert.False(string.IsNullOrEmpty(deliveryOption), "The reservation page lists no delivery options.");

        // 4. Finalize the reservation
        var finalizeResponse = await client.PostAsync("/reservations/finalize", new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("PaymentMethod", paymentMethod),
            new KeyValuePair<string, string>("DeliveryOption", deliveryOption)
        ]), cts.Token);
        AssertRedirectsTo("/reservations/review", finalizeResponse);

        // 5. Check out
        var checkoutResponse = await client.PostAsync("/reservations/checkout", null, cts.Token);
        AssertRedirectsTo("/reservations/checkedout", checkoutResponse);

        // 6. The payment eventually succeeds, and the order is shipped
        await WaitForLogLine(paymentGatewayLogs, $"Payment for reservation '{reservationId}' succeeded.", cts.Token);
        await WaitForLogLine(shippingLogs, line => line.Contains("will be shipped ASAP") || line.Contains("will be shipped at the venue"), cts.Token);
    }

    static void AssertRedirectsTo(string expectedLocation, HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(expectedLocation, response.Headers.Location?.OriginalString, ignoreCase: true);
    }

    static async Task<string> PollUntil(HttpClient client, string path, string expected, Func<string, bool> condition, CancellationToken cancellationToken)
    {
        var lastContent = string.Empty;
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var response = await client.GetAsync(path, cancellationToken);
                lastContent = await response.Content.ReadAsStringAsync(cancellationToken);
                if (response.IsSuccessStatusCode && condition(lastContent))
                {
                    return lastContent;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
            }
            catch (HttpRequestException)
            {
                // The website might not be listening yet
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        Assert.Fail($"{path} didn't show {expected} in time. Last response:{Environment.NewLine}{lastContent}");
        return lastContent;
    }

    /// <summary>Collects the log lines of a resource as they're written.</summary>
    static LogLines WatchLogs(DistributedApplication app, string resourceName)
    {
        var logLines = new LogLines(resourceName);
        var loggerService = app.Services.GetRequiredService<ResourceLoggerService>();
        // Logs are keyed by resource instance, which the resource overload resolves
        var resource = app.Services.GetRequiredService<DistributedApplicationModel>().Resources.Single(r => r.Name == resourceName);
        _ = Task.Run(async () =>
        {
            await foreach (var batch in loggerService.WatchAsync(resource))
            {
                foreach (var line in batch)
                {
                    logLines.Add(line.Content);
                }
            }
        });
        return logLines;
    }

    static Task WaitForLogLine(LogLines logLines, string expected, CancellationToken cancellationToken) =>
        WaitForLogLine(logLines, line => line.Contains(expected), cancellationToken, expected);

    static async Task WaitForLogLine(LogLines logLines, Func<string, bool> predicate, CancellationToken cancellationToken, string? description = null)
    {
        try
        {
            while (!logLines.Any(predicate))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            Assert.Fail($"The {logLines.ResourceName} logs don't contain the expected line{(description is null ? "" : $" '{description}'")}. Logs:{Environment.NewLine}{string.Join(Environment.NewLine, logLines.Snapshot())}");
        }
    }

    sealed class LogLines(string resourceName)
    {
        readonly List<string> lines = [];

        public string ResourceName { get; } = resourceName;

        public void Add(string line)
        {
            lock (lines) { lines.Add(line); }
        }

        public bool Any(Func<string, bool> predicate)
        {
            lock (lines) { return lines.Any(predicate); }
        }

        public string[] Snapshot()
        {
            lock (lines) { return lines.ToArray(); }
        }
    }
}
