using System.Web;
using System.Web.Routing;

namespace Grout.UMP.Handlers
{
    public class ApiRouteHandler : IRouteHandler
    {
        IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext)
        {
            return new ApiControllerHandler(requestContext.RouteData);
        }
    }
}