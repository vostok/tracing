using System;
using System.Collections.Generic;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    public class TraceConfiguration
    {
        public ITraceReporter TraceReporter { get; set; }

        /// <summary>
        /// Fields to be added as trace annotations from parent span
        /// </summary>
        public ISet<string> InheritedFieldsWhitelist { get; set; } = new HashSet<string>(StringComparer.Ordinal);
    }
}