using Grout.Base.Data;
using Grout.Base.DataClasses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Grout.Base.Logger;
using System.Reflection;

namespace Grout.Base
{
    public class PermissionSet : IDisposable, IPermissionSet
    {
        #region PrivateProperties

        private bool _disposed;

        /// <summary>
        ///     Holds Relational Data Provider Object
        /// </summary>
        private readonly IRelationalDataProvider _dataProvider;

        /// <summary>
        ///     Holds Query Builder object
        /// </summary>
        private readonly IQueryBuilder _queryBuilder;

        #endregion PrivateProperties

        /// <summary>
        ///
        /// </summary>
        //    public PermissionSet()
        //    {
        //        if (GlobalAppSettings.DbSupport == DataBaseType.MSSQLCE)
        //        {
        //            _dataProvider = new SqlCeRelationalDataAdapter(GlobalAppSettings.ConnectionString);
        //            _queryBuilder = new SqlCeQueryBuilder();
        //        }
        //        else
        //        {
        //            _dataProvider = new SqlRelationalDataAdapter(GlobalAppSettings.ConnectionString);
        //            _queryBuilder = new SqlQueryBuilder();
        //        }
        //    }

        //    /// <summary>
        //    ///
        //    /// </summary>
        //    /// <param name="builder"></param>
        //    /// <param name="provider"></param>
        //    public PermissionSet(IQueryBuilder builder, IRelationalDataProvider provider)
        //    {
        //        _queryBuilder = builder;
        //        _dataProvider = provider;
        //    }

        //    public bool IsUserPermissionExist(Permission permission)
        //    {
        //        try
        //        {

        //            var whereColumns = new List<ConditionColumn>
        //            {
        //                new ConditionColumn
        //                {
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.UserId,
        //                    Value = permission.TargetId,
        //                    Condition = Conditions.Equals
        //                },

        //                new ConditionColumn
        //                {
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId,
        //                    Value = (int)permission.PermissionAccess,
        //                    Condition = Conditions.Equals,
        //                    LogicalOperator = LogicalOperators.AND
        //                },
        //                new ConditionColumn
        //                {
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId,
        //                    Value = (int)permission.PermissionEntity,
        //                    Condition = Conditions.Equals,
        //                    LogicalOperator = LogicalOperators.AND
        //                },

        //                new ConditionColumn
        //                {
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.IsActive,
        //                    Value = true,
        //                    Condition = Conditions.Equals,
        //                    LogicalOperator = LogicalOperators.AND
        //                }
        //            };
        //            if (permission.ItemId == null)
        //            {
        //                whereColumns.Add(new ConditionColumn
        //                {
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.ItemId,
        //                    Value = permission.ItemId,
        //                    Condition = Conditions.IS,
        //                    LogicalOperator = LogicalOperators.AND
        //                });
        //            }
        //            else
        //            {
        //                whereColumns.Add(new ConditionColumn
        //                {
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.ItemId,
        //                    Value = permission.ItemId,
        //                    Condition = Conditions.Equals,
        //                    LogicalOperator = LogicalOperators.AND
        //                });
        //            }
        //            var dataResult = _dataProvider.ExecuteReaderQuery(
        //                     _queryBuilder.SelectAllRecordsFromTable(
        //                         GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName, whereColumns));
        //            if (dataResult.DataTable.Rows.Count > 0)
        //                return true;

        //        }
        //        catch (Exception ex)
        //        {
        //            LogExtension.LogError("Error in getting all permissions for the group", ex, MethodBase.GetCurrentMethod(), " UserId - " + permission.TargetId + " PermissionAccessId - " + (int)permission.PermissionAccess + " PermissionEntityId - " + (int)permission.PermissionEntity + " ItemId - " + permission.ItemId);
        //            return false;
        //        }
        //        return false;
        //    }

        //    public bool IsGroupPermissionExist(Permission permission)
        //    {
        //        try
        //        {

        //            var whereColumns = new List<ConditionColumn>
        //            {
        //                new ConditionColumn
        //                {
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId,
        //                    Value = permission.TargetId,
        //                    Condition = Conditions.Equals
        //                },

