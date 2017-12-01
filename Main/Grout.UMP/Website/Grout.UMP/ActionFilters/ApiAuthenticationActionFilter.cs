using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Script.Serialization;
using Grout.Base.DataClasses;
using System;
using Grout.Base;
using Grout.UMP.Models;

namespace Grout.UMP.ActionFilters
{
    public class ApiAuthenticationActionFilter : ActionFilterAttribute
    {
        //private readonly Designer designer = new Designer();
        private readonly JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
        private readonly Cryptography cryptography = new Cryptography();

        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            //var itemRequest = (ItemRequest)filterContext.ActionArguments["itemRequest"];

            //if (String.IsNullOrEmpty(itemRequest.UserName))
            //{
            //    var apiResponse = new ItemResponse
            //    {
            //        Status = false,
            //        StatusMessage = "Username should not be empty"
            //    };
            //    var response = new HttpResponseMessage
            //    {
            //        Content = new StringContent(javaScriptSerializer.Serialize(apiResponse))
            //    };
            //    filterContext.Response = response;
            //}

            //if (String.IsNullOrEmpty(itemRequest.Password))
            //{
            //    var apiResponse = new ItemResponse
            //    {
            //        Status = false,
            //        StatusMessage = "Password should not be empty"
            //    };
            //    var response = new HttpResponseMessage
            //    {
            //        Content = new StringContent(javaScriptSerializer.Serialize(apiResponse))
            //    };
            //    filterContext.Response = response;
            //}

            //var loginResponse = designer.Login(itemRequest.UserName, itemRequest.Password);

            //var status = loginResponse.ApiStatus;
            //var apiData = loginResponse.Data as ApiData;

            //if (!status)
            //{
            //    var apiResponse = new ItemResponse
            //    {
            //        Status = false,
            //        StatusMessage = "Internal server error"
            //    };
            //    var response = new HttpResponseMessage
            //    {
            //        Content = new StringContent(javaScriptSerializer.Serialize(apiResponse))
            //    };
            //    filterContext.Response = response;
            //}
            //else if (!apiData.Success)
            //{
            //    var apiResponse = new ItemResponse
            //    {
            //        Status = false,
            //        StatusMessage = apiData.Message
            //    };
            //    var response = new HttpResponseMessage
            //    {
            //        Content = new StringContent(javaScriptSerializer.Serialize(apiResponse))
            //    };
            //    filterContext.Response = response;
            //}

            base.OnActionExecuting(filterContext);
        }
    }
}