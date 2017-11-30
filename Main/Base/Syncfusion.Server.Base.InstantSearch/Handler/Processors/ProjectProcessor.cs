using System.Web.Script.Serialization;

namespace Syncfusion.UMP.InstantSearch.Base
{
    public class ProjectProcessor : ISearchProcessor
    {
        public string ProcessRequest(System.Web.HttpContext ctxt)
        {
            var serializer = new JavaScriptSerializer();
            var p = ctxt.Request["q"];
            var attr = serializer.Deserialize<SearchArgs>(ctxt.Request["attr"]);
            var projectObj = new InstantSearch();
            return projectObj.SearchProject(p, attr);
        }
    }
}