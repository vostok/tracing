using System;
using System.Net;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Extensions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Configuration;

// ReSharper disable PossibleInvalidOperationException

namespace Vostok.Tracing.Tests
{
    public class SpanBuilder_Tests
    {
        private TracerSettings settings;
        private TraceContext parentContext;
        private TraceContext currentContext;
        private IDisposable contextScope;
        private ISpanSender sender;
        private ISpan observedSpan;

        private SpanBuilder builder;

        [SetUp]
        public void TestSetup()
        {
            observedSpan = null;

            sender = Substitute.For<ISpanSender>();
            sender.When(s => s.Send(Arg.Any<ISpan>())).Do(info => observedSpan = info.Arg<ISpan>());

            settings = new TracerSettings { Sender = sender };

            parentContext = new TraceContext(Guid.NewGuid(), Guid.NewGuid());
            currentContext = new TraceContext(parentContext.TraceId, Guid.NewGuid());
            contextScope = Substitute.For<IDisposable>();

            builder = new SpanBuilder(settings, contextScope, currentContext, parentContext);
        }

        [Test]
        public void Should_set_correct_traceId_for_span_upon_construction()
        {
            builder.CurrentSpan.TraceId.Should().Be(currentContext.TraceId);
        }

        [Test]
        public void Should_set_correct_spanId_for_span_upon_construction()
        {
            builder.CurrentSpan.SpanId.Should().Be(currentContext.SpanId);
        }

        [Test]
        public void Should_set_correct_parent_spanId_for_span_upon_construction_when_there_is_a_parent_context()
        {
            builder.CurrentSpan.ParentSpanId.Should().Be(parentContext.SpanId);
        }

        [Test]
        public void Should_set_correct_parent_spanId_for_span_upon_construction_when_there_is_no_parent_context()
        {
            builder = new SpanBuilder(settings, contextScope, currentContext, null);

            builder.CurrentSpan.ParentSpanId.Should().BeNull();
        }

        [Test]
        public void Should_set_correct_begin_timestamp_for_span_upon_construction()
        {
            builder.CurrentSpan.BeginTimestamp.Should().BeCloseTo(DateTimeOffset.Now, 1.Seconds());
            builder.CurrentSpan.BeginTimestamp.Offset.Should().Be(DateTimeOffset.Now.Offset);
        }

        [Test]
        public void Should_set_host_annotation_with_value_from_settings_for_span_upon_construction()
        {
            settings.Host = Guid.NewGuid().ToString();

            InitializeBuilder();

            builder.CurrentSpan.Annotations[WellKnownAnnotations.Common.Host].Should().Be(settings.Host);
        }

        [Test]
        public void Should_set_host_annotation_with_default_value_for_span_upon_construction_when_not_configured_in_settings()
        {
            builder.CurrentSpan.Annotations[WellKnownAnnotations.Common.Host].Should().Be(Dns.GetHostName());
        }

        [Test]
        public void Should_set_application_annotation_with_value_from_settings_for_span_upon_construction()
        {
            settings.Application = Guid.NewGuid().ToString();

            InitializeBuilder();

            builder.CurrentSpan.Annotations[WellKnownAnnotations.Common.Application].Should().Be(settings.Application);
        }

        [Test]
        public void Should_not_set_application_annotation_for_span_upon_construction_when_not_configured_in_settings()
        {
            builder.CurrentSpan.Annotations.ContainsKey(WellKnownAnnotations.Common.Application).Should().BeFalse();
        }

        [Test]
        public void Should_set_environment_annotation_with_value_from_settings_for_span_upon_construction()
        {
            settings.Environment = Guid.NewGuid().ToString();

            InitializeBuilder();

            builder.CurrentSpan.Annotations[WellKnownAnnotations.Common.Environment].Should().Be(settings.Environment);
        }

        [Test]
        public void Should_not_set_environment_annotation_for_span_upon_construction_when_not_configured_in_settings()
        {
            builder.CurrentSpan.Annotations.ContainsKey(WellKnownAnnotations.Common.Environment).Should().BeFalse();
        }

