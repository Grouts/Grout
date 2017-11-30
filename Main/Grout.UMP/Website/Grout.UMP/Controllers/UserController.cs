using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Encryption;
using Grout.UMP.Models;

namespace Grout.UMP.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManagement _userDetails = new UserManagement();
        private readonly ApiHandler _apiHandler = new ApiHandler();
        private readonly UserModel _userProfile = new UserModel();
        private readonly JavaScriptSerializer _javascriptserializer = new JavaScriptSerializer();

        public ActionResult EditProfile()
        {
            var userDetail = _userDetails.FindUserByUserId(Convert.ToInt32(HttpContext.User.Identity.Name));
            ViewBag.ModifiedDate = userDetail.ModifiedDate;
            ViewBag.ProfileDetail = _javascriptserializer.Serialize(userDetail);
            return View();
        }

        public ActionResult EditPassword()
        {
            try
            {
                ViewBag.ProfileDetail = _userProfile.SpecificUserDetails(Convert.ToInt32(HttpContext.User.Identity.Name));
            }
            catch (HttpException e)
            {
                throw new HttpException(e.GetHttpCode(), e.Message);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            
            return View();
        }

        public ActionResult Profile()
        {
            var userDetail = _userDetails.FindUserByUserId(Convert.ToInt32(HttpContext.User.Identity.Name));
            var userGroups = _userDetails.GetAllGroupsOfUser(Convert.ToInt32(HttpContext.User.Identity.Name));
            var groupString = string.Empty;
            for (var group = 0; group < userGroups.Count; group++)
            {
                if (group != 0)
                {
                    groupString += ", ";
                }
                groupString += userGroups[group].Name;
            }
            ViewBag.GroupList = groupString;
            return View(userDetail);
        }

        public JsonResult UpdatePassword()
        {
            var crypto = new TokenCryptography();
            var userDetails = new UserManagement();

            try
            {
                var serializer = new JavaScriptSerializer();
                var headers = new Dictionary<string, object>();
                var oldPassword = HttpContext.Request["oldPassword"];
                var newPassword = HttpContext.Request["newPassword"];
                var confirmPassword = HttpContext.Request["confirmPassword"];

                if (oldPassword ==
                    userDetails.GetUserPassword(Convert.ToInt32(HttpContext.User.Identity.Name)))
                {

                    var ipAddress = Request.UserHostAddress;
                    var encryptedPassword = crypto.Encrypt(newPassword, ipAddress);
                    headers.Add("newpassword", Convert.ToBase64String(Encoding.UTF8.GetBytes(encryptedPassword)));
                    var responseObj =
                        serializer.Deserialize<ApiResponse>(_apiHandler.ApiProcessor("/api/accounts/changepassword",
                            headers,
                            new Dictionary<string, object>
                            {
                                {"userId", HttpContext.User.Identity.Name}
                            }));
                }
                else
                {
                    var result = new { status = false, key = "password", value = "Please enter correct old password." };
                    return Json(new { Data = result });
                }
            }
            catch (Exception)
            {
                var result = new { status = false, key = "error", value = "Password updation has been failed." };
                return Json(new { Data = result });
            }

            var finalResult = new { status = true, key = "success", value = "Password has been updated successfully." };
            return Json(new { Data = finalResult });
        }

        public JsonResult UpdateUserProfile()
        {
            int userId = _userDetails.GetUserId(HttpContext.Request["username"]);
            var userDetails = new UserManagement();
            var currentUserDetails = _userDetails.FindUserByUserId(userId);
            var currentEmail = currentUserDetails.Email;
            var timeNow = DateTime.UtcNow;
            if (currentEmail != HttpContext.Request["email"])
            {
                var emailList = _userDetails.GetAllActiveEmails();

                var isEmailExist = emailList.Find(f => f.Email == HttpContext.Request["email"]) == null;

                if (isEmailExist)
                {
                    var updateColumns = new List<UpdateColumn>
                    {
                        new UpdateColumn
                        {
                            ColumnName = GlobalAppSettings.DbColumns.DB_User.Email,
                            Value = HttpContext.Request["email"]
                        }
                    };
                    this._userDetails.UpdateUserProfileDetails(updateColumns, userId);
                }
                else
                {
                    var result = new { status = false, key = "email", value = "E-mail already exists" };
                    return Json(new { Data = result });
                }
            }
            try
            {
                var fullName = currentUserDetails.FirstName;

                if (fullName != HttpContext.Request["firstname"])
                {
                    var updateColumns = new List<UpdateColumn>
                    {
                        new UpdateColumn
                        {
                            ColumnName = GlobalAppSettings.DbColumns.DB_User.FirstName,
                            Value = HttpContext.Request["fullname"]
                        }
                    };
                    _userDetails.UpdateUserProfileDetails(updateColumns, userId);
                }
                var updateDetails = new List<UpdateColumn>
                {
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.FirstName,
                        Value = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(HttpContext.Request["firstname"])
                    },
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.LastName,
                        Value = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(HttpContext.Request["lastname"])
                    },
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.Contact,
                        Value = HttpContext.Request["mobile"]
                    },
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.DisplayName,
                        Value =
                            CultureInfo.CurrentCulture.TextInfo.ToTitleCase(HttpContext.Request["firstname"]) + " " +
                            CultureInfo.CurrentCulture.TextInfo.ToTitleCase(HttpContext.Request["lastname"])
                    },
                    new UpdateColumn {ColumnName = GlobalAppSettings.DbColumns.DB_User.ModifiedDate, Value = timeNow.ToString(GlobalAppSettings.GetDateTimeFormat())}
                };
                _userDetails.UpdateUserProfileDetails(updateDetails, userId);
                HttpContext.Session["displayname"] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(HttpContext.Request["firstname"]) + " " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(HttpContext.Request["lastname"]);
            }
            catch (Exception)
            {
                var result = new { status = false, key = "error", value = "User profile updation failed." };
                return Json(new { Data = result });
            }
            var formattedString = GlobalAppSettings.GetFormattedTimeZone(timeNow, userId);
            var finalResult = new { status = true, key = "success", value = "User Profile has been updated successfully." };
            return Json(new { Data = finalResult, formattedString });
        }

        public JsonResult ImageCollection(string username)
        {
            var html = _userProfile.GetallimagesofParticularUser(username);
            return Json(new { Data = html });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult UpdateProfilePicture(ProfilePicture selection)
        {
            var timeNow = DateTime.UtcNow;
            var formattedString = GlobalAppSettings.GetFormattedTimeZone(timeNow, selection.UserId);
            var imageName = _userProfile.UpdateUserAvatarDetails(selection, timeNow);
            return Json(new { imageName, formattedString });
        }

        public JsonResult DeleteAvatar()
        {
            var userId = _userDetails.GetUserId(Request["userName"]);
            return Json(new { status = _userProfile.DeleteAvatar(userId) });
        }

        public JsonResult TransferImage(string path, string username)
        {
            var userId = _userDetails.GetUserId(username);
            DateTime timeNow = DateTime.UtcNow;
            var formattedString = GlobalAppSettings.GetFormattedTimeZone(timeNow, userId);
            var imageName = _userProfile.DefaultavatarTransfer(path, username, timeNow);
            return Json(new { imageName, formattedString });
        }

        public ActionResult Avatar(string username, double imageSize)
        {
            var image = String.Empty;
            try
            {
                var user = _userDetails.FindUserByUserId(int.Parse(username));

                if (String.IsNullOrWhiteSpace(user.Avatar))
                {
                    return GetDefaultAvatar();
                }

                image = GlobalAppSettings.GetProfilePicturesPath() + user.UserName + "//" + imageSize + "//" + user.Avatar;
            }
            catch
            {
                var avatarName = _userDetails.FindUserByUserName(username).Avatar;

                if (String.IsNullOrWhiteSpace(avatarName))
                {
                    return GetDefaultAvatar();
                }

                image = GlobalAppSettings.GetProfilePicturesPath() + username + "//" + imageSize + "//" + avatarName;
            }

            return File(image, "image/png");
        }

        public ActionResult GetDefaultAvatar()
        {
            return File(AppDomain.CurrentDomain.BaseDirectory + "//content//images//ProfilePictures//default.png",
                "image/png");
        }
    }
}