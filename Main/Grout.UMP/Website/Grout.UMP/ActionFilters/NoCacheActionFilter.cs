using System.Web.Mvc;

namespace Grout.UMP.ActionFilters
{
    public class NoCacheActionFilter : OutputCacheAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }
    }
}