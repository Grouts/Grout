using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syncfusion.UMP.InstantSearch.Base
{
    public class SearchArgs
    {
        /// <summary>
        /// Gets or sets the ShowImage attribute
        /// </summary>
        public bool ShowImage { get; set; }
        /// <summary>
        /// Gets or sets the GetPayLoad attribute
        /// </summary>
        public bool GetPayLoad { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CurrentUser { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string baseUrl { get; set; }
    }
    //public class SearchAll
    //{
    //    public List<object> UserList { get; set; }
    //    public string Grouplist { get; set; }
    //    public string ProjectList { get; set; }
    //    public string ReportList { get; set; }
    //}
    public class userList
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string FullNameLower { get; set; }
        public string ExternalId { get; set; }
        public string Avatar { get; set; }
    }
    public class userGroupList
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
        public string GroupColor { get; set; }

    }
    public class projectList
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }

    }
    public class reportList
    {
        public string ReportName { get; set; }
        public string ReportDescription { get; set; }

    }
}
