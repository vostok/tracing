using System;
using System.Linq;
using Vostok.Context;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    //TODO: write tests
    public class TraceContextSerializer : IContextSerializer<TraceContext>
    {
        private const char Delimiter = ';';

        public string Serialize(TraceContext value)
        {
            return $"{value.TraceId}{Delimiter}{value.SpanId}";
        }

        public TraceContext Deserialize(string input)
        {
            var guids = input.Split(new[] {Delimiter}, StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToArray();
            if (guids.Length != 2)
            {
                throw new ArgumentException($"Input string provide invalid value: {input}");
            }

            return new TraceContext(guids[0], guids[1]);
        }
    }
}