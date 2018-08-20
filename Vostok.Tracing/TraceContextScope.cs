using System;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    // CR(iloktionov): Convert to internal.

    public class TraceContextScope : IDisposable
    {
        internal TraceContextScope(ITraceContext current, ITraceContext parent)
        {
            Current = current;
            Parent = parent;
        }

        public ITraceContext Current { get; }
        public ITraceContext Parent { get; }

        public void Dispose()
        {
            Tracer.CurrentTraceContext = Parent;
        }
    }
}