using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NServiceBus;
using NServiceBus.TransactionalSession;
using ServiceComposer.AspNetCore;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ViewModelComposition.Tests
{
    class FakeCompositionContext : ICompositionContext
    {
        readonly List<object> raisedEvents = new();

        public string RequestId => "test-request-id";
        public IReadOnlyList<object> RaisedEvents => raisedEvents;

        public Task RaiseEvent<TEvent>(TEvent @event)
        {
            raisedEvents.Add(@event!);
            return Task.CompletedTask;
        }

        public IList<ModelBindingArgument> GetArguments(ICompositionRequestsHandler owner) => [];
        public IList<ModelBindingArgument> GetArguments(ICompositionEventsSubscriber owner) => [];
        public IList<ModelBindingArgument> GetArguments<T>(ICompositionEventsHandler<T> owner) => [];
    }

    class FakeTransactionalSession : ITransactionalSession
    {
        readonly List<object> sentMessages = new();

        public IReadOnlyList<object> SentMessages => sentMessages;

        public NServiceBus.Persistence.ISynchronizedStorageSession SynchronizedStorageSession =>
            throw new NotSupportedException("SynchronizedStorageSession is not available in test context.");

        public string SessionId => "test-session-id";

        public Task Open(OpenSessionOptions options, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Send(object message, SendOptions sendOptions,
            CancellationToken cancellationToken = default)
        {
            sentMessages.Add(message);
            return Task.CompletedTask;
        }

        public Task Send<T>(Action<T> messageConstructor, SendOptions sendOptions,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("Message constructor overload not supported in tests; use the object-based Send overload instead.");
        }

        public Task Publish(object message, PublishOptions publishOptions,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("Message constructor overload not supported in tests; use the object-based Publish overload instead.");
        }

        public Task Commit(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Dispose() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    /// <summary>
    /// Captures the single handler registered by a composition subscriber for a given event type.
    /// </summary>
    class FakeCompositionEventsPublisher<TEvent> : ICompositionEventsPublisher
    {
        readonly Action<CompositionEventHandler<TEvent>> capture;

        public FakeCompositionEventsPublisher(Action<CompositionEventHandler<TEvent>> capture)
            => this.capture = capture;

        public void Subscribe<T>(CompositionEventHandler<T> handler)
        {
            if (handler is CompositionEventHandler<TEvent> typedHandler)
                capture(typedHandler);
        }
    }

    static class TestRequestBuilder
    {
        // Keys verified via reflection against ServiceComposer.AspNetCore 4.1.3:
        // HttpRequestExtensions.ComposedResponseModelKey = "composed-response-model"
        // HttpRequestExtensions.CompositionContextKey   = "composition-context"
        const string ComposedResponseModelKey = "composed-response-model";
        const string CompositionContextKey = "composition-context";

        public static (HttpRequest request, dynamic vm, FakeCompositionContext compositionContext)
            Build(
                Action<Dictionary<string, string>>? configureCookies = null,
                Action<RouteValueDictionary>? configureRouteValues = null,
                Action<Dictionary<string, Microsoft.Extensions.Primitives.StringValues>>? configureForm = null)
        {
            var httpContext = new DefaultHttpContext();
            dynamic vm = new ExpandoObject();
            var compositionContext = new FakeCompositionContext();

            httpContext.Items[ComposedResponseModelKey] = vm;
            httpContext.Items[CompositionContextKey] = compositionContext;

            if (configureCookies != null)
            {
                var cookies = new Dictionary<string, string>();
                configureCookies(cookies);
                var cookieHeader = string.Join("; ", cookies.Select(kv => $"{kv.Key}={kv.Value}"));
                httpContext.Request.Headers["Cookie"] = cookieHeader;
            }

            if (configureRouteValues != null)
            {
                configureRouteValues(httpContext.Request.RouteValues);
            }

            if (configureForm != null)
            {
                var formData = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>();
                configureForm(formData);
                httpContext.Request.Form = new FormCollection(formData);
            }

            return (httpContext.Request, vm, compositionContext);
        }
    }
}
