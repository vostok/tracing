// using System;
// using System.Diagnostics;
// using FluentAssertions;
// using NUnit.Framework;
// using Vostok.Tracing.Abstractions;
//
// namespace Vostok.Tracing.Tests
// {
//     [TestFixture]
//     public class TraceContextSerializer_Tests
//     {
//         private TraceContextSerializer serializer;
//         private Guid traceGuid;
//         private Guid spanGuid;
//
//         [SetUp]
//         public void SetUp()
//         {
//             serializer = new TraceContextSerializer();
//             traceGuid = Guid.NewGuid();
//             spanGuid = Guid.NewGuid();
//         }
//
//         [Test]
//         public void Serialize_success_serialize()
//         {
//             var traceContext = new TraceContext(traceGuid, spanGuid);
//
//             var serializedString = serializer.Serialize(traceContext);
//
//             serializedString.Should().Be($"{traceGuid};{spanGuid}");
//         }
//
//         [Test]
//         public void Deserialize_success_deserialize_when_inputdata_is_correct()
//         {
//             var serializedString = $"{traceGuid};{spanGuid}";
//             
//             var traceContext = serializer.Deserialize(serializedString);
//
//             traceContext.TraceId.Should().Be(traceGuid);
//             traceContext.SpanId.Should().Be(spanGuid);
//         }
//
//         [Test]
//         public void Deserialize_get_exception_when_inputdata_is_incorrect()
//         {
//             var serializedString = $"{traceGuid};{spanGuid};{Guid.NewGuid()}";
//
//             Assert.Throws<ArgumentException>(() => serializer.Deserialize(serializedString));
//         }
//     }
// }