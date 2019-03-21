using System;
using JetBrains.Annotations;
using Vostok.Context;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    [PublicAPI]
    public class Tracer : ITracer
    {
        private const string DistributedGlobalName = "vostok.tracing.context";

        private readonly TracerSettings settings;

        static Tracer()
        {
            FlowingContext.Configuration.RegisterDistributedGlobal(DistributedGlobalName, new TraceContextSerializer());
        }

        public Tracer(TracerSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public TraceContext CurrentContext
        {
            get => FlowingContext.Globals.Get<TraceContext>();
            set => FlowingContext.Globals.Set(value);
        }

        public ISpanBuilder BeginSpan()
        {
            var contextScope = BeginContextScope(out var oldContext, out var newContext);

            return new SpanBuilder(settings, contextScope, newContext, oldContext);
        }

        private IDisposable BeginContextScope(out TraceContext oldContext, out TraceContext newContext)
        {
            oldContext = CurrentContext;

            return FlowingContext.Globals.Use(newContext = new TraceContext(oldContext?.TraceId ?? Guid.NewGuid(), Guid.NewGuid()));
        }
    }
}