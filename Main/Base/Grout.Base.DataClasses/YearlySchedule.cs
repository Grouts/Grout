using System;

namespace Grout.Base.DataClasses
{
    [Serializable]
    public class YearlySchedule : IYearlyScheduler
    {
        public int DayOfMonth { get; set; }

        public MonthOfTheYear Month { get; set; }

        public string MonthsOfTheYear { get; set; }

        public int YearsInterval { get; set; }

    }
}
