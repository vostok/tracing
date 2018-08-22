using FluentAssertions;
using NUnit.Framework;

namespace Vostok.Tracing.Tests
{
    [TestFixture]
    public class Span_Tests
    {
        private Span span;

        [SetUp]
        public void SetUp()
        {
            span = new Span();
        }

        [Test]
        public void AddAnnotation_success()
        {
            span.AddAnnotation("key1", "value1", false);

            span.Annotations["key1"].Should().Be("value1");
        }

        [Test]
        public void AddAnnotation_overwrite_annotation_when_set_overwrite()
        {
            span.AddAnnotation("key1", "value1", false);

            span.AddAnnotation("key1", "valueX", true);

            span.Annotations["key1"].Should().Be("valueX");
        }

        [Test]
        public void AddAnnotation_not_overwrite_annotation_when_set_no_overwrite()
        {
            span.AddAnnotation("key1", "value1", false);

            span.AddAnnotation("key1", "valueX", false);

            span.Annotations["key1"].Should().Be("value1");
        }

        [Test]
        public void ClearAnnotations_success()
        {
            span.AddAnnotation("key1", "value1", false);
            span.AddAnnotation("key2", "value2", false);

            span.ClearAnnotations();

            span.Annotations.Should().BeEmpty();
        }
    }
}