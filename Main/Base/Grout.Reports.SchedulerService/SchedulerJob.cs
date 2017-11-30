using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Syncfusion.Server.Base.Logger;
using Syncfusion.Server.Base.Reports.Scheduler;

namespace Syncfusion.Server.Base.Reports.SchedulerService
{
    internal class SchedulerJob
    {
        private readonly ScheduleJobProcessor scheduleJobProcessor = new ScheduleJobProcessor(ConfigFolderPath,
            ExtractDirectoryPath);

        public static string ConfigFolderPath = GlobalAppSettings.GetConfigFilepath();

        public static string ExtractDirectoryPath = GlobalAppSettings.GetSchedulerExportPath() + "ExportedReports\\";

        public void ProcessSchedules(DateTime startTime, DateTime endTime)
        {
            LogExtension.LogInfo(
                "Processing schedules between " + startTime.ToString(CultureInfo.InvariantCulture) + " and " +
                endTime.ToString(CultureInfo.InvariantCulture), MethodBase.GetCurrentMethod());

            var tasks = new List<Task>();
            var cancellationTokenSource = new CancellationTokenSource();
            var scheduleJobs = scheduleJobProcessor.GetScheduleJobs(startTime, endTime);

            tasks.Add(Task.Factory.StartNew(() =>
                Parallel.ForEach(scheduleJobs, scheduleJob => new Task(() =>
                    scheduleJobProcessor.ProcessSchedule(scheduleJob.ScheduleId), cancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning).Delay(GetDifferenceTimeSpan(scheduleJob.CurrentScheduleTime))
                    .TimeOutAfter(cancellationTokenSource, scheduleJob.ScheduleId))));

            Task.WaitAll(tasks.ToArray());
        }

        public void ProcessInitSchedules(DateTime startTime, DateTime endTime)
        {
            var tasks = new List<Task>
            {
                Task.Factory.StartNew(() => ProcessSchedules(startTime, endTime)),
                Task.Factory.StartNew(() => ReschedulePastSchedulerJobs(startTime))
            };
            Task.WaitAll(tasks.ToArray());
        }

        public double GetDifferenceTimeSpan(DateTime dateTime)
        {
            return DateTime.UtcNow < dateTime ? dateTime.Subtract(DateTime.UtcNow).TotalMilliseconds : 100;
        }

        public void ReschedulePastSchedulerJobs(DateTime currentTime)
        {
            if (!File.Exists(GlobalAppSettings.GetSchedulerExportPath() + "config.xml")) return;
            try
            {
                var serviceStopTime = DeserializeTime(GlobalAppSettings.GetSchedulerExportPath() + "config.xml");
                var lastProcessedDate = Convert.ToDateTime(serviceStopTime.ScheduleEndTime, CultureInfo.InvariantCulture);
                var pastSchedules = scheduleJobProcessor.GetFailedJobs(lastProcessedDate, currentTime);
                scheduleJobProcessor.RescheduleUnProcessedJobs(pastSchedules);
                File.Delete(GlobalAppSettings.GetSchedulerExportPath() + "config.xml");
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception while re scheduling past schedules", e, MethodBase.GetCurrentMethod());
            }
        }

        public void UpdateTimedOutJob(Guid scheduleId)
        {
            scheduleJobProcessor.UpdateTimeOutStatus(scheduleId);
        }

        public void SerializeTime(ServiceStopTime serviceStopTime, string path)
        {
            try
            {
                var xmlserializer = new XmlSerializer(typeof(ServiceStopTime));
                using (var writer = new StreamWriter(path))
                {
                    xmlserializer.Serialize(writer, serviceStopTime);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError("Exception while serializing service stop time", ex, MethodBase.GetCurrentMethod());
            }
        }

        public ServiceStopTime DeserializeTime(string path)
        {
            var xmlSerializer = new XmlSerializer(typeof(ServiceStopTime));
            try
            {
                if (File.Exists(path))
                {
                    ServiceStopTime data;
                    using (var reader = new StreamReader(path))
                    {
                        data = (ServiceStopTime)xmlSerializer.Deserialize(reader);
                        reader.Close();
                    }
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError("Exception while deserializing service stop time", ex,
                    MethodBase.GetCurrentMethod());
            }
            return null;
        }
    }
}