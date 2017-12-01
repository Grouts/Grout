using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Grout.Base.Data;
using Grout.Base.DataClasses;
using Grout.Base.Logger;

namespace Grout.Base
{
    public class GroupManagement : IDisposable, IGroupManagement
    {
        private bool _disposed;
        private readonly IRelationalDataProvider _dataProvider;
        private readonly IQueryBuilder _queryBuilder;

        public GroupManagement(IQueryBuilder builder, IRelationalDataProvider provider)
        {
            _queryBuilder = builder;
            _dataProvider = provider;
        }

        public GroupManagement()
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

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        public List<Group> GetAllActiveGroups()
        {
            var result = new Result();

            var groupList = new List<Group>();
            try
            {

                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Group.IsActive,
                        Condition = Conditions.Equals,
                        Value = true
                    }
                };

                
                result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Group.DB_TableName, whereColumns));
                if (result.Status)
                {
                    groupList =
                        result.DataTable.AsEnumerable()
                            .Select(row => new Group
                            {
                                GroupId = row.Field<int>(GlobalAppSettings.DbColumns.DB_Group.GroupId),
                                GroupName = row.Field<string>(GlobalAppSettings.DbColumns.DB_Group.Name),
                                GroupDescription = row.Field<string>(GlobalAppSettings.DbColumns.DB_Group.Description),
                                GroupColor = row.Field<string>(GlobalAppSettings.DbColumns.DB_Group.Color),
                                CanDelete =
                                    (row.Field<int>(GlobalAppSettings.DbColumns.DB_Group.GroupId) == 1) ? false : true
                            }).ToList();
                }
                return groupList;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting group list", e,
                    MethodBase.GetCurrentMethod(), " Status - " + result.Status);
                return groupList;
            }
        }

        public int? AddGroup(Group group)
        {
            try
            {
                var values = new Dictionary<string, object>
                {
                    {GlobalAppSettings.DbColumns.DB_Group.Name, group.GroupName},
                    {GlobalAppSettings.DbColumns.DB_Group.Description, group.GroupDescription},
                    {GlobalAppSettings.DbColumns.DB_Group.Color, group.GroupColor},
                    {GlobalAppSettings.DbColumns.DB_Group.IsActive, true},
                    {
                        GlobalAppSettings.DbColumns.DB_Group.ModifiedDate,
                        DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    }
                };
                var output = new List<string>
                {
                    GlobalAppSettings.DbColumns.DB_Group.GroupId
                };
                var result = _dataProvider.ExecuteScalarQuery(_queryBuilder.AddToTable(
                    GlobalAppSettings.DbColumns.DB_Group.DB_TableName,
                    values, output));
                if (result.Status)
                {
                    return Convert.ToInt32(result.ReturnValue);
                }
                return null;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while adding group", e,
                    MethodBase.GetCurrentMethod(), " GroupName - " + group.GroupName + " GroupDescription - " + group.GroupDescription + " GroupColor - " + group.GroupColor);
                return null;
            }
        }

        public bool UpdateGroup(List<UpdateColumn> updateColumns, int groupId)
        {
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Group.GroupId,
                        Condition = Conditions.Equals,
                        Value = groupId
                    }
                };
                var result = _dataProvider.ExecuteNonQuery(
                    _queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_Group.DB_TableName, updateColumns,
                        whereColumns));
                return result.Status;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while updating group", e,
                    MethodBase.GetCurrentMethod(), " GroupId - " + groupId);
                return false;
            }
        }

        public DataResponse IsUserExistInGroup(int userId, int groupId)
        {
            var dataResponse = new DataResponse();
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_UserGroup.UserId,
                        Condition = Conditions.Equals,
                        Value = userId
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_UserGroup.GroupId,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = groupId
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_UserGroup.IsActive,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = true
                    }
                };
                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(
                            GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName, whereColumns));
                dataResponse.Success = result.Status;
                if (result.Status)
                {
                    dataResponse.Value = (result.DataTable.Rows.Count != 0);
                }
                return dataResponse;
            }
            catch (Exception e)
            {
                dataResponse.Success = false;
                LogExtension.LogError("Error while validating user existance in group", e,
                    MethodBase.GetCurrentMethod(), " UserId - " + userId + " GroupId - " + groupId);
                return dataResponse;
            }
        }

        public bool AddUserInGroup(int userId, int groupId)
        {
            try
            {
                var values = new Dictionary<string, object>
                {
                    {GlobalAppSettings.DbColumns.DB_UserGroup.GroupId, groupId},
                    {GlobalAppSettings.DbColumns.DB_UserGroup.UserId, userId},
                    {GlobalAppSettings.DbColumns.DB_UserGroup.IsActive, true},
                    {
                        GlobalAppSettings.DbColumns.DB_UserGroup.ModifiedDate,
                        DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    }
                };
                var result = _dataProvider.ExecuteNonQuery(
                    _queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName, values));
                return result.Status;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while add user in group", e,
                    MethodBase.GetCurrentMethod(), " UserId - " + userId + " GroupId - " + groupId);
                return false;
            }
        }

        public Group GetGroupByName(string groupName)
        {
            var groupDetail = new Group();
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Group.Name,
                        Condition = Conditions.Equals,
                        Value = groupName
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Group.IsActive,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = true
                    }
                };
                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Group.DB_TableName,
                            whereColumns));
                if (result.Status)
                {
                    groupDetail =
                        result.DataTable.AsEnumerable()
                            .Select(row => new Group
                            {
                                GroupId = row.Field<int>(GlobalAppSettings.DbColumns.DB_Group.GroupId),
                                GroupName = row.Field<string>(GlobalAppSettings.DbColumns.DB_Group.Name),
                                GroupDescription = row.Field<string>(GlobalAppSettings.DbColumns.DB_Group.Description),
                                GroupColor = row.Field<string>(GlobalAppSettings.DbColumns.DB_Group.Color)
                            }).FirstOrDefault();
                }
                return groupDetail;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting group details from group name", e,
                    MethodBase.GetCurrentMethod(), " GroupName - " + groupName + " GroupId - " + groupDetail.GroupId + " GroupDescription - " + groupDetail.GroupDescription + " GroupColor - " + groupDetail.GroupColor);
                return groupDetail;
            }
        }

        public Group GetGroupById(int groupId)
        {
            var groupDetail = new Group();
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Group.GroupId,
                        Condition = Conditions.Equals,
                        Value = groupId
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Group.IsActive,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = true
                    }
                };
                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Group.DB_TableName,
                            whereColumns));
                if (result.Status)
                {
                    groupDetail =
                        result.DataTable.AsEnumerable()
                            .Select(row => new Group
                            {
                                GroupId = row.Field<int>(GlobalAppSettings.DbColumns.DB_Group.GroupId),
                                GroupName = row.Field<string>(GlobalAppSettings.DbColumns.DB_Group.Name),
                                GroupDescription = row.Field<string>(GlobalAppSettings.DbColumns.DB_Group.Description),
                                GroupColor = row.Field<string>(GlobalAppSettings.DbColumns.DB_Group.Color)
                            }).FirstOrDefault();
                }
                return groupDetail;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting group details from groupid", e,
                    MethodBase.GetCurrentMethod(), " GroupId - " + groupId + " GroupName - " + groupDetail.GroupName + " GroupDescription - " + groupDetail.GroupDescription + " GroupColor - " + groupDetail.GroupColor);
                return groupDetail;
            }
        }

        public List<User> GetAllUsersOfGroup(int groupId, List<SortCollection> sorted, int? skip, int? take, string searchKey, List<FilterCollection> filterCollection, out int usersCount)
        {
            var userResult = new List<User>();
            var searchDescriptor = new List<String> { "DisplayName", "Email" };
            try
            {
                var selected = new List<SelectedColumn>
                {
                    new SelectedColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                        ColumnName = "*"
                    },
                    new SelectedColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName,
                        ColumnName = GlobalAppSettings.DbColumns.DB_UserGroup.GroupId
                    },
                    new SelectedColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName,
                        ColumnName = GlobalAppSettings.DbColumns.DB_UserGroup.UserId
                    }
                };
                var joinSpecification = new List<JoinSpecification>
                {
                    new JoinSpecification
                    {
                        Table = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                        Column =
                            new List<JoinColumn>
                            {
                                new JoinColumn
                                {
                                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                                    JoinedColumn = GlobalAppSettings.DbColumns.DB_User.UserId,
                                    Operation = Conditions.Equals,
                                    ParentTableColumn = GlobalAppSettings.DbColumns.DB_UserGroup.UserId,
                                    ParentTable = GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName
                                }
                            },
                        JoinType = JoinTypes.Inner
                    }
                };
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName,
                        ColumnName = GlobalAppSettings.DbColumns.DB_UserGroup.GroupId,
                        Condition = Conditions.Equals,
                        Value = groupId
                    },
                    new ConditionColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName,
                        ColumnName = GlobalAppSettings.DbColumns.DB_UserGroup.IsActive,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = true
                    }
                };
                var query = _queryBuilder.ApplyWhereClause(
                        _queryBuilder.ApplyMultipleJoins(GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName, selected,
                            joinSpecification), whereColumns);
                var result =
                    _dataProvider.ExecuteReaderQuery(FilteringHelper(query, searchKey, filterCollection, sorted, searchDescriptor));
                if (result.Status)
                {
                    var userList =
                        result.DataTable.AsEnumerable()
                            .Select(row => new User
                            {
                                UserId = row.Field<int>(GlobalAppSettings.DbColumns.DB_User.UserId),
                                UserName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.UserName),
                                FirstName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.FirstName),
                                LastName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.LastName),
                                DisplayName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.DisplayName),
                                Email = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Email),
                                Avatar = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Picture),
                                CreatedDate = row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_User.CreatedDate),
                                ModifiedDate = row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_User.ModifiedDate)
                            });

                    usersCount = userList.ToList().Count;
                    if (!String.IsNullOrWhiteSpace(skip.ToString()) && !String.IsNullOrWhiteSpace(take.ToString()))
                    {
                        userResult = userList.Skip(Convert.ToInt32(skip)).Take(Convert.ToInt32(take)).ToList();
                    }
                    else
                    {
                        userResult = userList.ToList();
                    }
                }
                else
                {
                    usersCount = 0;
                }
                return userResult;

            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting user list of group", e,
                    MethodBase.GetCurrentMethod(), " GroupId - " + groupId + " SearchKey - " + searchKey);
                usersCount = 0;
                return userResult;
            }
        }

        public bool DeleteGroup(int groupId)
        {
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Group.GroupId,
                        Condition = Conditions.Equals,
                        Value = groupId
                    }
                };
                var updateColumns = new List<UpdateColumn>
                {
                    new UpdateColumn {ColumnName = GlobalAppSettings.DbColumns.DB_Group.IsActive, Value = 0}
                };
                var result = _dataProvider.ExecuteNonQuery(_queryBuilder.UpdateRowInTable(
                    GlobalAppSettings.DbColumns.DB_Group.DB_TableName, updateColumns, whereColumns));
                return result.Status;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while deleting group", e,
                    MethodBase.GetCurrentMethod(), " GroupId - " + groupId);
                return false;
            }
        }

        public bool DeleteUserFromGroup(int userId, int groupId)
        {
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_UserGroup.UserId,
                        Condition = Conditions.Equals,
                        Value = userId
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_UserGroup.GroupId,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = groupId
                    }
                };

                var result = _dataProvider.ExecuteNonQuery(
                    _queryBuilder.DeleteRowFromTable(GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName,
                        whereColumns));
                return result.Status;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while removing user from group", e,
                    MethodBase.GetCurrentMethod(), " UserId - " + userId + " GroupId - " + groupId);
                return false;
            }
        }

        public DataResponse IsActiveGroup(int groupId)
        {
            var dataResponse = new DataResponse();
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Group.GroupId,
                        Condition = Conditions.Equals,
                        Value = groupId
                    }
                };

                var requiredColumns = new List<SelectedColumn>
                {
                    new SelectedColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Group.IsActive,
                    }
                };

                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectRecordsFromTable(GlobalAppSettings.DbColumns.DB_Group.DB_TableName,
                            requiredColumns, whereColumns));

                dataResponse.Success = result.Status;
                if (result.Status)
                {
                    dataResponse.Value = result.DataTable.AsEnumerable()
                        .Select(s => s.Field<bool>(GlobalAppSettings.DbColumns.DB_Group.IsActive))
                        .FirstOrDefault();
                }
                return dataResponse;
            }
            catch (Exception e)
            {
                dataResponse.Success = false;
                LogExtension.LogError("Error while validating group", e,
                    MethodBase.GetCurrentMethod(), " GroupId - " + groupId);
                return dataResponse;
            }
        }

        public DataResponse IsGroupAlreadyExist(string groupName)
        {
            var dataResponse = new DataResponse();
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Group.Name,
                        Condition = Conditions.Equals,
                        Value = groupName
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Group.IsActive,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = true
                    }
                };
                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Group.DB_TableName,
                            whereColumns));
                dataResponse.Success = result.Status;
                if (result.Status)
                {
                    dataResponse.Value = (result.DataTable.Rows.Count != 0);
                }
                return dataResponse;
            }
            catch (Exception e)
            {
                dataResponse.Success = false;
                LogExtension.LogError("Error while validating group name", e,
                    MethodBase.GetCurrentMethod(), " GroupName - " + groupName);
                return dataResponse;
            }
        }

       

        public List<Group> SearchUserInGroupwithGroupId(int userId, int groupId)
        {
            var groupList = new List<Group>();

            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserGroup.UserId,
                    Condition = Conditions.Equals,
                    Value = userId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserGroup.GroupId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = groupId
                }
            };

            try
            {
                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName,
                            whereColumns));
                if (result.Status)
                {
                    groupList =
                        result.DataTable.AsEnumerable()
                        .Select(r => new Group
                        {
                            GroupId = r.Field<int>(GlobalAppSettings.DbColumns.DB_UserGroup.GroupId)
                        }).ToList();
                }
                return groupList;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting group list with whereConditionColumns", e, MethodBase.GetCurrentMethod(), " UserId - " + userId + " GroupId - " + groupId);
                return groupList;
            }

        }

       

        public string FilteringHelper(string query, string searchKey, List<FilterCollection> filterCollection, List<SortCollection> sortCollection, List<string> searchDescriptor)
        {
            var sortKey = "ModifiedDate";
            var sortValue = "desc";
            if (sortCollection != null && sortCollection.Any())
            {
                if (sortCollection.ElementAt(0).Name == "GroupName")
                    sortCollection.ElementAt(0).Name = "Name";
                else if (sortCollection.ElementAt(0).Name == "GroupDescription")
                    sortCollection.ElementAt(0).Name = "Description";

                if (sortCollection.ElementAt(0).Direction.ToLower() == "descending" &&
                    !String.IsNullOrWhiteSpace(sortCollection.ElementAt(0).Name))
                {
                    sortKey = sortCollection.ElementAt(0).Name;
                    sortValue = "desc";
                }
                else if (sortCollection.ElementAt(0).Direction.ToLower() == "ascending" &&
                         !String.IsNullOrWhiteSpace(sortCollection.ElementAt(0).Name))
                {
                    sortKey = sortCollection.ElementAt(0).Name;
                    sortValue = "asc";
                }
            }
            var result = "SELECT * from (" + query +
                         ") GroupTable ";

            if (searchDescriptor != null && searchDescriptor.Any())
            {
                result += "WHERE (";
                foreach (var key in searchDescriptor)
                {
                    result += key + " like '%" + searchKey + "%'";
                    if (searchDescriptor.IndexOf(key) != searchDescriptor.Count - 1)
                    {
                        result += " OR ";
                    }
                }
                result += ")";
            }
            if (filterCollection != null && filterCollection.Any())
            {
                foreach (var filter in filterCollection)
                {
                    if (filter.PropertyName == "GroupName")
                        filter.PropertyName = "Name";
                    else if (filter.PropertyName == "GroupDescription")
                        filter.PropertyName = "Description";

                    result += " AND " + filter.PropertyName;
                    switch (filter.FilterType)
                    {
                        case "startswith":
                            result += " like '" + filter.FilterKey + "%'";
                            break;
                        case "endswith":
                            result += " like '%" + filter.FilterKey + "'";
                            break;
                        case "contains":
                            result += " like '%" + filter.FilterKey + "%'";
                            break;
                        case "equal":
                            result += " = '" + filter.FilterKey + "'";
                            break;
                        case "notequal":
                            result += " != '" + filter.FilterKey + "'";
                            break;
                        default:
                            result += " like '%" + filter.FilterKey + "%'";
                            break;
                    }

                }
            }
            result += " order by " + sortKey + " " + sortValue;
            return result;
        }
    }
}
