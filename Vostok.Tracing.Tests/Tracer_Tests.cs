using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing.Tests
{
    public class Tracer_Tests
    {
        private ITraceReporter traceReporter;
        private ITracer tracer;

        [SetUp]
        public void SetUp()
        {
            traceReporter = Substitute.For<ITraceReporter>();
            var traceConfiguration = new TraceConfiguration();
            traceConfiguration.TraceReporter = traceReporter;
            tracer = new Tracer(traceConfiguration);
        }

        [Test]
        public void CurrentContext_should_change_context_when_begin_child_span()
        {
            using (var span1 = tracer.BeginSpan())
            {
                var span1Context = tracer.CurrentContext;
                using (var span2 = tracer.BeginSpan())
                {
                    var span2Context = tracer.CurrentContext;

                    span1Context.TraceId.Should().Be(span2Context.TraceId);
                    span1Context.SpanId.Should().NotBe(span2Context.SpanId);
                }
            }
        }

        [Test]
        public void CurrentContext_should_change_context_back_when_end_child_span()
        {
            using (var span1 = tracer.BeginSpan())
            {
                var span1ContextBeforeStartChildSpan = tracer.CurrentContext;

                using (var span2 = tracer.BeginSpan())
                {
                    var span2Context = tracer.CurrentContext;

                    span1ContextBeforeStartChildSpan.TraceId.Should().Be(span2Context.TraceId);
                    span1ContextBeforeStartChildSpan.SpanId.Should().NotBe(span2Context.SpanId);
                }

                var span1ContextAfterStartChildSpan = tracer.CurrentContext;

                span1ContextBeforeStartChildSpan.TraceId.Should().Be(span1ContextAfterStartChildSpan.TraceId);
                span1ContextBeforeStartChildSpan.SpanId.Should().Be(span1ContextAfterStartChildSpan.SpanId);
            }
        }
    }
}