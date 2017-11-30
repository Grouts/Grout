using System.Web.Http.WebHost;
using System.Web.Routing;
using System.Web.SessionState;

namespace Grout.UMP.Handlers
{
    public class ApiControllerHandler : HttpControllerHandler, IRequiresSessionState
    {
        public ApiControllerHandler(RouteData routeData)
            : base(routeData)
        {
        }
    }
}