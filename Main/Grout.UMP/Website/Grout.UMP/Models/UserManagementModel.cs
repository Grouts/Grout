using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Logger;
using System.DirectoryServices;
using System.Globalization;


namespace Grout.UMP.Models
{
    public class UserManagementModel
    {
        private readonly Cryptography cryptograph = new Cryptography();
        private readonly UserManagement _userDetails = new UserManagement();
        private readonly JavaScriptSerializer _javascriptserializer = new JavaScriptSerializer();

        public String SpecificUserDetails(int userid)
        {
            return _javascriptserializer.Serialize(_userDetails.FindUserByUserId(userid));
        }

        public string GetallimagesofParticularUser(string username)
        {
            var pat = GlobalAppSettings.GetProfilePicturesPath();
            var path = pat + username + "\\150\\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(@pat + username);
            }
            var d = new DirectoryInfo(path);
            var files =
                d.GetFiles("*.png")
                    .OrderByDescending(t => t.LastWriteTime)
                    .Take(3)
                    .Union(d.GetFiles("*.jpg"))
                    .Union(d.GetFiles("etc"))
                    .ToArray(); //Getting Text files
            var imagelist = files.Select(file => "/Content/images/ProfilePictures/" + username + "/150/" + file.Name).ToList();
            return imagelist.Aggregate("", (current, value) => current + "<div style='float:left;' class='Imageclickevent' ><img class='image-settings-for-upload grayscale'  src='" + value + "'/></div>");
        }

