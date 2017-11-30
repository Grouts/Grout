using System;
using System.Linq;
using System.Web.Mvc;
using Grout.Base;
using Grout.Base.Item;

namespace Grout.UMP.ActionFilters
{
    public class ItemsOfCategoryActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var id = filterContext.HttpContext.Request.Url.Segments[2];

            var itemId = new Guid();

            if (String.IsNullOrEmpty(id) || Guid.TryParse(id, out itemId) == false)
                filterContext.Result = new RedirectResult("/categories", true);
            base.OnActionExecuting(filterContext);
        }
    }
}