        //                new ConditionColumn
        //                {
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionAccessId,
        //                    Value = (int)permission.PermissionAccess,
        //                    Condition = Conditions.Equals,
        //                    LogicalOperator = LogicalOperators.AND
        //                },
        //                new ConditionColumn
        //                {
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId,
        //                    Value = (int)permission.PermissionEntity,
        //                    Condition = Conditions.Equals,
        //                    LogicalOperator = LogicalOperators.AND
        //                },

        //                new ConditionColumn
        //                {
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.IsActive,
        //                    Value = true,
        //                    Condition = Conditions.Equals,
        //                    LogicalOperator = LogicalOperators.AND
        //                }
        //            };
        //            if (permission.ItemId == null)
        //            {
        //                whereColumns.Add(new ConditionColumn
        //                {
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.ItemId,
        //                    Value = permission.ItemId,
        //                    Condition = Conditions.IS,
        //                    LogicalOperator = LogicalOperators.AND
        //                });
        //            }
        //            else
        //            {
        //                whereColumns.Add(new ConditionColumn
        //                {
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.ItemId,
        //                    Value = permission.ItemId,
        //                    Condition = Conditions.Equals,
        //                    LogicalOperator = LogicalOperators.AND
        //                });
        //            }
        //            var dataResult = _dataProvider.ExecuteReaderQuery(
        //                     _queryBuilder.SelectAllRecordsFromTable(
        //                         GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName, whereColumns));
        //            if (dataResult.DataTable.Rows.Count > 0)
        //                return true;

        //        }
        //        catch (Exception ex)
        //        {
        //            LogExtension.LogError("Error in getting all permissions for the group", ex, MethodBase.GetCurrentMethod(), " GroupId - " + permission.TargetId + " PermissionAccessId - " + (int)permission.PermissionAccess + " PermissionEntityId - " + (int)permission.PermissionEntity + " ItemId - " + permission.ItemId);
        //            return false;
        //        }
        //        return false;
        //    }

        //    public bool AddPermissionToUser(Permission permission)
        //    {
        //        try
        //        {
        //            var query = new StringBuilder();

        //            object itemId = DBNull.Value;
        //            if (permission.ItemId.HasValue)
        //            {
        //                itemId = permission.ItemId.Value;
        //            }

        //            var values = new Dictionary<string, object>
        //            {
        //                {GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId, (int)permission.PermissionAccess},
        //                {GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId,(int)permission.PermissionEntity},
        //                {GlobalAppSettings.DbColumns.DB_UserPermission.ItemId, itemId},
        //                {GlobalAppSettings.DbColumns.DB_UserPermission.UserId, permission.TargetId},
        //                {GlobalAppSettings.DbColumns.DB_UserPermission.IsActive, true},
        //            };
        //            query.Append(_queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName,
        //                values));
        //            _dataProvider.ExecuteNonQuery(query.ToString());
        //            LogExtension.LogInfo("Successfully added permission to the user", MethodBase.GetCurrentMethod(), " UserId - " + permission.TargetId + " PermissionAccessId - " + (int)permission.PermissionAccess + " PermissionEntityId - " + (int)permission.PermissionEntity + " ItemId - " + permission.ItemId);
        //            return true;
        //        }

        //        catch (Exception ex)
        //        {
        //            LogExtension.LogError("Error in adding permission to the user", ex, MethodBase.GetCurrentMethod(), " UserId - " + permission.TargetId + " PermissionAccessId - " + (int)permission.PermissionAccess + " PermissionEntityId - " + (int)permission.PermissionEntity + " ItemId - " + permission.ItemId);
        //            return false;
        //        }
        //    }

        //    public bool AddPermissionToGroup(Permission permission)
        //    {
        //        try
        //        {
        //            var query = new StringBuilder();

        //            object itemId = DBNull.Value;
        //            if (permission.ItemId.HasValue)
        //            {
        //                itemId = permission.ItemId.Value;
        //            }

        //            var values = new Dictionary<string, object>
        //            {
        //                {GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionAccessId, (int)permission.PermissionAccess},
        //                {GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId,(int)permission.PermissionEntity},
        //                {GlobalAppSettings.DbColumns.DB_GroupPermission.ItemId, itemId},
        //                {GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId, permission.TargetId},
        //                {GlobalAppSettings.DbColumns.DB_GroupPermission.IsActive, true},
        //            };

