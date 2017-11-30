using System;

namespace Grout.Base.DataClasses
{
    [Serializable]
    public class MonthlyDowSchedule : IMonthlyDowScheduler
    {
        public DayOfTheWeek DayOfWeek { get; set; }

        public string DaysOfTheWeek { get; set; }

        public string WeeksOfTheMonth { get; set; }

        public WeekOfMonth WeekOfMonth { get; set; }

        public int Months { get; set; }


    }
}
