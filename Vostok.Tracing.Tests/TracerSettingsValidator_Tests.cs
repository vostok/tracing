using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Tracing.Configuration;

// ReSharper disable AssignNullToNotNullAttribute

namespace Vostok.Tracing.Tests
{
    [TestFixture]
    internal class TracerSettingsValidator_Tests
    {
        [Test]
        public void Should_consider_default_settings_valid()
        {
            TracerSettingsValidator.Validate(new TracerSettings());
        }

        [Test]
        public void Should_return_exactly_input_settings_when_they_are_valid()
        {
            var settings = new TracerSettings();

            TracerSettingsValidator.Validate(settings).Should().BeSameAs(settings);
        }

        [Test]
        public void Should_produce_an_error_when_given_settings_are_null()
        {
            new Action(() => TracerSettingsValidator.Validate(null))
                .Should().Throw<ArgumentNullException>().Which.ShouldBePrinted();
        }

        [Test]
        public void Should_produce_an_error_when_given_settings_have_null_sender()
        {
            new Action(() => TracerSettingsValidator.Validate(new TracerSettings { Sender = null }))
                .Should().Throw<ArgumentNullException>().Which.ShouldBePrinted();
        }
    }
}