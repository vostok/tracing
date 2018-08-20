﻿using System;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing.Tests
{
    public class SpanBuilder_Tests
    {
        private ISpanBuilder spanBuilder;
        private ITraceReporter traceReporter;

        private ISpan observedSpan;

        private readonly Guid traceId = Guid.NewGuid();
        private readonly Guid spanId = Guid.NewGuid();
        private readonly Guid parentSpanId = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            traceReporter = Substitute.For<ITraceReporter>();

            observedSpan = null;

            traceReporter
                .When(r => r.SendSpan(Arg.Any<ISpan>()))
                .Do(info => observedSpan = info.Arg<ISpan>());
        }

        [Test]
        public void Should_send_span()
        {
            using (spanBuilder = new SpanBuilder(CreateTraceContextScope(), traceReporter))
            {
            }
            traceReporter.Received(1).SendSpan(Arg.Any<ISpan>());
        }

        [Test]
        public void Should_send_span_with_endtimestamp_when_span_is_not_endless()
        {
            using (spanBuilder = new SpanBuilder(CreateTraceContextScope(), traceReporter))
            {
            }

            observedSpan.EndTimestamp.Should().NotBeNull();
        }

        [Test]
        public void Should_send_span_without_endtimestamp_when_span_is_endless()
        {
            using (spanBuilder = new SpanBuilder(CreateTraceContextScope(), traceReporter))
            {
                spanBuilder.IsEndless = true;
            }

            observedSpan.EndTimestamp.Should().BeNull();
        }

        [Test]
        public void Should_dispose_tracecontextscope()
        {
            var traceContextScope = CreateTraceContextScope();

            using (spanBuilder = new SpanBuilder(traceContextScope, traceReporter))
            {
            }

            //CR(n.kochnev): using disposed field is best idea from all
            traceContextScope.Disposed.Should().BeTrue();
        }

        [Test]
        public void Should_dispose_tracecontextscope_when_tracereporter_failed()
        {
            traceReporter.When(x=>x.SendSpan(Arg.Any<ISpan>())).Do(x => throw new Exception());

            var traceContextScope = CreateTraceContextScope();

            Assert.Throws<Exception>(
                () =>
                {
                    using (spanBuilder = new SpanBuilder(traceContextScope, traceReporter))
                    {
                    }
                });
            //CR(n.kochnev): using disposed field is best idea from all
            traceContextScope.Disposed.Should().BeTrue();
        }

        [Test]
        public void Should_send_span_with_parentspanid_when_parentcontextscope_exist()
        {
            var traceContextScope = CreateTraceContextScope(parentSpanId: parentSpanId);

            using (spanBuilder = new SpanBuilder(traceContextScope, traceReporter))
            {
            }

            observedSpan.ParentSpanId.Should().Be(parentSpanId);
        }

        [Test]
        public void Should_send_span_with_traceid_and_spanid_from_currentscope()
        {
            var traceContextScope = CreateTraceContextScope(traceId, spanId);

            using (spanBuilder = new SpanBuilder(traceContextScope, traceReporter))
            {
            }

            observedSpan.TraceId.Should().Be(traceId);
            observedSpan.SpanId.Should().Be(spanId);
        }

        [Test]
        public void Should_send_span_with_endtimestamp()
        {
            var traceContextScope = CreateTraceContextScope();

            using (spanBuilder = new SpanBuilder(traceContextScope, traceReporter))
            {
            }

            observedSpan.BeginTimestamp.Should().BeMoreThan(TimeSpan.Zero);
            observedSpan.EndTimestamp.Should().BeAfter(observedSpan.BeginTimestamp);
        }

        [Test]
        public void Should_send_span_with_empty_annotations_when_no_annotation_added()
        {
            var traceContextScope = CreateTraceContextScope();

            using (spanBuilder = new SpanBuilder(traceContextScope, traceReporter))
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

            using (spanBuilder = new SpanBuilder(traceContextScope, traceReporter))
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

            using (spanBuilder = new SpanBuilder(traceContextScope, traceReporter))
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

            using (spanBuilder = new SpanBuilder(traceContextScope, traceReporter))
            {
                spanBuilder.SetEndTimestamp(timestamp);
            }

            observedSpan.EndTimestamp.Should().Be(timestamp);
        }

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
    }
}