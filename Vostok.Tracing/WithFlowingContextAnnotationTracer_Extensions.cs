using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Context;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    [PublicAPI]
    public static class WithFlowingContextAnnotationTracerExtensions
    {
        [Pure]
        public static ITracer WithFlowingContextAnnotation(this ITracer tracer, [NotNull] string propertyKey, bool allowOverwrite = false)
        {
            return tracer.WithAnnotation(propertyKey, () => GetFlowingContextPropertyValue(propertyKey), allowOverwrite);
        }

        [Pure]
        public static ITracer WithFlowingContextAnnotations(this ITracer tracer, [ItemNotNull] IEnumerable<string> propertyKeys, bool allowOverwrite = false)
        {
            return tracer.WithAnnotations(GetFlowingContextPropertiesValue(propertyKeys), allowOverwrite);
        }

        private static Dictionary<string,string> GetFlowingContextPropertiesValue(IEnumerable<string> propertyKeys)
        {
            return propertyKeys.ToDictionary(propertyKey => propertyKey, GetFlowingContextPropertyValue);
        }

        private static string GetFlowingContextPropertyValue(string propertyName)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            return FlowingContext.Properties.Current.TryGetValue(propertyName, out var val) ? val?.ToString() : null;
        }
    }
}