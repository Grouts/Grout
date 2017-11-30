using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grout.Base.DataClasses
{
    public class UserDetailResponse
    {
        public byte[] Avatar { get; set; }
        public string ContactNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string TimeZone { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
    }
}
