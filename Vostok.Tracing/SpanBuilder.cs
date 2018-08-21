using System;
using System.Diagnostics;
using Vostok.Commons.Collections;
using Vostok.Context;
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
            AddAnnotationsFromParentTraceContext();
        }

        public bool IsEndless { get; set; }

        // CR(iloktionov): Not the best place for inheritance configuration.
        public void SetAnnotation<TValue>(string key, TValue value, bool copyToChild = false)
        {
            span.AddAnnotation(key, value?.ToString());
            if (copyToChild)
            {
                //will it work?
                contextScope.Current.InheritAnnotations.Add(key, value?.ToString());

                //maybe..
                //var newContext = contextScope.Current;
                //newContext.InheritAnnotations.Add(key, value?.ToString());
                //FlowingContext.Globals.Set(newContext);
            }
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
                // CR(iloktionov): What if reporter holds on to our Span object?
                reporter.SendSpan(span);
            }
            finally
            {
                CleanupSpan();
                objectPool.Return(span);
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

        private void AddAnnotationsFromParentTraceContext()
        {
            foreach (var currentInheritAnnotation in contextScope.Current.InheritAnnotations)
            {
                span.AddAnnotation(currentInheritAnnotation.Key, currentInheritAnnotation.Value);
            }
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