using System;
using System.Collections.Generic;
using Vostok.Commons.Collections;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    internal class Span : ISpan
    {
        private readonly SpanMetadata metadata;
        private readonly ImmutableArrayDictionary<string, string> annotations;

        public Span(SpanMetadata metadata, ImmutableArrayDictionary<string, string> annotations)
        {
            this.metadata = metadata;
            this.annotations = annotations;
        }

        public Guid TraceId => metadata.TraceId;

        public Guid SpanId => metadata.SpanId;

        public Guid? ParentSpanId => metadata.ParentSpanId;

        public DateTimeOffset BeginTimestamp => metadata.BeginTimestamp;

        public DateTimeOffset? EndTimestamp => metadata.EndTimestamp;

        public IReadOnlyDictionary<string, string> Annotations => annotations;
    }
}
