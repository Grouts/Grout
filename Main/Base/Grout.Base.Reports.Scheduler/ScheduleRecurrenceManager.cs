using System;
using System.Reflection;
using Grout.Base.DataClasses;
using Grout.Base.Logger;

namespace Grout.Base.Reports.Scheduler
{
    public class ScheduleRecurrenceManager
    {
        /// <summary>
        /// If schedule start time is equal to UTC time then the current datetime value is skipped and the next recurrence is calculated.
        /// </summary>
        /// <param name="currentScheduleDate"></param>
        /// <param name="recurrenceInfo"></param>
        /// <returns></returns>
        public DateTime? GetNextScheduleDate(DateTime currentScheduleDate, string recurrenceInfo)
        {
            LogExtension.LogInfo("Next schedule date calculation started", MethodBase.GetCurrentMethod(), " CurrentScheduleDate - " + currentScheduleDate + " recurrenceInfo - " + recurrenceInfo);
            DateTime? nextScheduleDate = new DateTime();
            var scheduleInfo = ScheduleHelper.XMLDeserializer(recurrenceInfo);
            try
            {
                if (scheduleInfo != null)
                {
                    switch (scheduleInfo.Type)
                    {
                        case TaskScheduleType.Daily:
                            var daily = new DailyRecurrence();
                            nextScheduleDate = daily.GetNextDailyScheduleDate(scheduleInfo, currentScheduleDate);
                            break;

                        case TaskScheduleType.DailyWeekDay:
                            var dailyWeekDay = new DailyRecurrence();
                            nextScheduleDate = dailyWeekDay.GetNextDailyWeekDayScheduleDate(scheduleInfo,
                                currentScheduleDate);
                            break;

                        case TaskScheduleType.Weekly:
                            var weekly = new WeeklyRecurrence();
                            nextScheduleDate = weekly.GetNextWeeklyScheduleDate(scheduleInfo, currentScheduleDate);
                            break;

                        case TaskScheduleType.Monthly:
                            var monthly = new MonthlyRecurrence();
                            nextScheduleDate = monthly.GetNextMonthlyScheduleDate(scheduleInfo, currentScheduleDate);
                            break;

                        case TaskScheduleType.MonthlyDOW:
                            var monthlyDOW = new MonthlyRecurrence();
                            nextScheduleDate = monthlyDOW.GetNextMonthlyDOWScheduleDate(scheduleInfo,
                                currentScheduleDate);
                            break;

                        case TaskScheduleType.Yearly:
                            var yearly = new YearlyRecurrence();
                            nextScheduleDate = yearly.GetNextYearlyScheduleDate(scheduleInfo, currentScheduleDate);
                            break;

                        case TaskScheduleType.YearlyDOW:
                            var yearlyDOW = new YearlyRecurrence();
                            nextScheduleDate = yearlyDOW.GetNextYearlyDOWScheduleDate(scheduleInfo, currentScheduleDate);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception is thrown in next date calculation", e, MethodBase.GetCurrentMethod(), " CurrentScheduleDate - " + currentScheduleDate + " recurrenceInfo - " + recurrenceInfo + " ScheduleInfoType - " + scheduleInfo.Type + " NextScheduleDate - " + nextScheduleDate);
                return nextScheduleDate;
            }
            LogExtension.LogInfo("Next schedule date calculation successfull", MethodBase.GetCurrentMethod(), " CurrentScheduleDate - " + currentScheduleDate + " recurrenceInfo - " + recurrenceInfo + " ScheduleInfoType - " + scheduleInfo.Type + " NextScheduleDate - " + nextScheduleDate);
            return nextScheduleDate;
        }
    }
}
