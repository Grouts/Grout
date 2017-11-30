namespace Syncfusion.Server.Base.DataClasses
{
    public class ResponseClass
    {
        public enum LoginResponse
        {
            ValidUser,
            InvalidUserName,
            InvalidPassword,
            ThrottledUser,
            DeactivatedUser,
            DeletedUser
        }
    }
}
