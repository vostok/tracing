using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    public interface ITraceConfiguration
    {
        ITraceReporter TraceReporter { get; }
    }
}