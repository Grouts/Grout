using System;
namespace Grout.Base.DataClasses
{
    public class ItemLogs
    {
        public int ItemLogId { get; set; }

        public string DisplayName { get; set; }

        public string UserName { get; set; }

        public string TargetUserFullName { get; set; }

        public string ReportLogType { get; set; }

        public DateTime UpdatedDate { get; set; }

        public string UpdatedDateString { get; set; }

        public string DisplayText { get; set; }

    }
}
