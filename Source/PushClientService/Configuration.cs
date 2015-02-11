using System.Configuration;

namespace PushClientService
{
    public static class Configuration
    {
        public static string PushProxyUrl { get; private set; }
        public static string CiUrl { get; private set; }

        static Configuration()
        {
            var appSettings = ConfigurationManager.AppSettings;
            CiUrl = appSettings["CI_URL"];
            PushProxyUrl = appSettings["PUSH_PROXY_URL"];
        }
    }
}
