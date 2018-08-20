using System;
using System.Net.Http;
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
        private TraceConfiguration traceConfiguration;

        private ISpan observedSpan;

        [SetUp]
        public void SetUp()
        {
            traceReporter = Substitute.For<ITraceReporter>();
            traceConfiguration = new TraceConfiguration();
            traceConfiguration.TraceReporter = traceReporter;
            tracer = new Tracer(traceConfiguration);

            observedSpan = null;

            traceReporter
                .When(r => r.SendSpan(Arg.Any<ISpan>()))
                .Do(info => observedSpan = (ISpan)info.Arg<Span>().Clone());
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

        [Test]
        public void BeginSpan_enrich_span_when_has_configuration()
        {
            traceConfiguration.EnrichSpanAction = builder =>
            {
                builder.SetAnnotation("test", "val");
            };

            using (var span1 = tracer.BeginSpan())
            {
            }

            observedSpan.Annotations["test"].Should().Be("val");
        }

        [Test]
        public void BeginSpan_enrich_span_for_each_spans_when_has_configuration()
        {
            const string customAnnotationKey = "key";
            const string customAnnotationValue = "value";

            traceReporter.SendSpan(
                Arg.Do<Span>(
                    x =>
                    {
                        x.Annotations.Keys.Should().Contain(customAnnotationKey);
                        x.Annotations[customAnnotationKey].Should().Contain(customAnnotationValue);
                    }));

            traceConfiguration.EnrichSpanAction = builder =>
            {
                builder.SetAnnotation(customAnnotationKey, customAnnotationValue);
            };

            using (var span1 = tracer.BeginSpan())
            {
                using (var span2 = tracer.BeginSpan())
                {
                }
            }
        }

        [Test]
        public void BeginSpan_no_enrich_span_when_no_configuration()
        {
            using (var span1 = tracer.BeginSpan())
            {
            }

            observedSpan.Annotations.Should().BeEmpty();
        }

        [Test]
        public void Test_complex_events()
        {
            traceConfiguration.EnrichSpanAction = builder =>
            {
                builder.SetAnnotation(AnnotationNames.Service, "my cool service");
                builder.SetAnnotation(AnnotationNames.Component, typeof(Tracer_Tests));
                builder.SetAnnotation(AnnotationNames.Host, Environment.MachineName);
            };

            const string url = "https://git.skbkontur.ru/ke/core-infra/blob/master/Kontur.Core.Infra/Utilities/UrlNormalizer.cs";
            using (var span1 = tracer.BeginHttpRequestClientSpan(new Uri(url), HttpMethod.Get, 312323))
            {
            }

            observedSpan.Annotations.Should().HaveCountGreaterThan(0);
        }
    }
}