        //            query.Append(_queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
        //                values));

        //            _dataProvider.ExecuteNonQuery(query.ToString());
        //            LogExtension.LogInfo("Successfully adding permission to the group", MethodBase.GetCurrentMethod(), " GroupId - " + permission.TargetId + " PermissionAccessId - " + (int)permission.PermissionAccess + " PermissionEntityId - " + (int)permission.PermissionEntity + " ItemId - " + permission.ItemId);
        //            return true;
        //        }

        //        catch (Exception ex)
        //        {
        //            LogExtension.LogError("Error in adding permission to the group", ex, MethodBase.GetCurrentMethod(), " GroupId - " + permission.TargetId + " PermissionAccessId - " + (int)permission.PermissionAccess + " PermissionEntityId - " + (int)permission.PermissionEntity + " ItemId - " + permission.ItemId);
        //            return false;
        //        }
        //    }

        //    public bool RemovePermissionFromUser(List<Permission> permission)
        //    {
        //        var whereColumns = new List<ConditionColumn>
        //            {
        //                new ConditionColumn
        //                {
        //                    TableName = GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName,
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.Id,
        //                    Condition = Conditions.IN,
        //                    Values = permission.Select(a=>a.PermissionId.ToString()).ToList()
        //                }
        //            };

        //        var updateColumns = new List<UpdateColumn>
        //            {
        //                new UpdateColumn
        //                {
        //                    TableName = GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName,
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.IsActive,
        //                    Value = false
        //                }
        //            };
        //        try
        //        {
        //            _dataProvider.ExecuteNonQuery(_queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName, updateColumns, whereColumns));

        //            LogExtension.LogInfo("Successfully removing permission from the User", MethodBase.GetCurrentMethod());
        //            return true;
        //        }

        //        catch (Exception ex)
        //        {
        //            LogExtension.LogError("Error in removing permission from the User", ex, MethodBase.GetCurrentMethod());
        //            return false;

        //        }
        //    }

        //    public bool RemovePermissionFromGroup(List<Permission> permission)
        //    {
        //        var whereColumns = new List<ConditionColumn>
        //            {
        //                new ConditionColumn
        //                {
        //                    TableName = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.Id,
        //                    Condition = Conditions.IN,
        //                    Values = permission.Select(a=>a.PermissionId.ToString()).ToList()
        //                }
        //            };

        //        var updateColumns = new List<UpdateColumn>
        //            {
        //                new UpdateColumn
        //                {
        //                    TableName = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.IsActive,
        //                    Value = false
        //                }
        //            };
        //        try
        //        {
        //            _dataProvider.ExecuteNonQuery(_queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName, updateColumns, whereColumns));

        //            LogExtension.LogInfo("Successfully removing permission from the Group", MethodBase.GetCurrentMethod());
        //            return true;
        //        }

        //        catch (Exception ex)
        //        {
        //            LogExtension.LogError("Error in removing permission from the Group", ex, MethodBase.GetCurrentMethod());
        //            return false;

        //        }
        //    }

        //    public List<Permission> GetGroupPermission(int groupId)
        //    {
        //        try
        //        {

        //            var whereColumns = new List<ConditionColumn>
        //            {
        //                new ConditionColumn
        //                {
        //                    TableName = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId,
        //                    Value = groupId,
        //                    Condition = Conditions.Equals
        //                },

