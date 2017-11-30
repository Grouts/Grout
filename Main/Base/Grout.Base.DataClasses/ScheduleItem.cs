using System;

namespace Grout.Base.DataClasses
{
    public class ScheduleItem
    {
        public Guid Id { get; set; }

        public Guid ScheduleId { get; set; }

        public Guid CategoryId { get; set; }

        public Guid ItemId { get; set; }

        public string Name { get; set; }

        public string RecurrenceType { get; set; }

        public int RecurrenceTypeId { get; set; }

        public string RecurrenceInfo { get; set; }

        public Schedule Recurrence { get; set; }

        public DateTime StartDate { get; set; }

        public string StartDateString { get; set; }

        public string EndType { get; set; }

        public DateTime? EndDate { get; set; }

        public string EndDateString { get; set; }

        public int EndAfter { get; set; }

        public DateTime? NextSchedule { get; set; }

        public int ExportTypeId { get; set; }

        public string ExportType { get; set; }

        public bool IsEnabled { get; set; }

        public int ModifiedById { get; set; }

        public DateTime ModifiedDate { get; set; }

        public int CreatedById { get; set; }

        public bool IsActive { get; set; }
    }
}
