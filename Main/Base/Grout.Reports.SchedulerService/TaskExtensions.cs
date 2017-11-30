using System;
using System.Threading;
using System.Threading.Tasks;

namespace Syncfusion.Server.Base.Reports.SchedulerService
{
    internal static class TaskExtensions
    {
        public static Task TimeOutAfter(this Task task, CancellationTokenSource cancellationTokenSource, Guid scheduleId)
        {
            if (!task.Wait(360000, cancellationTokenSource.Token) && !task.IsCompleted)
            {
                new SchedulerJob().UpdateTimedOutJob(scheduleId);
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }
            return task;
        }

        public static Task Delay(this Task task, double delayMilliSeconds)
        {
            if (delayMilliSeconds > Int32.MaxValue)
            {
                delayMilliSeconds = Int32.MaxValue;
            }
            var timer = new System.Timers.Timer();
            timer.Elapsed += (obj, args) =>
            {
                task.Start();
            };
            timer.Interval = delayMilliSeconds;
            timer.AutoReset = false;
            timer.Start();
            return task;
        }
    }
}