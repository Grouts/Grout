using System;

namespace Grout.Base.DataClasses
{
    [Serializable]
    public class YearlyDowSchedule : IYearlyDowScheduler
    {
        public WeekOfMonth WeekOfMonth { get; set; }

        public DayOfTheWeek DayOfWeek { get; set; }

        public MonthOfTheYear MonthOfYear { get; set; }

        public string DaysOfTheWeek { get; set; }

        public string WeeksOfTheMonth { get; set; }

        public string MonthsOfTheYear { get; set; }

        public int YearsInterval { get; set; }

    }
}