        public string UpdateUserAvatarDetails(ProfilePicture profile, DateTime timeNow)
        {
            try
            {
                var userId = _userDetails.GetUserId(profile.UserName);
                var destination = GlobalAppSettings.GetProfilePicturesPath();
                var newlocation = destination + profile.UserName;
                var imageName = Guid.NewGuid().ToString();
                var imageLocation150 = newlocation + "\\150\\";
                var imageLocation110 = newlocation + "\\110\\";
                var imageLocation64 = newlocation + "\\64\\";
                var imageLocation32 = newlocation + "\\32\\";
                var imageLocation18 = newlocation + "\\18\\";
                if (!Directory.Exists(imageLocation150))
                    Directory.CreateDirectory(imageLocation150);
                if (!Directory.Exists(imageLocation110))
                    Directory.CreateDirectory(imageLocation110);
                if (!Directory.Exists(imageLocation64))
                    Directory.CreateDirectory(imageLocation64);
                if (!Directory.Exists(imageLocation32))
                    Directory.CreateDirectory(imageLocation32);
                if (!Directory.Exists(imageLocation18))
                    Directory.CreateDirectory(imageLocation18);
                imageLocation150 = imageLocation150 + imageName + ".png";
                imageLocation110 = imageLocation110 + imageName + ".png";
                imageLocation64 = imageLocation64 + imageName + ".png";
                imageLocation32 = imageLocation32 + imageName + ".png";
                imageLocation18 = imageLocation18 + imageName + ".png";

                if (profile.IsNewFile)
                {
                    newlocation = newlocation + "\\150\\" + profile.ImageName;
                }
                else
                {
                    newlocation = newlocation + "\\" + profile.ImageName;
                }

                var resizedImage = Image.FromFile(newlocation);

                var bitMapImage = new Bitmap(150, 150);
                using (var graphicImageContent = Graphics.FromImage(bitMapImage))
                {
                    graphicImageContent.CompositingQuality = CompositingQuality.HighQuality;
                    graphicImageContent.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphicImageContent.SmoothingMode = SmoothingMode.HighQuality;
                    graphicImageContent.DrawImage(resizedImage, 0, 0, bitMapImage.Width, bitMapImage.Height);
                }
                var croppedImage =
                    bitMapImage.Clone(
                        new Rectangle(profile.LeftOfCropArea, profile.TopOfCropAea,
                            profile.Height, profile.Width),
                        resizedImage.PixelFormat);
                var newBitmap = new Bitmap(croppedImage);

                using (var graphicImageContent = Graphics.FromImage(bitMapImage))
                {
                    graphicImageContent.CompositingQuality = CompositingQuality.HighQuality;
                    graphicImageContent.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphicImageContent.SmoothingMode = SmoothingMode.HighQuality;
                    graphicImageContent.DrawImage(newBitmap, 0, 0, bitMapImage.Width, bitMapImage.Height);
                }
                var cropBitmap150 = new Bitmap(bitMapImage);
                cropBitmap150.Save(imageLocation150, ImageFormat.Png);

                bitMapImage = new Bitmap(110, 110);
                using (var graphicImageContent = Graphics.FromImage(bitMapImage))
                {
                    graphicImageContent.CompositingQuality = CompositingQuality.HighQuality;
                    graphicImageContent.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphicImageContent.SmoothingMode = SmoothingMode.HighQuality;
                    graphicImageContent.DrawImage(newBitmap, 0, 0, bitMapImage.Width, bitMapImage.Height);
                }
                var cropBitmap110 = new Bitmap(bitMapImage);
                cropBitmap110.Save(imageLocation110, ImageFormat.Png);

                bitMapImage = new Bitmap(64, 64);
                using (var graphicImageContent = Graphics.FromImage(bitMapImage))
                {
                    graphicImageContent.CompositingQuality = CompositingQuality.HighQuality;
                    graphicImageContent.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphicImageContent.SmoothingMode = SmoothingMode.HighQuality;
                    graphicImageContent.DrawImage(newBitmap, 0, 0, bitMapImage.Width, bitMapImage.Height);
                }
                var cropBitmap64 = new Bitmap(bitMapImage);
                cropBitmap64.Save(imageLocation64, ImageFormat.Png);

                bitMapImage = new Bitmap(32, 32);
                using (var graphicImageContent = Graphics.FromImage(bitMapImage))
                {
                    graphicImageContent.CompositingQuality = CompositingQuality.HighQuality;
                    graphicImageContent.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphicImageContent.SmoothingMode = SmoothingMode.HighQuality;
                    graphicImageContent.DrawImage(newBitmap, 0, 0, bitMapImage.Width, bitMapImage.Height);
                }
                var cropBitmap32 = new Bitmap(bitMapImage);
                cropBitmap32.Save(imageLocation32, ImageFormat.Png);

                bitMapImage = new Bitmap(18, 18);
                using (var graphicImageContent = Graphics.FromImage(bitMapImage))
                {
                    graphicImageContent.CompositingQuality = CompositingQuality.HighQuality;
                    graphicImageContent.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphicImageContent.SmoothingMode = SmoothingMode.HighQuality;
                    graphicImageContent.DrawImage(newBitmap, 0, 0, bitMapImage.Width, bitMapImage.Height);
                }
                var cropBitmap18 = new Bitmap(bitMapImage);
                cropBitmap18.Save(imageLocation18, ImageFormat.Png);

                var userManagement = new UserManagement();
                var resultedImageName = imageName + ".png";
                var updateColumns = new List<UpdateColumn> 
                {
                    new UpdateColumn 
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.Picture,
                        Value = resultedImageName
                    }
                };

                userManagement.UpdateUserProfileDetails(updateColumns, userId);

                return resultedImageName;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string DefaultavatarTransfer(string path, string username, DateTime timeNow)
        {
            var userId = _userDetails.GetUserId(username);
            var list = path.Split('/');
            var source = GlobalAppSettings.GetProfilePicturesPath() + "Default\\";
            source = source + list[list.Length - 1];
            var imageName = Guid.NewGuid().ToString();
            var newlocation = GlobalAppSettings.GetProfilePicturesPath() + username;
            var newProlocation = newlocation + "\\150\\";
            var newsmlocation = newlocation + "\\110\\";
            var newvsmlocation = newlocation + "\\64\\";
            var newhsmlocation = newlocation + "\\32\\";
            var newlowlocation = newlocation + "\\18\\";

            if (!Directory.Exists(newProlocation))
                Directory.CreateDirectory(newProlocation);
            if (!Directory.Exists(newsmlocation))
                Directory.CreateDirectory(newsmlocation);
            if (!Directory.Exists(newvsmlocation))
                Directory.CreateDirectory(newvsmlocation);
            if (!Directory.Exists(newhsmlocation))
                Directory.CreateDirectory(newhsmlocation);
            if (!Directory.Exists(newlowlocation))
                Directory.CreateDirectory(newlowlocation);

            newProlocation = newProlocation + imageName + ".png";
            newsmlocation = newsmlocation + imageName + ".png";
            newvsmlocation = newvsmlocation + imageName + ".png";
            newhsmlocation = newhsmlocation + imageName + ".png";
            newlowlocation = newlowlocation + imageName + ".png";

            var resizeImage = Image.FromFile(source);
            const int profileWidth = 150;
            const int derivedHeight = 150;
            var profilebmp = new Bitmap(profileWidth, derivedHeight);
            using (var gr = Graphics.FromImage(profilebmp))
            {
                gr.DrawImage(resizeImage, 0, 0, profileWidth, derivedHeight);
            }
            profilebmp.Save(newProlocation, ImageFormat.Png);

            const int smWidth = 110;
            const int smHeight = 110;
            var smbmp = new Bitmap(smWidth, smHeight);
            using (var gr = Graphics.FromImage(smbmp))
            {
                gr.DrawImage(resizeImage, 0, 0, smWidth, smHeight);
            }
            smbmp.Save(newsmlocation, ImageFormat.Png);

            const int vsmWidth = 64;
            const int vsmHeight = 64;
            var vsmbmp = new Bitmap(vsmWidth, vsmHeight);
            using (var gr = Graphics.FromImage(vsmbmp))
            {
                gr.DrawImage(resizeImage, 0, 0, vsmWidth, vsmHeight);
            }
            vsmbmp.Save(newvsmlocation, ImageFormat.Png);

            const int hsmWidth = 32;
            const int hsmHeight = 32;
            var hsmbmp = new Bitmap(hsmWidth, hsmHeight);
            using (var gr = Graphics.FromImage(hsmbmp))
            {
                gr.DrawImage(resizeImage, 0, 0, hsmWidth, hsmHeight);
            }
            hsmbmp.Save(newhsmlocation, ImageFormat.Png);

            const int lowWidth = 18;
            const int lowHeight = 18;
            var lowbmp = new Bitmap(lowWidth, lowHeight);
            using (var gr = Graphics.FromImage(lowbmp))
            {
                gr.DrawImage(resizeImage, 0, 0, lowWidth, lowHeight);
            }
            lowbmp.Save(newlowlocation, ImageFormat.Png);

            var destination = GlobalAppSettings.GetProfilePicturesPath();

            var validatelink = destination + username;

            if (!Directory.Exists(validatelink))
            {
                Directory.CreateDirectory(validatelink);
            }

            validatelink = validatelink + "\\";
            validatelink = validatelink + imageName;
            validatelink = validatelink + ".png";
            File.Copy(source, validatelink, true);
            var um = new UserManagement();

            var updateColumns = new List<UpdateColumn> 
            {
                new UpdateColumn 
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.Picture,
                    Value = imageName + ".png"
                }
            };

            um.UpdateUserProfileDetails(updateColumns, userId);

            profilebmp.Dispose();
            smbmp.Dispose();
            vsmbmp.Dispose();
            hsmbmp.Dispose();
            lowbmp.Dispose();

            return imageName + ".png";
        }

