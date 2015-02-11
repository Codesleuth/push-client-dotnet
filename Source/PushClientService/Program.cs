using log4net.Config;
using Topshelf;

namespace PushClientService
{
    public static class Program
    {
        public static void Main()
        {
            XmlConfigurator.Configure();

            HostFactory.Run(hc =>
            {
                hc.UseLog4Net();

                hc.Service<Server>(sc =>
                {
                    sc.ConstructUsing(ServiceFactory.CreateServer);
                    sc.WhenStarted(s => s.Start());
                    sc.WhenStopped(s => s.Stop());
                });

                hc.RunAsLocalSystem();
                hc.DependsOnEventLog();

                hc.SetDescription("Pushes proxied Github push events to Jenkins.");
                hc.SetServiceName("PushProxy.Service");
                hc.SetDisplayName("Push Proxy Client");

                hc.EnableServiceRecovery(rc =>
                {
                    rc.RestartService(1);
                    rc.RestartService(1);
                    rc.RestartService(1);
                    rc.SetResetPeriod(0);
                });
            });
                
        }
    }
}