using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    // CR(iloktionov): Convert to internal.

    public class SpanBuilder : ISpanBuilder
    {
        private readonly TraceContextScope contextScope;
        private readonly ITraceReporter reporter;
        private readonly Span span;
        private readonly Stopwatch stopwatch;

        // CR(iloktionov): Just use Dictionary in Span instead.
        private readonly Dictionary<string, string> spanAnnotations = new Dictionary<string, string>();

        // CR(iloktionov): Forgot to initialize BeginTimestamp.
        public SpanBuilder(TraceContextScope contextScope, ITraceReporter reporter)
        {
            this.contextScope = contextScope;
            this.reporter = reporter;
            span = new Span
            {
                TraceId = contextScope.Current.TraceId,
                SpanId = contextScope.Current.SpanId,
                ParentSpanId = contextScope.Parent?.SpanId
            };
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public bool IsCanceled { get; set; }
        public bool IsEndless { get; set; }

        public void SetAnnotation<TValue>(string key, TValue value)
        {
            spanAnnotations[key] = value?.ToString();
        }

        public void SetBeginTimestamp(DateTimeOffset timestamp)
        {
            span.BeginTimestamp = timestamp;
        }

        public void SetEndTimestamp(DateTimeOffset timestamp)
        {
            span.EndTimestamp = timestamp;
        }

        public void Dispose()
        {
            try
            {
                if (!IsCanceled)
                {
                    FinalizeSpan();
                    reporter.SendSpan(span);
                }
            }
            finally
            {
                contextScope.Dispose();
            }
        }

        private void FinalizeSpan()
        {
            span.Annotations = spanAnnotations;
            if (!IsEndless)
                span.EndTimestamp = span.BeginTimestamp + stopwatch.Elapsed;
        }
    }
}