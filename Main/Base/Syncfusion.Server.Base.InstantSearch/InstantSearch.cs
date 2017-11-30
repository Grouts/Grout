using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Web.Script.Serialization;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Globalization;
using Syncfusion.UMP.Base;


namespace Syncfusion.UMP.InstantSearch.Base
{
    
    public class InstantSearch
    {
        private static DataTable UserList = GetAllUsers();
        private static DataTable UserGroupList = GetAllGroups();
        private static DataTable  ReportList,ProjectList;
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        private static DataTable GetAllUsers()
        {
            var userUMP = new UserManagement();
            var User = userUMP.GetAllUsers();
            return User.dataTable.DefaultView.ToTable(false, new string[] { "UserName", "FullName", "Email", "FullNameLower", "ExternalId", "Avatar" });
        }
        private static DataTable GetAllGroups()
        {
            var groupUMP = new GroupManagement();
            var UserGroup = groupUMP.GroupTable();
            return UserGroup.DefaultView.ToTable(false, new string[] { "GroupId", "GroupName", "GroupDescription", "GroupNameLower", "GroupDescriptionLower", "GroupColor" });
        }
        //public static void InstantSearchInit()
        //{
        //    UserManagement userUMP = new UserManagement();
        //    GroupManagement groupUMP = new GroupManagement();
        //    ReportBaseUMP reportUMP = new ReportBaseUMP();
        //    ProjectManagement projectUMP = new ProjectManagement();
        //    var User = userUMP.FindAllUsers();
        //    var Report = reportUMP.ShowAllReports();
        //    var Project = projectUMP.GetAllProjects();
        //    UserList =User.dataTable.DefaultView.ToTable(false, new string[]{"UserName","FullName","Email","FullNameLower","ExternalId","Avatar" } );
        //    var UserGroup = groupUMP.GroupTable();
        //    UserGroupList = UserGroup.DefaultView.ToTable(false, new string[] { "GroupId", "GroupName", "GroupDescription", "GroupNameLower", "GroupDescriptionLower","GroupColor" });
        //    ReportList = Report.dataTable.DefaultView.ToTable(false, new string[] { "ReportName", "ReportDescription" });
        //    ProjectList = Project.DefaultView.ToTable(false, new string[] { "ProjectId", "ProjectName", "ProjectDescription", "ProjectNameLower", "ProjectDescriptionLower" });

        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string SearchUser(string query, SearchArgs args)
        {
            query = query.ToLower();
            var userlist = UserList.AsEnumerable().Select(row =>
          new userList
          {
              UserName = row.Field<string>("UserName"),
              FullName = row.Field<string>("FullName"),
              Email = row.Field<string>("Email"),
              FullNameLower = row.Field<string>("FullNameLower"),
              ExternalId = row.Field<string>("ExternalId"),
              Avatar = row.Field<string>("Avatar")

          }).Where(r => r.UserName != args.CurrentUser).ToList();
            var returnObj = new List<object>();
            var users = (from u in userlist where (
                             (((!string.IsNullOrEmpty(u.UserName)) ? u.UserName.ToLower() : "").StartsWith(query.ToLower()))
                             || (((!string.IsNullOrEmpty(u.FullName)) ? u.FullName.ToLower() : "").StartsWith(query.ToLower()))
                             || (((!string.IsNullOrEmpty(u.FullNameLower)) ? u.FullNameLower.ToLower() : "").StartsWith(query.ToLower()))
                             || (((!string.IsNullOrEmpty(u.Email)) ? (u.Email.IndexOf('@') == -1) ? u.Email.ToLower() : u.Email.Substring(0, u.Email.IndexOf('@')).ToLower() : "").StartsWith(query.ToLower())))
                         orderby (u.UserName + "" + u.FullName + "" + u.Email + "" + u.FullNameLower)
                         select u).ToList();
            if (args.ShowImage)
            {
                foreach (var user in users)
                {
                    var builder = new StringBuilder();
                    builder.Append("<div  id=\"");
                    builder.Append(user.FullNameLower);
                    builder.Append("\" class=\"InstantSearch\" >");
                    builder.Append("<div  data-id=\"");
                    builder.Append(user.ExternalId+"*U_"+user.UserName + "\"");
                    //Change this code to the column name which has the user proifile image
                    //if(string.IsNullOrWhiteSpace(user.UserName))

                    builder.Append("<span class=\"user-avatar\"><img src=\"" + args.baseUrl + "//uploadFiles//" + user.UserName + "//18//" + user.Avatar + "\" /></span>");
                    builder.Append("<span class=\"details\">");
                    builder.Append("<span class=\"fullname\">" + user.FullName + "</span>");
                    builder.Append("<span class=\"mail\">  - " + user.Email);
                    builder.Append("</span>");
                    builder.Append("</div>");
                    returnObj.Add(new
                    {
                        html = builder.ToString(),
                        fullName = user.UserName,
                        ExternalId=user.ExternalId
                    });
                }
            }
            else
            {
                foreach (var user in users)
                {
                    var builder = new StringBuilder();
                    builder.Append("<div  id=\"");
                    builder.Append(user.FullNameLower);
                    builder.Append("\" class=\"InstantSearch\" >");
                    builder.Append("<div  data-id=\"");
                    builder.Append(user.ExternalId + "*U_" + user.UserName + "\"");
                    builder.Append("<span class=\"details\">");
                    builder.Append("<span class=\"fullname\">" + user.FullName + "</span>");
                    builder.Append("<span class=\"mail\">  - " + user.Email);
                    //builder.Append(Regex.Replace(user.FullName, query, "<strong>" + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(query) + "</strong>", RegexOptions.IgnoreCase) + " ");
                    //builder.Append("<span class=\"mail\">  - " + Regex.Replace((user.Email.IndexOf('@') == -1) ? user.Email : user.Email.Substring(0, user.Email.IndexOf('@')), query, "<strong>" + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(query) + "</strong>", RegexOptions.IgnoreCase) + ((user.Email.IndexOf('@') == -1) ? user.Email : user.Email.Substring(user.Email.IndexOf("@"))));
                    builder.Append("</span>");
                    builder.Append("</div>");
                    returnObj.Add(new
                    {
                        html = builder.ToString(),
                        fullName = user.UserName,
                        ExternalId = user.ExternalId
                    });
                }
            }
            if (args.GetPayLoad)
            {
                foreach (var user in users)
                {
                    returnObj.Add(new
                    {
                        data = user,
                    });
                }
            }
            return serializer.Serialize(returnObj);

        }
        /// <summary>
        /// search the group name
        /// </summary>
        /// <param name="query">search the group name field</param>
        /// <returns>returns the group searched </returns>
        public string SearchGroup(string query, SearchArgs args)
        {
            query = query.ToLower();
            var usergrouplist = UserGroupList.AsEnumerable().Select(row =>
          new userGroupList
          {
              GroupId = row.Field<int>("GroupId"),
              GroupName = row.Field<string>("GroupName"),
              GroupDescription = row.Field<string>("GroupDescription"),
              GroupColor = row.Field<string>("GroupColor"),
          }).ToList();
            var returnObj = new List<object>();

            var groups = (from g in usergrouplist where ((((!string.IsNullOrEmpty(g.GroupName)) ? g.GroupName.ToLower() : "").StartsWith(query.ToLower()))||((!string.IsNullOrEmpty(g.GroupDescription)) ? g.GroupDescription.ToLower() : "").StartsWith(query.ToLower())) orderby (g.GroupName+""+g.GroupDescription) select g).Take(10).ToList();
            foreach (var group in groups)
            {
                var builder = new StringBuilder();
                builder.Append("<div data-color=\"");
                builder.Append(group.GroupColor );
                builder.Append("\" id=\"");
                builder.Append(group.GroupName);
                builder.Append("\" class=\"InstantSearch\" >");
                builder.Append("<span class=\"details\">");
                builder.Append(group.GroupName);
                //builder.Append(Regex.Replace(group.GroupName, query, "<strong>" + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(query) + "<strong>", RegexOptions.IgnoreCase));
                builder.Append("</span></div>");
                returnObj.Add(new
                {
                    html = builder.ToString(),
                    fullName = group.GroupId,
                });
            }
            if (args.GetPayLoad)
            {
                foreach (var group in groups)
                {
                    returnObj.Add(new
                    {
                        data = group
                    });
                }
            }
            return serializer.Serialize(returnObj);
        }
        /// <summary>
        /// search the project name
        /// </summary>
        /// <param name="query">search the project name field</param>
        /// <returns>returns the project searched </returns>
        public string SearchProject(string query, SearchArgs args)
        {
            query = query.ToLower();
            var projectlist = ProjectList.AsEnumerable().Select(row =>
          new projectList
          {
              ProjectId = row.Field<int>("ProjectId"),
              ProjectName = row.Field<string>("ProjectName"),
              ProjectDescription = row.Field<string>("ProjectDescription"),
          }).ToList();
            var returnObj = new List<object>();

            var projects = (from p in projectlist where ((((!string.IsNullOrEmpty(p.ProjectName)) ? p.ProjectName.ToLower() : "").StartsWith(query.ToLower())) || ((!string.IsNullOrEmpty(p.ProjectDescription)) ? p.ProjectDescription.ToLower() : "").StartsWith(query.ToLower())) orderby (p.ProjectName + "" + p.ProjectDescription) select p).Take(10).ToList();
            foreach (var project in projects)
            {
                var builder = new StringBuilder();
                builder.Append("<div  id=\"");
                builder.Append(project.ProjectName);
                builder.Append("\" class=\"InstantSearch\" >");
                builder.Append("<span class=\"details\">");
                builder.Append(Regex.Replace(project.ProjectName, query, "<strong>" + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(query) + "<strong>", RegexOptions.IgnoreCase));
                builder.Append("</span></div>");
                returnObj.Add(new
                {
                    html = builder.ToString(),
                    fullName = project.ProjectId,
                });
            }
            if (args.GetPayLoad)
            {
                foreach (var project in projects)
                {
                    returnObj.Add(new
                    {
                        data = project
                    });
                }
            }
            return serializer.Serialize(returnObj);
        }
        /// <summary>
        /// search the report name and description
        /// </summary>
        /// <param name="query">search the report field like that name and description</param>
        /// <returns>returns the report name and description </returns>
        public string SearchReport(string query, SearchArgs args)
        {
            query = query.ToLower();
            var reportlist = ReportList.AsEnumerable().Select(row =>
          new reportList
          {
              ReportName = row.Field<string>("ReportName"),
              ReportDescription = row.Field<string>("ReportDescription"),
          }).ToList();
            var returnObj = new List<object>();

            var reports = (from r in reportlist where ((((!string.IsNullOrEmpty(r.ReportName)) ? r.ReportName.ToLower() : "").StartsWith(query.ToLower())) || ((!string.IsNullOrEmpty(r.ReportDescription)) ? r.ReportDescription.ToLower() : "").StartsWith(query.ToLower())) orderby (r.ReportName + "" + r.ReportDescription) select r).Take(10).ToList();
            foreach (var report in reports)
            {
                var builder = new StringBuilder();
                builder.Append("<div  id=\"report_");
                builder.Append(report.ReportName);
                builder.Append("\" class=\"InstantSearch\" >");
                //if (report.Image != null)
                //    builder.Append("<span><img src=\"data:image/png;base64," + ImageResizer(report.Image, 24, 24) + "\"/></span>");
                //else
                    builder.Append("<span><img/></span>");
                builder.Append("<span class=\"details\">");
                builder.Append(Regex.Replace(report.ReportName, query, "<strong>" + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(query) + "</strong>", RegexOptions.IgnoreCase));
                builder.Append("</span></div>");
                returnObj.Add(new
                {
                    html = builder.ToString(),
                    fullName = report.ReportName
                });

            }
            if (args.GetPayLoad)
            {
                foreach (var report in reports)
                {
                    returnObj.Add(new
                    {
                        data = report
                    });
                }
            }
            return serializer.Serialize(returnObj);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string SearchAll(string query, SearchArgs args)
        {
            query = query.ToLower();
            var returnObj = new List<object>();
            var userlist = UserList.AsEnumerable().Select(row =>
          new userList
          {
              UserName = row.Field<string>("UserName"),
              FullName = row.Field<string>("FullName"),
              Email = row.Field<string>("Email"),
              FullNameLower = row.Field<string>("FullNameLower"),
          }).ToList();
            var projectlist = ProjectList.AsEnumerable().Select(row =>
          new projectList
          {
              ProjectName = row.Field<string>("ProjectName"),
              ProjectDescription = row.Field<string>("ProjectDescription"),
          }).ToList();
            var reportlist = ReportList.AsEnumerable().Select(row =>
          new reportList
          {
              ReportName = row.Field<string>("ReportName"),
              ReportDescription = row.Field<string>("ReportDescription"),
          }).ToList();
            var usergrouplist = UserGroupList.AsEnumerable().Select(row =>
          new userGroupList
          {
              GroupName = row.Field<string>("GroupName"),
              GroupDescription = row.Field<string>("GroupDescription"),
          }).ToList();
            var users = (from u in userlist where ((((!string.IsNullOrEmpty(u.UserName)) ? u.UserName.ToLower() : "").StartsWith(query.ToLower())) || (((!string.IsNullOrEmpty(u.FullName)) ? u.FullName.ToLower() : "").StartsWith(query.ToLower())) || (((!string.IsNullOrEmpty(u.FullNameLower)) ? u.FullNameLower.ToLower() : "").StartsWith(query.ToLower())) || u.Email.Substring(0, u.Email.IndexOf('@')).StartsWith(query) || (((!string.IsNullOrEmpty(u.Email)) ? u.Email.ToLower() : "").StartsWith(query.ToLower()))) orderby (u.UserName + "" + u.FullName + "" + u.Email + "" + u.FullNameLower) select u).ToList();
            var groups = (from g in usergrouplist where ((((!string.IsNullOrEmpty(g.GroupName)) ? g.GroupName.ToLower() : "").StartsWith(query.ToLower())) || ((!string.IsNullOrEmpty(g.GroupDescription)) ? g.GroupDescription.ToLower() : "").StartsWith(query.ToLower())) orderby (g.GroupName + "" + g.GroupDescription) select g).Take(10).ToList();
            var reports = (from r in reportlist where ((((!string.IsNullOrEmpty(r.ReportName)) ? r.ReportName.ToLower() : "").StartsWith(query.ToLower())) || ((!string.IsNullOrEmpty(r.ReportDescription)) ? r.ReportDescription.ToLower() : "").StartsWith(query.ToLower())) orderby (r.ReportName + "" + r.ReportDescription) select r).Take(10).ToList();
            var projects = (from p in projectlist where ((((!string.IsNullOrEmpty(p.ProjectName)) ? p.ProjectName.ToLower() : "").StartsWith(query.ToLower())) || ((!string.IsNullOrEmpty(p.ProjectDescription)) ? p.ProjectDescription.ToLower() : "").StartsWith(query.ToLower())) orderby (p.ProjectName + "" + p.ProjectDescription) select p).Take(10).ToList();
            if (args.ShowImage)
            {
                foreach (var user in users)
                {
                    var builder = new StringBuilder();
                    builder.Append("<div  id=\"");
                    builder.Append(user.FullNameLower);
                    builder.Append("\" class=\"InstantSearch\" >");
                    //Change this code to the column name which has the user proifile image
                    //if(string.IsNullOrWhiteSpace(user.UserName))
                    if (10 > 20)
                        builder.Append("<span class=\"user-avatar\"><img src=\"data:image/png;base64," + ImageResizer(user.UserName, 24, 24) + "\" /></span>");
                    else
                        builder.Append("<span class=\"user-avatar\"><img src=\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAQ38AAEN/Ab+JkPQAAAAadEVYdFNvZnR3YXJlAFBhaW50Lk5FVCB2My41LjExR/NCNwAAA4xJREFUSEul1XtQVFUcB/CzKyQSGa4o4NC049hMo8iAqTXSVGZKqf1BQ0zjCxn6IxB1fOSoGNuogc/KMDCjN0QYYinIOD5wfW3hI10VChDTDSubDE0Qgd2v33Ovd+fucJG1fjOf2bt3z/1977n3HBB+1Hiqon/JTdfufn+NAul/VQZ1Enqwj8LoP9VYaiejxnp2CqL7KhPtpm4NXxgXjYlkMpn059+k+yor3SJ9E0SEheLyoXx01BbDlpkIs9kb0kwPkd+VSj7NZbOijfOBi7uApp1w//w1UhOf1n73UCL5XQXkbR4fa8WrLz2JroZywLVP1VgO14ENeCAwQBuXT36XvX9IPziKFqF66xs4WJiBK4c28e4rgN+PAldIzqT2MzwVY9UCflSu9KP6UJMlNAS3T30InFwPnHqXzT4Hft0N/Fmjksc8N33KaC3gKgVTr/UwtQT1DcSNms13AzYqd6vOwKG6WKkEzJw6RguQSzqKeq1nyS2Xoauad64EbADOFwIXdgC/Vauavue5T5Ew7nEtQO7ykWRYZgqhBGoguSrwQ2k2A9hchjgLgPpSPhreuXw8DdvgOfsRHo0coAXIa0ZTt5Kb6hWSO/Jv8u7eT1anAT+9rwac/gCo+4KNv6UyHn+F5v05+r0gZxBDhhVAcuf+RcrdS0OjwtC0J0d9B9LZLWpI3ZdwO7cgeVKs1lxqo0jqsWbQP+QNkKKHRaDzxHtqgJyNfFTOfHTWrMUjg0P0AbUkV+A9K472kPfCCEswOhzvqAHyfSif6+E5sRbDrRZ9gI16rXi6RBDyj9mD4Zg4aYL6DjRyT5z7GC32VRgyIh4iIEg2byQL3bNMIjwmQox6PUM8k9UiZtshVrRj1KLvfAPkeziTh+2FNoi3OiAW/wExa3+zSCopEGPSE8Xg6EFKL5+KfGKgSPqmVMxrbBXZXRA2ePWfexKtR1b7hlDqykKfcQoZOLf+ppiclyuCw+TCYQWF9hHJZZXC5ul+AZmXXoOzbKlP83ZHLqIWHDYcr8hq84jnV2WqASOSnxPLbngMB0qcUXEBD3QBx0pWwLzsuvF4TbrzqrA8xveSVLLVcIDOkjXcZLqArHWbeN54xl7Z/H1C7gIhZu11GQ7QeXF5ibe5+/g6xC2uMBzXzYyqX/ivPXOKSKl2iTRHa0+s0ze33bTbOtsOv327rmzhrb7Tyg3H+Ug5cFmMnfPyHfAYOsmAWDZmAAAAAElFTkSuQmCC\" /></span>");
                    builder.Append("<span class=\"details\">");
                    builder.Append(Regex.Replace(user.FullName, query, "<strong>" + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(query) + "</strong>", RegexOptions.IgnoreCase) + " ");
                    builder.Append(" - " + Regex.Replace(user.Email.Substring(0, user.Email.IndexOf('@')), query, "<strong>" + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(query) + "</strong>", RegexOptions.IgnoreCase) + user.Email.Substring(user.Email.IndexOf("@")));
                    builder.Append("</span>");
                    builder.Append("</div>");
                    returnObj.Add(new
                    {
                        html = builder.ToString(),
                        fullName = user.FullName,
                    });
                }
            }
            else
            {
                foreach (var user in users)
                {
                    var builder = new StringBuilder();
                    builder.Append("<div  id=\"");
                    builder.Append(user.FullNameLower);
                    builder.Append("\" class=\"InstantSearch\" >");
                    builder.Append("<span class=\"details\">");
                    builder.Append(Regex.Replace(user.FullName, query, "<strong>" + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(query) + "</strong>", RegexOptions.IgnoreCase) + " ");
                    builder.Append(" - " + Regex.Replace(user.Email.Substring(0, user.Email.IndexOf('@')), query, "<strong>" + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(query) + "</strong>", RegexOptions.IgnoreCase) + user.Email.Substring(user.Email.IndexOf("@")));
                    builder.Append("</span>");
                    builder.Append("</div>");
                    returnObj.Add(new
                    {
                        html = builder.ToString(),
                        fullName = user.FullName,
                    });
                }
            }
            foreach (var project in projects)
            {
                var builder = new StringBuilder();
                builder.Append("<div  id=\"");
                builder.Append(project.ProjectName);
                builder.Append("\" class=\"InstantSearch\" >");
                builder.Append("<span class=\"details\">");
                builder.Append(Regex.Replace(project.ProjectName, query, "<strong>" + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(query) + "<strong>", RegexOptions.IgnoreCase));
                builder.Append("</span></div>");
                returnObj.Add(new
                {
                    html = builder.ToString(),
                    fullName = project.ProjectName
                });
            }
            foreach (var group in groups)
            {
                var builder = new StringBuilder();
                builder.Append("<div  id=\"");
                builder.Append(group.GroupName);
                builder.Append("\" class=\"InstantSearch\" >");
                builder.Append("<span class=\"details\">");
                builder.Append(Regex.Replace(group.GroupName, query, "<strong>" + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(query) + "<strong>", RegexOptions.IgnoreCase));
                builder.Append("</span></div>");
                returnObj.Add(new
                {
                    html = builder.ToString(),
                    fullName = group.GroupName
                });
            }
            foreach (var report in reports)
            {
                var builder = new StringBuilder();
                builder.Append("<div  id=\"report_");
                builder.Append(report.ReportName);
                builder.Append("\" class=\"InstantSearch\" >");
                //if (report.Image != null)
                //    builder.Append("<span><img src=\"data:image/png;base64," + ImageResizer(report.Image, 24, 24) + "\"/></span>");
                //else
                builder.Append("<span><img/></span>");
                builder.Append("<span class=\"details\">");
                builder.Append(Regex.Replace(report.ReportName, query, "<strong>" + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(query) + "</strong>", RegexOptions.IgnoreCase));
                builder.Append("</span></div>");
                returnObj.Add(new
                {
                    html = builder.ToString(),
                    fullName = report.ReportName
                });

            }
            if (args.GetPayLoad)
            {
                foreach (var user in users)
                {
                    returnObj.Add(new
                    {
                        data = user,
                    });
                }
                foreach (var group in groups)
                {
                    returnObj.Add(new
                    {
                        data = group
                    });
                }
            
                foreach (var report in reports)
                {
                    returnObj.Add(new
                    {
                        data = report
                    });
                }
                foreach (var project in projects)
                {
                    returnObj.Add(new
                    {
                        data = project
                    });
                }
            }
            return serializer.Serialize(returnObj);
        }
        /// <summary>
        /// Dynamically resizes images
        /// </summary>
        /// <param name="source">Base64 Image string</param>
        /// <param name="height">new height</param>
        /// <param name="width">new width</param>
        /// <returns>Base64 string of resized image</returns>
        private string ImageResizer(string source, int height, int width)
        {
            var bytes = Convert.FromBase64String(source);
            Image image;
            using (var stream = new MemoryStream(bytes))
            {
                image = Image.FromStream(stream);
            }
            var s = new Size();
            s.Height = height; 
            s.Width = width;
            var newImg = new Bitmap(image, s);

            using (var ms = new MemoryStream())
            {
                newImg.Save(ms, ImageFormat.Png); 
                bytes = ms.ToArray();
            }
            return Convert.ToBase64String(bytes);
        }
        /// <summary>
        ///  Dynamically resizes images
        /// </summary>
        /// <param name="bytes">Image as byte array</param>
        /// <param name="height">New Image height</param>
        /// <param name="width">New Image width</param>
        /// <returns></returns>
        private string ImageResizer(byte[] bytes, int height, int width)
        {
            Image image;
            using (var stream = new MemoryStream(bytes))
            {
                image = Image.FromStream(stream);
            }
            var s = new Size();
            s.Height = height;
            s.Width = width;
            var newImg = new Bitmap(image, s);

            using (var ms = new MemoryStream())
            {
                newImg.Save(ms, ImageFormat.Png);
                bytes = ms.ToArray();
            }
            return Convert.ToBase64String(bytes);
        }

    }
}
