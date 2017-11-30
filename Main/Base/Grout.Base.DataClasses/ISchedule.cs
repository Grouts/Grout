using System;

namespace Grout.Base.DataClasses
{
    internal interface ISchedule
    {
        TaskScheduleType Type { get; set; }
        Guid ScheduleId { get; set; }
        bool Enabled { get; set; }
        int Occurrences { get; set; }
        DateTime StartBoundary { get; set; }
        DateTime? EndBoundary { get; set; }
    }

    internal interface ITimelyScheduler
    {

    }

    internal interface IDailyScheduler
    {
        int DaysInterval { get; set; }
    }

    internal interface IDailyWeekDayScheduler
    {
        bool WeekDay { get; set; }
    }

    internal interface IWeeklyScheduler
    {
        DaysOfWeek DaysOfWeek { get; set; }
        int WeeksInterval { get; set; }
    }

    internal interface IMonthlyScheduler
    {
        int DayOfMonth { get; set; }
        int Months { get; set; }
    }

    internal interface IMonthlyDowScheduler
    {
        WeekOfMonth WeekOfMonth { get; set; }
        DayOfTheWeek DayOfWeek { get; set; }
        int Months { get; set; }
    }

    internal interface IYearlyScheduler
    {
        MonthOfTheYear Month { get; set; }
        int DayOfMonth { get; set; }
        int YearsInterval { get; set; }
    }

    internal interface IYearlyDowScheduler
    {
        WeekOfMonth WeekOfMonth { get; set; }
        DayOfTheWeek DayOfWeek { get; set; }
        MonthOfTheYear MonthOfYear { get; set; }
        int YearsInterval { get; set; }
    }
}
