﻿using NSubstitute;
using NUnit.Framework;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing.Tests
{
    public class Tracer_Tests
    {
        private ITraceReporter traceReporter;
        private ITracer tracer;

        // private ISpan observedSpan;

        [SetUp]
        public void SetUp()
        {
            traceReporter = Substitute.For<ITraceReporter>();
            var traceConfiguration = new TraceConfiguration();
            traceConfiguration.TraceReporter = traceReporter;
            tracer = new Tracer(traceConfiguration);

            // observedSpan = null;

            // traceReporter
                // .When(r => r.SendSpan(Arg.Any<ISpan>()))
                // .Do(info => observedSpan = info.Arg<ISpan>());
        }

        [Test]
        public void BeginSpan_should_send_spans()
        {
            using (var span1 = tracer.BeginSpan())
            {
                using (var span2 = tracer.BeginSpan())
                {
                }
            }

            traceReporter.Received(2).SendSpan(Arg.Any<Span>());
        }
    }
}