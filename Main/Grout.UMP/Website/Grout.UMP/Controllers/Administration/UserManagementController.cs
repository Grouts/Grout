using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Grout.UMP.Models;
using Grout.Base;
using Grout.Base.DataClasses;
using System.Globalization;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using Grout.UMP.ActionFilters;
using Grout.Base.Encryption;
using System.Text;

namespace Grout.UMP.Controllers
{
    [Authorize(Order = 1)]
    [AdminActionFilter(Order = 2)]
    public class UserManagementController : Controller
    {
        private readonly JavaScriptSerializer javascriptserializer = new JavaScriptSerializer();
        private readonly UserManagement userDetails = new UserManagement();
        private readonly GroupManagement groupManagement = new GroupManagement();
        private readonly UserModel userProfile = new UserModel();
        private readonly ApiHandler _apiHandler = new ApiHandler();

        public ActionResult Index()
        {
            ViewBag.Groups = groupManagement.GetAllActiveGroups();
            ViewBag.LoggedUser = userDetails.GetUserName(Convert.ToInt16(HttpContext.User.Identity.Name));
            return View();
        }

        [HttpPost]
        public JsonResult GetAllUserList(int? skip, int? take, string searchKey, List<SortCollection> sorted, List<FilterCollection> filterCollection)
        {
            return Json(UserManagementModel.GetAllUserList(skip, take, searchKey, sorted, filterCollection));
        }

