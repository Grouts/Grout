using System;

namespace Grout.Base.DataClasses
{
    public class ScheduleContent
    {
        public Guid ScheduleId { get; set; }

        public Guid ItemId { get; set; }

        public ExportType ExportTypeId { get; set; }

        public string ExportFormat { get; set; }

        public string ScheduleName { get; set; }

        public string FileLocation { get; set; }

        public DateTime CurrentScheduleTime { get; set; }

        public string RecurrenceInfo { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string ReportName { get; set; }
    }
}
