using System.Configuration;

namespace PushClientService
{
    public static class Configuration
    {
        public static string PushProxyUrl { get; private set; }
        public static string CiUrl { get; private set; }
        public static string Secret { get; private set; }
        public static int ConcurrentPushes { get; private set; }
        public static int MaxPushQueue { get; private set; }

        static Configuration()
        {
            var appSettings = ConfigurationManager.AppSettings;
            PushProxyUrl = appSettings["PUSH_PROXY_URL"];
            CiUrl = appSettings["CI_URL"];
            Secret = appSettings["SECRET"];
            ConcurrentPushes = int.Parse(appSettings["CONCURRENT_PUSHES"]);
            MaxPushQueue = int.Parse(appSettings["MAX_PUSH_QUEUE"]);
        }
    }
}
