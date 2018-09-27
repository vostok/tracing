using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Context;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    [PublicAPI]
    public static class WithFlowingContextExtensions
    {
        /// <summary>
        /// <para>Returns a wrapper tracer that adds the value of <see cref="FlowingContext"/> global of given type <typeparamref name="T"/> to each returned <see cref="ISpanBuilder"/> as annotation.</para>
        /// <para>Uses given <paramref name="annotationName"/> as the name for added span annotation.</para>
        /// <para>By default, existing properties are not overwritten. This can be changed via <paramref name="allowOverwrite"/> parameter.</para>
        /// <para>By default, <c>null</c> values are not added to spans. This can be changed via <paramref name="allowNullValues"/> parameter.</para>
        /// </summary>
        [Pure]
        public static ITracer WithFlowingContextGlobal<T>(
            [NotNull] this ITracer tracer,
            [NotNull] string annotationName,
            bool allowOverwrite = false,
            bool allowNullValues = false)
        {
            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            if (annotationName == null)
                throw new ArgumentNullException(nameof(annotationName));

            return tracer.WithAnnotation(annotationName, 
                () => FlowingContext.Globals.Get<T>(), allowOverwrite, allowNullValues);
        }

        /// <summary>
        /// <para>Returns a wrapper tracer that adds the value of <see cref="FlowingContext"/> property with given <param name="contextPropertyName"></param> to each returned <see cref="ISpanBuilder"/> as annotation.</para>
        /// <para>By default, the value is added to span with <paramref name="contextPropertyName"/>. This can be changed by providing a non-null <paramref name="annotationName"/> parameter.</para>
        /// <para>By default, existing properties are not overwritten. This can be changed via <paramref name="allowOverwrite"/> parameter.</para>
        /// <para>By default, <c>null</c> values are not added to spans. This can be changed via <paramref name="allowNullValues"/> parameter.</para>
        /// </summary>
        [Pure]
        public static ITracer WithFlowingContextProperty(
            [NotNull] this ITracer tracer, 
            [NotNull] string contextPropertyName,
            [CanBeNull] string annotationName = null,
            bool allowOverwrite = false,
            bool allowNullValues = false)
        {
            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            if (contextPropertyName == null)
                throw new ArgumentNullException(nameof(contextPropertyName));

            return tracer.WithAnnotation(annotationName ?? contextPropertyName, 
                () => GetContextPropertyOrNull(contextPropertyName), allowOverwrite, allowNullValues);
        }

        /// <summary>
        /// <para>Returns a wrapper tracer that adds values of all properties with names in given <paramref name="names"/> array from <see cref="FlowingContext"/>  to each returned <see cref="ISpanBuilder"/> as annotation.</para>
        /// <para>By default, existing properties are not overwritten. This can be changed via <paramref name="allowOverwrite"/> parameter.</para>
        /// <para>By default, <c>null</c> values are not added to spans. This can be changed via <paramref name="allowNullValues"/> parameter.</para>
        /// </summary>
        [Pure]
        public static ITracer WithFlowingContextProperties(
            [NotNull] this ITracer tracer, 
            [NotNull] string[] names, 
            bool allowOverwrite = false,
            bool allowNullValues = false)
        {
            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            if (names == null)
                throw new ArgumentNullException(nameof(names));

            return tracer.WithAnnotations(() => GetContextProperties(names), allowOverwrite, allowNullValues);
        }

        /// <summary>
        /// <para>Returns a wrapper tracer that adds values all named properties from <see cref="FlowingContext"/> to each returned <see cref="ISpanBuilder"/> as annotation.</para>
        /// <para>By default, existing properties are not overwritten. This can be changed via <paramref name="allowOverwrite"/> parameter.</para>
        /// <para>By default, <c>null</c> values are not added to spans. This can be changed via <paramref name="allowNullValues"/> parameter.</para>
        /// </summary>
        [Pure]
        public static ITracer WithAllFlowingContextProperties(
            [NotNull] this ITracer tracer,
            bool allowOverwrite = false,
            bool allowNullValues = false)
        {
            if (tracer == null)
                throw new ArgumentNullException(nameof(tracer));

            return tracer.WithAnnotations(() => FlowingContext.Properties.Current.Select(pair => (pair.Key, pair.Value)), allowOverwrite, allowNullValues);
        }

        [CanBeNull]
        private static object GetContextPropertyOrNull(string name)
        {
            return FlowingContext.Properties.Current.TryGetValue(name, out var value) ? value : null;
        }

        [NotNull]
        private static IEnumerable<(string, object)> GetContextProperties(string[] names)
        {
            var currentProperties = FlowingContext.Properties.Current;

            foreach (var name in names)
            {
                if (currentProperties.TryGetValue(name, out var value))
                    yield return (name, value);
            }
        }
    }
}