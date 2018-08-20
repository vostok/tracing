using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    internal class Span : ISpan, ICloneable
    {
        private readonly Dictionary<string, string> annotations;

        public Span()
        {
            annotations = new Dictionary<string, string>();
        }

        public Guid TraceId { get; set; }
        public Guid SpanId { get; set; }
        public Guid? ParentSpanId { get; set; }
        public DateTimeOffset BeginTimestamp { get; set; }
        public DateTimeOffset? EndTimestamp { get; set; }
        public IReadOnlyDictionary<string, string> Annotations => annotations;

        public void AddAnnotation(string key, string value)
        {
            annotations.Add(key, value);
        }

        public void ClearAnnotations()
        {
            annotations.Clear();
        }
        
        public object Clone()
        {
            var clonedSpan = new Span()
            {
                TraceId = TraceId,
                SpanId = SpanId,
                ParentSpanId = ParentSpanId,
                BeginTimestamp = BeginTimestamp,
                EndTimestamp = EndTimestamp
            };

            foreach (var x in Annotations)
            {
                clonedSpan.AddAnnotation(x.Key, x.Value);
            }

            return clonedSpan;
        }
    }
}