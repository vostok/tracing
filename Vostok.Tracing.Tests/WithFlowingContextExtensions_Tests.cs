using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Context;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing.Tests
{
    public class WithFlowingContextExtensions_Tests
    {
        private ITracer baseTracer;
        private ITracer enrichedTracer;
        private ISpanBuilder spanBuilder;

        private string globalName;
        private string propertyName1;
        private string propertyName2;
        private string propertyName3;

        [SetUp]
        public void TestSetup()
        {
            baseTracer = Substitute.For<ITracer>();
            spanBuilder = Substitute.For<ISpanBuilder>();

            baseTracer.BeginSpan().Returns(spanBuilder);

            FlowingContext.Properties.Clear();

            globalName = Guid.NewGuid().ToString();

            propertyName1 = Guid.NewGuid().ToString();
            propertyName2 = Guid.NewGuid().ToString();
            propertyName3 = Guid.NewGuid().ToString();
        }

        [Test]
        public void WithFlowingContextGlobal_should_return_a_tracer_that_adds_global_value_of_given_type_to_spans()
        {
            FlowingContext.Globals.Set("123");

            enrichedTracer = baseTracer.WithFlowingContextGlobal<string>(globalName);

            enrichedTracer.BeginSpan();

            spanBuilder.Received().SetAnnotation(globalName, "123", Arg.Any<bool>());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WithFlowingContextGlobal_should_pass_ovewrite_flag_to_span_builder(bool allowOverwrite)
        {
            FlowingContext.Globals.Set("123");

            enrichedTracer = baseTracer.WithFlowingContextGlobal<string>(globalName, allowOverwrite);

            enrichedTracer.BeginSpan();

            spanBuilder.Received().SetAnnotation(Arg.Any<string>(), Arg.Any<string>(), allowOverwrite);
        }

        [Test]
        public void WithFlowingContextGlobal_should_not_add_null_values_by_default()
        {
            FlowingContext.Globals.Set(null as string);

            enrichedTracer = baseTracer.WithFlowingContextGlobal<string>(globalName);

            enrichedTracer.BeginSpan();

            spanBuilder.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void WithFlowingContextGlobal_should_add_null_values_when_explicitly_asked_to()
        {
            FlowingContext.Globals.Set(null as string);

            enrichedTracer = baseTracer.WithFlowingContextGlobal<string>(globalName, allowNullValues: true);

            enrichedTracer.BeginSpan();

            spanBuilder.Received(1).SetAnnotation(globalName, null, Arg.Any<bool>());
        }

        [Test]
        public void WithFlowingContextProperty_should_return_a_tracer_that_adds_given_property_from_context_to_spans()
        {
            FlowingContext.Properties.Set(propertyName1, "value");

            enrichedTracer = baseTracer.WithFlowingContextProperty(propertyName1);

            enrichedTracer.BeginSpan();

            spanBuilder.Received(1).SetAnnotation(propertyName1, "value", Arg.Any<bool>());
        }

        [Test]
        public void WithFlowingContextProperty_should_respect_annotation_name_parameter()
        {
            FlowingContext.Properties.Set(propertyName1, "value");

            enrichedTracer = baseTracer.WithFlowingContextProperty(propertyName1, propertyName2);

            enrichedTracer.BeginSpan();

            spanBuilder.Received(1).SetAnnotation(propertyName2, "value", Arg.Any<bool>());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WithFlowingContextProperty_should_pass_overwrite_flag_to_span_builder(bool allowOverwrite)
        {
            FlowingContext.Properties.Set(propertyName1, "value");

            enrichedTracer = baseTracer.WithFlowingContextProperty(propertyName1, allowOverwrite: allowOverwrite);

            enrichedTracer.BeginSpan();

            spanBuilder.Received(1).SetAnnotation(Arg.Any<string>(), Arg.Any<string>(), allowOverwrite);
        }

        [Test]
        public void WithFlowingContextProperty_should_not_allow_null_values_by_default()
        {
            FlowingContext.Properties.Set(propertyName1, null);

            enrichedTracer = baseTracer.WithFlowingContextProperty(propertyName1);

            enrichedTracer.BeginSpan();

            spanBuilder.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void WithFlowingContextProperty_should_allow_null_values_when_asked_to()
        {
            FlowingContext.Properties.Set(propertyName1, null);

            enrichedTracer = baseTracer.WithFlowingContextProperty(propertyName1, allowNullValues: true);

            enrichedTracer.BeginSpan();

            spanBuilder.Received(1).SetAnnotation(propertyName1, null, Arg.Any<bool>());
        }

        [Test]
        public void WithFlowingContextProperties_should_return_a_tracer_that_adds_given_context_properties_to_spans()
        {
            FlowingContext.Properties.Set(propertyName1, "value1");
            FlowingContext.Properties.Set(propertyName2, "value2");

            enrichedTracer = baseTracer.WithFlowingContextProperties(new[] { propertyName1, propertyName2, propertyName3 });

            enrichedTracer.BeginSpan();

            spanBuilder.ReceivedCalls().Should().HaveCount(2);
            spanBuilder.Received(1).SetAnnotation(propertyName1, "value1", Arg.Any<bool>());
            spanBuilder.Received(1).SetAnnotation(propertyName2, "value2", Arg.Any<bool>());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WithFlowingContextProperties_should_pass_overwrite_flag_to_span_builder(bool allowOverwrite)
        {
            FlowingContext.Properties.Set(propertyName1, "value1");
            FlowingContext.Properties.Set(propertyName2, "value2");

            enrichedTracer = baseTracer.WithFlowingContextProperties(new[] { propertyName1, propertyName2, propertyName3 }, allowOverwrite);

            enrichedTracer.BeginSpan();

            spanBuilder.ReceivedCalls().Should().HaveCount(2);
            spanBuilder.Received(2).SetAnnotation(Arg.Any<string>(), Arg.Any<string>(), allowOverwrite);
        }

        [Test]
        public void WithFlowingContextProperties_should_not_allow_null_values_by_default()
        {
            FlowingContext.Properties.Set(propertyName1, null);
            FlowingContext.Properties.Set(propertyName2, null);

            enrichedTracer = baseTracer.WithFlowingContextProperties(new[] { propertyName1, propertyName2, propertyName3 });

            enrichedTracer.BeginSpan();

            spanBuilder.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void WithFlowingContextProperties_should_allow_null_values_when_asked_to()
        {
            FlowingContext.Properties.Set(propertyName1, null);
            FlowingContext.Properties.Set(propertyName2, null);

            enrichedTracer = baseTracer.WithFlowingContextProperties(new[] { propertyName1, propertyName2, propertyName3 }, allowNullValues: true);

            enrichedTracer.BeginSpan();

            spanBuilder.ReceivedCalls().Should().HaveCount(2);
            spanBuilder.Received(1).SetAnnotation(propertyName1, null, Arg.Any<bool>());
            spanBuilder.Received(1).SetAnnotation(propertyName2, null, Arg.Any<bool>());
        }

        [Test]
        public void WithAllFlowingContextProperties_should_return_a_tracer_that_adds_all_context_properties_to_spans()
        {
            FlowingContext.Properties.Set(propertyName1, "value1");
            FlowingContext.Properties.Set(propertyName2, "value2");
            FlowingContext.Properties.Set(propertyName3, "value3");

            enrichedTracer = baseTracer.WithAllFlowingContextProperties();

            enrichedTracer.BeginSpan();

            spanBuilder.ReceivedCalls().Should().HaveCount(3);
            spanBuilder.Received(1).SetAnnotation(propertyName1, "value1", Arg.Any<bool>());
            spanBuilder.Received(1).SetAnnotation(propertyName2, "value2", Arg.Any<bool>());
            spanBuilder.Received(1).SetAnnotation(propertyName3, "value3", Arg.Any<bool>());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WithAllFlowingContextProperties_should_pass_overwrite_flag_to_span_builder(bool allowOverwrite)
        {
            FlowingContext.Properties.Set(propertyName1, "value1");
            FlowingContext.Properties.Set(propertyName2, "value2");
            FlowingContext.Properties.Set(propertyName3, "value3");

            enrichedTracer = baseTracer.WithAllFlowingContextProperties(allowOverwrite);

            enrichedTracer.BeginSpan();

            spanBuilder.Received(3).SetAnnotation(Arg.Any<string>(), Arg.Any<string>(), allowOverwrite);
        }

        [Test]
        public void WithAllFlowingContextProperties_should_not_allow_null_values_by_default()
        {
            FlowingContext.Properties.Set(propertyName1, null);
            FlowingContext.Properties.Set(propertyName2, null);
            FlowingContext.Properties.Set(propertyName3, null);

            enrichedTracer = baseTracer.WithAllFlowingContextProperties();

            enrichedTracer.BeginSpan();

            spanBuilder.ReceivedCalls().Should().BeEmpty();
        }

        [Test]
        public void WithAllFlowingContextProperties_should_allow_null_values_when_asked_to()
        {
            FlowingContext.Properties.Set(propertyName1, null);
            FlowingContext.Properties.Set(propertyName2, null);
            FlowingContext.Properties.Set(propertyName3, null);

            enrichedTracer = baseTracer.WithAllFlowingContextProperties(allowNullValues: true);

            enrichedTracer.BeginSpan();

            spanBuilder.ReceivedCalls().Should().HaveCount(3);
            spanBuilder.Received(1).SetAnnotation(propertyName1, null, Arg.Any<bool>());
            spanBuilder.Received(1).SetAnnotation(propertyName2, null, Arg.Any<bool>());
            spanBuilder.Received(1).SetAnnotation(propertyName3, null, Arg.Any<bool>());
        }
    }
}
