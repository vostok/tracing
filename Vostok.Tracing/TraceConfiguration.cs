using System;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    public class TraceConfiguration
    {
        public ITraceReporter TraceReporter { get; set; }

        // CR(iloktionov): This is better implemented with an extension over ITracer in abstractions.
        // CR(iloktionov): See https://github.com/vostok/logging.abstractions/blob/master/Vostok.Logging.Abstractions/Extensions/WithPropertyLogExtensions.cs
        public Action<ISpanBuilder> EnrichSpanAction { get; set; }
    }
}