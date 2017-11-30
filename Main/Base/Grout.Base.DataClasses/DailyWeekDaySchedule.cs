using System;
namespace Grout.Base.DataClasses
{
    [Serializable]
    public class DailyWeekDaySchedule : IDailyWeekDayScheduler
    {
        public bool WeekDay { get; set; }
    }
}
