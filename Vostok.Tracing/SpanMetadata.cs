﻿using System;

namespace Vostok.Tracing
{
    internal class SpanMetadata
    {
        public SpanMetadata(
            Guid traceId,
            Guid spanId,
            Guid? parentSpanId,
            DateTimeOffset beginTimestamp,
            DateTimeOffset? endTimestamp)
        {
            TraceId = traceId;
            SpanId = spanId;
            ParentSpanId = parentSpanId;
            BeginTimestamp = beginTimestamp;
            EndTimestamp = endTimestamp;
        }

        public Guid TraceId { get; }

        public Guid SpanId { get; }

        public Guid? ParentSpanId { get; }

        public DateTimeOffset BeginTimestamp { get; }

        public DateTimeOffset? EndTimestamp { get; }

        public SpanMetadata SetBeginTimestamp(DateTimeOffset timestamp) =>
            new SpanMetadata(TraceId, SpanId, ParentSpanId, timestamp, EndTimestamp);

        public SpanMetadata SetEndTimestamp(DateTimeOffset? timestamp) =>
            new SpanMetadata(TraceId, SpanId, ParentSpanId, BeginTimestamp, timestamp);
    }
}