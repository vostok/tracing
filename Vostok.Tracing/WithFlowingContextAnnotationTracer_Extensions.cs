using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Context;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    public static class WithFlowingContextAnnotationTracer_Extensions
    {
        [Pure]
        public static ITracer WithFlowingContextPropertyAnnotation(this ITracer tracer, string flowingContextPropertyKey, bool allowOverwrite = false)
        {
            return new WithFlowingContextPropertiesAnnotationTracer(tracer, new[] {flowingContextPropertyKey}, allowOverwrite);
        }

        [Pure]
        public static ITracer WithFlowingContextPropertyAnnotations(this ITracer tracer, IEnumerable<string> flowingContextPropertyKeys, bool allowOverwrite = false)
        {
            return new WithFlowingContextPropertiesAnnotationTracer(tracer, flowingContextPropertyKeys, allowOverwrite);
        }

        private class WithFlowingContextPropertiesAnnotationTracer : ITracer
        {
            private readonly ITracer baseTracer;
            private readonly IEnumerable<string> flowingContextPropertyKeys;
            private readonly bool allowOverwrite;

            public WithFlowingContextPropertiesAnnotationTracer(ITracer baseTracer, IEnumerable<string> flowingContextPropertyKeys, bool allowOverwrite)
            {
                this.baseTracer = baseTracer;
                this.flowingContextPropertyKeys = flowingContextPropertyKeys;
                this.allowOverwrite = allowOverwrite;
            }

            public TraceContext CurrentContext
            {
                get => baseTracer.CurrentContext;
                set => baseTracer.CurrentContext = value;
            }

            public ISpanBuilder BeginSpan()
            {
                var span = baseTracer.BeginSpan();

                foreach (var flowingContextPropertyKey in flowingContextPropertyKeys)
                {
                    if (FlowingContext.Properties.Current.TryGetValue(flowingContextPropertyKey, out var val))
                    {
                        span.SetAnnotation(flowingContextPropertyKey, val?.ToString(), allowOverwrite);
                    }
                }

                return span;
            }
        }
    }
}