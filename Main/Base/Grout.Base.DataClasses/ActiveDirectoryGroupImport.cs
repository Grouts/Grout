using System;

namespace Grout.Base.DataClasses
{
    public class ActiveDirectoryGroupImport
    {
        public string GroupName { get; set; }

        public string DisplayMessage { get; set; }

        public bool IsExist { get; set; }
    }
}
