using System;

namespace Grout.Base.DataClasses
{
    public class FailedJob
    {
        public Guid ScheduleId { get; set; }

        public DateTime CurrentScheduleTime { get; set; }

        public string RecurrenceInfo { get; set; }
    }
}
