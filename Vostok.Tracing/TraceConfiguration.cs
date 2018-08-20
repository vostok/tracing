using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    public class TraceConfiguration
    {
        public ITraceReporter TraceReporter { get; set; }
    }
}