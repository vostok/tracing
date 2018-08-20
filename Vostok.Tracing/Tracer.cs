using System;
using Vostok.Context;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    public class Tracer : ITracer
    {
        private const string DistributedGlobalName = "vostok.tracing.context";

        static Tracer()
        {
            FlowingContext.Configuration.RegisterDistributedGlobal(DistributedGlobalName, new TraceContextSerializer());
        }

        public Tracer(TraceConfiguration traceConfiguration)
        {
            TraceConfiguration = traceConfiguration;
        }

        public TraceConfiguration TraceConfiguration { get; set; }

        public TraceContext CurrentContext
        {
            get => FlowingContext.Globals.Get<TraceContext>();
            set => FlowingContext.Globals.Set(value);
        }

        public ISpanBuilder BeginSpan()
        {
            var newScope = BeginContextScope();
            return new SpanBuilder(newScope, TraceConfiguration.TraceReporter);
        }

        private TraceContextScope BeginContextScope()
        {
            var oldContext = CurrentContext;
            var newContext = new TraceContext(oldContext?.TraceId ?? Guid.NewGuid(), Guid.NewGuid());

            CurrentContext = newContext;

            return new TraceContextScope(newContext, oldContext);
        }
    }
}