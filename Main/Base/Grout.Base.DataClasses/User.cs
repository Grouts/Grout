using System;
using System.Collections.Generic;
using Grout.Base;

namespace Grout.Base.DataClasses
{
    public class User
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DisplayName { get; set; }

        public string TrimmedDisplayName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(DisplayName) == false)
                {
                    return DisplayName.Length > 35 ? DisplayName.Substring(0, 35) + "..." : DisplayName;
                }
                return String.Empty;
            }
            set
            {

            }
        }

        public string Email { get; set; }

        public string Password { get; set; }

        public bool IsActivated { get; set; }

        public string Avatar { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string TimeZone { get; set; }

        public UserStatus Status { get; set; }

        public string StatusDescription
        {
            get { return Status == UserStatus.Active ? Status.ToString() : "Inactive"; }
        }

        public List<UserGroup> UserGroups { get; set; }

        public DateTime? LastLogin { get; set; }

        public bool IsDeleted { get; set; }

        public string ContactNumber { get; set; }

        public DateTime ActivationExpirationDate { get; set; }

        public string ActivationCode { get; set; }

        public string ResetPasswordCode { get; set; }

        public bool IsActive { get; set; }

    }
}
