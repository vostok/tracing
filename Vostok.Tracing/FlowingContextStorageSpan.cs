using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    internal class FlowingContextStorageSpan
    {
        internal static FlowingContextStorageSpan CreateFromSpan(ISpan span)
        {
            var flowingContextStorageSpan = new FlowingContextStorageSpan()
            {
                Span = span
            };
            return flowingContextStorageSpan;
        }

        internal ISpan Span { get; set; }
    }
}