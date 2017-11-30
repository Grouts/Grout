using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Grout.UMP.Models
{
    public static class ImageManager
    {
        /// <summary>
        /// Resize image file without affecting the aspect ratio
        /// </summary>
        /// <param name="image">Image file which is to be resized</param>
        /// <param name="width">Required with to be resized</param>
        /// <param name="height">Required with to be resized</param>
        /// <returns></returns>
        public static Image ResizeImage(Image image, int width, int height)
        {
            var sourceWidth = image.Width;
            var sourceHeight = image.Height;
            var sourcePositionX = 0;
            var sourcePositionY = 0;
            var destinationPostionX = 0;
            var destinationPositionY = 0;

            float percentRatio = 0;
            float widthPercentRatio = 0;
            float heightPercentRatio = 0;

            widthPercentRatio = ((float)width / (float)sourceWidth);
            heightPercentRatio = ((float)height / (float)sourceHeight);

            if (heightPercentRatio < widthPercentRatio)
            {
                percentRatio = heightPercentRatio;
                destinationPostionX = Convert.ToInt16((width -
                              (sourceWidth * percentRatio)) / 2);
            }
            else
            {
                percentRatio = widthPercentRatio;
                destinationPositionY = Convert.ToInt16((height -
                              (sourceHeight * percentRatio)) / 2);
            }

            var destinationWidth = (int)(sourceWidth * percentRatio);
            var destinationHeight = (int)(sourceHeight * percentRatio);

            var bitMap = new Bitmap(width, height,
                              PixelFormat.Format32bppPArgb);
            bitMap.SetResolution(image.HorizontalResolution,
                             image.VerticalResolution);

            var graphics = Graphics.FromImage(bitMap);
            graphics.Clear(Color.FromKnownColor(KnownColor.Transparent));
            graphics.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;

            graphics.DrawImage(image,
                new Rectangle(destinationPostionX, destinationPositionY, destinationWidth, destinationHeight),
                new Rectangle(sourcePositionX, sourcePositionY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            graphics.Dispose();
            return bitMap;
        }
    }
}