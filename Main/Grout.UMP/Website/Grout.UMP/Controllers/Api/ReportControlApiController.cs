using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Reports.Scheduler;
using Grout.UMP.Models.API;

namespace Grout.UMP.Controllers
{
    
    public class ReportControlApiController : ApiController
    {
        //
        // GET: /Viewer/
        JavaScriptSerializer _serializer = new JavaScriptSerializer();
        private readonly Viewer _viewerApi = new Viewer();

        //public object GetResource(string key, string resourcetype, bool isPrint)
        //{
        //    return ReportHelper.GetResource(key, resourcetype, isPrint);
        //}

        //public void OnInitReportOptions(ReportViewerOptions reportOption)
        //{
            
        //    var userDetail = new UserManagement().FindUserByUserId(Convert.ToInt32(HttpContext.Current.User.Identity.Name));

        //    var cryptography=new Cryptography();

        //    reportOption.ReportModel.ReportServerCredential =
        //        new NetworkCredential(userDetail.UserName, userDetail.Password);

        //    reportOption.ReportModel.ReportingServer = new UMPServer();
        //}

        //public void OnReportLoaded(ReportViewerOptions reportOption)
        //{
        ////    throw new NotImplementedException();
        //}

        //public object PostReportAction(Dictionary<string, object> jsonResult)
        //{
        //    return ReportHelper.ProcessReport(jsonResult, this);
        //}

        //[HttpPost]
        //public ReportViewerDefinition GetReportById(ItemRequest itemRequest)
        //{
        //    return _viewerApi.GetReportById(itemRequest);
        //}
    }
}