        public ActionResult UserManagement()
        {
            try
            {
                if (String.IsNullOrWhiteSpace(Request["username"]))
                    throw new HttpException(404, "Page Not Found");

                var userId = userDetails.GetUserId(Request["username"]);
                
                if (userId == 0)
                    throw new HttpException(404, "Page Not Found");
                ViewBag.UserDetails = userDetails.FindUserByUserId(userId);
                ViewBag.CookiesName = HttpContext.User.Identity.Name;
                ViewBag.LoggedUser = userDetails.GetUserName(Convert.ToInt16(HttpContext.User.Identity.Name));

                return View();
            }
            catch (HttpException e)
            {
                throw new HttpException(e.GetHttpCode(), e.Message);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        
        public ActionResult UploadFileFormAction()
        {
            var result = "";
            var userList = new List<Dictionary<string, string>>();
            var uploadedCsvPath= "";

            var httpPostedFileBase = Request.Files["csvfile"];

            if (httpPostedFileBase != null && httpPostedFileBase.ContentLength > 0)
            {
                var fileName = httpPostedFileBase.FileName.Split('\\').Last();
                var extension = Path.GetExtension(fileName);
                var uploadedFilesPath = GlobalAppSettings.GetUploadedFilesPath();

                if (Directory.Exists(uploadedFilesPath) == false)
                {
                    Directory.CreateDirectory(uploadedFilesPath);
                }

                if (extension == ".csv")
                {
                    uploadedCsvPath = String.Format("{0}\\{1}", uploadedFilesPath, fileName);

                    if (System.IO.File.Exists(uploadedCsvPath))
                    {
                        System.IO.File.Delete(uploadedCsvPath);
                    }

                    httpPostedFileBase.SaveAs(uploadedCsvPath);
                    userList = new UserManagementModel().SaveuserBulkUpload(uploadedCsvPath);
                    result = "Success";
                }
                else
                    result = "Error";
            }

            ViewBag.Pathname = uploadedCsvPath;
            ViewBag.UsersList = Json(new { Data = userList });
            ViewBag.UserExists = httpPostedFileBase != null && userList.Count == 0;
            ViewBag.ser = GlobalAppSettings.Serializer.Serialize(ViewBag.UsersList);
            ViewBag.UserCount = userList.Count;
            ViewBag.result = result;

            return View();
        }

        [Authorize]
        public JsonResult SaveSelectedCsvUser()
        {
            var userNames = Request["userNames"];
            var emailIds = Request["emailIds"];
            var allUsersList = new UserManagementModel().SaveuserBulkUpload(Request["AllUSerList"]);
            return Json(new { Data = new UserManagementModel().SubmitUsersBulkUpload(userNames, emailIds, allUsersList) });
        }

        [Authorize]
        public bool IsPresentusername(string userName)
        {
            var activeUser = userDetails.GetAllActiveInactiveUsers();
            return activeUser.Find(f => f.UserName.ToLower() == userName) != null;
        }

        [Authorize]
        public bool IsPresentEmailId(string emailId)
        {
            return new UserManagement().IsExistingEmail(emailId);
        }

        public JsonResult UpdatePassword()
        {
            var crypto = new TokenCryptography();

            try
            {
                var userId = Request["userId"];
                var serializer = new JavaScriptSerializer();
                var headers = new Dictionary<string, object>();
                var newPassword = HttpContext.Request["newpassword"];
                var confirmPassword = HttpContext.Request["confirmpassword"];

                var ipAddress = Request.UserHostAddress;
                var encryptedPassword = crypto.Encrypt(newPassword, ipAddress);
                headers.Add("newpassword", Convert.ToBase64String(Encoding.UTF8.GetBytes(encryptedPassword)));
                var responseObj =
                    serializer.Deserialize<ApiResponse>(_apiHandler.ApiProcessor("/api/accounts/changepassword",
                        headers,
                        new Dictionary<string, object>
                        {
                            {"userId", userId}
                        }));

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
            int userId = this.userDetails.GetUserId(HttpContext.Request["username"]);
            var currentUserDetails = this.userDetails.FindUserByUserId(userId);
            var currentEmail = currentUserDetails.Email;
            var timeNow = DateTime.UtcNow;
            if (currentEmail != HttpContext.Request["email"])
            {
                var emailList = this.userDetails.GetAllActiveEmails();

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
                    this.userDetails.UpdateUserProfileDetails(updateColumns, userId);
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
                    this.userDetails.UpdateUserProfileDetails(updateColumns, userId);
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
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.ModifiedDate,
                        Value = timeNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    },
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.IsActive,
                        Value = HttpContext.Request["status"]
                    }
                };
                this.userDetails.UpdateUserProfileDetails(updateDetails, userId);
            }
            catch (Exception)
            {
                var result = new { status = false, key = "error", value = "User profile updation has been failed" };
                return Json(new { Data = result });
            }
            var formattedString = GetFormattedTimeZone(timeNow, userId);
            var finalResult = new { status = true, key = "success", value = "User profile has been updated successfully." };
            return Json(new { Data = finalResult, formattedString });
        }

