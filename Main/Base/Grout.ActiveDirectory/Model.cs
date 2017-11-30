using System;
using System.Collections.Generic;

namespace Syncfusion.Server.ActiveDirectory.Base
{
    public class Domain
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class Objects
    {
        public string CountryKey = "c";
        public string DisplayNameKey = "displayName";
        public string GivenNameKey = "givenName";
        public string LastNameKey = "sn";
        public string MailKey = "mail";
        public string MobileKey = "mobile";
        public string SamAccountNameKey = "samaccountname";
        public string Statekey = "st";
        public string TelephoneNumberKey = "telephoneNumber";
        public string ObjectName { get; set; }
        public string ObjectType { get; set; }
        public string ObjectProperties { get; set; }
        public string ObjectPropertyValue { get; set; }
    }

    public class Users
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string EmailId { get; set; }
        public string ContactNumber { get; set; }
    }

    public class Groups
    {
        public string GroupKeyName = "group";
        public Guid GroupId { get; set; }
        public List<Users> Users { get; set; }
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }

    }
}