using System;

namespace Grout.UMP.Models
{
    public class ScheduleViewModel
    {
        public Guid Id { get; set; }
        public Guid ItemId { get; set; }
        public string Name { get; set; }
        public string ItemName { get; set; }
        public bool IsEnabled { get; set; }
        public dynamic LastRun { get; set; }
        public string LastRunString { get; set; }
        public dynamic NextSchedule { get; set; }
        public string NextScheduleString { get; set; }
        public string Status { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
    }
}