using System;
using System.Diagnostics;
using Vostok.Commons.Collections;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    internal class SpanBuilder : ISpanBuilder
    {
        private readonly TraceContextScope contextScope;
        private readonly UnboundedObjectPool<Span> objectPool;
        private readonly ITraceReporter reporter;
        private readonly Span span;
        private readonly Stopwatch stopwatch;

        public SpanBuilder(TraceContextScope contextScope, UnboundedObjectPool<Span> objectPool, ITraceReporter reporter)
        {
            this.contextScope = contextScope;
            this.objectPool = objectPool;
            this.reporter = reporter;

            stopwatch = Stopwatch.StartNew();

            span = objectPool.Acquire();
            InitializeSpan();
        }

        public bool IsEndless { get; set; }

        public void SetAnnotation<TValue>(string key, TValue value)
        {
            span.AddAnnotation(key, value?.ToString());
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
                FinalizeSpan();

                var sendResult = reporter.SendSpan(span);
                if (sendResult == SpanSendResult.Sended)
                {
                    CleanupSpan();
                    objectPool.Return(span);
                }
            }
            finally
            {
                contextScope.Dispose();
            }
        }

        private void InitializeSpan()
        {
            span.TraceId = contextScope.Current.TraceId;
            span.SpanId = contextScope.Current.SpanId;
            span.ParentSpanId = contextScope.Parent?.SpanId;
            span.BeginTimestamp = DateTimeOffset.UtcNow;
        }

        private void FinalizeSpan()
        {
            if (!IsEndless && !span.EndTimestamp.HasValue)
                span.EndTimestamp = span.BeginTimestamp + stopwatch.Elapsed;
        }

        private void CleanupSpan()
        {
            span.ClearAnnotations();
            span.ParentSpanId = null;
            span.EndTimestamp = null;
        }
    }
}