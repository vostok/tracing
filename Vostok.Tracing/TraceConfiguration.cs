using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    public class TraceConfiguration : ITraceConfiguration
    {
        public ITraceReporter TraceReporter { get; set; }
    }
}