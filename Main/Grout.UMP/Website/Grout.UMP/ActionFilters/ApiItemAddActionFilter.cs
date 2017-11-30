using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Script.Serialization;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Item;

namespace Grout.UMP.ActionFilters
{
    public class ApiItemAddActionFilter : ActionFilterAttribute
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

            if (String.IsNullOrWhiteSpace(itemRequest.ServerPath) == false)
            {
                var splitPath = itemRequest.ServerPath.Split('/');
                var itemDetail = itemManagement.GetItemDetailsFromItemName(splitPath[1]);
                itemRequest.CategoryId = itemDetail.Id;
            }

            filterContext.ActionArguments["itemRequest"] = itemRequest;

            var userId = userManagement.GetUserId(itemRequest.UserName);

            var permissionAccess = itemManagement.GetItemTypesWithCreateAccess(userId);

            if (permissionAccess[ItemType] == false)
            {
                var apiResponse = new ItemResponse
                {
                    Status = false,
                    StatusMessage = "Permission denied to add item - " + ItemType.ToString()
                };
                var response = new HttpResponseMessage
                {
                    Content = new StringContent(javaScriptSerializer.Serialize(apiResponse))
                };
                filterContext.Response = response;
            }
            else
            {
                if (String.IsNullOrEmpty(itemRequest.Name))
                {
                    var apiResponse = new ItemResponse
                    {
                        Status = false,
                        StatusMessage = "Item name should not be empty"
                    };
                    var response = new HttpResponseMessage
                    {
                        Content = new StringContent(javaScriptSerializer.Serialize(apiResponse))
                    };
                    filterContext.Response = response;
                }

                if (itemManagement.IsItemNameAlreadyExists(itemRequest.Name, itemRequest.CategoryId))
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