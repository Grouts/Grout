using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using Grout.Base;
using Grout.Base.DataClasses;
using System.Globalization;

namespace Grout.UMP.Models
{
    public class UserModel
    {
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
            var userManagement = new UserManagement();
            try
            {
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

                newlocation = newlocation + "\\" + profile.ImageName;

                var resizedImage = Image.FromFile(newlocation);

                var bitMapImage = new Bitmap(200, 200);

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

                var resultedImageName = imageName + ".png";

                var updateColumns = new List<UpdateColumn> 
                {
                    new UpdateColumn 
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.Picture,
                        Value = resultedImageName
                    },
                    new UpdateColumn 
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.ModifiedDate,
                        Value = timeNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    }
                };

                userManagement.UpdateUserProfileDetails(updateColumns, profile.UserId);

                return resultedImageName;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool DeleteAvatar(int userId)
        {
            var updateColumns = new List<UpdateColumn> 
                {
                    new UpdateColumn 
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.Picture,
                        Value = null
                    },
                    new UpdateColumn 
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.ModifiedDate,
                        Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    }
                };

            return _userDetails.UpdateUserProfileDetails(updateColumns, userId);
        }

        public string DefaultavatarTransfer(string path, string username, DateTime timeNow)
        {
            var id = 2;
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

            um.UpdateUserProfileDetails(updateColumns, id);

            profilebmp.Dispose();
            smbmp.Dispose();
            vsmbmp.Dispose();
            hsmbmp.Dispose();
            lowbmp.Dispose();

            return imageName + ".png";
        }
    }
}