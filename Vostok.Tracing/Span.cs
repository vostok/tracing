using System;
using System.Collections.Generic;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    public class Span : ISpan
    {
        public Guid TraceId { get; set; }
        public Guid SpanId { get; set; }
        public Guid? ParentSpanId { get; set; }
        public DateTimeOffset BeginTimestamp { get; set; }
        public DateTimeOffset? EndTimestamp { get; set; }
        public IReadOnlyDictionary<string, string> Annotations { get; set; }
    }
}