using System;

namespace Grout.Base.DataClasses
{
    public class ScheduleJob
    {
        public Guid ScheduleId { get; set; }

        public DateTime CurrentScheduleTime { get; set; }
    }
}
