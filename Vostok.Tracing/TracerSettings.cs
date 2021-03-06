﻿using System;
using JetBrains.Annotations;
using Vostok.Tracing.Abstractions;

namespace Vostok.Tracing
{
    /// <summary>
    /// Configuration governing behaviour of <see cref="Tracer"/>.
    /// </summary>
    [PublicAPI]
    public class TracerSettings
    {
        public TracerSettings([NotNull] ISpanSender sender)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
        }

        /// <summary>
        /// <para>Gets or sets the sender used to offload constructed spans. See <see cref="ISpanSender"/> for more details.</para>
        /// <para>This configuration parameter is <b>required</b>: <c>null</c> values are not allowed.</para>
        /// </summary>
        [NotNull]
        public ISpanSender Sender { get; }

        /// <summary>
        /// <para>Gets or sets the value that will be automatically set for <see cref="WellKnownAnnotations.Common.Host"/> annotation in spans.</para>
        /// <para>This configuration parameter is <b>optional</b>: if it's value is <c>null</c>, <see cref="System.Net.Dns.GetHostName"/> will be used as default value.</para>
        /// </summary>
        [CanBeNull]
        public string Host { get; set; }

        /// <summary>
        /// <para>Gets or sets the value that will be automatically set for <see cref="WellKnownAnnotations.Common.Application"/> annotation in spans.</para>
        /// <para>This configuration parameter is <b>optional</b>: if it's value is <c>null</c>, current process name will be used as default value.</para>
        /// </summary>
        [CanBeNull]
        public string Application { get; set; }

        /// <summary>
        /// <para>Gets or sets the value that will be automatically set for <see cref="WellKnownAnnotations.Common.Environment"/> annotation in spans.</para>
        /// <para>This configuration parameter is <b>optional</b>: if it's value is <c>null</c>, corresponding annotation won't be added to spans.</para>
        /// </summary>
        [CanBeNull]
        public string Environment { get; set; }
    }
}