using System;
using Vostok.Context;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    public class Tracer : ITracer
    {
        public const string TraceIdContextName = "Vostok.Tracing.TraceId";
        public const string SpanIdContextName = "Vostok.Tracing.SpanId";

        public Tracer(ITraceConfiguration traceConfiguration)
        {
            TraceConfiguration = traceConfiguration;
        }

        public ITraceConfiguration TraceConfiguration { get; set; }

        public ITraceContext CurrentContext
        {
            get => CurrentTraceContext;
            set => CurrentTraceContext = value;
        }

        // CR(iloktionov): 1. Use FlowingContext.Globals instead of properties
        // CR(iloktionov): 2. We probably shouldn't provide two ways to set context in public API (remove static property?)
        // CR(iloktionov): 3. Register TraceContext as a distributed global in FlowingContext (and provide a serializer)

        public static ITraceContext CurrentTraceContext
        {
            get
            {
                var properties = FlowingContext.Properties;

                var traceId = properties.Get<Guid>(TraceIdContextName);
                if (traceId == default(Guid))
                    return null;

                var spanId = properties.Get<Guid>(SpanIdContextName);
                if (spanId == default(Guid))
                    return null;

                return new TraceContext(traceId, spanId);
            }
            set
            {
                var properties = FlowingContext.Properties;

                if (value == null)
                {
                    properties.Remove(TraceIdContextName);
                    properties.Remove(SpanIdContextName);
                }
                else
                {
                    properties.Set(TraceIdContextName, value.TraceId);
                    properties.Set(SpanIdContextName, value.SpanId);
                }
            }
        }

        public ISpanBuilder BeginSpan()
        {
            var newScope = BeginContextScope();
            return new SpanBuilder(newScope, TraceConfiguration.TraceReporter);
        }

        public TraceContextScope BeginContextScope()
        {
            var oldContext = CurrentTraceContext;
            var newContext = new TraceContext(oldContext?.TraceId ?? Guid.NewGuid(), Guid.NewGuid());

            CurrentTraceContext = newContext;

            return new TraceContextScope(newContext, oldContext);
        }
    }
}