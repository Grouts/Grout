using System;

namespace Grout.Base.DataClasses
{
    [Serializable]
    public class Schedule : ISchedule
    {
        public Guid ScheduleId { get; set; }

        public Guid ItemId { get; set; }

        public DateTime StartBoundary { get; set; }

        public DateTime? EndBoundary { get; set; }

        public bool Enabled { get; set; }

        public DateTime CreatedDate { get; set; }

        public string ItemName { get; set; }

        public DateTime NextSchedule { get; set; }

        public string Name { get; set; }

        public int Occurrences { get; set; }

        public EndType EndType { get; set; }

        public TaskScheduleType Type { get; set; }

        public DailySchedule DailySchedule { get; set; }

        public DailyWeekDaySchedule DailyWeekDaySchedule { get; set; }

        public WeeklySchedule WeeklySchedule { get; set; }

        public MonthlySchedule MonthlySchedule { get; set; }

        public MonthlyDowSchedule MonthlyDowSchedule { get; set; }

        public YearlySchedule YearlySchedule { get; set; }

        public YearlyDowSchedule YearlyDowSchedule { get; set; }

    }
}
