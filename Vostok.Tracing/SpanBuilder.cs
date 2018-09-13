using System;
using System.Diagnostics;
using System.Net;
using Vostok.Commons.Collections;
using Vostok.Context;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    internal class SpanBuilder : ISpanBuilder
    {
        private readonly TraceContextScope contextScope;
        private readonly UnboundedObjectPool<Span> objectPool;
        private readonly TraceConfiguration configuration;
        private readonly Span span;
        private readonly Stopwatch stopwatch;
        private readonly ISpan parentSpan;

        public SpanBuilder(TraceContextScope contextScope, UnboundedObjectPool<Span> objectPool, TraceConfiguration configuration)
        {
            this.contextScope = contextScope;
            this.objectPool = objectPool;
            this.configuration = configuration;

            stopwatch = Stopwatch.StartNew();

            span = objectPool.Acquire();

            parentSpan = FlowingContext.Globals.Get<FlowingContextStorageSpan>()?.Span;
            FlowingContext.Globals.Set(new FlowingContextStorageSpan(span));

            InitializeSpan();
            EnrichSpanWithInheritedFields();
            SetDefaultAnnotations();
        }

        public bool IsEndless { get; set; }

        public void SetAnnotation(string key, string value, bool allowOverwrite = true)
        {
            span.SetAnnotation(key, value, allowOverwrite);
        }

        public void SetBeginTimestamp(DateTimeOffset timestamp)
        {
            span.BeginTimestamp = timestamp;
        }

        public void SetEndTimestamp(DateTimeOffset? timestamp)
        {
            span.EndTimestamp = timestamp;
            IsEndless = timestamp == null;
        }

        public void Dispose()
        {
            try
            {
                FinalizeSpan();

                var sendResult = configuration.SpanSender.Send(span);
                if (sendResult == SpanSendResult.Sent)
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

        private void SetDefaultAnnotations()
        {
            span.SetAnnotation(WellKnownAnnotations.Common.Host, Dns.GetHostName(), true);
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

        private void EnrichSpanWithInheritedFields()
        {
            if (parentSpan == null)
                return;

            foreach (var field in configuration.InheritedFieldsWhitelist)
            {
                if (parentSpan.Annotations.TryGetValue(field, out var value))
                    SetAnnotation(field, value);
            }
        }

        internal class FlowingContextStorageSpan
        {
            public FlowingContextStorageSpan(ISpan span)
            {
                Span = span;
            }

            public ISpan Span { get; set; }
        }
    }
}