using System;
using System.Collections.Generic;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    internal class Span : ISpan
    {
        private readonly Dictionary<string, string> annotations;

        internal Span()
        {
            annotations = new Dictionary<string, string>();
        }

        public Guid TraceId { get; set; }
        public Guid SpanId { get; set; }
        public Guid? ParentSpanId { get; set; }
        public DateTimeOffset BeginTimestamp { get; set; }
        public DateTimeOffset? EndTimestamp { get; set; }
        public IReadOnlyDictionary<string, string> Annotations => annotations;

        public void AddAnnotation(string key, string value, bool allowOverwrite)
        {
            if (!annotations.ContainsKey(key))
            {
                annotations.Add(key, value);
                return;
            }
            if (allowOverwrite)
                annotations[key] = value;
        }

        public void ClearAnnotations()
        {
            annotations.Clear();
        }
    }
}