using System;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Syncfusion.Server.Base.Reports.SchedulerService.Installer
{
    class Program
    {
        static ServiceController _service;
        
        static void Main(string[] args)
        {
            if (args.Length != 0 && args[0].ToLower() != "install")
            {
                if (IsServiceInstalled())
                {
                    switch (args[0].ToLower())
                    {
                        case "stop":
                            if (IsServiceInstalled())
                            {
                                if (_service.Status == ServiceControllerStatus.Running)
                                {
                                    _service.Stop();
                                    _service.WaitForStatus(ServiceControllerStatus.Stopped);
                                    Console.WriteLine("Service has been stopped successfully");
                                }
                                if (_service.Status == ServiceControllerStatus.Stopped)
                                {
                                    Console.WriteLine("Service has been stopped already");
                                }
                            }
                            else
                            {
                                Console.WriteLine("No service has been found on the specified name");
                            }
                            break;
                        case "start":
                            if (IsServiceInstalled())
                            {
                                if (_service.Status == ServiceControllerStatus.Stopped)
                                {
                                    _service.Start();
                                    _service.WaitForStatus(ServiceControllerStatus.Running);
                                }
                            }
                            else
                            {
                                Console.WriteLine("No service has been found on the specified name");
                            }
                            break;
                        case "uninstall":
                        default:
                            if (IsServiceInstalled())
                            {
                                ManagedInstallerClass.InstallHelper(new string[] { "/u", "Syncfusion.Server.Base.Reports.SchedulerService.exe" });
                                Console.WriteLine("Service has been uninstalled successfully");
                            }
                            else
                            {
                                Console.WriteLine("No service has been found on the specified name");
                            }
                            break;
                    }
                }
            }
            else
            {
                try
                {
                    if (IsServiceInstalled() && _service.Status == ServiceControllerStatus.Stopped)
                    {
                        _service.Start();
                        _service.WaitForStatus(ServiceControllerStatus.Running);
                    }
                    else
                    {
                        ManagedInstallerClass.InstallHelper(new string[] { "Syncfusion.Server.Base.Reports.SchedulerService.exe" });
                        _service.Start();
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        static bool IsServiceInstalled()
        {
            try
            {
                _service = new ServiceController(SchedulerService.GetSchedulerServiceName());
                return (!String.IsNullOrEmpty(_service.ServiceName));
            }
            catch
            {
                return false;
            }
        }
    }
}
