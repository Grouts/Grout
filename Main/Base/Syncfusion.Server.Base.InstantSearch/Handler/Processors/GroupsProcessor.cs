using System.Web.Script.Serialization;

namespace Syncfusion.UMP.InstantSearch.Base
{
    public class GroupsProcessor : ISearchProcessor
    {
        public string ProcessRequest(System.Web.HttpContext ctxt)
        {
            var serializer = new JavaScriptSerializer();
            var qr = ctxt.Request["q"];
            var attr = serializer.Deserialize<SearchArgs>(ctxt.Request["attr"]);
            var groupObj = new InstantSearch();
            return groupObj.SearchGroup(qr, attr);
        }
    }
}