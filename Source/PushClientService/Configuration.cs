using System.Configuration;

namespace PushClientService
{
    public static class Configuration
    {
        public static string PushProxyUrl { get; private set; }
        public static string CiUrl { get; private set; }
        public static string Secret { get; private set; }

        static Configuration()
        {
            var appSettings = ConfigurationManager.AppSettings;
            PushProxyUrl = appSettings["PUSH_PROXY_URL"];
            CiUrl = appSettings["CI_URL"];
            Secret = appSettings["SECRET"];
        }
    }
}
