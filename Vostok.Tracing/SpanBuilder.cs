using System;
using System.Diagnostics;
using System.Net;
using Vostok.Commons.Collections;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    internal class SpanBuilder : ISpanBuilder
    {
        private readonly TraceContextScope contextScope;
        private readonly TracerSettings configuration;
        private readonly Stopwatch stopwatch;

        private volatile SpanMetadata metadata;
        private volatile ImmutableArrayDictionary<string, string> annotations;

        public SpanBuilder(TraceContextScope contextScope, TracerSettings configuration)
        {
            this.contextScope = contextScope;
            this.configuration = configuration;

            var currentTimestamp = DateTimeOffset.UtcNow;

            stopwatch = Stopwatch.StartNew();

            metadata = new SpanMetadata(
                contextScope.Current.TraceId, 
                contextScope.Current.SpanId,
                contextScope.Parent?.SpanId,
                currentTimestamp,
                currentTimestamp);

            annotations = ImmutableArrayDictionary<string, string>.Empty;

            SetDefaultAnnotations();
        }

        public ISpan CurrentSpan => new Span(metadata, annotations);

        public void SetAnnotation(string key, string value, bool allowOverwrite = true)
        {
            annotations = annotations.Set(key, value, allowOverwrite);
        }

        public void SetBeginTimestamp(DateTimeOffset timestamp)
        {
            metadata = metadata.SetBeginTimestamp(timestamp);
        }

        public void SetEndTimestamp(DateTimeOffset? timestamp)
        {
            metadata = metadata.SetEndTimestamp(timestamp);
        }

        public void Dispose()
        {
            try
            {
                FinalizeSpan();

                configuration.Sender.Send(CurrentSpan);
            }
            finally
            {
                contextScope.Dispose();
            }
        }

        private void SetDefaultAnnotations()
        {
            SetAnnotation(WellKnownAnnotations.Common.Host, Dns.GetHostName());
        }

        private void FinalizeSpan()
        {
            if (metadata.EndTimestamp.HasValue)
                SetEndTimestamp(metadata.BeginTimestamp + stopwatch.Elapsed);
        }
    }
}