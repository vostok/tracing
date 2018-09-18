using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Tracing.Abstractions;
using Vostok.Tracing.Helpers;

namespace Vostok.Tracing.Tests.Helpers
 {
     [TestFixture]
     public class TraceContextSerializer_Tests
     {
         private TraceContext context;
         private TraceContextSerializer serializer;

         [SetUp]
         public void SetUp()
         {
             context = new TraceContext(Guid.NewGuid(), Guid.NewGuid());
             serializer = new TraceContextSerializer();
         }

         [Test]
         public void Should_successfully_deserialize_previously_serialized_context()
         {
             serializer.Deserialize(serializer.Serialize(context)).Should().BeEquivalentTo(context);
         }

         [Test]
         public void Serialization_should_use_semicolon_delimiter()
         {
             serializer.Serialize(context).Should().Be($"{context.TraceId};{context.SpanId}");
         }

         [Test]
         public void Deserialization_should_be_case_insensitive()
         {
             serializer.Deserialize(serializer.Serialize(context).ToLower()).Should().BeEquivalentTo(context);
             serializer.Deserialize(serializer.Serialize(context).ToUpper()).Should().BeEquivalentTo(context);
         }

         [TestCase("")]
         [TestCase("21EC3C39-388B-4014-BFB2-59E4074ECD06")]
         [TestCase("21EC3C39-388B-4014-BFB2-59E4074ECD06+604EB5DA-ED37-4E0F-8C53-5B984F3BE60E")]
         public void Should_fail_when_deserializing_from_incorrect_input(string input)
         {
             new Action(() => serializer.Deserialize(input)).Should().Throw<FormatException>().Which.ShouldBePrinted();
         }
     }
 }