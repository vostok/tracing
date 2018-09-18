using System;
using JetBrains.Annotations;
using Vostok.Context;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    [PublicAPI]
    public static class WithInheritanceExtensions
    {
        /// <summary>
        /// <para>Returns a wrapper tracer that facilitates inheritance of annotatio with given <paramref name="name"/> from parent to child spans inside current process.</para>
        /// <para>This mechanism is best illustrated by the following code snippet:</para>
        /// <code>
        ///     tracer = tracer.WithAnnotationInheritance(new [] { "url" });
        /// 
        ///     using (var parentBuilder = tracer.BeginSpan())
        ///     {
        ///         parentBuilder.SetAnnotation("url", ...);
        ///         ...
        ///         using (var childBuilder = tracer.BeginSpan())
        ///         {
        ///             // childBuilder automatically gets 'url' from parent
        ///         }
        ///     }
        /// </code><br/>
        /// <para>By default, existing properties are not overwritten. This can be changed via <paramref name="allowOverwrite"/> parameter.</para>
        /// <para>By default, <c>null</c> values are not inherited. This can be changed via <paramref name="allowNullValues"/> parameter.</para>
        /// <para>By default, annotations are only inherited by direct child spans, but do not propagate further (depth = <c>1</c>). This can be changed via <paramref name="maximumDepth"/> parameter.</para>
        /// </summary>
        [Pure]
        public static ITracer WithAnnotationInheritance(
            [NotNull] this ITracer tracer,
            [NotNull] string name,
            bool allowOverwrite = false,
            bool allowNullValues = false,
            int maximumDepth = 1)
        {
            return new WithInheritanceTracer(tracer, new [] { name }, allowOverwrite, allowNullValues, maximumDepth);
        }

        /// <summary>
        /// <para>Returns a wrapper tracer that facilitates inheritance of annotations with given <paramref name="names"/> from parent to child spans inside current process.</para>
        /// <para>This mechanism is best illustrated by the following code snippet:</para>
        /// <code>
        ///     tracer = tracer.WithAnnotationInheritance(new [] { "url", "method" });
        /// 
        ///     using (var parentBuilder = tracer.BeginSpan())
        ///     {
        ///         parentBuilder.SetAnnotation("url", ...);
        ///         parentBuilder.SetAnnotation("method", ...);
        ///         ...
        ///         using (var childBuilder = tracer.BeginSpan())
        ///         {
        ///             // childBuilder automatically gets 'url' and 'method' from parent
        ///         }
        ///     }
        /// </code><br/>
        /// <para>By default, existing properties are not overwritten. This can be changed via <paramref name="allowOverwrite"/> parameter.</para>
        /// <para>By default, <c>null</c> values are not inherited. This can be changed via <paramref name="allowNullValues"/> parameter.</para>
        /// <para>By default, annotations are only inherited by direct child spans, but do not propagate further (depth = <c>1</c>). This can be changed via <paramref name="maximumDepth"/> parameter.</para>
        /// </summary>
        [Pure]
        public static ITracer WithAnnotationsInheritance(
            [NotNull] this ITracer tracer,
            [NotNull] string[] names,
            bool allowOverwrite = false,
            bool allowNullValues = false,
            int maximumDepth = 1)
        {
            return new WithInheritanceTracer(tracer, names, allowOverwrite, allowNullValues, maximumDepth);
        }

        private class WithInheritanceTracer : ITracer
        {
            private readonly ITracer tracer;
            private readonly string[] names;
            private readonly bool allowOverwrite;
            private readonly bool allowNullValues;
            private readonly int maximumDepth;

            public WithInheritanceTracer(
                ITracer tracer, 
                string[] names, 
                bool allowOverwrite, 
                bool allowNullValues, 
                int maximumDepth)
            {
                if (maximumDepth < 1)
                    throw new ArgumentOutOfRangeException(nameof(maximumDepth), $"Maximum inheritance depth must be positive (got '{maximumDepth}').");

                this.tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
                this.names = names ?? throw new ArgumentNullException(nameof(names));
                this.allowOverwrite = allowOverwrite;
                this.allowNullValues = allowNullValues;
                this.maximumDepth = maximumDepth;
            }

            public TraceContext CurrentContext
            {
                get => tracer.CurrentContext;
                set => tracer.CurrentContext = value;
            }

            public ISpanBuilder BeginSpan()
            {
                var currentContext = FlowingContext.Globals.Get<InheritanceContext>();

                var spanBuilder = tracer.BeginSpan();

                EnrichSpanWithInheritedAnnotations(spanBuilder, currentContext);

                var newContext = CreateNewContext(spanBuilder, currentContext);

                if (ReferenceEquals(currentContext, newContext))
                    return spanBuilder;

                return new SpanBuilderWrapper(spanBuilder, FlowingContext.Globals.Use(newContext));
            }

            private void EnrichSpanWithInheritedAnnotations([NotNull] ISpanBuilder builder, [CanBeNull] InheritanceContext context)
            {
                var parentSpan = context?.ParentSpanProvider();
                if (parentSpan == null)
                    return;

                foreach (var name in names)
                {
                    if (parentSpan.Annotations.TryGetValue(name, out var parentValue) && (allowNullValues || parentValue != null))
                    {
                        builder.SetAnnotation(name, parentValue, allowOverwrite);
                    }
                }
            }

            [CanBeNull]
            private InheritanceContext CreateNewContext([NotNull] ISpanBuilder builder, [CanBeNull] InheritanceContext oldContext)
            {
                var currentDepth = oldContext?.CurrentDepth ?? 0;
                if (currentDepth >= maximumDepth)
                    return null;

                return new InheritanceContext(() => builder.CurrentSpan, currentDepth + 1);
            }
        }

        private class InheritanceContext
        {
            public InheritanceContext(Func<ISpan> parentSpanProvider, int currentDepth)
            {
                ParentSpanProvider = parentSpanProvider;
                CurrentDepth = currentDepth;
            }

            public Func<ISpan> ParentSpanProvider { get; }

            public int CurrentDepth { get; }
        }

        private class SpanBuilderWrapper : ISpanBuilder
        {
            private readonly ISpanBuilder builder;
            private readonly IDisposable contextScope;

            public SpanBuilderWrapper(ISpanBuilder builder, IDisposable contextScope)
            {
                this.builder = builder;
                this.contextScope = contextScope;
            }

            public ISpan CurrentSpan => builder.CurrentSpan;

            public void Dispose()
            {
                contextScope.Dispose();
                builder.Dispose();
            }

            public void SetAnnotation(string key, string value, bool allowOverwrite = true)
            {
                builder.SetAnnotation(key, value, allowOverwrite);
            }

            public void SetBeginTimestamp(DateTimeOffset timestamp)
            {
                builder.SetBeginTimestamp(timestamp);
            }

            public void SetEndTimestamp(DateTimeOffset? timestamp)
            {
                builder.SetEndTimestamp(timestamp);
            }
        }
    }
}