        [Test]
        public void Should_not_send_span_anywhere_upon_construction()
        {
            sender.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void SetAnnotation_should_be_able_to_add_new_annotations()
        {
            builder.SetAnnotation("k1", "v1");
            builder.SetAnnotation("k2", "v2");
            builder.SetAnnotation("k3", "v3");

            builder.CurrentSpan.Annotations.Keys.Should().Contain(new[] {"k1", "k2", "k3"});
        }

        [Test]
        public void SetAnnotation_should_overwrite_existing_values_by_default()
        {
            builder.SetAnnotation("k", "v1");
            builder.SetAnnotation("k", "v2");

            builder.CurrentSpan.Annotations["k"].Should().Be("v2");
        }

        [Test]
        public void SetAnnotation_should_not_overwrite_existing_values_when_explicitly_asked_not_to()
        {
            builder.SetAnnotation("k", "v1");
            builder.SetAnnotation("k", "v2", false);

            builder.CurrentSpan.Annotations["k"].Should().Be("v1");
        }

        [Test]
        public void Dispose_should_send_constructed_span()
        {
            builder.Dispose();

            sender.Received(1).Send(Arg.Any<ISpan>());
        }

        [Test]
        public void Dispose_should_dispose_trace_context_scope()
        {
            builder.Dispose();

            contextScope.Received(1).Dispose();
        }

        [Test]
        public void Dispose_should_dispose_trace_context_scope_even_when_sender_fails()
        {
            sender
                .When(s => s.Send(Arg.Any<ISpan>()))
                .Throw(new Exception("I fail."));

            try
            {
                builder.Dispose();
            }
            catch (Exception error)
            {
                Console.Out.WriteLine(error);
            }

            contextScope.Received(1).Dispose();
        }

        [Test]
        public void Dispose_should_provide_constructed_span_with_end_timestamp()
        {
            Thread.Sleep(1);
            
            builder.Dispose();

            observedSpan.Should().NotBeNull();
            observedSpan.EndTimestamp.Should().HaveValue().And.Subject.Value.Should().BeAfter(observedSpan.BeginTimestamp);
        }

        [Test]
        public void Dispose_should_send_a_span_equivalent_to_current_constructed_span_except_for_end_timestamp()
        {
            builder.SetAnnotation("k1", "v1");
            builder.SetAnnotation("k2", "v2");
            builder.SetAnnotation("k3", "v3");

            var currentSpan = builder.CurrentSpan;

            builder.Dispose();

            observedSpan.Should().BeEquivalentTo(currentSpan, options => options.Excluding(span => span.EndTimestamp));
        }

        [Test]
        public void SetBeginTimestamp_should_modify_constructed_span_begin_timestamp()
        {
            var timestamp = DateTimeOffset.UtcNow + 5.Hours();

            builder.SetBeginTimestamp(timestamp);

            builder.CurrentSpan.BeginTimestamp.Should().Be(timestamp);

            builder.Dispose();

            observedSpan.BeginTimestamp.Should().Be(timestamp);
        }

        [Test]
        public void SetEndTimestamp_should_modify_constructed_span_end_timestamp()
        {
            var timestamp = DateTimeOffset.UtcNow - 5.Hours();

            builder.SetEndTimestamp(timestamp);

            builder.CurrentSpan.EndTimestamp.Should().Be(timestamp);

            builder.Dispose();

            observedSpan.EndTimestamp.Should().Be(timestamp);
        }

        [Test]
        public void SetEndTimestamp_should_be_able_to_set_a_null_value_that_wont_be_overridden_during_dispose()
        {
            builder.SetEndTimestamp(null);

            builder.CurrentSpan.EndTimestamp.Should().BeNull();

            builder.Dispose();

            observedSpan.EndTimestamp.Should().BeNull();
        }

        [Test]
        public void CurrentSpan_property_should_return_an_immutable_value_unaffected_by_further_annotation_changes()
        {
            builder.SetAnnotation("k1", "v1");

            var snapshot = builder.CurrentSpan;

            builder.SetAnnotation("k2", "v2");

            snapshot.Annotations.ContainsKey("k2").Should().BeFalse();
        }

        [Test]
        public void CurrentSpan_property_should_return_an_immutable_value_unaffected_by_further_timestamp()
        {
            var snapshot = builder.CurrentSpan;

            var beginTimestamp = snapshot.BeginTimestamp;
            var endTimestamp = snapshot.EndTimestamp;

            builder.SetBeginTimestamp(beginTimestamp + 1.Hours());
            builder.SetEndTimestamp(endTimestamp + 1.Hours());

            snapshot.BeginTimestamp.Should().Be(beginTimestamp);
            snapshot.EndTimestamp.Should().Be(endTimestamp);
        }

        private void InitializeBuilder()
        {
            builder = new SpanBuilder(settings, contextScope, currentContext, parentContext);
        }
    }
}
