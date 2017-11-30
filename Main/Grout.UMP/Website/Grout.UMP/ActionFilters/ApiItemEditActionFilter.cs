using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Script.Serialization;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Item;

namespace Grout.UMP.ActionFilters
{
    public class ApiItemEditActionFilter : ActionFilterAttribute
    {
        private readonly JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        private readonly Cryptography cryptography = new Cryptography();
        private readonly ItemManagement itemManagement = new ItemManagement(GlobalAppSettings.QueryBuilder,
            GlobalAppSettings.DataProvider);
        private readonly UserManagement userManagement = new UserManagement(GlobalAppSettings.QueryBuilder,
            GlobalAppSettings.DataProvider);
        public ItemType ItemType { get; set; }

        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            var itemRequest = (ItemRequest)filterContext.ActionArguments["itemRequest"];
            var itemDetail = itemManagement.GetItemDetailsFromItemPath(itemRequest.ServerPath);
            if (itemDetail == null)
            {
                var apiResponse = new ItemResponse
                {
                    Status = false,
                    StatusMessage = "You do not have permission to edit this item or the item does not exist."
                };
                var response = new HttpResponseMessage
                {
                    Content = new StringContent(javaScriptSerializer.Serialize(apiResponse))
                };
                filterContext.Response = response;
            }
            itemRequest.ItemId = itemDetail.Id;
            itemRequest.CategoryId = itemDetail.CategoryId;
            var itemId = itemRequest.ItemId;
            filterContext.ActionArguments["itemRequest"] = itemRequest;
            var userId = userManagement.GetUserId(itemRequest.UserName);

            var itemsList = itemManagement.GetItems(userId, ItemType, null, null, null, null, null, itemId);

            if (itemsList.result.Any(a => a.Id == itemId && a.CanWrite) == false)
            {
                var apiResponse = new ItemResponse
                {
                    Status = false,
                    StatusMessage = "You do not have permission to edit this item or the item does not exist."
                };
                var response = new HttpResponseMessage
                {
                    Content = new StringContent(javaScriptSerializer.Serialize(apiResponse))
                };
                filterContext.Response = response;
            }
            else
            {
                if (itemManagement.IsItemNameAlreadyExistsForUpdate(itemRequest.Name, itemRequest.CategoryId, itemId))
                {
                    var apiResponse = new ItemResponse
                    {
                        Status = false,
                        StatusMessage = "Item with the same name is already exist in the specified Category"
                    };
                    var response = new HttpResponseMessage
                    {
                        Content = new StringContent(javaScriptSerializer.Serialize(apiResponse))
                    };
                    filterContext.Response = response;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}