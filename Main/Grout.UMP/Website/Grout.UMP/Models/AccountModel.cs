using System;
using System.Web;
using Grout.Base;

namespace Grout.UMP.Models
{
    public class AccountModel
    {
        public static void UpdateSession()
        {
            var userDetail =
                new UserManagement().FindUserByUserId(Convert.ToInt32(HttpContext.Current.User.Identity.Name));

            HttpContext.Current.Session["displayname"] = userDetail.DisplayName;
            HttpContext.Current.Session["firstname"] = userDetail.FirstName;
            HttpContext.Current.Session["lastname"] = userDetail.LastName;
            HttpContext.Current.Session["IsAdmin"] = GlobalAppSettings.IsAdmin(userDetail.UserId);
        }
    }
}