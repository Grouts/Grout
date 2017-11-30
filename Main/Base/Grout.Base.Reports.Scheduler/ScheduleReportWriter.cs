using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Grout.Base.DataClasses;
using Grout.Base.Logger;

namespace Grout.Base.Reports.Scheduler
{
    internal class ScheduleReportWriter
    {
        public static bool GenerateReport(string exportDirectory, ScheduleContent scheduleContent,string userName,string password)
        {
            LogExtension.LogInfo("Report exporting started", MethodBase.GetCurrentMethod(), " ExportDirectory - " + exportDirectory + " UserName - " + userName + " Password - " + password);
            var exportStatus = false;
            //try
            //{
            //    if (Directory.Exists(exportDirectory + scheduleContent.ScheduleId))
            //    {
            //        Array.ForEach(Directory.GetFiles(exportDirectory + scheduleContent.ScheduleId), File.Delete);
            //    }
            //    else
            //    {
            //        Directory.CreateDirectory(exportDirectory + scheduleContent.ScheduleId);
            //    }

            //    var thread = new Thread(delegate()
            //    {
            //        try
            //        {
            //            var reportContent = new ReportWriter();
            //            reportContent.ReportingServer = new UMPServer();
            //            reportContent.ReportServerCredential =
            //    new System.Net.NetworkCredential(userName, password);
            //            reportContent.ReportServerUrl = GlobalAppSettings.SystemSettings.BaseUrl + "/api/reportserverapi";
            //            reportContent.ReportPath = scheduleContent.ItemId.ToString();

            //            switch (scheduleContent.ExportTypeId)
            //            {
            //                case ExportType.Pdf:
            //                    reportContent.Save(
            //                        exportDirectory + scheduleContent.ScheduleId + "\\" + scheduleContent.ReportName +
            //                        ".pdf", WriterFormat.PDF);
            //                    break;

            //                case ExportType.Excel:
            //                    reportContent.Save(
            //                        exportDirectory + scheduleContent.ScheduleId + "\\" + scheduleContent.ReportName +
            //                        ".xls", WriterFormat.Excel);
            //                    break;

            //                case ExportType.Word:
            //                    reportContent.Save(
            //                        exportDirectory + scheduleContent.ScheduleId + "\\" + scheduleContent.ReportName +
            //                        ".doc", WriterFormat.Word);
            //                    break;

            //                default:
            //                    reportContent.Save(
            //                        exportDirectory + scheduleContent.ScheduleId + "\\" + scheduleContent.ReportName +
            //                        ".html", WriterFormat.HTML);
            //                    break;
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            exportStatus = false;
            //            LogExtension.LogError("Exception while exporting report", ex, MethodBase.GetCurrentMethod(), " ExportDirectory - " + exportDirectory + " UserName - " + userName + " Password - " + password + " ScheduleId - " + scheduleContent.ScheduleId + " ExportTypeId" + scheduleContent.ExportTypeId);
            //        }
            //        exportStatus = true;
            //    });
            //    thread.SetApartmentState(ApartmentState.STA);
            //    thread.Start();
            //    thread.Join();
            //}
            //catch (Exception ex)
            //{
            //    exportStatus = false;
            //    LogExtension.LogError("Exception while exporting report", ex, MethodBase.GetCurrentMethod(), " ExportDirectory - " + exportDirectory + " UserName - " + userName + " Password - " + password + " ScheduleId - " + scheduleContent.ScheduleId + " ExportTypeId" + scheduleContent.ExportTypeId);
            //}
            return exportStatus;
        }
    }
}