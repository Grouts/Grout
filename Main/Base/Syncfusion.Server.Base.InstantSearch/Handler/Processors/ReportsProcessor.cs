using System.Web.Script.Serialization;

namespace Syncfusion.UMP.InstantSearch.Base
{
    public class ReportsProcessor:ISearchProcessor
    {
        public string ProcessRequest(System.Web.HttpContext ctxt)
        {
            var serializer = new JavaScriptSerializer();
            var qry=ctxt.Request["q"];
            var attr = serializer.Deserialize <SearchArgs>(ctxt.Request["attr"]);
            var reportObj = new InstantSearch();
            return reportObj.SearchReport(qry, attr);
        }
    }
}