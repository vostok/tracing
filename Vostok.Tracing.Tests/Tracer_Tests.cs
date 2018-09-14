// using System;
// using System.Net.Http;
// using FluentAssertions;
// using NSubstitute;
// using NUnit.Framework;
// using Vostok.Tracing.Abstractions;
//
// namespace Vostok.Tracing.Tests
// {
//     public class Tracer_Tests
//     {
//         private ISpanSender spanSender;
//         private ITracer tracer;
//         private TraceConfiguration traceConfiguration;
//
//         private ISpan observedSpan;
//
//         [SetUp]
//         public void SetUp()
//         {
//             spanSender = Substitute.For<ISpanSender>();
//             traceConfiguration = new TraceConfiguration();
//             traceConfiguration.SpanSender = spanSender;
//             tracer = new Tracer(traceConfiguration);
//
//             observedSpan = null;
//
//             spanSender
//                 .When(r => r.Send(Arg.Any<ISpan>()))
//                 .Do(info => observedSpan = info.Arg<Span>().Clone());
//         }
//
//         [Test]
//         public void CurrentContext_should_change_context_when_begin_child_span()
//         {
//             using (var span1 = tracer.BeginSpan())
//             {
//                 var span1Context = tracer.CurrentContext;
//                 using (var span2 = tracer.BeginSpan())
//                 {
//                     var span2Context = tracer.CurrentContext;
//
//                     span1Context.TraceId.Should().Be(span2Context.TraceId);
//                     span1Context.SpanId.Should().NotBe(span2Context.SpanId);
//                 }
//             }
//         }
//
//         [Test]
//         public void CurrentContext_should_change_context_back_when_end_child_span()
//         {
//             using (var span1 = tracer.BeginSpan())
//             {
//                 var span1ContextBeforeStartChildSpan = tracer.CurrentContext;
//
//                 using (var span2 = tracer.BeginSpan())
//                 {
//                     var span2Context = tracer.CurrentContext;
//
//                     span1ContextBeforeStartChildSpan.TraceId.Should().Be(span2Context.TraceId);
//                     span1ContextBeforeStartChildSpan.SpanId.Should().NotBe(span2Context.SpanId);
//                 }
//
//                 var span1ContextAfterStartChildSpan = tracer.CurrentContext;
//
//                 span1ContextBeforeStartChildSpan.TraceId.Should().Be(span1ContextAfterStartChildSpan.TraceId);
//                 span1ContextBeforeStartChildSpan.SpanId.Should().Be(span1ContextAfterStartChildSpan.SpanId);
//             }
//         }
//
//         [Test]
//         public void BeginSpan_enrich_span_only_with_default_annotations_when_no_span_configuration()
//         {
//             using (var span1 = tracer.BeginSpan())
//             {
//             }
//
//             observedSpan.Annotations.Should().HaveCount(1);
//             observedSpan.Annotations.ContainsKey(WellKnownAnnotations.Common.Host).Should().BeTrue();
//         }
//     }
// }