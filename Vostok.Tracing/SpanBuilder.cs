using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Configuration;
using Vostok.Tracing.Helpers;
using SpanAnnotations = Vostok.Commons.Collections.ImmutableArrayDictionary<string, string>;

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
            if (metadata.EndTimestamp.HasValue)
                metadata = metadata.SetEndTimestamp(metadata.BeginTimestamp + watch.Elapsed);

            using (contextScope)
            {
                settings.Sender.Send(CurrentSpan);
            }
        }

        private static SpanMetadata ConstructInitialMetadata([NotNull] TraceContext currentContext, [CanBeNull] TraceContext parentContext)
        {
            // TODO(iloktionov): Use something more precise on Windows and .NET Framework.
            var currentTimestamp = DateTimeOffset.UtcNow;

            return new SpanMetadata(
                currentContext.TraceId,
                currentContext.SpanId,
                parentContext?.SpanId,
                currentTimestamp,
                currentTimestamp);
        }

        private static SpanAnnotations ConstructInitialAnnotations([NotNull] TracerSettings settings)
        {
            var annotations = SpanAnnotations.Empty
                .Set(WellKnownAnnotations.Common.Host, settings.Host ?? EnvironmentHelper.Host);

            if (settings.Environment != null)
                annotations = annotations.Set(WellKnownAnnotations.Common.Environment, settings.Environment);

            if (settings.Application != null)
                annotations = annotations.Set(WellKnownAnnotations.Common.Application, settings.Application);

            return annotations;
        }
    }
}
