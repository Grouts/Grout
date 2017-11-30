using System;

namespace Grout.Base.DataClasses
{
    [Serializable]
    public class WeeklySchedule : IWeeklyScheduler
    {
        public DaysOfWeek DaysOfWeek { get; set; }

        public int WeeksInterval { get; set; }

    }
}
