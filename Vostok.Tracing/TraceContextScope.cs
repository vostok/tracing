using System;
using Vostok.Context;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    internal class TraceContextScope : IDisposable
    {
        internal TraceContextScope(TraceContext current, TraceContext parent)
        {
            Current = current;
            Parent = parent;
        }

        public TraceContext Current { get; }
        public TraceContext Parent { get; }

        public void Dispose()
        {
            FlowingContext.Globals.Set(Parent);
        }
    }
}