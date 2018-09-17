using System;
using JetBrains.Annotations;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    [PublicAPI]
    public static class WithInheritanceExtensions
    {
        [Pure]
        public static ITracer WithAnnotationsInheritance(
            [NotNull] this ITracer tracer,
            [NotNull] string[] names,
            bool allowOverwrite = false,
            int inheritanceDepth = 1)
        {
            // TODO(iloktionov): implement

            throw new NotImplementedException();
        }
    }
}
