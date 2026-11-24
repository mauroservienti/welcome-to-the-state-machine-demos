using Microsoft.Extensions.Configuration;
using Particular.Aspire.Hosting.ServicePlatform.Transport;

var builder = DistributedApplication.CreateBuilder(args);

var rabbitUser = builder.AddParameter("rabbit-user", "guest");
var rabbitPassword = builder.AddParameter("rabbit-password", "guest", secret: true);
var transport = builder.AddRabbitMQ("transport", rabbitUser, rabbitPassword)
    .WithManagementPlugin();

/*
 * One PostgreSQL server hosting one database per logical store,
 * mirroring the database containers defined in the dev container.
 * Resource names are what endpoints look up via ConnectionStrings:<name>.
 */
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var ticketingDatabase = postgres.AddDatabase("ticketing-database", "ticketing_database");
var financeDatabase = postgres.AddDatabase("finance-database", "finance_database");
var financeServiceDatabase = postgres.AddDatabase("finance-service-database", "finance_service_database");
// The payment gateway simulates an external system; its own database also avoids concurrent
// NServiceBus installers racing on the same schema when endpoints start at the same time
var paymentGatewayDatabase = postgres.AddDatabase("finance-paymentgateway-database", "finance_paymentgateway_database");
var reservationsDatabase = postgres.AddDatabase("reservations-database", "reservations_database");
var reservationsServiceDatabase = postgres.AddDatabase("reservations-service-database", "reservations_service_database");
var shippingServiceDatabase = postgres.AddDatabase("shipping-service-database", "shipping_service_database");
var websiteDatabase = postgres.AddDatabase("website-database", "website_database");

/*
 * The Particular platform (ServiceControl, ServicePulse) is for monitoring and isn't
 * needed to run the system. The end-to-end tests disable it, with ParticularPlatform:Enabled=false.
 */
Func<IResourceBuilder<ProjectResource>, IResourceBuilder<ProjectResource>> withParticularPlatform = project => project;
if (builder.Configuration.GetValue("ParticularPlatform:Enabled", true))
{
    var platform = builder.AddParticularPlatform("particular")
        .WithTransportRabbitMQ(RabbitMqRouting.ClassicConventionalRouting, transport)
        .AddDefaultComponents();
    withParticularPlatform = project => project.WithParticularPlatform(platform);
}

// NServiceBus.RabbitMQ uses the management API too, which Aspire exposes on a random port
IResourceBuilder<ProjectResource> WithTransport(IResourceBuilder<ProjectResource> project) => project
    .WithReference(transport).WaitFor(transport)
    .WithEnvironment("RabbitMQ__ManagementApi__Url", transport.GetEndpoint("management"))
    .WithEnvironment("RabbitMQ__ManagementApi__UserName", rabbitUser)
    .WithEnvironment("RabbitMQ__ManagementApi__Password", rabbitPassword);

var createRequiredDatabases = builder.AddProject<Projects.CreateRequiredDatabases>("create-required-databases")
    .WithReference(ticketingDatabase).WaitFor(ticketingDatabase)
    .WithReference(financeDatabase).WaitFor(financeDatabase)
    .WithReference(reservationsDatabase).WaitFor(reservationsDatabase);

withParticularPlatform(WithTransport(builder.AddProject<Projects.Finance_PaymentGateway>("finance-paymentgateway"))
    .WithReference(paymentGatewayDatabase).WaitFor(paymentGatewayDatabase));

withParticularPlatform(WithTransport(builder.AddProject<Projects.Finance_Service>("finance-service"))
    .WithReference(financeServiceDatabase).WaitFor(financeServiceDatabase)
    .WithReference(financeDatabase)
    .WaitForCompletion(createRequiredDatabases));

withParticularPlatform(WithTransport(builder.AddProject<Projects.Reservations_Service>("reservations-service"))
    .WithReference(reservationsServiceDatabase).WaitFor(reservationsServiceDatabase)
    .WithReference(reservationsDatabase)
    .WaitForCompletion(createRequiredDatabases));

withParticularPlatform(WithTransport(builder.AddProject<Projects.Shipping_Service>("shipping-service"))
    .WithReference(shippingServiceDatabase).WaitFor(shippingServiceDatabase));

withParticularPlatform(WithTransport(builder.AddProject<Projects.Website>("website"))
    .WithReference(websiteDatabase).WaitFor(websiteDatabase)
    .WithReference(ticketingDatabase)
    .WithReference(financeDatabase)
    .WithReference(reservationsDatabase)
    .WaitForCompletion(createRequiredDatabases)
    .WithExternalHttpEndpoints());

builder.Build().Run();
