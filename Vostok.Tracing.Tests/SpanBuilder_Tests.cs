using System;
using System.Collections.Generic;
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
        private readonly Guid traceId = Guid.NewGuid();
        private readonly Guid spanId = Guid.NewGuid();
        private readonly Guid parentSpanId = Guid.NewGuid();
        private ISpanSender spanSender;
        private UnboundedObjectPool<Span> objectPool;
        private TraceConfiguration traceConfiguration;

        private ISpan observedSpan;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            objectPool = new UnboundedObjectPool<Span>(() => new Span());
        }

        [SetUp]
        public void SetUp()
        {
            spanSender = Substitute.For<ISpanSender>();
            traceConfiguration = new TraceConfiguration()
            {
                SpanSender = spanSender
            };
            FlowingContext.Globals.Set<TraceContext>(null);
            FlowingContext.Globals.Set<SpanBuilder.FlowingContextStorageSpan>(null);

            observedSpan = null;

            spanSender
                .When(r => r.Send(Arg.Any<ISpan>()))
                .Do(info => observedSpan = info.Arg<Span>().Clone());
        }

        [Test]
        public void Should_send_span_by_default()
        {
            using (var spanBuilder = new SpanBuilder(CreateTraceContextScope(), objectPool, traceConfiguration))
            {
            }

            spanSender.Received(1).Send(Arg.Any<ISpan>());
        }

        [Test]
        public void Should_send_span_with_endtimestamp_when_span_is_not_endless()
        {
            using (var spanBuilder = new SpanBuilder(CreateTraceContextScope(), objectPool, traceConfiguration))
            {
            }

            observedSpan.EndTimestamp.Should().NotBeNull();
        }

        [Test]
        public void Should_send_span_without_endtimestamp_when_span_is_endless()
        {
            using (var spanBuilder = new SpanBuilder(CreateTraceContextScope(), objectPool, traceConfiguration))
            {
                spanBuilder.MakeEndless();
            }

            observedSpan.EndTimestamp.Should().BeNull();
        }

        [Test]
        public void Should_dispose_tracecontextscope()
        {
            var traceContextScope = CreateTraceContextScope();

            using (var spanBuilder = new SpanBuilder(traceContextScope, objectPool, traceConfiguration))
            {
            }

            var context = FlowingContext.Globals.Get<TraceContext>();
            context.Should().BeNull();
        }

        [Test]
        public void Should_dispose_tracecontextscope_when_tracereporter_failed()
        {
            spanSender.When(x => x.Send(Arg.Any<ISpan>())).Do(x => throw new Exception());

            var traceContextScope = CreateTraceContextScope();

            Assert.Throws<Exception>(
                () =>
                {
                    using (var spanBuilder = new SpanBuilder(traceContextScope, objectPool, traceConfiguration))
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

            using (var spanBuilder = new SpanBuilder(traceContextScope, objectPool, traceConfiguration))
            {
            }

            observedSpan.ParentSpanId.Should().Be(parentSpanId);
        }

        [Test]
        public void Should_send_span_with_traceid_and_spanid_from_currentscope()
        {
            var traceContextScope = CreateTraceContextScope(traceId, spanId);

            using (var spanBuilder = new SpanBuilder(traceContextScope, objectPool, traceConfiguration))
            {
            }

            observedSpan.TraceId.Should().Be(traceId);
            observedSpan.SpanId.Should().Be(spanId);
        }

        [Test]
        public void Should_send_span_with_endtimestamp_grater_then_begintimestamp()
        {
            var traceContextScope = CreateTraceContextScope();

            using (var spanBuilder = new SpanBuilder(traceContextScope, objectPool, traceConfiguration))
            {
            }

            observedSpan.EndTimestamp.Should().BeAfter(observedSpan.BeginTimestamp);
        }

        [Test]
        public void Should_send_span_with_empty_annotations_when_no_annotation_added()
        {
            var traceContextScope = CreateTraceContextScope();

            using (var spanBuilder = new SpanBuilder(traceContextScope, objectPool, traceConfiguration))
            {
            }

            observedSpan.Annotations.Should().HaveCount(1);
            observedSpan.Annotations.ContainsKey(WellKnownAnnotations.Host).Should().BeTrue();
        }

        [Test]
        public void Should_send_span_with_added_annotation_after_adding()
        {
            var traceContextScope = CreateTraceContextScope();

            const string customAnnotationKey = "key";
            const string customAnnotationValue = "value";

            using (var spanBuilder = new SpanBuilder(traceContextScope, objectPool, traceConfiguration))
            {
                spanBuilder.SetAnnotation(customAnnotationKey, customAnnotationValue);
            }

            observedSpan.Annotations.ContainsKey(customAnnotationKey).Should().BeTrue();
            observedSpan.Annotations[customAnnotationKey].Should().Be(customAnnotationValue);
        }

        [Test]
        public void Should_send_span_with_custom_begintimestamp_when_called()
        {
            var traceContextScope = CreateTraceContextScope();

            var timestamp = new DateTimeOffset(2018, 07, 09, 09, 0, 0, new TimeSpan(5, 0, 0));

            using (var spanBuilder = new SpanBuilder(traceContextScope, objectPool, traceConfiguration))
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

            using (var spanBuilder = new SpanBuilder(traceContextScope, objectPool, traceConfiguration))
            {
                spanBuilder.SetEndTimestamp(timestamp);
            }

            observedSpan.EndTimestamp.Should().Be(timestamp);
        }

        [Test]
        public void Should_inherit_annotation_from_parent_span_when_whitelist_contain_key()
        {
            SetTraceContextInheritedFieldsWhitelist("name1");

            using (var spanBuilder = new SpanBuilder(CreateTraceContextScope(traceId), objectPool, traceConfiguration))
            {
                spanBuilder.SetAnnotation("name1", "value1");
                using (var sp2 = new SpanBuilder(CreateTraceContextScope(traceId), objectPool, traceConfiguration))
                {
                }

                observedSpan.Annotations.ContainsKey("name1").Should().BeTrue();
                observedSpan.Annotations["name1"].Should().Be("value1");
            }
        }

        [Test]
        public void Should_not_inherit_annotation_from_parent_span_when_whitelist_is_empty()
        {
            SetTraceContextInheritedFieldsWhitelist();

            using (var spanBuilder = new SpanBuilder(CreateTraceContextScope(traceId), objectPool, traceConfiguration))
            {
                spanBuilder.SetAnnotation("name1", "value1");
                using (var sp2 = new SpanBuilder(CreateTraceContextScope(traceId), objectPool, traceConfiguration))
                {
                }

                observedSpan.Annotations.ContainsKey("name1").Should().BeFalse();
            }
        }

        [Test]
        public void Should_not_inherit_annotation_from_parent_span_when_key_not_in_spanbuilder()
        {
            SetTraceContextInheritedFieldsWhitelist("nameX");

            using (var spanBuilder = new SpanBuilder(CreateTraceContextScope(traceId), objectPool, traceConfiguration))
            {
                spanBuilder.SetAnnotation("name1", "value1");
                using (var sp2 = new SpanBuilder(CreateTraceContextScope(traceId), objectPool, traceConfiguration))
                {
                }

                observedSpan.Annotations.ContainsKey("nameX").Should().BeFalse();
            }
        }

        [Test]
        public void Should_inherit_annotation_through_three_levels()
        {
            SetTraceContextInheritedFieldsWhitelist("name1");

            using (var parentSpan = new SpanBuilder(CreateTraceContextScope(traceId), objectPool, traceConfiguration))
            {
                parentSpan.SetAnnotation("name1", "value1");
                using (var level1Span = new SpanBuilder(CreateTraceContextScope(traceId), objectPool, traceConfiguration))
                {
                    using (var level2Span = new SpanBuilder(CreateTraceContextScope(traceId), objectPool, traceConfiguration))
                    {
                        using (var level3Span = new SpanBuilder(CreateTraceContextScope(traceId), objectPool, traceConfiguration))
                        {
                        }

                        observedSpan.Annotations.ContainsKey("name1").Should().BeTrue();
                        observedSpan.Annotations["name1"].Should().Be("value1");
                    }

                    observedSpan.Annotations.ContainsKey("name1").Should().BeTrue();
                    observedSpan.Annotations["name1"].Should().Be("value1");
                }

                observedSpan.Annotations.ContainsKey("name1").Should().BeTrue();
                observedSpan.Annotations["name1"].Should().Be("value1");
            }
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

        private void SetTraceContextInheritedFieldsWhitelist(params string[] inheritedFieldsWhitelist)
        {
            traceConfiguration = new TraceConfiguration()
            {
                SpanSender = spanSender,
                InheritedFieldsWhitelist = new HashSet<string>(inheritedFieldsWhitelist)
            };
        }
    }
}