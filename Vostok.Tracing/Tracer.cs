using System;
using Vostok.Commons.Collections;
using Vostok.Context;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    public class Tracer : ITracer
    {
        private const string DistributedGlobalName = "vostok.tracing.context";

        private readonly UnboundedObjectPool<Span> objectPool;

        static Tracer()
        {
            FlowingContext.Configuration.RegisterDistributedGlobal(DistributedGlobalName, new TraceContextSerializer());
        }

        public Tracer(TraceConfiguration traceConfiguration)
        {
            TraceConfiguration = traceConfiguration;
            objectPool = new UnboundedObjectPool<Span>(() => new Span());
        }

        public TraceContext CurrentContext
        {
            get => FlowingContext.Globals.Get<TraceContext>();
            set => FlowingContext.Globals.Set(value);
        }

        public ISpanBuilder BeginSpan()
        {
            var newScope = BeginContextScope();
            var spanBuilder = new SpanBuilder(newScope, objectPool, TraceConfiguration);

            return spanBuilder;
        }

        internal TraceConfiguration TraceConfiguration { get; set; }

        private TraceContextScope BeginContextScope()
        {
            var oldContext = CurrentContext;
            var newContext = new TraceContext(oldContext?.TraceId ?? Guid.NewGuid(), Guid.NewGuid());

            CurrentContext = newContext;

            return new TraceContextScope(newContext, oldContext);
        }
    }
}