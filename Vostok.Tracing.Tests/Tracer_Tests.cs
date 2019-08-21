using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Context;
using Vostok.Tracing.Abstractions;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable ObjectCreationAsStatement

namespace Vostok.Tracing.Tests
{
    public class Tracer_Tests
    {
        private TracerSettings settings;
        private Tracer tracer;

        [SetUp]
        public void SetUp()
        {
            settings = new TracerSettings(new DevNullSpanSender());
            tracer = new Tracer(settings);

            FlowingContext.Globals.Set(null as TraceContext);
        }

        [Test]
        public void Should_register_trace_context_as_a_distributed_global_in_flowing_context_configuration()
        {
            string serialized;

            var context = new TraceContext(Guid.NewGuid(), Guid.NewGuid());

            using (FlowingContext.Globals.Use(context))
            {
                serialized = FlowingContext.SerializeDistributedGlobals();
            }

            FlowingContext.RestoreDistributedGlobals(serialized);

            FlowingContext.Globals.Get<TraceContext>().Should().BeEquivalentTo(context);
        }

        [Test]
        public void CurrentContext_property_getter_should_return_a_global_from_flowing_context()
        {
            var context = new TraceContext(Guid.NewGuid(), Guid.NewGuid());

            using (FlowingContext.Globals.Use(context))
            {
                tracer.CurrentContext.Should().BeSameAs(context);
            }
        }

        [Test]
        public void CurrentContext_property_setter_should_set_a_global_in_flowing_context()
        {
            var context = new TraceContext(Guid.NewGuid(), Guid.NewGuid());

            tracer.CurrentContext = context;

            FlowingContext.Globals.Get<TraceContext>().Should().BeSameAs(context);
        }

        [Test]
        public void CurrentContext_property_setter_should_set_null()
        {
            var context = new TraceContext(Guid.NewGuid(), Guid.NewGuid());

            tracer.CurrentContext = context;

            tracer.CurrentContext.Should().BeEquivalentTo(context);

            tracer.CurrentContext = null;

            tracer.CurrentContext.Should().BeNull();
        }

        [Test]
        public void BeginSpan_should_create_a_new_context_if_none_is_present()
        {
            tracer.CurrentContext.Should().BeNull();

            tracer.BeginSpan();

            tracer.CurrentContext.Should().NotBeNull();
        }

        [Test]
        public void BeginSpan_should_inherit_trace_id_from_existing_context()
        {
            tracer.BeginSpan();

            var traceId = tracer.CurrentContext.TraceId;

            tracer.BeginSpan();

            tracer.CurrentContext.TraceId.Should().Be(traceId);
        }

        [Test]
        public void BeginSpan_should_generate_new_span_id_when_branching_from_existing_context()
        {
            tracer.BeginSpan();

            var spanId = tracer.CurrentContext.SpanId;

            tracer.BeginSpan();

            tracer.CurrentContext.SpanId.Should().NotBe(spanId);
        }

        [Test]
        public void BeginSpan_should_enable_trace_context_scoping_via_builder_disposal()
        {
            tracer.CurrentContext.Should().BeNull();

            using (tracer.BeginSpan())
            {
                var context1 = tracer.CurrentContext;

                context1.Should().NotBeNull();

                using (tracer.BeginSpan())
                {
                    var context2 = tracer.CurrentContext;

                    context2.Should().NotBeSameAs(context1);

                    using (tracer.BeginSpan())
                    {
                        var context3 = tracer.CurrentContext;

                        context3.Should().NotBeSameAs(context2);
                    }

                    tracer.CurrentContext.Should().BeSameAs(context2);
                }

                tracer.CurrentContext.Should().BeSameAs(context1);
            }

            tracer.CurrentContext.Should().BeNull();
        }

        [Test]
        public void BeginSpan_should_correctly_establish_parent_span_relations()
        {
            using (var builder1 = tracer.BeginSpan())
            {
                var context1 = tracer.CurrentContext;

                builder1.CurrentSpan.ParentSpanId.Should().BeNull();

                using (var builder2 = tracer.BeginSpan())
                {
                    var context2 = tracer.CurrentContext;

                    builder2.CurrentSpan.ParentSpanId.Should().Be(context1.SpanId);

                    using (var builder3 = tracer.BeginSpan())
                    {
                        builder3.CurrentSpan.ParentSpanId.Should().Be(context2.SpanId);
                    }
                }
            }
        }
    }
}
