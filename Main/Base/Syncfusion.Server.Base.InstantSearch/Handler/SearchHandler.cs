using System.Web;

namespace Syncfusion.UMP.InstantSearch.Base
{
    public class SearchHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            var type = context.Request["context"];
            var process = SearchFactory.Create(type);
            context.Response.Write(process.ProcessRequest(context));
        }

    }
}