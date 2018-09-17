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

        static Tracer()
        {
            FlowingContext.Configuration.RegisterDistributedGlobal(DistributedGlobalName, new TraceContextSerializer());
        }

        public Tracer(TraceConfiguration traceConfiguration)
        {
            ValidateConfiguration(traceConfiguration);

            TraceConfiguration = traceConfiguration;
        }

        public TraceContext CurrentContext
        {
            get => FlowingContext.Globals.Get<TraceContext>();
            set => FlowingContext.Globals.Set(value);
        }

        public ISpanBuilder BeginSpan()
        {
            var newScope = BeginContextScope();
            var spanBuilder = new SpanBuilder(newScope, TraceConfiguration);

            return spanBuilder;
        }

        private TraceConfiguration TraceConfiguration { get; set; }

        private TraceContextScope BeginContextScope()
        {
            var oldContext = CurrentContext;
            var newContext = new TraceContext(oldContext?.TraceId ?? Guid.NewGuid(), Guid.NewGuid());

            CurrentContext = newContext;

            return new TraceContextScope(newContext, oldContext);
        }

        private void ValidateConfiguration(TraceConfiguration configuration)
        {
            if (configuration.Sender == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
        }
    }
}