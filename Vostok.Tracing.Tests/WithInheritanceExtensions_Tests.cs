using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing.Tests
{
    [TestFixture]
    internal class WithInheritanceExtensions_Tests
    {
        private TracerSettings settings;
        private Tracer baseTracer;
        private ITracer enrichedTracer;

        private string annotation1;
        private string annotation2;
        private string annotation3;

        [SetUp]
        public void TestSetup()
        {
            settings = new TracerSettings(Substitute.For<ISpanSender>());

            baseTracer = new Tracer(settings) { CurrentContext = null };

            annotation1 = Guid.NewGuid().ToString();
            annotation2 = Guid.NewGuid().ToString();
            annotation3 = Guid.NewGuid().ToString();
        }

        [Test]
        public void WithAnnotationsInheritance_should_return_a_tracer_that_inherits_given_annotations_to_child_spans()
        {
            enrichedTracer = baseTracer.WithAnnotationsInheritance(new[] { annotation1, annotation2 });

            using (var rootBuilder = enrichedTracer.BeginSpan())
            {
                rootBuilder.SetAnnotation(annotation1, "value1");
                rootBuilder.SetAnnotation(annotation2, "value2");

                using (var childBuilder = enrichedTracer.BeginSpan())
                {
                    childBuilder.CurrentSpan.Annotations[annotation1].Should().Be("value1");
                    childBuilder.CurrentSpan.Annotations[annotation2].Should().Be("value2");
                }
            }
        }

        [Test]
        public void WithAnnotationsInheritance_should_not_inherit_anything_when_there_is_no_parent_span()
        {
            enrichedTracer = baseTracer.WithAnnotationsInheritance(new[] { annotation1, annotation2 });

            using (var rootBuilder = enrichedTracer.BeginSpan())
            {
                rootBuilder.CurrentSpan.Annotations.ContainsKey(annotation1).Should().BeFalse();
                rootBuilder.CurrentSpan.Annotations.ContainsKey(annotation2).Should().BeFalse();
            }
        }

        [Test]
        public void WithAnnotationsInheritance_should_not_inherit_annotations_not_present_in_parent_span()
        {
            enrichedTracer = baseTracer.WithAnnotationsInheritance(new[] { annotation1, annotation2, annotation3 });

            using (var rootBuilder = enrichedTracer.BeginSpan())
            {
                rootBuilder.SetAnnotation(annotation2, "value2");

                using (var childBuilder = enrichedTracer.BeginSpan())
                {
                    childBuilder.CurrentSpan.Annotations.ContainsKey(annotation1).Should().BeFalse();
                    childBuilder.CurrentSpan.Annotations.ContainsKey(annotation3).Should().BeFalse();
                }
            }
        }

        [Test]
        public void WithAnnotationsInheritance_should_not_inherit_annotations_outside_from_given_list()
        {
            enrichedTracer = baseTracer.WithAnnotationsInheritance(new[] { annotation1, annotation2 });

            using (var rootBuilder = enrichedTracer.BeginSpan())
            {
                rootBuilder.SetAnnotation(annotation3, "value3");

                using (var childBuilder = enrichedTracer.BeginSpan())
                {
                    childBuilder.CurrentSpan.Annotations.ContainsKey(annotation3).Should().BeFalse();
                }
            }
        }

        [Test]
        public void WithAnnotationsInheritance_should_be_limited_by_a_depth_equal_to_one_by_default()
        {
            enrichedTracer = baseTracer.WithAnnotationsInheritance(new[] { annotation1, annotation2 });

            using (var rootBuilder = enrichedTracer.BeginSpan())
            {
                rootBuilder.SetAnnotation(annotation1, "value1");
                rootBuilder.SetAnnotation(annotation2, "value2");

                using (var childBuilder = enrichedTracer.BeginSpan())
                {
                    childBuilder.CurrentSpan.Annotations[annotation1].Should().Be("value1");
                    childBuilder.CurrentSpan.Annotations[annotation2].Should().Be("value2");

                    using (var deepChildBuilder = enrichedTracer.BeginSpan())
                    {
                        deepChildBuilder.CurrentSpan.Annotations.ContainsKey(annotation1).Should().BeFalse();
                        deepChildBuilder.CurrentSpan.Annotations.ContainsKey(annotation2).Should().BeFalse();
                    }
                }
            }
        }

        [Test]
        public void WithAnnotationsInheritance_should_be_able_to_inherit_to_arbitrary_depth_if_configured_so()
        {
            enrichedTracer = baseTracer.WithAnnotationsInheritance(new[] { annotation1, annotation2 }, maximumDepth: 2);

            using (var rootBuilder = enrichedTracer.BeginSpan())
            {
                rootBuilder.SetAnnotation(annotation1, "value1");
                rootBuilder.SetAnnotation(annotation2, "value2");

                using (var childBuilder = enrichedTracer.BeginSpan())
                {
                    childBuilder.CurrentSpan.Annotations[annotation1].Should().Be("value1");
                    childBuilder.CurrentSpan.Annotations[annotation2].Should().Be("value2");

                    using (var deepChildBuilder = enrichedTracer.BeginSpan())
                    {
                        deepChildBuilder.CurrentSpan.Annotations[annotation1].Should().Be("value1");
                        deepChildBuilder.CurrentSpan.Annotations[annotation2].Should().Be("value2");
                    }
                }
            }
        }

        [Test]
        public void WithAnnotationsInheritance_should_not_leak_trace_context_outside_of_using_blocks()
        {
            enrichedTracer = baseTracer.WithAnnotationsInheritance(new[] {annotation1, annotation2});

            using (enrichedTracer.BeginSpan())
            using (enrichedTracer.BeginSpan())
            using (enrichedTracer.BeginSpan())
            {

            }

            enrichedTracer.CurrentContext.Should().BeNull();
        }

        [Test]
        public void WithAnnotationsInheritance_should_not_leak_inheritance_context_outside_of_using_blocks()
        {
            enrichedTracer = baseTracer.WithAnnotationsInheritance(new[] { annotation1, annotation2 }, maximumDepth: int.MaxValue);

            using (var rootBuilder = enrichedTracer.BeginSpan())
            {
                rootBuilder.SetAnnotation(annotation1, "value1");
                rootBuilder.SetAnnotation(annotation2, "value2");

                using (enrichedTracer.BeginSpan())
                using (enrichedTracer.BeginSpan())
                {
                }
            }

            using (var rootBuilder = enrichedTracer.BeginSpan())
            {
                rootBuilder.CurrentSpan.Annotations.ContainsKey(annotation1).Should().BeFalse();
                rootBuilder.CurrentSpan.Annotations.ContainsKey(annotation2).Should().BeFalse();
            }
        }

        [Test]
        public void WithAnnotationsInheritance_should_not_overwrite_existing_values_by_default()
        {
            enrichedTracer = baseTracer
                .WithAnnotation(annotation2, "valueX")
                .WithAnnotationsInheritance(new[] {annotation1, annotation2, annotation3});

            using (var rootBuilder = enrichedTracer.BeginSpan())
            {
                rootBuilder.SetAnnotation(annotation2, "value2");

                using (var childBuilder = enrichedTracer.BeginSpan())
                {
                    childBuilder.CurrentSpan.Annotations[annotation2].Should().Be("valueX");
                }
            }
        }

        [Test]
        public void WithAnnotationsInheritance_should_overwrite_existing_values_when_asked_to()
        {
            enrichedTracer = baseTracer
                .WithAnnotation(annotation2, "valueX")
                .WithAnnotationsInheritance(new[] { annotation1, annotation2, annotation3 }, true);

            using (var rootBuilder = enrichedTracer.BeginSpan())
            {
                rootBuilder.SetAnnotation(annotation2, "value2");

                using (var childBuilder = enrichedTracer.BeginSpan())
                {
                    childBuilder.CurrentSpan.Annotations[annotation2].Should().Be("value2");
                }
            }
        }

        [Test]
        public void WithAnnotationsInheritance_should_not_inherit_null_values_by_default()
        {
            enrichedTracer = baseTracer.WithAnnotationsInheritance(new[] { annotation1, annotation2 });

            using (var rootBuilder = enrichedTracer.BeginSpan())
            {
                rootBuilder.SetAnnotation(annotation1, null);
                rootBuilder.SetAnnotation(annotation2, null);

                using (var childBuilder = enrichedTracer.BeginSpan())
                {
                    childBuilder.CurrentSpan.Annotations.ContainsKey(annotation1).Should().BeFalse();
                    childBuilder.CurrentSpan.Annotations.ContainsKey(annotation2).Should().BeFalse();
                }
            }
        }

        [Test]
        public void WithAnnotationsInheritance_should_inherit_null_values_when_asked_to()
        {
            enrichedTracer = baseTracer.WithAnnotationsInheritance(new[] { annotation1, annotation2 }, allowNullValues: true);

            using (var rootBuilder = enrichedTracer.BeginSpan())
            {
                rootBuilder.SetAnnotation(annotation1, null);
                rootBuilder.SetAnnotation(annotation2, null);

                using (var childBuilder = enrichedTracer.BeginSpan())
                {
                    childBuilder.CurrentSpan.Annotations[annotation1].Should().BeNull();
                    childBuilder.CurrentSpan.Annotations[annotation2].Should().BeNull();
                }
            }
        }
    }
}