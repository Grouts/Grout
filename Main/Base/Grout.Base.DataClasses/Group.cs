using System;
using System.Collections.Generic;

namespace Grout.Base.DataClasses
{
    public class Group
    {
        public int GroupId { get; set; }
        
        public string GroupName { get; set; }

        public string GroupColor { get; set; }

        public string GroupDescription { get; set; }

        public bool CanDelete { get; set; }

        public List<User> Users { get; set; }

        public int UsersCount { get; set; }


    }
}