        //                new ConditionColumn
        //                {
        //                    TableName = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.IsActive,
        //                    Value = true,
        //                    Condition = Conditions.Equals,
        //                    LogicalOperator = LogicalOperators.AND
        //                }
        //            };
        //            var groupjoinSpecification = new List<JoinSpecification>
        //            {
        //                new JoinSpecification
        //                {
        //                    Table = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
        //                    Column =
        //                        new List<JoinColumn>
        //                        {
        //                            new JoinColumn
        //                            {
        //                                TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
        //                                JoinedColumn = GlobalAppSettings.DbColumns.DB_Item.Id,
        //                                Operation = Conditions.Equals,
        //                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_GroupPermission.ItemId,
        //                                ParentTable = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName
        //                            },
        //                            new JoinColumn
        //                            {
        //                                ConditionValue = true,
        //                                Operation = Conditions.Equals,
        //                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_Item.IsActive,
        //                                ParentTable = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
        //                                LogicalOperator = LogicalOperators.AND
        //                            }
        //                        },
        //                    JoinType = JoinTypes.Left
        //                },
        //                new JoinSpecification
        //                {
        //                    Table = GlobalAppSettings.DbColumns.DB_Group.DB_TableName,
        //                    Column = new List<JoinColumn>
        //                    {
        //                        new JoinColumn
        //                        {
        //                            TableName = GlobalAppSettings.DbColumns.DB_Group.DB_TableName,
        //                            JoinedColumn = GlobalAppSettings.DbColumns.DB_Group.GroupId,
        //                            Operation = Conditions.Equals,
        //                            ParentTableColumn = GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId,
        //                            ParentTable = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName
        //                        },
        //                        new JoinColumn
        //                        {
        //                            ConditionValue = true,
        //                            Operation = Conditions.Equals,
        //                            ParentTableColumn = GlobalAppSettings.DbColumns.DB_Group.IsActive,
        //                            ParentTable = GlobalAppSettings.DbColumns.DB_Group.DB_TableName,
        //                            LogicalOperator = LogicalOperators.AND
        //                        }
        //                    }
        //                }
        //            };

        //            var groupselectColumns = new List<SelectedColumn>
        //            {
        //                new SelectedColumn {TableName = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName, ColumnName = "*"},
        //                new SelectedColumn
        //                {
        //                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name
        //                },
        //            };
        //            var dataResult = _dataProvider.ExecuteReaderQuery(
        //                 _queryBuilder.ApplyWhereClause(
        //                _queryBuilder.ApplyMultipleJoins(GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName, groupselectColumns,
        //                groupjoinSpecification), whereColumns)
        //                );
        //            var tResult = dataResult.DataTable.AsEnumerable().Select(a =>
        //                new Permission
        //                {
        //                    PermissionAccess = a.Field<PermissionAccess>(GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionAccessId),
        //                    PermissionEntity = a.Field<PermissionEntity>(GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId),
        //                    TargetId = a.Field<int>(GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId),
        //                    ItemId = a.Field<Guid?>(GlobalAppSettings.DbColumns.DB_GroupPermission.ItemId),
        //                    ItemName = a.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Name),
        //                    IsUserPermission = false,
        //                    PermissionId = a.Field<int>(GlobalAppSettings.DbColumns.DB_GroupPermission.Id)
        //                }).OrderByDescending(a => a.PermissionId).ToList();

        //            //To remove the items that are inactive. We are doing left join with Items table and so, all the items will be pulled that are 
        //            //coupled with the permissions table.
        //            tResult.RemoveAll(r => r.ItemId != null && String.IsNullOrWhiteSpace(r.ItemName));

        //            return tResult;

        //        }
        //        catch (Exception ex)
        //        {
        //            LogExtension.LogError("Error in getting all permissions for the group", ex, MethodBase.GetCurrentMethod(), " GroupId - " + groupId);
        //            return null;
        //        }
        //    }

        //    public List<Permission> GetUserPermission(int userId)
        //    {
        //        try
        //        {
        //            var userGroups = new UserManagement().GetAllGroupsOfUser(userId);
        //            var userwhereColumns = new List<ConditionColumn>
        //            {
        //                new ConditionColumn
        //                {
        //                    TableName=GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName,
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.UserId,
        //                    Value = userId,
        //                    Condition = Conditions.Equals
        //                },

        //                new ConditionColumn
        //                {
        //                    TableName=GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName,
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.IsActive,
        //                    Value = true,
        //                    Condition = Conditions.Equals,
        //                    LogicalOperator = LogicalOperators.AND
        //                }
        //            };

