using Grout.Base;
using System;
using System.Web.Mvc;

namespace Grout.UMP.ActionFilters
{
    public class AdminActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated == false)
                filterContext.Result = new RedirectResult("/login", true);
            else
            {
                var isAdmin = GlobalAppSettings.IsAdmin(Convert.ToInt32(filterContext.HttpContext.User.Identity.Name));
                if (isAdmin == false)
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = "../Home/PermissionDenied"
                    };
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}