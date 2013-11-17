using System;
using System.IO;
using log4net.Config;
using System.ServiceProcess;

namespace Perrich.RunAsService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            XmlConfigurator.ConfigureAndWatch(
                new FileInfo(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile)
            );

#if !DEBUG
            ServiceBase[] ServicesToRun;

            // More than one user Service may run within the same process. To add
            // another service to this process, change the following line to
            // create a second service object. For example,
            //
            //   ServicesToRun = new ServiceBase[] {new Service1(), new MySecondUserService()};
            //
            ServicesToRun = new ServiceBase[] { new RunAsService() };

            ServiceBase.Run(ServicesToRun);
#else
            var service = new RunAsService();
            service.StartCommand();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#endif
        }
    }
}