        //            var userjoinSpecification = new List<JoinSpecification>
        //            {
        //                new JoinSpecification
        //                {
        //                    Table = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
        //                    Column =
        //                        new List<JoinColumn>
        //                        {
        //                            new JoinColumn
        //                            {
        //                                TableName = GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName,
        //                                JoinedColumn = GlobalAppSettings.DbColumns.DB_UserPermission.ItemId,
        //                                Operation = Conditions.Equals,
        //                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_Item.Id,
        //                                ParentTable = GlobalAppSettings.DbColumns.DB_Item.DB_TableName
        //                            },
        //                            new JoinColumn
        //                            {
        //                                ConditionValue = true,
        //                                Operation = Conditions.Equals,
        //                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_Item.IsActive,
        //                                ParentTable = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
        //                                LogicalOperator = LogicalOperators.AND
        //                            }
        //                        },
        //                    JoinType = JoinTypes.Left
        //                },
        //                new JoinSpecification
        //                {
        //                    Table = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
        //                    Column = new List<JoinColumn>
        //                    {
        //                        new JoinColumn
        //                        {
        //                            TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
        //                            JoinedColumn = GlobalAppSettings.DbColumns.DB_User.UserId,
        //                            Operation = Conditions.Equals,
        //                            ParentTableColumn = GlobalAppSettings.DbColumns.DB_UserPermission.UserId,
        //                            ParentTable = GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName
        //                        },
        //                        new JoinColumn
        //                        {
        //                            ConditionValue = false,
        //                            Operation = Conditions.Equals,
        //                            ParentTableColumn = GlobalAppSettings.DbColumns.DB_User.IsDeleted,
        //                            ParentTable = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
        //                            LogicalOperator = LogicalOperators.AND
        //                        }
        //                    }
        //                }
        //            };

        //            var userselectColumns = new List<SelectedColumn>
        //            {
        //                new SelectedColumn {TableName = GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName, ColumnName = "*"},
        //                new SelectedColumn
        //                {
        //                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name
        //                },
        //            };
        //            var userPermissions = _dataProvider.ExecuteReaderQuery(
        //                _queryBuilder.ApplyWhereClause(
        //            _queryBuilder.ApplyMultipleJoins(GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName, userselectColumns,
        //                userjoinSpecification), userwhereColumns)
        //                );

        //            var _userPermission = userPermissions.DataTable.AsEnumerable().Select(a =>
        //                new Permission
        //                    {
        //                        PermissionAccess = a.Field<PermissionAccess>(GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId),
        //                        PermissionEntity = a.Field<PermissionEntity>(GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId),
        //                        TargetId = a.Field<int>(GlobalAppSettings.DbColumns.DB_UserPermission.UserId),
        //                        ItemId = a.Field<Guid?>(GlobalAppSettings.DbColumns.DB_UserPermission.ItemId),
        //                        ItemName = a.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Name),
        //                        IsUserPermission = true,
        //                        GroupName = null,
        //                        PermissionId = a.Field<int>(GlobalAppSettings.DbColumns.DB_UserPermission.Id)
        //                    }).OrderByDescending(a => a.PermissionId).ToList();

        //            if (userGroups.Any())
        //            {
        //                var groupWhereColumns = new List<ConditionColumn>
        //            {
        //                new ConditionColumn
        //                {
        //                    TableName=GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId,
        //                    Condition = Conditions.IN,
        //                    Values = userGroups.Select(s=>s.Id.ToString()).ToList()
        //                },
        //                new ConditionColumn
        //                    {
        //                        TableName=GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
        //                        ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.IsActive,
        //                        Value = true,
        //                        Condition = Conditions.Equals,
        //                        LogicalOperator = LogicalOperators.AND
        //                    }
        //            };
        //                var groupjoinSpecification = new List<JoinSpecification>
        //            {
        //                new JoinSpecification
        //                {
        //                    Table = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
        //                    Column =
        //                        new List<JoinColumn>
        //                        {
        //                            new JoinColumn
        //                            {
        //                                TableName = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
        //                                JoinedColumn = GlobalAppSettings.DbColumns.DB_GroupPermission.ItemId,
        //                                Operation = Conditions.Equals,
        //                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_Item.Id,
        //                                ParentTable = GlobalAppSettings.DbColumns.DB_Item.DB_TableName
        //                            },
        //                            new JoinColumn
        //                            {
        //                                ConditionValue = true,
        //                                Operation = Conditions.Equals,
        //                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_Item.IsActive,
        //                                ParentTable = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
        //                                LogicalOperator = LogicalOperators.AND
        //                            }
        //                        },
        //                    JoinType = JoinTypes.Left
        //                },
        //                new JoinSpecification
        //                {
        //                    Table=GlobalAppSettings.DbColumns.DB_Group.DB_TableName,
        //                    Column = new List<JoinColumn>
        //                    {
        //                        new JoinColumn
        //                        {
        //                            TableName = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
        //                            JoinedColumn = GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId,
        //                            Operation = Conditions.Equals,
        //                            ParentTableColumn = GlobalAppSettings.DbColumns.DB_Group.GroupId,
        //                            ParentTable = GlobalAppSettings.DbColumns.DB_Group.DB_TableName
        //                        }
        //                    },
        //                    JoinType = JoinTypes.Left
        //                }
        //            };

