using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using Syncfusion.Server.Base.Reports.Scheduler;

namespace Syncfusion.Server.Base.Reports.SchedulerService
{
    [RunInstaller(true)]
    public partial class SchedulerServiceInstaller : Installer
    {
        private readonly ServiceInstaller serviceInstaller;

        private readonly ServiceProcessInstaller processInstaller;

        public SchedulerServiceInstaller()
        {
            var serviceName = ScheduleHelper.GetScheduleServiceName();

            processInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;

            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = serviceName;
            serviceInstaller.DisplayName = serviceName;
            serviceInstaller.Description = "Generates and emails reports on schedule";

            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
        }
    }
}
