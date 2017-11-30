using System;

namespace Grout.Base.DataClasses
{
    [Serializable]
    public class MonthlySchedule : IMonthlyScheduler
    {
        public int DayOfMonth { get; set; }

        public int Months { get; set; }

    }
}
