using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Encryption;
using Grout.Base.Logger;
using Grout.UMP.Models;

namespace Grout.UMP.Controllers
{
    public class AccountsController : Controller
    {
        private readonly JavaScriptSerializer _javaScriptSerializer = new JavaScriptSerializer();
        private readonly ApiHandler _apiHandler = new ApiHandler();
        private readonly IUserManagement _userManagement = new UserManagement();

        public ActionResult Login()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                GlobalAppSettings.SetTimeZone();
                return new RedirectResult("/reports");
            }

            var settings = new SystemSettingsSerializer().Deserialize(GlobalAppSettings.GetConfigFilepath() + ServerSetup.Configuration);

            if (settings == null)
            {
                return Redirect("/startup");
            }

            TempData["password"] = "none";
            TempData["username"] = "none";
            ViewBag.ReturnURL = Request["returnUrl"] ?? (HttpContext.Request.Cookies["mobile_cookie"] != null ? HttpContext.Request.Cookies["mobile_cookie"].Value : "");

            return View();
        }

        [HttpPost]
        public string SavePasswordActivate()
        {
            if (Request["Password"].Length > 4)
            {
                var encrypt = new Cryptography();
                var userId = Request["UserId"];
                var password = Convert.ToBase64String(encrypt.Encryption(Request["Password"]));
                return _userManagement.ActivateUser(Convert.ToInt32(userId), password).ToString();
            }
            return "Failure";
        }

        public ActionResult AccountActivation(string activationCode)
        {
            Session.Abandon();
            FormsAuthentication.SignOut();
            try
            {
                ViewBag.AccountPresentDetails = false;
                var userDetails = _userManagement.FindUserByActivationCode(activationCode);


                if (userDetails == null || userDetails.IsDeleted)
                    return View();

                if (activationCode != null && activationCode != "default")
                {
                    if (DateTime.UtcNow.Date <= userDetails.ActivationExpirationDate.Date)
                    {
                        ViewBag.AccountPresentDetails = true;
                        ViewBag.Userid = userDetails.UserId;
                    }
                }
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

        [HttpPost]
        public ActionResult Login(string username, string password, string remember, string returnUrl)
        {
            var tokenCryptography = new TokenCryptography();

            if (username != null && password != null)
            {
                var ipAddress = Request.UserHostAddress;
                var encryptedUsername = tokenCryptography.Encrypt(username, ipAddress);
                var encryptedPassword = tokenCryptography.Encrypt(password, ipAddress);
                LogExtension.LogInfo("Login Encryption done", MethodBase.GetCurrentMethod(), " UserName - " + username + " Password - " + password + " Remember - " + remember + " ReturnUrl - " + returnUrl);

                var headers = new Dictionary<string, object>
                {
                    {
                        "Authorization",
                        Convert.ToBase64String(Encoding.UTF8.GetBytes(encryptedUsername + ":" + encryptedPassword))
                    }
                };
                LogExtension.LogInfo("Login API requested", MethodBase.GetCurrentMethod());
                var apiResponse = _javaScriptSerializer.Deserialize<ApiResponse>(_apiHandler.ApiProcessor("/api/accounts/login", headers,
                    new Dictionary<string, object>()));

                var apiData = DictionaryHelper.GetObject(apiResponse.Data as Dictionary<string, object>, typeof(ApiData)) as ApiData;

                LogExtension.LogInfo("Login API Data received", MethodBase.GetCurrentMethod(), " UserName - " + username + " Password - " + password + " Remember - " + remember + " ReturnUrl - " + returnUrl);
                LogExtension.LogInfo("Login result is " + apiData.StatusText, MethodBase.GetCurrentMethod(), " UserName - " + username + " Password - " + password + " Remember - " + remember + " ReturnUrl - " + returnUrl);

                var isValid = apiData.StatusText;

                switch (isValid.ToLower())
                {
                    case "validuser":
                        var userDetail = _userManagement.FindUserByUserName(username);

                        FormsAuthentication.SetAuthCookie(userDetail.UserId.ToString(), remember != null && remember.ToLower().Trim() == "on");                        

                        GlobalAppSettings.SetTimeZone(userDetail.UserId);

                        HttpContext.Session["displayname"] = userDetail.DisplayName;
                        HttpContext.Session["firstname"] = userDetail.FirstName;
                        HttpContext.Session["lastname"] = userDetail.LastName;
                        HttpContext.Session["IsAdmin"] = GlobalAppSettings.IsAdmin(userDetail.UserId);

                        _userManagement.UpdateLoginTime(userDetail.UserId, DateTime.UtcNow);

                        if (String.IsNullOrWhiteSpace(returnUrl))
                        {
                            return RedirectToAction("reports", "reports");
                        }

                        return Redirect(returnUrl);

                    case "invalidpassword":
                        TempData["currentValue"] = username;
                        TempData["errorUserName"] = "";
                        TempData["errorPassword"] = apiData.Message;
                        TempData["errorUserStatus"] = "";
                        TempData["errorPasswordStatus"] = "inline-block";
                        return View();

                    case "throttleduser":
                        TempData["errorUserName"] = "";
                        TempData["errorPassword"] = "";
                        TempData["User"] = apiData.Message;
                        return View();

                    case "invalidusername":
                        TempData["currentValue"] = username;
                        TempData["errorUserName"] = apiData.Message;
                        TempData["errorPassword"] = "";
                        TempData["errorUserStatus"] = "inline-block";
                        TempData["errorPasswordStatus"] = "";
                        return View();

                    case "deactivateduser":
                        TempData["errorUserName"] = "";
                        TempData["errorPassword"] = "";
                        TempData["errorUserStatus"] = "inline-block";
                        TempData["User"] = apiData.Message;
                        return View();

                    default:
                        TempData["errorUserName"] = "";
                        TempData["errorPassword"] = "";
                        TempData["errorUserStatus"] = "inline-block";
                        TempData["User"] = apiData.Message;
                        return View();
                }
            }
            return View();
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            FormsAuthentication.SignOut();
            return new RedirectResult("/accounts/login");
        }

        #region Forgot password

        /// <summary>
        /// View for forgot password action
        /// </summary>
        /// <returns></returns>
        public ActionResult ForgotPassword()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return View();
        }

        /// <summary>
        /// View page confirming password code sent successfully.
        /// </summary>
        /// <returns></returns>
        public ActionResult CodeConfirmation()
        {
            return View();
        }

        /// <summary>
        /// View file for change password action
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangePassword()
        {
            Session.Abandon();
            FormsAuthentication.SignOut();
            if (Request["userid"] != null && Request["recoverycode"] != null)
            {
                var userId = Convert.ToInt32(Request["userid"]);
                var userDetail = _userManagement.FindUserByUserId(userId);
                if (userDetail.ResetPasswordCode == Request["recoverycode"])
                {
                    return View();
                }
                TempData["message"] = "Invalid link";
                return Redirect("/accounts/forgot-password/code-confirmation?userid=" + userId);
            }
            TempData["errorMessage"] = "Invalid link";
            return Redirect("/accounts/forgot-password");
        }

        /// <summary>
        /// Get the emailid or username as key from user and validate it, if it doesn't exist in our database error message will be return to the user.
        /// If the key exist, the dynamic random reset password code will be generated and send to the user with the reset password link.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ForgotPassword(string key)
        {
            
           
            if (String.IsNullOrWhiteSpace(key))
               return View();

            var userDetail = _userManagement.IsValidEmail(key)
                ? _userManagement.FindUserByEmail(key)
                : _userManagement.FindUserByUserName(key);

            if (userDetail == null)
            {
                TempData["errorMessage"] = "Invalid username or email address";
                return View();
            }
            else
            {
              
                var result = _userManagement.SetResetPasswordForUser(userDetail.UserId);
                if (result.Success)
                {
                    var resetPasswordCode = result.Value.ToString();

                    var
                        emailBody =
                            "<html xmlns='http://www.w3.org/1999/xhtml' xmlns='http://www.w3.org/1999/xhtml' xmlns:v='urn:schemas-microsoft-com:vml' xmlns:o='urn:schemas-microsoft-com:office:office'><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8'><meta name='viewport' content='width=device-width'>	<!--[if gte mso 9]><xml> <o:OfficeDocumentSettings>  <o:AllowPNG/>  <o:PixelsPerInch>96</o:PixelsPerInch> </o:OfficeDocumentSettings></xml><![endif]--></head><body style='min-width: 100%;-webkit-text-size-adjust: 100%;-ms-text-size-adjust: 100%;margin: 0;padding: 0;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;text-align: left;line-height: 19px;font-size: 14px;background-color: rgb(244,244,244);padding-top: 10px;width: 100% !important;'><center style='width: 100%;min-width: 580px;'><table style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='email-container' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='email-header' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;background-color: #ef5350;padding-right: 10px;padding-left: 10px;padding-top: 10px;padding-bottom: 18px;border-top-right-radius: 5px;border-top-left-radius: 5px;border-collapse: collapse !important;'><table class='body container' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;height: 100%;width: 580px;margin: 0;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;line-height: 19px;font-size: 14px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='sync-inc wrapper' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 2px 10px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><h6 style='color: white;font-family: &quot;Segoe UI&quot;,sans-serif !important;font-weight: 500;padding: 0;margin: 0;text-align: left;line-height: 1.3;word-break: normal;font-size: 20px;font-style: normal;'><a href='"
                            + GlobalAppSettings.SystemSettings.BaseUrl + "/accounts/login' style='color: white;text-decoration: none;'>"
                            + GlobalAppSettings.SystemSettings.OrganizationName + "</a></h6></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='wrapper last' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 50px 0px 10px 273px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><img src='"
                            + GlobalAppSettings.SystemSettings.BaseUrl + "/Content/Images/Application/"
                            + GlobalAppSettings.SystemSettings.MainScreenLogo + "' style='outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;width: auto;max-width: 100%;float: none;clear: both;margin: 0 auto;'></td></tr><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='welcome wrapper last' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 17px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><h6 style='color: white;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: lighter;padding: 0;margin: 0;text-align: center;line-height: 1.3;word-break: normal;font-size: 20px;'>RESET YOUR LOST PASSWORD</h6></td></tr></tbody></table></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='message-body' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;background-color: white;padding-right: 10px;padding-left: 10px;border-bottom-right-radius: 5px;border-bottom-left-radius: 5px;border-collapse: collapse !important;'><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='activation-msg' style='padding-top: 20px;padding-left: 4px; padding-bottom: 7px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto; vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><p style='margin-bottom: 4px;margin: 0;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;'>Hello "
                            + userDetail.DisplayName + ",</p></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='activation-msg' style='padding-top: 5px;padding-left: 4px; padding-bottom: 18px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto; vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><p style='margin-bottom: 4px;margin: 0;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;'>We have received a request to reset the password for your grout account. Please click Reset to set a new password.</p></td></tr></tbody></table><table style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td style='padding-left: 4px;padding-bottom: 0px; padding-top: 0px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><table class='block-grid five-up' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;width: 100%;max-width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='activate-btn center' style='border-radius: 3px; padding: 0px;'><div><!--[if mso]><v:roundrect xmlns:v='urn:schemas-microsoft-com:vml' xmlns:w='urn:schemas-microsoft-com:office:word' href='"
                            + GlobalAppSettings.SystemSettings.BaseUrl + "/accounts/forgot-password/change-password?userid="
                            + userDetail.UserId + "&recoverycode="
                            + resetPasswordCode +"' style='height:28px;v-text-anchor:middle;width:100px;' arcsize='11%' stroke='f' fillcolor='#ef5350'><w:anchorlock/><center><![endif]--><a href='"
                            + GlobalAppSettings.SystemSettings.BaseUrl + "/accounts/forgot-password/change-password?userid="
                            + userDetail.UserId + "&recoverycode="
                            + resetPasswordCode + "'style='background-color:#ef5350;border-radius:3px;color:#ffffff;display:inline-block; font-family: &quot;Segoe UI&quot;,sans-serif; font-size:14px;font-weight:500;line-height:28px;text-align:center;text-decoration:none;width:100px;-webkit-text-size-adjust:none;'>RESET</a><!--[if mso]></center></v:roundrect><![endif]--></div></td></tr></tbody></table></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='signature' style='padding-left: 4px;padding-bottom: 0px; padding-top: 18px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><p style='margin-bottom: 6px;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;margin-top: 0px;'>Regards,</p><p style='margin: 0;margin-bottom: 10px;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;'>"
                            + GlobalAppSettings.SystemSettings.OrganizationName + " </p></td></tr></table></td></tr></tbody></table></td></tr></tbody></table></center></body></html>";

                    GlobalAppSettings.SendCustomMail(userDetail.Email, emailBody, GlobalAppSettings.SystemSettings.OrganizationName + ": Reset password");

                    TempData["message"] = "An email with instructions on how to change your password has been sent to " + userDetail.Email;
                        return Redirect("/accounts/forgot-password/code-confirmation?userId=" + userDetail.UserId);
                }
                else
                {
                    TempData["errorMessage"] = "Internal server error. Please try again.";
                    return View();
                }
            }
        }

        /// <summary>     
        /// Post action for getting the password and confirm password and resetting it.
        /// If the user id and reset code doesn't match with database, the error message showed here, otherwise it will show the change password form.
        /// After changed the password, the user will be redirected to the login page.
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdatePassword()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            var userId = Convert.ToInt32(Request["userId"]);
            var code = Request["recoveryCode"];
            var password = Request["password"];
            var userDetail = _userManagement.FindUserByUserId(userId);
            if (userDetail.ResetPasswordCode == code)
            {
                var encrypt = new Cryptography();
                var encryptedPassword = Convert.ToBase64String(encrypt.Encryption(password));
                var updateColumns = new List<UpdateColumn>
                {
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.Password,
                        Value = encryptedPassword
                    },
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.ModifiedDate,
                        Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    },
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.ResetPasswordCode,
                        Value = _userManagement.GenerateRandomCode(12)
                    },
                };
                var result = _userManagement.UpdateUserProfileDetails(updateColumns, userId);
                if (result)
                {
                    TempData["User"] = "Password has been changed successfully.";
                    return Redirect("../login");
                }
                else
                {
                    TempData["Message"] = "Internal server error while updating password. Please try again.";
                    return Redirect("../forgot-Password/change-password?userid=" + userId + "&recovercode=" + code);
                }
            }
            TempData["Message"] = "Invalid link";
            return Redirect("../accounts/forgot-Password/change-password?userid=" + userId + "&recovercode=" + code);
        }

        /// <summary>
        /// Resend the mail with newly generated reset password code
        /// </summary>
        /// <returns></returns>
        public ActionResult ResendForgotPasswordMail()
        {
            var userId = Convert.ToInt32(Request["userId"]);
            var userDetail = _userManagement.FindUserByUserId(userId);
            if (userDetail == null)
            {
                TempData["errorMessage"] = "Invalid link";
                return Redirect("/accounts/forgot-password");
            }
            var result = _userManagement.SetResetPasswordForUser(userId);
            if (result.Success)
            {
                var resetPasswordCode = result.Value.ToString();
                var emailBody =
                            "<html xmlns='http://www.w3.org/1999/xhtml' xmlns='http://www.w3.org/1999/xhtml' xmlns:v='urn:schemas-microsoft-com:vml' xmlns:o='urn:schemas-microsoft-com:office:office'><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8'><meta name='viewport' content='width=device-width'>	<!--[if gte mso 9]><xml> <o:OfficeDocumentSettings>  <o:AllowPNG/>  <o:PixelsPerInch>96</o:PixelsPerInch> </o:OfficeDocumentSettings></xml><![endif]--></head><body style='min-width: 100%;-webkit-text-size-adjust: 100%;-ms-text-size-adjust: 100%;margin: 0;padding: 0;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;text-align: left;line-height: 19px;font-size: 14px;background-color: rgb(244,244,244);padding-top: 10px;width: 100% !important;'><center style='width: 100%;min-width: 580px;'><table style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='email-container' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='email-header' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;background-color: #ef5350;padding-right: 10px;padding-left: 10px;padding-top: 10px;padding-bottom: 18px;border-top-right-radius: 5px;border-top-left-radius: 5px;border-collapse: collapse !important;'><table class='body container' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;height: 100%;width: 580px;margin: 0;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;line-height: 19px;font-size: 14px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='sync-inc wrapper' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 2px 10px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><h6 style='color: white;font-family: &quot;Segoe UI&quot;,sans-serif !important;font-weight: 500;padding: 0;margin: 0;text-align: left;line-height: 1.3;word-break: normal;font-size: 20px;font-style: normal;'><a href='"
                            + GlobalAppSettings.SystemSettings.BaseUrl + "/accounts/login' style='color: white;text-decoration: none;'>"
                            + GlobalAppSettings.SystemSettings.OrganizationName + "</a></h6></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='wrapper last' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 50px 0px 10px 273px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><img src='"
                            + GlobalAppSettings.SystemSettings.BaseUrl + "/Content/Images/Application/"
                            + GlobalAppSettings.SystemSettings.MainScreenLogo + "' style='outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;width: auto;max-width: 100%;float: none;clear: both;margin: 0 auto;'></td></tr><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='welcome wrapper last' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 17px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><h6 style='color: white;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: lighter;padding: 0;margin: 0;text-align: center;line-height: 1.3;word-break: normal;font-size: 20px;'>RESET YOUR LOST PASSWORD</h6></td></tr></tbody></table></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='message-body' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;background-color: white;padding-right: 10px;padding-left: 10px;border-bottom-right-radius: 5px;border-bottom-left-radius: 5px;border-collapse: collapse !important;'><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='activation-msg' style='padding-top: 20px;padding-left: 4px; padding-bottom: 7px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto; vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><p style='margin-bottom: 4px;margin: 0;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;'>Hello "
                            + userDetail.DisplayName + ",</p></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='activation-msg' style='padding-top: 5px;padding-left: 4px; padding-bottom: 18px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto; vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><p style='margin-bottom: 4px;margin: 0;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;'>We have received a request to reset the password for your grout account. Please click Reset to set a new password.</p></td></tr></tbody></table><table style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td style='padding-left: 4px;padding-bottom: 0px; padding-top: 0px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><table class='block-grid five-up' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;width: 100%;max-width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='activate-btn center' style='border-radius: 3px; padding: 0px;'><div><!--[if mso]><v:roundrect xmlns:v='urn:schemas-microsoft-com:vml' xmlns:w='urn:schemas-microsoft-com:office:word' href='"
                            + GlobalAppSettings.SystemSettings.BaseUrl + "/accounts/forgot-password/change-password?userid="
                            + userDetail.UserId + "&recoverycode="
                            + resetPasswordCode + "' style='height:28px;v-text-anchor:middle;width:100px;' arcsize='11%' stroke='f' fillcolor='#ef5350'><w:anchorlock/><center><![endif]--><a href='"
                            + GlobalAppSettings.SystemSettings.BaseUrl + "/accounts/forgot-password/change-password?userid="
                            + userDetail.UserId + "&recoverycode="
                            + resetPasswordCode + "'style='background-color:#ef5350;border-radius:3px;color:#ffffff;display:inline-block; font-family: &quot;Segoe UI&quot;,sans-serif; font-size:14px;font-weight:500;line-height:28px;text-align:center;text-decoration:none;width:100px;-webkit-text-size-adjust:none;'>RESET</a><!--[if mso]></center></v:roundrect><![endif]--></div></td></tr></tbody></table></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='signature' style='padding-left: 4px;padding-bottom: 0px; padding-top: 18px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><p style='margin-bottom: 6px;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;margin-top: 0px;'>Regards,</p><p style='margin: 0;margin-bottom: 10px;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;'>"
                            + GlobalAppSettings.SystemSettings.OrganizationName + " </p></td></tr></table></td></tr></tbody></table></td></tr></tbody></table></center></body></html>";
                GlobalAppSettings.SendCustomMail(userDetail.Email, emailBody,GlobalAppSettings.SystemSettings.OrganizationName + ": Reset password");
                TempData["message"] = "Email resent successfully.";
                return Redirect("/accounts/forgot-password/code-confirmation?userid=" + userId);
            }
            else
            {
                TempData["errorMessage"] = "Internal server error occurs. Please try again.";
                return View("/accounts/forgot-password");
            }
        }

        #endregion
    }
}