        public static bool SendActivationCode(string displayName, string userName, string toAddress, string activationUrl, DateTime activationExpirationDate, bool isResendActivationCode)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(toAddress))
                    return false;

                string bodyOftheContent;
                if (isResendActivationCode) //E-mail Template yet to be design
                {
                    bodyOftheContent =
                        "<html xmlns='http://www.w3.org/1999/xhtml' xmlns='http://www.w3.org/1999/xhtml' xmlns:v='urn:schemas-microsoft-com:vml' xmlns:o='urn:schemas-microsoft-com:office:office'><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8'><meta name='viewport' content='width=device-width'>	<!--[if gte mso 9]><xml> <o:OfficeDocumentSettings>  <o:AllowPNG/>  <o:PixelsPerInch>96</o:PixelsPerInch> </o:OfficeDocumentSettings></xml><![endif]--></head><body style='min-width: 100%;-webkit-text-size-adjust: 100%;-ms-text-size-adjust: 100%;margin: 0;padding: 0;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;text-align: left;line-height: 19px;font-size: 14px;background-color: rgb(244,244,244);padding-top: 10px;width: 100% !important;'><center style='width: 100%;min-width: 580px;'><table style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='email-container' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='email-header' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;background-color: #2196F3;padding-right: 10px;padding-left: 10px;padding-top: 10px;padding-bottom: 18px;border-top-right-radius: 5px;border-top-left-radius: 5px;border-collapse: collapse !important;'><table class='body container' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;height: 100%;width: 580px;margin: 0;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;line-height: 19px;font-size: 14px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='sync-inc wrapper' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 2px 10px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><h6 style='color: white;font-family: &quot;Segoe UI&quot;,sans-serif !important;font-weight: 500;padding: 0;margin: 0;text-align: left;line-height: 1.3;word-break: normal;font-size: 20px;font-style: normal;'><a href='"
                          + GlobalAppSettings.SystemSettings.BaseUrl + "/accounts/login' style='color: white;text-decoration: none;'>"
                        + GlobalAppSettings.SystemSettings.OrganizationName +
                        "</a></h6></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='wrapper last' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 25px 0px 10px 273px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><img src='"
                        + GlobalAppSettings.SystemSettings.BaseUrl + "/Content/Images/Application/"
                        + GlobalAppSettings.SystemSettings.MainScreenLogo +
                        "' style='outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;width: auto;max-width: 100%;float: none;clear: both;margin: 0 auto;'></td></tr><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='welcome wrapper last' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><h6 style='color: white;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: lighter;padding: 0;margin: 0;text-align: center;line-height: 1.3;word-break: normal;font-size: 20px;'>Welcome to "
                        + GlobalAppSettings.SystemSettings.OrganizationName +
                        "</h6></td></tr><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='welcome-msg wrapper last' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><p style='margin: 0;margin-bottom: 10px;color: white;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: 300;padding: 0;text-align: center;line-height: 19px;font-size: 16px;font-style: normal;'>NEW USER ACCOUNT ACTIVATION</p></td></tr></tbody></table></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='message-body' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;background-color: white;padding-right: 10px;padding-left: 10px;border-bottom-right-radius: 5px;border-bottom-left-radius: 5px;border-collapse: collapse !important;'><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='activation-msg' style='padding-top: 20px;padding-left: 4px; padding-bottom: 7px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto; vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><table style='border-collapse: collapse'><tbody><tr><td style='padding-bottom: 5px; padding-left: 0px;'><p style='margin: 0px;margin: 0;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;'>Hello "
                        + displayName +
                        ",</p></td></tr></tbody></table><p style='margin-bottom: 2px;margin: 0;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;text-align: left;line-height: 19px;font-size: 12px;'>Your Grout account has been created successfully. Please click Activate to activate your account.</p></td></tr></tbody></table><table class='block-grid five-up' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;width: 100%;max-width: 580px;'><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='user-credentials' style='padding-left: 4px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;display: inline-block;width: 96px;border-collapse: collapse !important;'><p style='margin: 0;margin-bottom: 10px;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0px;text-align: left;line-height: 19px;font-size: 12px;'>Username:</p></td><td class='user-id' style='padding-bottom: 7px;word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;display: inline-block;width: 470px;border-collapse: collapse !important;'><p style='margin: 0;margin-bottom: 10px;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: 300;text-align: left;line-height: 19px;font-size: 12px;font-style: normal;'>"
                        + userName +
                        "</p></td></tr></table><table style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td style='padding-left: 4px;padding-bottom: 0px; padding-top: 0px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><table class='block-grid five-up' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;width: 100%;max-width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='activate-btn center' style='border-radius: 3px; padding: 0px;'><div><!--[if mso]><v:roundrect xmlns:v='urn:schemas-microsoft-com:vml' xmlns:w='urn:schemas-microsoft-com:office:word' href='"
                        + activationUrl +
                        "' style='height:28px;v-text-anchor:middle;width:100px;' arcsize='11%' stroke='f' fillcolor='#2196f3'><w:anchorlock/><center><![endif]--><a href='"
                        + activationUrl +
                        "'style='background-color:#2196f3; color: #ffffff; border-radius:3px;color:#ffffff !important; display:inline-block; font-family: &quot; Segoe UI &quot;,sans-serif; font-size:14px;font-weight:300;line-height:28px;text-align:center;text-decoration:none;width:100px;-webkit-text-size-adjust:none;'>ACTIVATE</a><!--[if mso]></center></v:roundrect><![endif]--></div></td></tr></tbody></table></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='valid-date' style='padding-top: 16px;padding-left: 4px;padding-bottom: 0px;word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto; vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><p style='margin: 0;margin-bottom: 11px;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0px; text-align: left;line-height: 19px;font-size: 12px;'>This link will only be valid until "
                        + activationExpirationDate.ToString(GlobalAppSettings.SystemSettings.DateFormat) +
                        "</p></td></tr></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='signature' style='padding-left: 4px;padding-bottom: 0px; padding-top: 0px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><p style='margin-bottom: 6px;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;margin-top: 0px;'>Regards,</p><p style='margin: 0;margin-bottom: 10px;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;'>"
                        + GlobalAppSettings.SystemSettings.OrganizationName +
                        "</p></td></tr></table></td></tr></tbody></table></td></tr></tbody></table></center></body></html>";
                }
                else
                {
                    bodyOftheContent =
                        "<html xmlns='http://www.w3.org/1999/xhtml' xmlns='http://www.w3.org/1999/xhtml' xmlns:v='urn:schemas-microsoft-com:vml' xmlns:o='urn:schemas-microsoft-com:office:office'><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8'><meta name='viewport' content='width=device-width'>	<!--[if gte mso 9]><xml> <o:OfficeDocumentSettings>  <o:AllowPNG/>  <o:PixelsPerInch>96</o:PixelsPerInch> </o:OfficeDocumentSettings></xml><![endif]--></head><body style='min-width: 100%;-webkit-text-size-adjust: 100%;-ms-text-size-adjust: 100%;margin: 0;padding: 0;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;text-align: left;line-height: 19px;font-size: 14px;background-color: rgb(244,244,244);padding-top: 10px;width: 100% !important;'><center style='width: 100%;min-width: 580px;'><table style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='email-container' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='email-header' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;background-color: #2196F3;padding-right: 10px;padding-left: 10px;padding-top: 10px;padding-bottom: 18px;border-top-right-radius: 5px;border-top-left-radius: 5px;border-collapse: collapse !important;'><table class='body container' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;height: 100%;width: 580px;margin: 0;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;line-height: 19px;font-size: 14px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='sync-inc wrapper' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 2px 10px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><h6 style='color: white;font-family: &quot;Segoe UI&quot;,sans-serif !important;font-weight: 500;padding: 0;margin: 0;text-align: left;line-height: 1.3;word-break: normal;font-size: 20px;font-style: normal;'><a href='"
                          + GlobalAppSettings.SystemSettings.BaseUrl + "/accounts/login' style='color: white;text-decoration: none;'>"
                        + GlobalAppSettings.SystemSettings.OrganizationName +
                        "</a></h6></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='wrapper last' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 25px 0px 10px 273px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><img src='"
                        + GlobalAppSettings.SystemSettings.BaseUrl + "/Content/Images/Application/"
                        + GlobalAppSettings.SystemSettings.MainScreenLogo +
                        "' style='outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;width: auto;max-width: 100%;float: none;clear: both;margin: 0 auto;'></td></tr><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='welcome wrapper last' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><h6 style='color: white;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: lighter;padding: 0;margin: 0;text-align: center;line-height: 1.3;word-break: normal;font-size: 20px;'>Welcome to "
                        + GlobalAppSettings.SystemSettings.OrganizationName +
                        "</h6></td></tr><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='welcome-msg wrapper last' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><p style='margin: 0;margin-bottom: 10px;color: white;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: 300;padding: 0;text-align: center;line-height: 19px;font-size: 16px;font-style: normal;'>NEW USER ACCOUNT ACTIVATION</p></td></tr></tbody></table></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='message-body' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;background-color: white;padding-right: 10px;padding-left: 10px;border-bottom-right-radius: 5px;border-bottom-left-radius: 5px;border-collapse: collapse !important;'><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='activation-msg' style='padding-top: 20px;padding-left: 4px; padding-bottom: 7px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto; vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><table style='border-collapse: collapse'><tbody><tr><td style='padding-bottom: 5px; padding-left: 0px;'><p style='margin: 0px;margin: 0;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;'>Hello "
                        + displayName +
                        ",</p></td></tr></tbody></table><p style='margin-bottom: 2px;margin: 0;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;text-align: left;line-height: 19px;font-size: 12px;'>Your Grout account has been created successfully. Please click Activate to activate your account.</p></td></tr></tbody></table><table class='block-grid five-up' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;width: 100%;max-width: 580px;'><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='user-credentials' style='padding-left: 4px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;display: inline-block;width: 96px;border-collapse: collapse !important;'><p style='margin: 0;margin-bottom: 10px;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0px;text-align: left;line-height: 19px;font-size: 12px;'>Username</p></td><td class='user-id' style='padding-bottom: 7px;word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;display: inline-block;width: 470px;border-collapse: collapse !important;'><p style='margin: 0;margin-bottom: 10px;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: 300;text-align: left;line-height: 19px;font-size: 12px;font-style: normal;'>"
                        + userName +
                        "</p></td></tr></table><table style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td style='padding-left: 4px;padding-bottom: 0px; padding-top: 0px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><table class='block-grid five-up' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;width: 100%;max-width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='activate-btn center' style='border-radius: 3px; padding: 0px;'><div><!--[if mso]><v:roundrect xmlns:v='urn:schemas-microsoft-com:vml' xmlns:w='urn:schemas-microsoft-com:office:word' href='"
                        + activationUrl +
                        "' style='height:28px;v-text-anchor:middle;width:100px;' arcsize='11%' stroke='f' fillcolor='#2196f3'><w:anchorlock/><center><![endif]--><a href='"
                        + activationUrl +
                        "'style='background-color:#2196f3; color: #ffffff; border-radius:3px;color:#ffffff !important; display:inline-block; font-family: &quot; Segoe UI &quot;,sans-serif; font-size:14px;font-weight:300;line-height:28px;text-align:center;text-decoration:none;width:100px;-webkit-text-size-adjust:none;'>ACTIVATE</a><!--[if mso]></center></v:roundrect><![endif]--></div></td></tr></tbody></table></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='valid-date' style='padding-top: 16px;padding-left: 4px;padding-bottom: 0px;word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto; vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><p style='margin: 0;margin-bottom: 11px;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0px; text-align: left;line-height: 19px;font-size: 12px;'>This link will only be valid until "
                        + activationExpirationDate.ToString(GlobalAppSettings.SystemSettings.DateFormat) +
                        "</p></td></tr></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='signature' style='padding-left: 4px;padding-bottom: 0px; padding-top: 0px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><p style='margin-bottom: 6px;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;margin-top: 0px;'>Regards,</p><p style='margin: 0;margin-bottom: 10px;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;'>"
                        + GlobalAppSettings.SystemSettings.OrganizationName +
                        "</p></td></tr></table></td></tr></tbody></table></td></tr></tbody></table></center></body></html>";
                }
                string subject = GlobalAppSettings.SystemSettings.OrganizationName + ": new account activation ";
                GlobalAppSettings.SendCustomMail(toAddress, bodyOftheContent, subject);
            }
            catch (Exception e)
            {

            }
            return true;
        }

        public List<Dictionary<string, string>> SaveuserBulkUpload(string filePath)
        {
            var dataTablefromCsv = GetDataTableFromCsvFile(filePath);

            return (from DataRow dr in dataTablefromCsv.Rows select dataTablefromCsv.Columns.Cast<DataColumn>().ToDictionary(dt => dt.ColumnName, dt => dr[dt].ToString())).ToList();
        }

        public DataTable GetDataTableFromCsvFile(string csvFilePath)
        {
            DataTable csvData = new DataTable();
            try
            {
                using (var csvReader = new StreamReader(csvFilePath))
                {

                    int rowCount = 0;
                    string[] columnNames = null;
                    while (!csvReader.EndOfStream)
                    {
                        var csvStreamData = csvReader.ReadLine().Trim();
                        if (csvStreamData.Length > 0)
                        {
                            var csvDataValues = csvStreamData.Split(',');
                            if (rowCount == 0)
                            {
                                rowCount = 1;
                                columnNames = csvStreamData.Split(',');
                                csvData = new DataTable();

                                foreach (string csvcolumn in columnNames)
                                {
                                    DataColumn csvDataColumn = new DataColumn(csvcolumn, typeof(string));

                                    csvDataColumn.DefaultValue = string.Empty;

                                    csvData.Columns.Add(csvDataColumn);
                                }
                            }
                            else
                            {
                                if (csvData != null)
                                {
                                    DataRow csvDataRow = csvData.NewRow();

                                    for (int i = 0; i < columnNames.Length; i++)
                                    {
                                        csvDataRow[columnNames[i]] = csvDataValues[i] == null
                                            ? string.Empty
                                            : csvDataValues[i].ToString();
                                    }
                                    csvData.Rows.Add(csvDataRow);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return csvData;
            }
            return csvData;
        }

      
        public List<CsvUserImport> SubmitUsersBulkUpload(string userNames, string emailIds, List<Dictionary<string, string>> allUserList)
        {
            var userImportList = new List<CsvUserImport>();
            var userNameList = userNames.Split(',').ToList();
            var emailList = emailIds.Split(',').ToList();
            var umpUser = new User();
            var usermanagement = new UserManagement();
            var password = "NA";

            var activeUserList = usermanagement.GetAllActiveInactiveUsers();

            var duplicateUsername = userNameList.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            var duplicateEmailid = emailList.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();

            foreach (var user in allUserList)
            {
                var isEmailInvalid = !usermanagement.IsValidEmail(user["Email"]);
                var isUserInvalid = user["Username"].Contains(" ");
                var isEmailExist = activeUserList.Find(f => f.Email.ToLower() == user["Email"].ToLower()) != null;
                var isUserExist = activeUserList.Find(f => f.UserName.ToLower() == user["Username"].ToLower()) != null;

                if (String.IsNullOrWhiteSpace(user["Username"]) || String.IsNullOrWhiteSpace(user["Email"]) || String.IsNullOrWhiteSpace(user["Fullname"]) || String.IsNullOrWhiteSpace(user["Password"]))
                {
                    var message = "";
                    if (String.IsNullOrWhiteSpace(user["Username"]) && String.IsNullOrWhiteSpace(user["Email"]) && String.IsNullOrWhiteSpace(user["Fullname"]) && String.IsNullOrWhiteSpace(user["Password"]))
                    {
                        message = "Username, Email, Name and Password should not be empty";
                    }
                    else if (String.IsNullOrWhiteSpace(user["Username"]) && !String.IsNullOrWhiteSpace(user["Email"]) && !String.IsNullOrWhiteSpace(user["Fullname"]) && !String.IsNullOrWhiteSpace(user["Password"]))
                    {
                        message = "Username sholud not be empty";
                    }
                    else if (!String.IsNullOrWhiteSpace(user["Username"]) && String.IsNullOrWhiteSpace(user["Email"]) && !String.IsNullOrWhiteSpace(user["Fullname"]) && !String.IsNullOrWhiteSpace(user["Password"]))
                    {
                        message = "Email sholud not be empty";
                    }
                    else if (!String.IsNullOrWhiteSpace(user["Username"]) && !String.IsNullOrWhiteSpace(user["Email"]) && String.IsNullOrWhiteSpace(user["Fullname"]) && !String.IsNullOrWhiteSpace(user["Password"]))
                    {
                        message = "Name should not be empty";
                    }
                    else if (!String.IsNullOrWhiteSpace(user["Username"]) && !String.IsNullOrWhiteSpace(user["Email"]) && !String.IsNullOrWhiteSpace(user["Fullname"]) && String.IsNullOrWhiteSpace(user["Password"]))
                    {
                        message = "Password should not be empty";
                    }
                    else if (!String.IsNullOrWhiteSpace(user["Username"]) && String.IsNullOrWhiteSpace(user["Email"]) && String.IsNullOrWhiteSpace(user["Fullname"]) && !String.IsNullOrWhiteSpace(user["Password"]))
                    {
                        message = "Email and Name sholud not be empty";
                    }
                    else if (String.IsNullOrWhiteSpace(user["Username"]) && !String.IsNullOrWhiteSpace(user["Email"]) && String.IsNullOrWhiteSpace(user["Fullname"]) && !String.IsNullOrWhiteSpace(user["Password"]))
                    {
                        message = "Username and Name sholud not be empty";
                    }
                    else if (String.IsNullOrWhiteSpace(user["Username"]) && String.IsNullOrWhiteSpace(user["Email"]) && !String.IsNullOrWhiteSpace(user["Fullname"]) && !String.IsNullOrWhiteSpace(user["Password"]))
                    {
                        message = "Username and Email sholud not be empty";
                    }
                    else if (String.IsNullOrWhiteSpace(user["Username"]) && !String.IsNullOrWhiteSpace(user["Email"]) && !String.IsNullOrWhiteSpace(user["Fullname"]) && String.IsNullOrWhiteSpace(user["Password"]))
                    {
                        message = "Username and Password sholud not be empty";
                    }
                    else if (!String.IsNullOrWhiteSpace(user["Username"]) && String.IsNullOrWhiteSpace(user["Email"]) && !String.IsNullOrWhiteSpace(user["Fullname"]) && String.IsNullOrWhiteSpace(user["Password"]))
                    {
                        message = "Email and Password sholud not be empty";
                    }
                    else if (!String.IsNullOrWhiteSpace(user["Username"]) && !String.IsNullOrWhiteSpace(user["Email"]) && String.IsNullOrWhiteSpace(user["Fullname"]) && String.IsNullOrWhiteSpace(user["Password"]))
                    {
                        message = "Name and Password sholud not be empty";
                    }
                    else if (String.IsNullOrWhiteSpace(user["Username"]) && String.IsNullOrWhiteSpace(user["Email"]) && String.IsNullOrWhiteSpace(user["Fullname"]) && !String.IsNullOrWhiteSpace(user["Password"]))
                    {
                        message = "Username, Email and Name sholud not be empty";
                    }
                    else if (!String.IsNullOrWhiteSpace(user["Username"]) && String.IsNullOrWhiteSpace(user["Email"]) && String.IsNullOrWhiteSpace(user["Fullname"]) && String.IsNullOrWhiteSpace(user["Password"]))
                    {
                        message = "Email, Name and Password sholud not be empty";
                    }
                    else if (String.IsNullOrWhiteSpace(user["Username"]) && String.IsNullOrWhiteSpace(user["Email"]) && !String.IsNullOrWhiteSpace(user["Fullname"]) && String.IsNullOrWhiteSpace(user["Password"]))
                    {
                        message = "Username, Email and Password sholud not be empty";
                    }
                    else if (String.IsNullOrWhiteSpace(user["Username"]) && !String.IsNullOrWhiteSpace(user["Email"]) && String.IsNullOrWhiteSpace(user["Fullname"]) && String.IsNullOrWhiteSpace(user["Password"]))
                    {
                        message = "Username, Name and Password sholud not be empty";
                    }
                    var userImport = new CsvUserImport
                    {
                        UserName = user["Username"],
                        Email = user["Email"],
                        DisplayMessage = message,
                        IsExist = false
                    };
                    userImportList.Add(userImport);
                }

                else if (duplicateUsername.Contains(user["Username"]) || duplicateEmailid.Contains(user["Email"]))
                {
                    var userImport = new CsvUserImport
                    {
                        UserName = user["Username"],
                        Email = user["Email"],
                        DisplayMessage = ((duplicateUsername.Contains(user["Username"]) && duplicateEmailid.Contains(user["Email"])) ? "Username and Email id already exists in the uploaded file" : (!duplicateUsername.Contains(user["Username"]) && !duplicateEmailid.Contains(user["Email"]) ? "" : (duplicateEmailid.Contains(user["Email"]) ? "Email id already exists in the uploaded file" : "Username already exists in the uploaded file"))),
                        IsExist = false
                    };
                    userImportList.Add(userImport);
                }

                else if ((isEmailInvalid || isUserInvalid))
                {
                    var userImport = new CsvUserImport
                    {
                        UserName = user["Username"],
                        Email = user["Email"],
                        DisplayMessage = ((isEmailInvalid && isUserInvalid) ? "Invalid Username and Email id" : (!isUserInvalid && !isEmailInvalid ? "" : (isEmailInvalid ? "Invalid Email id" : "Invalid username"))),
                        IsExist = false
                    };
                    userImportList.Add(userImport);
                }
                else
                {
                    var userOrEmailExist = activeUserList.Find(f => f.UserName.ToLower() == user["Username"].ToLower() || f.Email.ToLower() == user["Email"].ToLower());

                    if (userOrEmailExist != null)
                    {
                        var userImport = new CsvUserImport
                        {
                            UserName = user["Username"],
                            Email = user["Email"],
                            DisplayMessage = (isEmailExist && isUserExist) ? "Username and Email Id already exists" : (isUserExist ? "Username already exists" : "Email id already exists"),
                            IsExist = false
                        };
                        userImportList.Add(userImport);
                    }
                }
            }

            if (userImportList.Count == 0)
            {
                for (var t = 0; t < allUserList.Count(); t++)
                {
                    if (allUserList[t]["Password"] != "")
                        password = allUserList[t]["Password"];
                    umpUser.Password = Convert.ToBase64String(cryptograph.Encryption(password));
                    umpUser.CreatedDate = DateTime.UtcNow;
                    umpUser.ModifiedDate = DateTime.UtcNow;
                    umpUser.IsActive = false;
                    umpUser.IsDeleted = false;
                    umpUser.UserName = allUserList[t]["Username"].Trim();
                    umpUser.FirstName = allUserList[t]["Fullname"].Trim();
                    umpUser.LastName = "";
                    umpUser.DisplayName = (umpUser.FirstName.Trim() + " " + umpUser.LastName.Trim()).Trim();
                    umpUser.Email = allUserList[t]["Email"].Trim();
                    umpUser.TimeZone = GlobalAppSettings.SystemSettings.TimeZone;
                    var activationCode = String.Empty;
                    var activationExpirationDate = new DateTime();
                    var result = _userDetails.AddUser(umpUser, out activationExpirationDate, out activationCode);
                    LogExtension.LogInfo("Bulk Upload - User has been added successfully - " + t + 1 + " of " + allUserList.Count, MethodBase.GetCurrentMethod());
                    if (result.Status)
                    {
                        var activationUrl = GlobalAppSettings.SystemSettings.BaseUrl + "/accounts/activate?ActivationCode=" + activationCode;
                        var toAddress = allUserList[t]["Email"];
                        const bool isResendActivationCode = false;
                        SendActivationCode(umpUser.FirstName, umpUser.UserName, umpUser.Email, activationUrl, activationExpirationDate, isResendActivationCode);
                    }
                }
            }
            return userImportList;
        }

        public static EntityData<User> GetAllUserList(int? skip, int? take, string searchKey,
            List<SortCollection> sorted, List<FilterCollection> filterCollection)
        {
            var skipValue = skip.HasValue ? skip.Value : 0;
            var takeValue = take.HasValue ? take.Value : 10;

            var _userDataTable = new UserManagement().GetUserDetail(sorted, skip, take, searchKey, filterCollection);
            int _totalCount = _userDataTable.Rows.Count;

            var _userList = _userDataTable.AsEnumerable().Select
                   (row => new User()
                   {
                       UserId = row.Field<int>("Id"),
                       UserName = row.Field<string>("UserName"),
                       FirstName = row.Field<string>("FirstName"),
                       LastName = row.Field<string>("LastName"),
                       DisplayName = row.Field<string>("DisplayName"),
                       Email = row.Field<string>("Email"),
                       Status = row.Field<bool>("IsActive") ? UserStatus.Active : UserStatus.InActive
                   }).Skip(skipValue).Take(takeValue).ToList();

            return new EntityData<User>
            {
                result = _userList,
                count = _totalCount
            };
        }


        public string AddUserinGroup(List<string> Data)
        {
            var userList = Convert.ToString(Data[0]);
            var users = userList.Split(',').ToList();
            var groupMgmt = new GroupManagement();
            var groupId = Convert.ToInt32(Data[1]);
            for (var t = 0; t < users.Count(); t++)
            {
                var userid = Convert.ToInt32(users[t]);
                var Val = groupMgmt.SearchUserInGroupwithGroupId(userid, groupId);
                if (Val.Count() == 0)
                    groupMgmt.AddUserInGroup(userid, groupId);
            }
            return "success";
        }
    }
}