        //                var groupselectColumns = new List<SelectedColumn>
        //            {
        //                new SelectedColumn
        //                {
        //                    TableName = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
        //                    ColumnName = "*"
        //                },
        //                new SelectedColumn
        //                {
        //                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name
        //                },
        //                new SelectedColumn
        //                {
        //                    TableName = GlobalAppSettings.DbColumns.DB_Group.DB_TableName,
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_Group.Name,
        //                    AliasName = "GroupName"
        //                }
        //            };
        //                var groupPermissions = _dataProvider.ExecuteReaderQuery(
        //                     _queryBuilder.ApplyWhereClause(
        //                    _queryBuilder.ApplyMultipleJoins(GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName, groupselectColumns,
        //                    groupjoinSpecification), groupWhereColumns)
        //                    );

        //                var _groupPermission = groupPermissions.DataTable.AsEnumerable().Select(a =>
        //                   new Permission
        //                   {
        //                       PermissionAccess = a.Field<PermissionAccess>(GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionAccessId),
        //                       PermissionEntity = a.Field<PermissionEntity>(GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId),
        //                       TargetId = a.Field<int>(GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId),
        //                       ItemId = a.Field<Guid?>(GlobalAppSettings.DbColumns.DB_GroupPermission.ItemId),
        //                       ItemName = a.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Name),
        //                       IsUserPermission = false,
        //                       GroupName = a.Field<string>("GroupName"),
        //                       PermissionId = a.Field<int>(GlobalAppSettings.DbColumns.DB_GroupPermission.Id)
        //                   }).OrderByDescending(a => a.PermissionId).ToList();
        //                _userPermission.AddRange(_groupPermission);
        //            }

        //            //To remove the items that are inactive. We are doing left join with Items table and so, all the items will be pulled that are 
        //            //coupled with the permissions table.
        //            _userPermission.RemoveAll(r => r.ItemId != null && String.IsNullOrWhiteSpace(r.ItemName));

        //            return _userPermission;
        //        }
        //        catch (Exception ex)
        //        {
        //            return null;
        //        }
        //    }

        //    public DataTable GetPermissionEntity(int accessMode)
        //    {
        //        var whereColumns = new List<ConditionColumn>();
        //        if (accessMode == (int)PermissionAccess.Create)
        //        {
        //            whereColumns = new List<ConditionColumn>
        //            {
        //                new ConditionColumn
        //                {
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_PermissionEntity.EntityType,
        //                    Condition = Conditions.Equals,
        //                    Value = (int)EntityType.AllType
        //                },
        //                new ConditionColumn
        //                {
        //                    ColumnName = GlobalAppSettings.DbColumns.DB_PermissionEntity.EntityType,
        //                    Condition = Conditions.Equals,
        //                    LogicalOperator=LogicalOperators.OR,
        //                    Value = (int)EntityType.InType
        //                }

        //            };
        //        }
        //        whereColumns.Add(
        //            new ConditionColumn
        //            {
        //                ColumnName = GlobalAppSettings.DbColumns.DB_PermissionEntity.IsActive,
        //                Condition = Conditions.Equals,
        //                LogicalOperator = LogicalOperators.AND,
        //                Value = true
        //            });


        //        var permissionEntities = _dataProvider.ExecuteReaderQuery(
        //            _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_PermissionEntity.DB_TableName,whereColumns)).DataTable;

        //        return permissionEntities;
        //    }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //    /// <summary>
        //    ///
        //    /// </summary>
        //    /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.                    
                }
                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}