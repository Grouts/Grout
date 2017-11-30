using System;
using System.Linq;
using System.Web.Mvc;
using Grout.Base;
using Grout.Base.Item;

namespace Grout.UMP.ActionFilters
{
    public class ItemAccessActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var id = filterContext.HttpContext.Request["id"];

            var itemId = new Guid();

            if (String.IsNullOrEmpty(id) || Guid.TryParse(id, out itemId) == false)
                filterContext.Result = new RedirectResult("/reports", true);
            else
            {
                var itemManagement = new ItemManagement(GlobalAppSettings.QueryBuilder, GlobalAppSettings.DataProvider);
                var itemDetail = itemManagement.GetItemDetailsFromItemId(itemId, false);

                if (itemDetail == null)
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = "../Home/PermissionDenied"
                    };
                }
                else
                {
                    var itemsList = itemManagement.GetItems(Convert.ToInt32(filterContext.HttpContext.User.Identity.Name), itemDetail.ItemType, null, null, null, null, null, itemId);
                    if (!itemsList.result.Any(a => a.Id == itemId && a.CanRead))
                    {
                        filterContext.Result = new ViewResult
                        {
                            ViewName = "../Home/PermissionDenied"
                        };
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}