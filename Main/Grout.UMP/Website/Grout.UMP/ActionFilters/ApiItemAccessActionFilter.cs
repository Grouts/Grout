using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Item;
using System;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Script.Serialization;

namespace Grout.UMP.ActionFilters
{
    public class ApiItemAccessActionFilter : ActionFilterAttribute
    {
        private readonly JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        private readonly Cryptography cryptography = new Cryptography();

        public ItemType ItemType { get; set; }

        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            var itemRequest = (ItemRequest)filterContext.ActionArguments["itemRequest"];

            var itemManagement = new ItemManagement(GlobalAppSettings.QueryBuilder, GlobalAppSettings.DataProvider);

            var itemDetails = new ItemDetail();
            if (itemRequest.ServerPath != null)
            {
                itemRequest.ItemId = itemManagement.GetItemDetailsFromItemPath(itemRequest.ServerPath).Id;
            }
            if (itemRequest.ItemId == Guid.Empty && (itemRequest.ItemType == ItemType.Datasource || itemRequest.ItemType == ItemType.File))
            {
                itemDetails = itemManagement.GetItemDetailsFromItemName(itemRequest.Name, itemRequest.ItemType);
                if (itemDetails != null)
                {
                    itemRequest.ItemId = itemDetails.Id;
                }
                else
                {
                    var apiResponse = new ItemResponse
                    {
                        Status = false,
                        StatusMessage = "Invalid request values"
                    };
                    var response = new HttpResponseMessage
                    {
                        Content = new StringContent(javaScriptSerializer.Serialize(apiResponse))
                    };
                    filterContext.Response = response;
                }
            }

            filterContext.ActionArguments["itemRequest"] = itemRequest;
            var userId = new UserManagement(GlobalAppSettings.QueryBuilder, GlobalAppSettings.DataProvider).GetUserId(itemRequest.UserName);
            var itemsList =
                itemManagement.GetItems(
                    new UserManagement(GlobalAppSettings.QueryBuilder, GlobalAppSettings.DataProvider).GetUserId(
                        itemRequest.UserName), ItemType, null, null, null, null, null, itemRequest.ItemId);

            HttpContext.Current.Session["UserId"] = userId;


            if (itemsList.result.Any(a => a.Id == itemRequest.ItemId && a.CanRead) == false)
            {
                var apiResponse = new ItemResponse
                {
                    Status = false,
                    StatusMessage = "You do not have permission to access this item or the item does not exist."
                };
                var response = new HttpResponseMessage
                {
                    Content = new StringContent(javaScriptSerializer.Serialize(apiResponse))
                };
                filterContext.Response = response;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}