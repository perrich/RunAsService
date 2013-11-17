using System.ComponentModel;
using System.Configuration.Install;

namespace Perrich.RunAsService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            var service = new RunAsService();

            serviceInstaller1.DisplayName = service.DisplayName;
            serviceInstaller1.Description = service.Description;
            serviceInstaller1.ServiceName = service.ServiceName;
        }
    }
}