        public JsonResult PostAction()
        {
            var output = new Dictionary<string, string>();
            JsonResult jsonOutput;

            try
            {
                var userobj = new User
                {
                    FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Request["firstname"]),
                    LastName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Request["lastname"]),
                    UserName = Request["username"].ToLower(),
                    Email = Request["emailid"].ToLower()
                };
                userobj.DisplayName = (userobj.FirstName.Trim() + " " + userobj.LastName.Trim()).Trim();
                userobj.Password = "na";                
                userobj.CreatedDate = DateTime.UtcNow;
                userobj.ModifiedDate = DateTime.UtcNow;
                userobj.TimeZone = GlobalAppSettings.SystemSettings.TimeZone;
                userobj.IsActivated = false;
                userobj.IsActive = false;
                userobj.IsDeleted = false;
                var activationCode = String.Empty;
                var activationExpirationDate = new DateTime();
                var result = userDetails.AddUser(userobj, out activationExpirationDate, out activationCode);
                if (result.Status)
                {
                    var activationUrl = GlobalAppSettings.SystemSettings.BaseUrl +
                                        "/accounts/activate?ActivationCode=" +
                                        activationCode;
                    const bool isResendActivationCode = false;
                    UserManagementModel.SendActivationCode(userobj.FirstName, userobj.UserName, userobj.Email,
                        activationUrl, activationExpirationDate, isResendActivationCode);

                    output.Add("result", "success");
                    jsonOutput = Json(new { Data = output });
                }
                else
                {
                    output.Add("result", "error");
                    jsonOutput = Json(new { Data = output });
                }
            }
            catch (Exception)
            {
                output.Add("result", "error");
                jsonOutput = Json(new { Data = output });
            }
            return jsonOutput;
        }

        [HttpPost]
        public JsonResult ResendActivationCode()
        {
            var userId = userDetails.GetUserId(Request["username"]);
            var output = new Dictionary<string, string>();
            JsonResult jsonOutput;

            try
            {
                var userobj = new User
                {
                    FirstName = Request["firstname"],
                    UserName = Request["username"],
                    Email = Request["email"],
                    ActivationCode = userDetails.GenerateRandomCode(12),
                    ActivationExpirationDate =
                        DateTime.UtcNow.AddDays(GlobalAppSettings.SystemSettings.ActivationExpirationDays)
                };
                const bool isResendActivationCode = true;

                var activationUrl = GlobalAppSettings.SystemSettings.BaseUrl + "/accounts/activate?ActivationCode=" +
                                    userobj.ActivationCode;
                var updateColumns = new List<UpdateColumn>
                {
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.ModifiedDate,
                        Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    },
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.ActivationCode,
                        Value = userobj.ActivationCode
                    },
                    new UpdateColumn 
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.ActivationExpirationDate,
                        Value = userobj.ActivationExpirationDate.ToString(GlobalAppSettings.GetDateTimeFormat())
                    }
                };

                var result = userDetails.UpdateUserProfileDetails(updateColumns, userId);
                if (result)
                {
                    UserManagementModel.SendActivationCode(userobj.FirstName, userobj.UserName, userobj.Email,
                        activationUrl, userobj.ActivationExpirationDate, isResendActivationCode);
                    output.Add("result", "success");
                    jsonOutput = Json(new { Data = output });
                }
                else
                {
                    output.Add("result", "error");
                    jsonOutput = Json(new { Data = output });
                }
            }
            catch (Exception)
            {
                output.Add("result", "error");
                jsonOutput = Json(new { Data = output });
            }

            return jsonOutput;
        }

        [HttpPost]
        public JsonResult ActivateUser()
        {
            var userId = userDetails.GetUserId(Request["username"]);
            var output = new Dictionary<string, string>();
            JsonResult jsonOutput;

            try
            {
                var userobj = new User
                {
                    FirstName = Request["firstname"],
                    UserName = Request["username"],
                    Email = Request["email"],
                    ActivationCode = userDetails.GenerateRandomCode(12),
                    ActivationExpirationDate =
                        DateTime.UtcNow.AddDays(GlobalAppSettings.SystemSettings.ActivationExpirationDays)
                };
                const bool isResendActivationCode = false;
                var activationUrl = GlobalAppSettings.SystemSettings.BaseUrl + "/accounts/activate?ActivationCode=" +
                                    userobj.ActivationCode;

                var updateColumns = new List<UpdateColumn>
                {
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.ModifiedDate,
                        Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    },
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.ActivationCode,
                        Value = userobj.ActivationCode
                    },
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.ActivationExpirationDate,
                        Value = userobj.ActivationExpirationDate.ToString(GlobalAppSettings.GetDateTimeFormat())
                    }
                };

                var result = userDetails.UpdateUserProfileDetails(updateColumns, userId);
                if (result)
                {
                    UserManagementModel.SendActivationCode(userobj.FirstName, userobj.UserName, userobj.Email,
                        activationUrl, userobj.ActivationExpirationDate, isResendActivationCode);
                    output.Add("result", "success");
                    jsonOutput = Json(new { Data = output });
                }
                else
                {
                    output.Add("result", "error");
                    jsonOutput = Json(new { Data = output });
                }
            }
            catch (Exception)
            {
                output.Add("result", "error");
                jsonOutput = Json(new { Data = output });
            }

            return jsonOutput;
        }

        public JsonResult ImageCollection(string username)
        {
            var html = userProfile.GetallimagesofParticularUser(username);
            return Json(new { Data = html });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult UpdateProfilePicture(ProfilePicture selection)
        {
            var userId = userDetails.GetUserId(selection.UserName);
            DateTime timeNow = DateTime.UtcNow;
            var formattedString = GetFormattedTimeZone(timeNow, userId);
            var imageName = userProfile.UpdateUserAvatarDetails(selection, timeNow);
            return Json(new { imageName, formattedString });
        }

        public JsonResult TransferImage(string path, string username)
        {
            var userId = userDetails.GetUserId(username);
            DateTime timeNow = DateTime.UtcNow;
            var formattedString = GetFormattedTimeZone(timeNow, userId);
            var imageName = userProfile.DefaultavatarTransfer(path, username, timeNow);
            return Json(new { imageName, formattedString });
        }

        public string GetFormattedTimeZone(DateTime timeNow, int userId)
        {
            var timeZone = new UserManagement().GetUserPreferTimeZone(userId);
            DateTime date = GlobalAppSettings.GetCovertedTimeZone(timeNow, timeZone);
            var formattedString = date.ToString(GlobalAppSettings.SystemSettings.DateFormat + " hh:mm:ss tt");
            return formattedString;
        }

        public ActionResult Avatar(string username, double imageSize)
        {
            var avatarName = userDetails.FindUserByUserName(username).Avatar;

            if (String.IsNullOrWhiteSpace(avatarName))
            {
                return new UserController().GetDefaultAvatar();
            }

            var image = GlobalAppSettings.GetProfilePicturesPath() + username + "/" + imageSize + "/" + avatarName;
            
            return File(image, "image/png");
        }

        public bool DeleteFromUserList()
        {
            var users = Request["Users"].Split(',').ToList();
            for (var t = 0; t < users.Count(); t++)
            {
                var userid = new UserManagement().GetUserId(users[t]);
                userDetails.DeleteUser(userid);
            }
            return true;
        }

        [Authorize]
        public bool DeleteSingleFromUserList()
        {
            int userId = Convert.ToInt32(Request["UserId"]);
            return userDetails.DeleteUser(userId);
        }

        public JsonResult SaveUserIntoGroup()
        {
            if (groupManagement.IsGroupAlreadyExist(Request["GroupName"]).Value.ToString() == "True")
            {
                var result = new { status = false, value = "Group name already exists" };
                return Json(new { Data = result });
            }

            try
            {
                var group = new Group
                {
                    GroupName = Request["GroupName"],
                    GroupDescription = Request["GroupDescription"],
                    GroupColor = Request["GroupColor"],
                };

                var groupUsers = Request["GroupUsers"].Split(',').ToList();

                var groupId = groupManagement.AddGroup(group);

                foreach (var user in groupUsers)
                {
                    var userId = Convert.ToInt32(user);
                    groupManagement.AddUserInGroup(userId, Convert.ToInt32(groupId));
                }
            }
            catch
            {
                var result = new { status = false, value = "Group creation has been failed." };
                return Json(new { Data = result });
            }
            var finalResult = new { status = true, value = "Group has been created successfully." };
            return Json(new { Data = finalResult });
        }

        public string UpdateUserIntoGroup()
        {
            var data = new List<string> { Request["GroupUsers"], Request["GroupId"] };
            var addGroup = new UserManagementModel().AddUserinGroup(data);
            return addGroup;
        }

        public FileResult DownloadTemplate()
        {
            return new FilePathResult("~/Content/Documents/Template.csv", "text/csv");
        }
    }
}