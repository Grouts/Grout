using System.Collections.Generic;

namespace Grout.Base.DataClasses
{
    public class ReportUploadResponse
    {
        public bool Status { get; set; }

        public bool IsShared { get; set; }

        public string UploadedReportName { get; set; }

        public List<string> SharedDataSourceList { get; set; }
    }
}
