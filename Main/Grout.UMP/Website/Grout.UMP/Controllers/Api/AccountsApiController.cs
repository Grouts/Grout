using System;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Grout.Base.DataClasses;
using Grout.Base.Logger;
using Grout.UMP.Models;

namespace Grout.UMP.Controllers
{
    public class AccountsApiController : ApiController
    {
        private readonly Accounts accounts = new Accounts();
        private readonly Designer designerApi=new Designer();

        /// <summary>
        /// Api for reprot designer
        /// </summary>
        /// <param name="itemRequest">Itemrequest from report designer contains username,password</param>
        /// <returns>Boolean</returns>
        public bool Login(ItemRequest itemRequest)
        {
            var apiResponse = designerApi.Login(itemRequest.UserName, itemRequest.Password);
            var apiData = (ApiData) apiResponse.Data;
            return apiData.Success;
        }

        /// <summary>
        /// Api for report designer to identify whether request from UMP server or SSRS server
        /// </summary>
        /// <returns>Boolean</returns>
        public bool IsValid()
        {
            return true;
        }

        [HttpGet]
        public ApiResponse Login()
        {
            LogExtension.LogInfo("API Request connected with api controller", MethodBase.GetCurrentMethod());
            var authorization = HttpContext.Current.Request.Headers["Authorization"];
            var requestMode = HttpContext.Current.Request.Headers["RequestMode"];
            var loginResponse = accounts.Login(authorization, HttpContext.Current.Request.UserHostAddress, requestMode);
            return loginResponse;
        }

        [HttpGet]        
        public ApiResponse ChangePassword()
        {
            var userId = Convert.ToInt32(HttpContext.Current.Request.QueryString["userId"]);
            var newPassword = HttpContext.Current.Request.Headers["newpassword"];
            var changePasswordResponse = accounts.ChangePassword(newPassword, userId);
            return changePasswordResponse;
        }
    }
}
