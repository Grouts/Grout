namespace Grout.Base.DataClasses
{
    public class ProfilePicture
    {
        public int LeftOfCropArea { get; set; }  // X1

        public int TopOfCropAea { get; set; }  // Y1

        public int LeftToCropArea { get; set; }  // X2

        public int TopToCropArea { get; set; }  // Y2

        public int Height { get; set; }

        public int Width { get; set; }

        public string ImageName { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public bool IsNewFile { get; set; }
    }
}
