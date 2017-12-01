using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using Grout.Base;
using Grout.Base.Data;
using Grout.Base.DataClasses;

namespace Grout.UMP.Models
{
    public class GroupModels
    {
        private readonly GroupManagement _groupManagement = new GroupManagement();
        private readonly UserManagement _userManagement = new UserManagement();
        private readonly IRelationalDataProvider _dataProvider;
        private readonly IQueryBuilder _queryBuilder;

        public GroupModels()
        {
            if (GlobalAppSettings.DbSupport == DataBaseType.MSSQLCE)
            {
                _dataProvider = new SqlCeRelationalDataAdapter(Connection.ConnectionString);
                _queryBuilder = new SqlCeQueryBuilder();
            }
            else
            {
                _dataProvider = new SqlRelationalDataAdapter(Connection.ConnectionString);
                _queryBuilder = new SqlQueryBuilder();
            }
        }

        public Group GetGroupById(int groupId)
        {
            return _groupManagement.GetGroupById(groupId);
        }


        public bool EditGroup(Group groupInfo)
        {
            var updateColumns = new List<UpdateColumn>
                {
                    new UpdateColumn {ColumnName = GlobalAppSettings.DbColumns.DB_Group.Color, Value = groupInfo.GroupColor},
                    new UpdateColumn {ColumnName = GlobalAppSettings.DbColumns.DB_Group.Description, Value = groupInfo.GroupDescription},
                    new UpdateColumn {ColumnName = GlobalAppSettings.DbColumns.DB_Group.Name, Value = groupInfo.GroupName}
                };
            return _groupManagement.UpdateGroup(updateColumns, groupInfo.GroupId);
        }

        public bool DeleteGroup(int groupId)
        {
            return _groupManagement.DeleteGroup(groupId);
        }

        public EntityData<User> GetUsersOfGroup(int groupId, List<SortCollection> sorted, int? skip, int? take, string searchKey = "", List<FilterCollection> filterCollection = null)
        {
            int? skipValue = String.IsNullOrWhiteSpace(skip.ToString()) ? (int?)null : skip.Value;
            var takeValue = String.IsNullOrWhiteSpace(skip.ToString()) ? (int?)null : take.Value;
            int usersCount;
            var userList = _groupManagement.GetAllUsersOfGroup(groupId, sorted, skipValue, takeValue, searchKey, filterCollection,out usersCount);
            return new EntityData<User>
            {
                result = userList,
                count = usersCount
            };
        }

        public EntityData<Group> GetActiveGroups(List<SortCollection> sorted, int? skip, int? take,
            string searchKey = null, List<FilterCollection> filterCollection = null)
        {
            var skipValue = skip.HasValue ? skip.Value : 0;
            var takeValue = take.HasValue ? take.Value : 10;
            var orderby = OrderByType.Asc;
            var searchDescriptor = new List<String> { "Name", "Description" };

            var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Group.IsActive,
                        Condition = Conditions.Equals,
                        Value = true
                    }
                };

           

            var query = _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Group.DB_TableName, whereColumns);



            var data =
                _dataProvider.ExecuteReaderQuery(_groupManagement.FilteringHelper(query, searchKey, filterCollection, sorted, searchDescriptor));

            var result = data.DataTable.AsEnumerable().Select(row => new Group
            {
                GroupId = row.Field<int>(GlobalAppSettings.DbColumns.DB_Group.GroupId),
                GroupName = row.Field<string>(GlobalAppSettings.DbColumns.DB_Group.Name),
                GroupDescription = row.Field<string>(GlobalAppSettings.DbColumns.DB_Group.Description),
                GroupColor = row.Field<string>(GlobalAppSettings.DbColumns.DB_Group.Color),
                CanDelete =
                    (row.Field<int>(GlobalAppSettings.DbColumns.DB_Group.GroupId) == 1) ? false : true
             
            }).Skip(skipValue).Take(takeValue).ToList();
            

            return new EntityData<Group>
            {
                result = result,
                count = data.DataTable.Rows.Count
            };
            
        }

     
        public bool AddUserinGroup(List<int> userList,int groupId)
        {
            try
            {
                for (var r = 0; r < userList.Count; r++)
                {
                    var userId = userList[r];
                    var isPresent = _groupManagement.SearchUserInGroupwithGroupId(userId, groupId);

                    if (isPresent.Count == 0)
                        _groupManagement.AddUserInGroup(userId, groupId);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CheckGroupName(string Groupname)
        {
            return _groupManagement.GetGroupByName(Groupname) != null ? true : false;
        }

    }
}