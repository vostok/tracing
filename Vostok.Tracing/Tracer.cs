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
            this.settings = TracerSettingsValidator.Validate(settings);
        }

        public TraceContext CurrentContext
        {
            get => FlowingContext.Globals.Get<TraceContext>();
            set => FlowingContext.Globals.Set(value);
        }

        public ISpanBuilder BeginSpan()
        {
            var newScope = BeginContextScope();
            var spanBuilder = new SpanBuilder(newScope, settings);

            return spanBuilder;
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