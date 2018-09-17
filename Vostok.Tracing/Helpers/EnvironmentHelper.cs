using System.Net;

namespace Vostok.Tracing.Helpers
{
    internal static class EnvironmentHelper
    {
        static EnvironmentHelper()
        {
            try
            {
                Host = Dns.GetHostName();
            }
            catch
            {
                Host = "unknown";
            }
        }

        public static string Host { get; }
    }
}
