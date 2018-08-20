using System;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
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