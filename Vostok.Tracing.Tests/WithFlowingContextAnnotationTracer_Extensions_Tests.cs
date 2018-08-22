using System.Collections.Concurrent;
using NSubstitute;
using NUnit.Framework;
using Vostok.Context;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing.Tests
{
    public class WithFlowingContextAnnotationTracer_Extensions_Tests
    {
        private ITracer baseTracer;
        private ITracer enrichTracer;
        private ISpanBuilder spanBuilder;

        [SetUp]
        public void TestSetup()
        {
            baseTracer = Substitute.For<ITracer>();
            spanBuilder = Substitute.For<ISpanBuilder>();

            baseTracer.BeginSpan().Returns(spanBuilder);

            FlowingContext.Properties.Clear();
        }

        [Test]
        public void WithFlowingContextPropertyAnnotation_should_return_a_tracer_that_setted_annotation_when_key_exist_in_context()
        {
            FlowingContext.Properties.Set("name1", "value1");
            enrichTracer = baseTracer.WithFlowingContextPropertyAnnotation("name1");

            enrichTracer.BeginSpan();

            spanBuilder.Received().SetAnnotation("name1", "value1", false);
        }

        [Test]
        public void WithFlowingContextPropertyAnnotation_should_return_a_tracer_that_not_setted_annotation_when_key_not_exist_in_context()
        {
            enrichTracer = baseTracer.WithFlowingContextPropertyAnnotation("name1");

            enrichTracer.BeginSpan();

            spanBuilder.DidNotReceive().SetAnnotation("name1", Arg.Any<string>(), false);
        }

        [Test]
        public void WithFlowingContextPropertyAnnotation_should_return_a_tracer_that_not_setted_annotation_when_key_not_ask()
        {
            FlowingContext.Properties.Set("name2", "value2");
            enrichTracer = baseTracer.WithFlowingContextPropertyAnnotation("name1");

            enrichTracer.BeginSpan();

            spanBuilder.DidNotReceive().SetAnnotation("name2", "value2", false);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void WithFlowingContextPropertyAnnotation_should_return_a_tracer_that_pass_allowOverwrite_to_spanbuilder(bool allowOverwrite)
        {
            FlowingContext.Properties.Set("name1", "value1");
            enrichTracer = baseTracer.WithFlowingContextPropertyAnnotation("name1", allowOverwrite);

            enrichTracer.BeginSpan();

            spanBuilder.Received().SetAnnotation("name1", "value1", allowOverwrite);
        }

        [Test]
        public void WithFlowingContextPropertyAnnotations_should_return_a_tracer_that_setted_annotations_when_keys_exist_in_context()
        {
            FlowingContext.Properties.Set("name1", "value1");
            FlowingContext.Properties.Set("name2", "value2");
            enrichTracer = baseTracer.WithFlowingContextPropertyAnnotations(new[] {"name1", "name2"});

            enrichTracer.BeginSpan();

            spanBuilder.Received().SetAnnotation("name1", "value1", false);
            spanBuilder.Received().SetAnnotation("name2", "value2", false);
        }

        [Test]
        public void WithFlowingContextPropertyAnnotations_should_return_a_tracer_that_setted_annotation_when_not_all_keys_exist_in_context()
        {
            FlowingContext.Properties.Set("name1", "value1");
            enrichTracer = baseTracer.WithFlowingContextPropertyAnnotations(new[] { "name1", "name2" });

            enrichTracer.BeginSpan();

            spanBuilder.Received().SetAnnotation("name1", "value1", false);
            spanBuilder.DidNotReceive().SetAnnotation("name2", Arg.Any<string>(), false);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void WithFlowingContextPropertyAnnotations_should_return_a_tracer_that_pass_allowOverwrite_to_spanbuilder(bool allowOverwrite)
        {
            FlowingContext.Properties.Set("name1", "value1");
            FlowingContext.Properties.Set("name2", "value2");
            enrichTracer = baseTracer.WithFlowingContextPropertyAnnotations(new[] { "name1", "name2" }, allowOverwrite);

            enrichTracer.BeginSpan();

            spanBuilder.Received().SetAnnotation("name1", "value1", allowOverwrite);
            spanBuilder.Received().SetAnnotation("name2", "value2", allowOverwrite);
        }
    }
}