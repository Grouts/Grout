using System.Web.Script.Serialization;

namespace Syncfusion.UMP.InstantSearch.Base
{
    class AllProcesser : ISearchProcessor
    {
        public string ProcessRequest(System.Web.HttpContext ctxt)
        {
            var serializer = new JavaScriptSerializer();
            var qry = ctxt.Request["q"];
            var attr = serializer.Deserialize<SearchArgs>(ctxt.Request["attr"]);
            var userObj = new InstantSearch();
            return userObj.SearchAll(qry, attr);
        }
    }
}