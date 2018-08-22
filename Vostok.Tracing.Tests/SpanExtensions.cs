using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing.Tests
{
    internal static class SpanExtensions
    {
        internal static ISpan Clone(this ISpan span)
        {
            var clonedSpan = new Span()
            {
                TraceId = span.TraceId,
                SpanId = span.SpanId,
                ParentSpanId = span.ParentSpanId,
                BeginTimestamp = span.BeginTimestamp,
                EndTimestamp = span.EndTimestamp
            };

            foreach (var x in span.Annotations)
            {
                clonedSpan.AddAnnotation(x.Key, x.Value, true);
            }

            return clonedSpan;
        }
    }
}