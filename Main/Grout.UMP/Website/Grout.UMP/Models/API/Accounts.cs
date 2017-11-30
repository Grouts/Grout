using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Encryption;

namespace Grout.UMP.Models
{
    public class Accounts
    {
        private readonly UserManagement userManagement = new UserManagement(GlobalAppSettings.QueryBuilder, GlobalAppSettings.DataProvider);
        private readonly TokenCryptography crypto = new TokenCryptography();

        public ApiResponse Login(string authHeader, string ipAddress, string requestMode = null)
        {
            var authorization = authHeader;
            var status = true;
            var isValid = LoginResponse.DeactivatedUser;
            var responseMessage = string.Empty;

            if (!string.IsNullOrEmpty(authorization))
            {
                if (!string.IsNullOrEmpty(requestMode))
                {
                    var requestModeString = Encoding.UTF8.GetString(Convert.FromBase64String(requestMode));
                    var requestModeDecrypt =
                        crypto.Decrypt(requestModeString)
                            .Split(';')
                            .Select(part => part.Split('='))
                            .Where(part => part.Length == 2)
                            .ToDictionary(sp => sp[0], sp => sp[1]);
                    ipAddress = requestModeDecrypt["IP"];
                }

                var encodeUsernamePwd = Encoding.UTF8.GetString(Convert.FromBase64String(authorization));
                var splitUsernamePwd = encodeUsernamePwd.IndexOf(":");

                var encryptusername = encodeUsernamePwd.Substring(0, splitUsernamePwd);
                var usernameIp = crypto.Decrypt(encryptusername);
                var usernameDic =
                    usernameIp.Split(';')
                        .Select(e => e.Split('='))
                        .Where(e => e.Length == 2)
                        .ToDictionary(a => a[0], a => a[1]);
                var username = usernameDic["plainText"];

                var encryptPassword = encodeUsernamePwd.Substring(splitUsernamePwd + 1);
                var passwordIp = crypto.Decrypt(encryptPassword);
                var passwordDic =
                    passwordIp.Split(';')
                        .Select(part => part.Split('='))
                        .Where(part => part.Length == 2)
                        .ToDictionary(sp => sp[0], sp => sp[1]);
                var password = passwordDic["plainText"];

                isValid = IsValidCredentials(username, password);

                if (ipAddress == passwordDic["IP"])
                {
                    switch (isValid)
                    {
                        case LoginResponse.ValidUser:
                            userManagement.AddUserToken(userManagement.GetUserId(username), ipAddress, String.Empty);
                            responseMessage = "Logged in successfully";
                            break;
                        case LoginResponse.InvalidPassword:
                            status = false;
                            responseMessage = "Invalid password";
                            break;
                        case LoginResponse.ThrottledUser:
                            status = false;
                            responseMessage = "User has been locked";
                            break;
                        case LoginResponse.InvalidUserName:
                            status = false;
                            responseMessage = "Invalid username";
                            break;
                        case LoginResponse.DeactivatedUser:
                            status = false;
                            responseMessage = "Invalid user";
                            break;
                        case LoginResponse.DeletedUser:
                            status = false;
                            responseMessage = "Deleted user";
                            break;
                    }
                }
                else
                {
                    status = false;
                    responseMessage = "You are not an authorized user";
                }
            }
            return new ApiResponse
            {
                ApiStatus = true,
                Data =
                    new ApiData
                    {
                        Success = status,
                        Message = responseMessage,
                        StatusText = isValid.ToString()
                    }
            };
        }

        public ApiResponse ChangePassword(string newPassword, int userId)
        {
            var cryptograph = new Cryptography();
            var status = false;
            var responseMessage = String.Empty;

            var encryptedNewPassword = Encoding.UTF8.GetString(Convert.FromBase64String(newPassword));
            var decryptNewpassword = crypto.Decrypt(encryptedNewPassword);

            var newPassDic =
                decryptNewpassword.Split(';')
                    .Select(part => part.Split('='))
                    .Where(part => part.Length == 2)
                    .ToDictionary(sp => sp[0], sp => sp[1]);

            try
            {
                userManagement.UpdateUserProfileDetails(new List<UpdateColumn>{new UpdateColumn{
                    ColumnName=GlobalAppSettings.DbColumns.DB_User.Password,Value= Convert.ToBase64String(cryptograph.Encryption(newPassDic["plainText"]))
                }}, userId);

                status = true;

                responseMessage = "Password has been changed successfully.";
            }
            catch (Exception ex)
            {
                responseMessage = "Password change unsuccessful.";
            }

            return new ApiResponse()
            {
                ApiStatus = true,
                Data = new ApiData { Message = responseMessage, Success = status }
            };
        }

        public LoginResponse IsValidCredentials(string userName, string password)
        {
            var user = userManagement.FindUserByUserName(userName);

            if (user != null)
            {

                var decryptedPassword = String.Empty;
                try
                {
                    decryptedPassword = (user.Password);
                }
                catch (Exception ex)
                {
                }

                var isActive = false;
                var isDeleted = false;
                try
                {
                    isActive = user.Status == UserStatus.Active;
                }
                catch (Exception ex)
                {
                }

                try
                {
                    isDeleted = user.IsDeleted;
                }
                catch (Exception ex)
                {
                }

                if (isActive && !isDeleted)
                {
                    if (password == decryptedPassword)
                    {
                        return LoginResponse.ValidUser;
                    }

                    return LoginResponse.InvalidPassword;
                }

                if (!isActive)
                {
                    return LoginResponse.DeactivatedUser;
                }

                return LoginResponse.DeletedUser;

            }

            return LoginResponse.InvalidUserName;
        }
    }
}