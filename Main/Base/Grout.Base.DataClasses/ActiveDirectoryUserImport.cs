using System;

namespace Grout.Base.DataClasses
{
    public class ActiveDirectoryUserImport
    {
        public string UserName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName { get; set; }

        public string DisplayMessage { get; set; }

        public bool IsExist { get; set; }

        public string EmailId { get; set; }

        public string ContactNumber { get; set; }
    }
}
