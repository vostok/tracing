using System;
using Vostok.Context;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing.Helpers
{
    internal class TraceContextSerializer : IContextSerializer<TraceContext>
    {
        private const char Delimiter = ';';
        private static char[] DelimiterArray = {Delimiter};

        public string Serialize(TraceContext value) => $"{value.TraceId}{Delimiter}{value.SpanId}";

        public TraceContext Deserialize(string input)
        {
            var parts = input.Split(DelimiterArray, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2 ||
                !Guid.TryParse(parts[0], out var traceId) ||
                !Guid.TryParse(parts[1], out var spanId))
            {
                throw new FormatException($"Failed to parse {nameof(TraceContext)} from following input: '{input}'.");
            }

            return new TraceContext(traceId, spanId);
        }
    }
}
