using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Vostok.Commons.Environment;
using Vostok.Commons.Time;
using Vostok.Tracing.Abstractions;
using SpanAnnotations = Vostok.Commons.Collections.ImmutableArrayDictionary<string, object>;

namespace Vostok.Tracing
{
    internal class SpanBuilder : ISpanBuilder
    {
        private readonly TracerSettings settings;
        private readonly IDisposable contextScope;
        private readonly Stopwatch watch;

        private volatile SpanMetadata metadata;
        private volatile SpanAnnotations annotations;

        public SpanBuilder(
            [NotNull] TracerSettings settings,
            [NotNull] IDisposable contextScope,
            [NotNull] TraceContext currentContext,
            [CanBeNull] TraceContext parentContext)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.contextScope = contextScope ?? throw new ArgumentNullException(nameof(contextScope));

            metadata = ConstructInitialMetadata(currentContext, parentContext);
            annotations = ConstructInitialAnnotations(settings);
            watch = Stopwatch.StartNew();
        }

        public ISpan CurrentSpan => new Span(metadata, annotations);

        public void SetAnnotation(string key, object value, bool allowOverwrite = true)
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
            if (metadata.EndTimestamp == DateTimeOffset.MinValue)
                metadata = metadata.SetEndTimestamp(metadata.BeginTimestamp + watch.Elapsed);

            using (contextScope)
            {
                settings.Sender.Send(CurrentSpan);
            }
        }

        private static SpanMetadata ConstructInitialMetadata([NotNull] TraceContext currentContext, [CanBeNull] TraceContext parentContext)
        {
            var beginTimestamp = PreciseDateTime.Now;
            var endTimestamp = DateTimeOffset.MinValue;

            var parentSpanId = parentContext?.SpanId;
            if (parentSpanId == Guid.Empty)
                parentSpanId = null;

            return new SpanMetadata(
                currentContext.TraceId,
                currentContext.SpanId,
                parentSpanId,
                beginTimestamp,
                endTimestamp);
        }

        private static SpanAnnotations ConstructInitialAnnotations([NotNull] TracerSettings settings)
        {
            //(deniaa): 16 annotations - is a common size (power of two) for a spans according to HttpRequest{XXX}SpanBuilder.
            //(deniaa): There are spans for Cassandra with 18+ annotations, but it's still better to start with a capacity of 16 than the default 4.
            //(deniaa): There are "empty" spans, but they rare and usually this is a bug.
            var annotations = new SpanAnnotations(16);
            
            //(deniaa): We just create this ImmutableArrayDictionary and guarantee that we are single holder of this reference in this method.
            //(deniaa): So here we can use AppendUnsafe method.
            annotations.AppendUnsafe(WellKnownAnnotations.Common.Host, settings.Host ?? EnvironmentInfo.Host);
            annotations.AppendUnsafe(WellKnownAnnotations.Common.Application, settings.Application ?? EnvironmentInfo.Application);

            if (settings.Environment != null)
                annotations.AppendUnsafe(WellKnownAnnotations.Common.Environment, settings.Environment);

            return annotations;
        }
    }
}