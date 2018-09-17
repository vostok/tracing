using JetBrains.Annotations;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    [PublicAPI]
    public class TraceConfiguration
    {
        [NotNull]
        public ISpanSender SpanSender { get; set; }
    }
}