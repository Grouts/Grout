using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;
using Syncfusion.Server.Base.Logger;
using Syncfusion.Server.Base.Reports.Scheduler;

namespace Syncfusion.Server.Base.Reports.SchedulerService
{
    public class SchedulerService : ServiceBase
    {
        readonly Timer timer = new Timer();
        public static DateTime ProcessStartTime, ProcessEndTime;

        public SchedulerService()
        {
            CanStop = true;
            CanPauseAndContinue = true;
            AutoLog = true;
            ServiceName = ConfigurationManager.AppSettings["SchedulerServiceName"];
        }

        public static void Main()
        {
            Run(new SchedulerService());
        }

        public void RecurringTrigger(object sender, ElapsedEventArgs args)
        {
            LogExtension.LogInfo("Recurring event triggered", MethodBase.GetCurrentMethod());
            ProcessStartTime = ProcessEndTime;
            ProcessEndTime = DateTime.UtcNow.AddMinutes(3);
            new Task(() => new SchedulerJob().ProcessSchedules(ProcessStartTime, ProcessEndTime)).Start();
        }

        protected override void OnStart(string[] args)
        {
            LogExtension.LogInfo("Service started", MethodBase.GetCurrentMethod());
            try
            {
                timer.Interval = 180000;
                timer.Elapsed += RecurringTrigger;
                timer.Start();
                base.OnStart(args);
                ProcessStartTime = DateTime.UtcNow;
                ProcessEndTime = DateTime.UtcNow.AddMinutes(3);
                new Task(() => new SchedulerJob().ProcessInitSchedules(ProcessStartTime, ProcessEndTime)).Start();
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception is thrown while starting service", e, MethodBase.GetCurrentMethod());
            }
        }

        protected override void OnStop()
        {
            var serviceStopTime = new ServiceStopTime { ScheduleEndTime = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture) };

            try
            {
                var schedulerExportPath = GlobalAppSettings.GetSchedulerExportPath();
                if (Directory.Exists(schedulerExportPath) == false)
                {
                    Directory.CreateDirectory(schedulerExportPath);
                }

                new SchedulerJob().SerializeTime(serviceStopTime,
                    GlobalAppSettings.GetSchedulerExportPath() + "config.xml");
                LogExtension.LogInfo("Service stopped", MethodBase.GetCurrentMethod());
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception is thrown while stopping service", e, MethodBase.GetCurrentMethod());
            }
            finally
            {
                base.OnStop();
            }
        }

        public static string GetSchedulerServiceName()
        {
            return ScheduleHelper.GetScheduleServiceName();
        }
    }
}
