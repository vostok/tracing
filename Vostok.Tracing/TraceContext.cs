using System;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    // CR(iloktionov): Remove ITraceContext and move TraceContext to abstractions module.

    public class TraceContext : ITraceContext
    {
        public TraceContext()
        {
            TraceId = Guid.NewGuid();
            SpanId = Guid.NewGuid();
        }

        public TraceContext(Guid traceId, Guid spanId)
        {
            TraceId = traceId;
            SpanId = spanId;
        }

        public Guid TraceId { get; }
        public Guid SpanId { get; }
    }
}