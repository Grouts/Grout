using System;
using System.Collections.Generic;
using System.Linq;
using Grout.Base;

namespace Grout.UMP.Models
{
    public class SearchManager
    {
        public Dictionary<string, string> GetAllActiveGroupsAndUsers()
        {
            return new Dictionary<string, string>
            {
                {"users", SearchDropDown.GetDropDownOptions((GetAllActiveUsers()))},
                {"groups", SearchDropDown.GetDropDownOptions(GetAllActiveGroups())}
            };
        }

        public List<SearchObject> GetAllActiveUsers()
        {
            UserManagement userManagement = new UserManagement();
            var userList =
                userManagement.GetAllUsers().OrderBy(x => x.DisplayName)
                    .Select(
                        row =>
                            new SearchObject
                            {
                                Key = row.UserName,
                                Value = String.IsNullOrEmpty(row.DisplayName) ? row.FirstName + " " + row.LastName : row.DisplayName
                            }).Distinct().ToList();
            return userList;
        }

        public List<SearchObject> GetAllActiveGroups()
        {
            GroupManagement groupManagement = new GroupManagement();
            var groupList = groupManagement.GetAllActiveGroups().OrderBy(x => x.GroupName)
               .Select(
                   row =>
                       new SearchObject
                       {
                           Key = row.GroupId.ToString(),
                           Value = row.GroupName,
                           Attribute = row.GroupColor
                       }).Distinct().ToList();
            return groupList;
        }
    }

    public class SearchObject
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Attribute { get; set; }
    }

    public class FilterSearchObject
    {
        public int Key { get; set; }
        public string Value { get; set; }
        public string Attribute { get; set; }
    }
}