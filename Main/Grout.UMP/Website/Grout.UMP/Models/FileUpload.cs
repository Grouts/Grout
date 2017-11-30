using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using Grout.Base;
using Grout.Base.DataClasses;

namespace Grout.UMP.Models
{
    public class FileUpload : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            var fileName = String.Empty;
            var timeStamp = context.Request["timeStamp"];
            var imageType = context.Request["imageType"] == "loginlogo"
                ? UploadImageTypes.LoginLogo
                : context.Request["imageType"] == "mainlogo"
                    ? UploadImageTypes.MainScreenLogo
                    : context.Request["imageType"] == "favicon"
                        ? UploadImageTypes.Favicon
                        : UploadImageTypes.ProfilePicture;

            if (imageType != UploadImageTypes.ProfilePicture)
            {
                var targetFolder = GlobalAppSettings.GetApplicationImagesPath();

                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }
                var file = context.Request.Files[0];
                if (file.ContentLength != 0)
                {
                    switch (imageType)
                    {
                        case UploadImageTypes.LoginLogo:
                            fileName = "login_logo_" + timeStamp + ".png";
                            break;
                        case UploadImageTypes.MainScreenLogo:
                            fileName = "main_logo_" + timeStamp + ".png";
                            break;
                        case UploadImageTypes.Favicon:
                            fileName = "favicon_" + timeStamp + ".png";
                            break;
                    }
                    var binaryReader = new BinaryReader(file.InputStream);
                    var memoryBytes = binaryReader.ReadBytes(file.ContentLength);
                    using (var memoryStream = new MemoryStream(memoryBytes))
                    {
                        var imageStream = Image.FromStream(memoryStream);
                        if (imageType == UploadImageTypes.Favicon)
                        {
                            imageStream.Save(targetFolder + "\\" + fileName, ImageFormat.Png);
                        }
                        else if (imageType == UploadImageTypes.MainScreenLogo)
                        {
                            var resizedImage = ImageManager.ResizeImage(imageStream, 40, 40);
                            resizedImage.Save(targetFolder + "\\" + fileName, ImageFormat.Png);
                        }
                        else
                        {
                            var resizedImage = ImageManager.ResizeImage(imageStream, 200, 120);
                            resizedImage.Save(targetFolder + "\\" + fileName, ImageFormat.Png);
                        }
                    }
                }
            }
            else
            {
                var targetFolder = GlobalAppSettings.GetProfilePicturesPath();

                var file = context.Request.Files[0];

                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                if (file.ContentLength != 0)
                {
                    if (Directory.Exists(targetFolder + "\\" + context.Request["userName"]) == false)
                    {
                        Directory.CreateDirectory(targetFolder + "\\" + context.Request["userName"]);
                    }
                    
                    if (File.Exists(targetFolder + "\\" + context.Request["userName"] + "\\" + "profile_picture_" + timeStamp + ".png"))
                    {
                        File.Delete(targetFolder + "\\" + context.Request["userName"] + "\\" + "profile_picture_" + timeStamp + ".png");
                    }
                    var binaryReader = new BinaryReader(file.InputStream);
                    var memoryBytes = binaryReader.ReadBytes(file.ContentLength);
                    using (var memoryStream = new MemoryStream(memoryBytes))
                    {
                        var imageStream = Image.FromStream(memoryStream);
                        imageStream.Save(targetFolder + "\\" + context.Request["userName"] + "\\" + "profile_picture_" + timeStamp + ".png" , ImageFormat.Png);
                    }

                }
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}