using System;
using JetBrains.Annotations;

namespace Vostok.Tracing
{
    internal static class TracerSettingsValidator
    {
        public static TracerSettings Validate([NotNull] TracerSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (settings.Sender == null)
                throw new ArgumentNullException(nameof(settings.Sender));

            return settings;
        }
    }
}