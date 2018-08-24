using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    [PublicAPI]
    public class TraceConfiguration
    {
        [NotNull]
        public ISpanSender SpanSender { get; set; }

        /// <summary>
        /// Fields to be added as trace annotations from parent span
        /// </summary>
        public ISet<string> InheritedFieldsWhitelist { get; set; } = new HashSet<string>(StringComparer.Ordinal);
    }
}