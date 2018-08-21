using System;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Collections;
using Vostok.Context;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing.Tests
{
    public class SpanBuilder_Tests
    {
        private static int ind = 0;
        private readonly Guid traceId = Guid.NewGuid();
        private readonly Guid spanId = Guid.NewGuid();
        private readonly Guid parentSpanId = Guid.NewGuid();
        private ISpanBuilder spanBuilder;
        private ITraceReporter traceReporter;
        private UnboundedObjectPool<Span> objectPool;

        private ISpan observedSpan;

        private static TraceContextScope CreateTraceContextScope(Guid? traceId = null, Guid? spanId = null, Guid? parentSpanId = null)
        {
            var currentContext = new TraceContext(traceId ?? Guid.NewGuid(), spanId ?? Guid.NewGuid());
            TraceContext parentTraceContext = null;
            if (parentSpanId.HasValue)
            {
                parentTraceContext = new TraceContext(currentContext.TraceId, parentSpanId.Value);
            }

            var traceContextScope = new TraceContextScope(currentContext, parentTraceContext);
            return traceContextScope;
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            objectPool = new UnboundedObjectPool<Span>(() => new Span());
        }

        [SetUp]
        public void SetUp()
        {
            traceReporter = Substitute.For<ITraceReporter>();

            FlowingContext.Globals.Set<TraceContext>(null);

            observedSpan = null;

            traceReporter
                .When(r => r.SendSpan(Arg.Any<ISpan>()))
                .Do(info => observedSpan = info.Arg<Span>().Clone());
        }

        [Test]
        public void Should_send_span()
        {
            using (spanBuilder = new SpanBuilder(CreateTraceContextScope(), objectPool, traceReporter))
            {
            }

            traceReporter.Received(1).SendSpan(Arg.Any<ISpan>());
        }

        [Test]
        public void Should_send_span_with_endtimestamp_when_span_is_not_endless()
        {
            using (spanBuilder = new SpanBuilder(CreateTraceContextScope(), objectPool, traceReporter))
            {
            }

            observedSpan.EndTimestamp.Should().NotBeNull();
        }

        [Test]
        public void Should_send_span_without_endtimestamp_when_span_is_endless()
        {
            using (spanBuilder = new SpanBuilder(CreateTraceContextScope(), objectPool, traceReporter))
            {
                spanBuilder.IsEndless = true;
            }

            observedSpan.EndTimestamp.Should().BeNull();
        }

        [Test]
        public void Should_dispose_tracecontextscope()
        {
            var traceContextScope = CreateTraceContextScope();

            using (spanBuilder = new SpanBuilder(traceContextScope, objectPool, traceReporter))
            {
            }

            var context = FlowingContext.Globals.Get<TraceContext>();
            context.Should().BeNull();
        }

        [Test]
        public void Should_dispose_tracecontextscope_when_tracereporter_failed()
        {
            traceReporter.When(x => x.SendSpan(Arg.Any<ISpan>())).Do(x => throw new Exception());

            var traceContextScope = CreateTraceContextScope();

            Assert.Throws<Exception>(
                () =>
                {
                    using (spanBuilder = new SpanBuilder(traceContextScope, objectPool, traceReporter))
                    {
                    }
                });

            var context = FlowingContext.Globals.Get<TraceContext>();
            context.Should().BeNull();
        }

        [Test]
        public void Should_send_span_with_parentspanid_when_parentcontextscope_exist()
        {
            var traceContextScope = CreateTraceContextScope(parentSpanId: parentSpanId);

            using (spanBuilder = new SpanBuilder(traceContextScope, objectPool, traceReporter))
            {
            }

            observedSpan.ParentSpanId.Should().Be(parentSpanId);
        }

        [Test]
        public void Should_send_span_with_traceid_and_spanid_from_currentscope()
        {
            var traceContextScope = CreateTraceContextScope(traceId, spanId);

            using (spanBuilder = new SpanBuilder(traceContextScope, objectPool, traceReporter))
            {
            }

            observedSpan.TraceId.Should().Be(traceId);
            observedSpan.SpanId.Should().Be(spanId);
        }

        [Test]
        public void Should_send_span_with_endtimestamp()
        {
            var traceContextScope = CreateTraceContextScope();

            using (spanBuilder = new SpanBuilder(traceContextScope, objectPool, traceReporter))
            {
            }

            observedSpan.BeginTimestamp.Should().BeMoreThan(TimeSpan.Zero);
            observedSpan.EndTimestamp.Should().BeAfter(observedSpan.BeginTimestamp);
        }

        [Test]
        public void Should_send_span_with_empty_annotations_when_no_annotation_added()
        {
            var traceContextScope = CreateTraceContextScope();

            using (spanBuilder = new SpanBuilder(traceContextScope, objectPool, traceReporter))
            {
            }

            observedSpan.Annotations.Should().BeEmpty();
        }

        [Test]
        public void Should_send_span_with_added_annotation_after_adding()
        {
            var traceContextScope = CreateTraceContextScope();

            const string customAnnotationKey = "key";
            const string customAnnotationValue = "value";

            using (spanBuilder = new SpanBuilder(traceContextScope, objectPool, traceReporter))
            {
                spanBuilder.SetAnnotation(customAnnotationKey, customAnnotationValue);
            }

            observedSpan.Annotations.Count.Should().Be(1);

            var addedAnnotation = observedSpan.Annotations.First();
            addedAnnotation.Key.Should().Be(customAnnotationKey);
            addedAnnotation.Value.Should().Be(customAnnotationValue);
        }

        [Test]
        public void Should_send_span_with_custom_begintimestamp_when_called()
        {
            var traceContextScope = CreateTraceContextScope();

            var timestamp = new DateTimeOffset(2018, 07, 09, 09, 0, 0, new TimeSpan(5, 0, 0));

            using (spanBuilder = new SpanBuilder(traceContextScope, objectPool, traceReporter))
            {
                spanBuilder.SetBeginTimestamp(timestamp);
            }

            observedSpan.BeginTimestamp.Should().Be(timestamp);
        }

        [Test]
        public void Should_send_span_with_custom_endtimestamp_when_called()
        {
            var traceContextScope = CreateTraceContextScope();

            var timestamp = new DateTimeOffset(2018, 07, 09, 09, 0, 0, new TimeSpan(5, 0, 0));

            using (spanBuilder = new SpanBuilder(traceContextScope, objectPool, traceReporter))
            {
                spanBuilder.SetEndTimestamp(timestamp);
            }

            observedSpan.EndTimestamp.Should().Be(timestamp);
        }
    }
}