using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    // CR(iloktionov): Remove this interface.

    public interface ITraceConfiguration
    {
        ITraceReporter TraceReporter { get; }
    }
}