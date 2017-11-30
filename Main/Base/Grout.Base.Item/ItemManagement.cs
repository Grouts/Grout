using Grout.Base.Data;
using Grout.Base.DataClasses;
using Grout.Base.Logger;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace Grout.Base.Item
{
    public class ItemManagement : IItemManagement
    {
        private readonly IRelationalDataProvider _dataProvider;
        private readonly IQueryBuilder _queryBuilder;
        private readonly IUserManagement _userManagement = new UserManagement();
        public List<Guid> cloneHeriarchy = new List<Guid>();

        public ItemManagement(IQueryBuilder builder, IRelationalDataProvider provider)
        {
            _queryBuilder = builder;
            _dataProvider = provider;
        }

        public ItemManagement()
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

        public List<Permission> GetUserPermissions(int userId, PermissionEntity permissionEntity, List<string> itemId)
        {
            var userGroups = _userManagement.GetAllGroupsOfUser(userId).Select(r => r.Id.ToString()).Distinct().ToList();
            var userWhereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId,
                    Condition = Conditions.Equals,
                    Value = (int)permissionEntity
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.ItemId,
                    Condition = Conditions.IN,
                    LogicalOperator=LogicalOperators.AND,
                    Values = itemId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.UserId,
                    Condition = Conditions.Equals,
                    LogicalOperator=LogicalOperators.AND,
                    Value = userId
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            var groupWhereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId,
                    Condition = Conditions.IN,
                    Values = userGroups
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId,
                    Condition = Conditions.Equals,
                    LogicalOperator=LogicalOperators.AND,
                    Value = (int)permissionEntity
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            var selectedColumns = new List<SelectedColumn>
            {
                new SelectedColumn
                {
                    ColumnName=GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId
                },
                new SelectedColumn
                {
                    ColumnName=GlobalAppSettings.DbColumns.DB_UserPermission.ItemId
                }
            };
            var userPermissions = _queryBuilder.SelectRecordsFromTable(GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName, selectedColumns, userWhereColumns);
            var groupPermissions = _queryBuilder.SelectRecordsFromTable(GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName, selectedColumns, groupWhereColumns);
            var queryConc = userPermissions + " UNION " + groupPermissions;
            var result = _dataProvider.ExecuteReaderQuery(queryConc);
            return result.DataTable.AsEnumerable().Select(a =>
                new Permission
                {
                    PermissionAccess = (PermissionAccess)Enum.Parse(typeof(PermissionAccess), a.Field<int>(GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId).ToString()),
                    ItemId = a.Field<Guid>(GlobalAppSettings.DbColumns.DB_UserPermission.ItemId)
                }).ToList();
        }

        public Result GetUserItemsForSpecificPermission(int userId, PermissionEntity permissionEntity, PermissionAccess permissionAccess)
        {
            var userGroups = _userManagement.GetAllGroupsOfUser(userId).Select(r => r.Id.ToString()).Distinct().ToList();
            var userWhereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId,
                    Condition = Conditions.Equals,
                    Value = (int)permissionEntity
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId,
                    Condition = Conditions.Equals,
                    LogicalOperator=LogicalOperators.AND,
                    Value = (int)permissionAccess
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.UserId,
                    Condition = Conditions.Equals,
                    LogicalOperator=LogicalOperators.AND,
                    Value = userId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            var query = _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName, userWhereColumns);
            if (userGroups.Count != 0)
            {
                var groupWhereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId,
                        Condition = Conditions.IN,
                        Values = userGroups
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId,
                        Condition = Conditions.Equals,
                        LogicalOperator=LogicalOperators.AND,
                        Value = (int)permissionEntity
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionAccessId,
                        Condition = Conditions.Equals,
                        LogicalOperator=LogicalOperators.AND,
                        Value = (int)permissionAccess
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.IsActive,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = true
                    }
                };
                query += " UNION " + _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName, groupWhereColumns);
            }

            var result = _dataProvider.ExecuteReaderQuery(query);
            return result;
        }

        public List<ItemDetail> CreateAccessCategoriesOfItem(int userId, ItemType itemType)
        {
            var resultList = new List<ItemDetail>();
            var groupIds = _userManagement.GetAllGroupsOfUser(userId).Select(r => r.Id.ToString()).Distinct().ToList();
            var permissionAccessValue = (int)PermissionAccess.Create;
            var permissionEntityGlobalValue = 0;
            var permissionEntitySpecificValue = 0;
            switch (itemType)
            {
                case ItemType.Dashboard:
                    permissionEntityGlobalValue = (int)PermissionEntity.AllDashboards;
                    permissionEntitySpecificValue = (int)PermissionEntity.DashboardsInCategory;
                    break;

                case ItemType.Report:
                    permissionEntityGlobalValue = (int)PermissionEntity.AllReports;
                    permissionEntitySpecificValue = (int)PermissionEntity.ReportsInCategory;
                    break;

                case ItemType.Datasource:
                    permissionEntityGlobalValue = (int)PermissionEntity.AllDataSources;
                    break;

                case ItemType.File:
                    permissionEntityGlobalValue = (int)PermissionEntity.AllFiles;
                    break;
            }
            var hasGlobalPermission = HasGlobalCreatePermissionOnItem(userId, permissionEntityGlobalValue, groupIds);
            if (hasGlobalPermission)
            {
                return GetAllCategoriesOfSystem();
            }
            else
            {
                var whereUserColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId,
                    Condition = Conditions.Equals,
                    Value = permissionAccessValue
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.UserId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = userId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = permissionEntitySpecificValue
                },
                new ConditionColumn
                {
                    TableName=GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                },
                new ConditionColumn
                {
                    TableName=GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
                var joinUserSpecification =
                    new JoinSpecification
                    {
                        Table = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                        Column =
                            new List<JoinColumn>
                            {
                                new JoinColumn
                                {
                                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                                    JoinedColumn = GlobalAppSettings.DbColumns.DB_Item.Id,
                                    Operation = Conditions.Equals,
                                    ParentTableColumn = GlobalAppSettings.DbColumns.DB_UserPermission.ItemId,
                                    ParentTable = GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName
                                }
                            },
                        JoinType = JoinTypes.Inner
                    };
                var whereGroupColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionAccessId,
                    Condition = Conditions.Equals,
                    Value = permissionAccessValue
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId,
                    Condition = Conditions.IN,
                    LogicalOperator = LogicalOperators.AND,
                    Values = groupIds
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = permissionEntitySpecificValue
                },
                new ConditionColumn
                {
                    TableName=GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                },
                new ConditionColumn
                {
                    TableName=GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
                var joinGroupSpecification =
                    new JoinSpecification
                    {
                        Table = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                        Column =
                        new List<JoinColumn>
                        {
                            new JoinColumn
                            {
                                TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_Item.Id,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_GroupPermission.ItemId,
                                ParentTable = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName
                            }
                        },
                        JoinType = JoinTypes.Inner
                    };
                var selectColumn = new List<SelectedColumn>
            {
                new SelectedColumn {
                    TableName=GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Id
                },new SelectedColumn {
                    TableName=GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name
                }
            };
                var query = _queryBuilder.ApplyWhereClause(_queryBuilder.ApplyJoins(GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName, selectColumn, joinUserSpecification), whereUserColumns);
                if (groupIds.Any())
                {
                    query += " UNION " + _queryBuilder.ApplyWhereClause(_queryBuilder.ApplyJoins(GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName, selectColumn, joinGroupSpecification), whereGroupColumns);
                }
                var result = _dataProvider.ExecuteReaderQuery(query);
                if (result.Status)
                {
                    resultList = result.DataTable.AsEnumerable().Select(row => new ItemDetail
                    {
                        Id = row.Field<Guid>(GlobalAppSettings.DbColumns.DB_Item.Id),
                        Name = row.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Name)
                    }).ToList();
                }
                return resultList;
            }
        }

        public bool HasGlobalCreatePermissionOnItem(int userId, int permissionEntityValue, List<string> groupIds)
        {
            var permissionAccessValue = (int)PermissionAccess.Create;
            var whereUserColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId,
                    Condition = Conditions.Equals,
                    Value = permissionAccessValue
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.UserId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = userId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = permissionEntityValue
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            var whereGroupColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionAccessId,
                    Condition = Conditions.Equals,
                    Value = permissionAccessValue
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId,
                    Condition = Conditions.IN,
                    LogicalOperator = LogicalOperators.AND,
                    Values = groupIds
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = permissionEntityValue
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            var query = _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName, whereUserColumns);
            if (groupIds.Any())
            {
                query += " UNION " + _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName, whereGroupColumns);
            }
            var result = _dataProvider.ExecuteReaderQuery(query);
            if (result.Status)
            {
                return result.DataTable.Rows.Count > 0;
            }
            return false;
        }

        public List<ItemDetail> GetAllCategoriesOfSystem()
        {
            var resultList = new List<ItemDetail>();
            var whereUserColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    Value = true
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ItemTypeId,
                    Condition = Conditions.Equals,
                    LogicalOperator=LogicalOperators.AND,
                    Value = (int) ItemType.Category
                }
            };
            var result = _dataProvider.ExecuteReaderQuery(_queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, whereUserColumns));
            if (result.Status)
            {
                resultList = result.DataTable.AsEnumerable().Select(row => new ItemDetail
                {
                    Id = row.Field<Guid>(GlobalAppSettings.DbColumns.DB_Item.Id),
                    Name = row.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Name)
                }).ToList();
            }
            return resultList;
        }

        public List<ItemDetail> CheckCanSchedule(int userId, ItemType itemType, List<ItemDetail> itemList)
        {
            var canCreateSchedule = GetUserItemsForSpecificPermission(userId, PermissionEntity.AllSchedules, PermissionAccess.Create).DataTable.AsEnumerable().ToList().Count > 0 ? true : false;
            if (canCreateSchedule)
            {
                itemList = itemList.Select(x => { x.CanSchedule = true; return x; }).ToList();
            }
            return itemList;
        }

        public EntityData<ItemDetail> GetItems(int userId, ItemType itemType, List<SortCollection> sortCollection = null, List<FilterCollection> filterSettings = null, string searchQuery = "", int? skip = 0, int? take = 0, Guid? itemId = null, bool isAllCategorySearch = false)
        {
            var items = new List<ItemDetail>();
            var itemDataTable = new DataTable();

            var searchDescriptor = new List<string> { "Name", "Description", "CreatedByDisplayName" };
            if (itemType == ItemType.Report && isAllCategorySearch)
            {
                searchDescriptor = new List<string> { "Name", "Description", "CategoryName", "CreatedByDisplayName" };
            }

            var groupIds = _userManagement.GetAllGroupsOfUser(userId).Select(r => r.Id).Distinct().ToList();
            var globalPermissions = GetGlobalPermissionOfItem(userId, itemType);
            List<Permission> categories = null;

            if (itemType == ItemType.Report || itemType == ItemType.Dashboard)
            {
                categories = GetCategories(userId, groupIds, itemType);
            }

            if (globalPermissions[PermissionAccess.ReadWriteDelete] ||
                globalPermissions[PermissionAccess.Read] || globalPermissions[PermissionAccess.ReadWrite])
            {
                var permissionEntityValue = (itemType == ItemType.Category) ? (int)PermissionEntity.AllCategories : (itemType == ItemType.Report) ? (int)PermissionEntity.AllReports : (itemType == ItemType.Dashboard) ? (int)PermissionEntity.AllDashboards : (itemType == ItemType.File) ? (int)PermissionEntity.AllFiles : (int)PermissionEntity.AllSchedules;
                var permissionAccessValue = globalPermissions[PermissionAccess.ReadWriteDelete] ? (int)PermissionAccess.ReadWriteDelete : globalPermissions[PermissionAccess.ReadWrite] ? (int)PermissionAccess.ReadWrite : (int)PermissionAccess.Read;
                items = ItemsView(userId, groupIds, permissionAccessValue, permissionEntityValue, itemType, categories, sortCollection, filterSettings, searchQuery, searchDescriptor, itemId);
            }
            else
            {
                items = GetAllItems(userId, groupIds, itemType, categories, sortCollection, filterSettings, searchQuery, searchDescriptor, itemId);
            }

            var currentTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone());

            foreach (var item in items)
            {
                item.ItemCreatedDate =
                    TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.CreatedDate), currentTimeZoneInfo);
                item.ItemModifiedDate =
                    TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(item.ModifiedDate), currentTimeZoneInfo);
                item.CreatedDate =
                    item.ItemCreatedDate.ToString(GlobalAppSettings.SystemSettings.DateFormat + " hh:mm tt");
                item.ModifiedDate =
                    item.ItemModifiedDate.ToString(GlobalAppSettings.SystemSettings.DateFormat + " hh:mm tt");
            }

            var itemsCount = items.Count();

            if ((skip == 0 && take == 0) || (skip == null && take == null))
            {
                return new EntityData<ItemDetail>
                {
                    result = items.ToList(),
                    count = itemsCount
                };
            }

            return new EntityData<ItemDetail>
            {
                result = itemsCount > skip.GetValueOrDefault() ? items.Skip(skip.GetValueOrDefault()).Take(take.GetValueOrDefault()).ToList() : items.Skip(itemsCount != 0 ? (itemsCount % 10 != 0 ? ((itemsCount / 10) * 10) : (((itemsCount / 10) - 1) * 10)) : 0).Take(take.GetValueOrDefault()).ToList(),
                count = itemsCount
            };
        }

        public Dictionary<PermissionAccess, bool> GetGlobalPermissionOfItem(int userId, ItemType itemType)
        {
            var groupIds = _userManagement.GetAllGroupsOfUser(userId).Select(r => r.Id.ToString()).Distinct().ToList();
            var permissionEntityValue = 0;
            switch (itemType)
            {
                case ItemType.Category:
                    permissionEntityValue = (int)PermissionEntity.AllCategories;
                    break;

                case ItemType.Dashboard:
                    permissionEntityValue = (int)PermissionEntity.AllDashboards;
                    break;

                case ItemType.Datasource:
                    permissionEntityValue = (int)PermissionEntity.AllDataSources;
                    break;

                case ItemType.Report:
                    permissionEntityValue = (int)PermissionEntity.AllReports;
                    break;

                case ItemType.File:
                    permissionEntityValue = (int)PermissionEntity.AllFiles;
                    break;

                case ItemType.Schedule:
                    permissionEntityValue = (int)PermissionEntity.AllSchedules;
                    break;
            }
            var selectColumn = new List<SelectedColumn>
            {
                new SelectedColumn {ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId}
            };
            var whereUserColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.ItemId,
                    Condition = Conditions.IS,
                    Value = null
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.UserId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = userId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = permissionEntityValue
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            var whereGroupColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.ItemId,
                    Condition = Conditions.IS,
                    Value = null
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId,
                    Condition = Conditions.IN,
                    LogicalOperator = LogicalOperators.AND,
                    Values = groupIds
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = permissionEntityValue
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            var query =
                _queryBuilder.SelectRecordsFromTable(GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName, selectColumn,
                    whereUserColumns);
            if (groupIds.Any())
            {
                var groupQuery = _queryBuilder.SelectRecordsFromTable(GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName, selectColumn,
                        whereGroupColumns);
                query += " UNION " + groupQuery;
            }
            var result = _dataProvider.ExecuteReaderQuery(query);
            var permissionAccessIds = new List<int>();
            if (result.Status)
            {
                permissionAccessIds =
                    result.DataTable.AsEnumerable()
                        .Select(r => r.Field<int>(GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId))
                        .Distinct()
                        .ToList();
            }
            var globalPermissions = new Dictionary<PermissionAccess, bool>
            {
                {PermissionAccess.Read, permissionAccessIds.Contains((int) PermissionAccess.Read) || permissionAccessIds.Contains((int) PermissionAccess.ReadWrite) || permissionAccessIds.Contains((int) PermissionAccess.ReadWriteDelete)},
                {PermissionAccess.Create, permissionAccessIds.Contains((int) PermissionAccess.Create)},
                {PermissionAccess.ReadWrite, permissionAccessIds.Contains((int) PermissionAccess.ReadWrite) || permissionAccessIds.Contains((int) PermissionAccess.ReadWriteDelete)},
                {PermissionAccess.ReadWriteDelete, permissionAccessIds.Contains((int) PermissionAccess.ReadWriteDelete)}
            };
            return globalPermissions;
        }

        public List<Permission> GetCategories(int userId, List<int> groupList, ItemType? itemType = null, string categoryId = null)
        {
            var groupIds = _userManagement.GetAllGroupsOfUser(userId).Select(r => r.Id.ToString()).Distinct().ToList();
            var permissionEntityValue = 0;
            var permissionEntityValues = new List<string>();
            if (itemType != null)
            {
                switch (itemType)
                {
                    case ItemType.Dashboard:
                        permissionEntityValue = (int)PermissionEntity.DashboardsInCategory;
                        break;

                    case ItemType.Report:
                        permissionEntityValue = (int)PermissionEntity.ReportsInCategory;
                        break;
                }
            }
            else
            {
                permissionEntityValues.AddRange(
                    new List<string>
                    {
                        ((int) PermissionEntity.DashboardsInCategory).ToString(),
                        ((int) PermissionEntity.ReportsInCategory).ToString()
                    });
            }

            var selectColumn = new List<SelectedColumn>
            {
                new SelectedColumn {ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.ItemId},
                new SelectedColumn {ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId}
            };

            var whereUserColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.UserId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = userId
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }, 
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId,
                    Condition = Conditions.NotEquals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = (int)PermissionAccess.Create
                }
            };
            if (itemType != null)
            {
                whereUserColumns.Add(new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = permissionEntityValue
                });
            }
            else
            {
                whereUserColumns.Add(new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId,
                    Condition = Conditions.IN,
                    LogicalOperator = LogicalOperators.AND,
                    Values = permissionEntityValues
                });
            }
            if (categoryId != null)
            {
                whereUserColumns.Add(new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.ItemId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = categoryId
                });
            }

            var query =
                _queryBuilder.SelectRecordsFromTable(GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName, selectColumn,
                    whereUserColumns);
            if (groupIds.Count != 0)
            {
                var whereGroupColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId,
                    Condition = Conditions.IN,
                    LogicalOperator = LogicalOperators.AND,
                    Values = groupIds
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }, 
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId,
                    Condition = Conditions.NotEquals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = (int)PermissionAccess.Create
                }
                
            };
                if (itemType != null)
                {
                    whereGroupColumns.Add(new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = permissionEntityValue
                    });
                }
                else
                {
                    whereGroupColumns.Add(new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId,
                        Condition = Conditions.IN,
                        LogicalOperator = LogicalOperators.AND,
                        Values = permissionEntityValues
                    });
                }
                if (categoryId != null)
                {
                    whereGroupColumns.Add(new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.ItemId,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = categoryId
                    });
                }

                query = query + " UNION " + _queryBuilder.SelectRecordsFromTable(GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName, selectColumn,
                    whereGroupColumns);
            }
            var result = new List<Permission>();

            var dataTable = _dataProvider.ExecuteReaderQuery(query).DataTable;
            if (dataTable.Rows.Count > 0)
            {
                result = dataTable.AsEnumerable().Select(row => new Permission
                {
                    ItemId = row.Field<Guid>(GlobalAppSettings.DbColumns.DB_UserPermission.ItemId),
                    PermissionEntity =
                        (PermissionEntity)
                            row.Field<int>(GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId),
                    ItemType =
                        ((PermissionEntity)
                            row.Field<int>(GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId) ==
                         PermissionEntity.ReportsInCategory)
                            ? ItemType.Report
                            : ItemType.Dashboard
                }).ToList();
            }
            return result;
        }


        public DataTable GetSpecificItems(int userId, ItemType itemType)
        {
            const string joinAliasName = "User_modified";
            var groupIds = _userManagement.GetAllGroupsOfUser(userId).Select(r => r.Id.ToString()).Distinct().ToList();
            var whereUserColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    Value = true
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ItemTypeId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = (int) itemType
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.UserId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = userId
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            var whereGroupColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    Value = true
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ItemTypeId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = (int) itemType
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };

            if (groupIds.Any())
            {
                whereGroupColumns.Add(new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId,
                    Condition = Conditions.IN,
                    LogicalOperator = LogicalOperators.AND,
                    Values = groupIds
                });
            }
            if (itemType == ItemType.Category)
            {
                var rejectEntityIds = new List<string>
                {
                    ((int) PermissionEntity.DashboardsInCategory).ToString(),                    
                    ((int) PermissionEntity.ReportsInCategory).ToString()                    
                };
                whereUserColumns.Add(new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId,
                    Condition = Conditions.NOTIN,
                    LogicalOperator = LogicalOperators.AND,
                    Values = rejectEntityIds
                });
                if (groupIds.Any())
                {
                    whereGroupColumns.Add(new ConditionColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
                        ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId,
                        Condition = Conditions.NOTIN,
                        LogicalOperator = LogicalOperators.AND,
                        Values = rejectEntityIds
                    });
                }
            }
            var selectColumns = new List<SelectedColumn>
            {
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Id
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Description
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ParentId
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ItemTypeId
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.CreatedById
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.CreatedDate
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ModifiedById
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ModifiedDate
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.FirstName,
                    AliasName = "CreatedByFirstName"
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.LastName,
                    AliasName = "CreatedByLastName"
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.DisplayName,
                    AliasName = "CreatedByDisplayName"
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.IsActive,
                    AliasName = "IsCreatedByActive"
                },
                new SelectedColumn
                {
                    JoinAliasName = joinAliasName,
                    AliasName = "ModifiedByFirstName",
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.FirstName
                },
                new SelectedColumn
                {
                    JoinAliasName = joinAliasName,
                    AliasName = "ModifiedByLastName",
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.LastName
                },
                new SelectedColumn
                {
                    JoinAliasName = joinAliasName,
                    AliasName = "ModifiedByDisplayName",
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.DisplayName
                },
                new SelectedColumn
                {
                    JoinAliasName = joinAliasName,
                    AliasName = "IsModifiedByActive",
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.IsActive
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    AliasName = "ParentItemName",
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name,
                    JoinAliasName = "ParentItemTable"
                }
            };

            var userSelectedColumns = new List<SelectedColumn>(selectColumns);
            userSelectedColumns.Add(new SelectedColumn
            {
                TableName = GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName,
                ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId
            });
            userSelectedColumns.Add(new SelectedColumn
            {
                TableName = GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName,
                ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId
            });

            var groupSelectedColumn = new List<SelectedColumn>(selectColumns);
            groupSelectedColumn.Add(new SelectedColumn
            {
                TableName = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
                ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionAccessId
            });
            groupSelectedColumn.Add(new SelectedColumn
            {
                TableName = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
                ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId
            });

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
                                TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_Item.CreatedById,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_User.Id,
                                ParentTable = GlobalAppSettings.DbColumns.DB_User.DB_TableName
                            }
                        },
                    JoinType = JoinTypes.Inner
                },
                new JoinSpecification
                {
                    Table = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    Column =
                        new List<JoinColumn>
                        {
                            new JoinColumn
                            {
                                TableName = joinAliasName,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_User.Id,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_Item.ModifiedById,
                                ParentTable = GlobalAppSettings.DbColumns.DB_Item.DB_TableName
                            }
                        },
                    JoinType = JoinTypes.Inner,
                    JoinTableAliasName = joinAliasName
                },
                new JoinSpecification
                {
                    Table = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    Column =
                        new List<JoinColumn>
                        {
                            new JoinColumn
                            {
                                TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_Item.Id,
                                Operation = Conditions.Equals,
                                ParentTable = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_Item.ParentId
                            }
                        },
                    JoinType = JoinTypes.Left,
                    JoinTableAliasName = "ParentItemTable"
                }
            };

            var userJoinSpecifications = new List<JoinSpecification>(joinSpecification)
            {
                new JoinSpecification
                {
                    Table = GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName,
                    Column =
                        new List<JoinColumn>
                        {
                            new JoinColumn
                            {
                                TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_Item.Id,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_UserPermission.ItemId,
                                ParentTable = GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName
                            }
                        },
                    JoinType = JoinTypes.Inner
                }
            };

            var groupJoinSpecifications = new List<JoinSpecification>(joinSpecification)
            {
                new JoinSpecification
                {
                    Table = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName,
                    Column =
                        new List<JoinColumn>
                        {
                            new JoinColumn
                            {
                                TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_Item.Id,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_GroupPermission.ItemId,
                                ParentTable = GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName
                            }
                        },
                    JoinType = JoinTypes.Inner
                }
            };

            var resultantQuery = _queryBuilder.ApplyWhereClause(
                _queryBuilder.ApplyMultipleJoins(GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    userSelectedColumns,
                    userJoinSpecifications), whereUserColumns);

            if (groupIds.Any())
            {
                var groupQuery = _queryBuilder.ApplyWhereClause(
                _queryBuilder.ApplyMultipleJoins(GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    groupSelectedColumn,
                    groupJoinSpecifications), whereGroupColumns);

                resultantQuery += " UNION " + groupQuery;
            }

            var result = _dataProvider.ExecuteReaderQuery(resultantQuery);

            return result.DataTable;
        }

        public DataResponse SaveItemVersion(Guid itemId, int createdById, int rollBackVersionNo, int prevVersions, string itemName, string comment, ItemType itemType, DateTime modifiedDateTime)
        {
            var result = new DataResponse();
            var values = new Dictionary<string, object>
            {
                {GlobalAppSettings.DbColumns.DB_ItemVersion.ItemId, itemId},
                {GlobalAppSettings.DbColumns.DB_ItemVersion.ItemName, itemName},
                {GlobalAppSettings.DbColumns.DB_ItemVersion.VersionNumber, prevVersions},
                {GlobalAppSettings.DbColumns.DB_ItemVersion.RolledbackVersionNumber, rollBackVersionNo},
                {GlobalAppSettings.DbColumns.DB_ItemVersion.IsCurrentVersion, true},
                {GlobalAppSettings.DbColumns.DB_ItemVersion.CreatedById, createdById},
                {GlobalAppSettings.DbColumns.DB_ItemVersion.IsActive, true},
                {GlobalAppSettings.DbColumns.DB_ItemVersion.Comment, comment},
                {GlobalAppSettings.DbColumns.DB_ItemVersion.ItemTypeId,(int) itemType},
                {GlobalAppSettings.DbColumns.DB_ItemVersion.CreatedDate, modifiedDateTime.ToString(GlobalAppSettings.GetDateTimeFormat())}
            };
            var outPutColumn = new List<string> { GlobalAppSettings.DbColumns.DB_ItemVersion.Id };

            try
            {
                result.Success = true;
                result.Value = (_dataProvider.ExecuteScalarQuery(_queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName, values, outPutColumn))).ReturnValue;
                LogExtension.LogInfo("Item Version has been saved successfully", MethodBase.GetCurrentMethod(), " ItemId - " + itemId + " ItemName - " + itemName + " PreviousVersions - " + prevVersions + " RollBackVersionNumber - " + rollBackVersionNo + " ItemTypeId - " + (int)itemType + " ModifiedDate - " + modifiedDateTime);
            }
            catch (SqlException e)
            {
                result.Success = false;
                LogExtension.LogError("Error in saving Item version", e, MethodBase.GetCurrentMethod(), " ItemId - " + itemId + " ItemName - " + itemName + " PreviousVersions - " + prevVersions + " RollBackVersionNumber - " + rollBackVersionNo + " ItemTypeId - " + (int)itemType + " ModifiedDate - " + modifiedDateTime);
                return result;
            }

            return result;
        }

        public bool UpdateNameInVersionTable(Guid itemId, int itemVersion, string itemName)
        {
            var _result = new Result();
            var updateColumns = new List<UpdateColumn>();
            var updatewhereColumns = new List<ConditionColumn>();
            updateColumns.Add(new UpdateColumn { ColumnName = GlobalAppSettings.DbColumns.DB_ItemVersion.ItemName, Value = itemName });
            updatewhereColumns.AddRange(new List<ConditionColumn>{new ConditionColumn
            {
                ColumnName = GlobalAppSettings.DbColumns.DB_ItemVersion.ItemId,
                Condition = Conditions.Equals,
                Value = itemId
            },
            new ConditionColumn
            {
                ColumnName = GlobalAppSettings.DbColumns.DB_ItemVersion.VersionNumber,
                Condition = Conditions.Equals,
                LogicalOperator = LogicalOperators.AND,
                Value = itemVersion
            },
            new ConditionColumn
            {
                TableName = GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName,
                ColumnName = GlobalAppSettings.DbColumns.DB_ItemVersion.IsActive,
                Condition = Conditions.Equals,
                LogicalOperator = LogicalOperators.AND,
                Value = true
            }});
            try
            {
                _result = _dataProvider.ExecuteNonQuery(_queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName, updateColumns, updatewhereColumns));
            }
            catch (Exception e)
            {
                LogExtension.LogError("Can't update in item version table", e, MethodBase.GetCurrentMethod(), " ItemId - " + itemId + " ItemName - " + itemName + " ItemVersion - " + itemVersion);
                return false;
            }
            return _result.Status;
        }

        public bool UpdateItemCurrentVersion(Guid itemId)
        {
            var _result = new Result();
            var updateColumns = new List<UpdateColumn>();
            var updatewhereColumns = new List<ConditionColumn>();
            updateColumns.Add(new UpdateColumn { ColumnName = GlobalAppSettings.DbColumns.DB_ItemVersion.IsCurrentVersion, Value = false });
            updatewhereColumns.AddRange(new List<ConditionColumn>{
            new ConditionColumn
            {
                ColumnName = GlobalAppSettings.DbColumns.DB_ItemVersion.ItemId,
                Condition = Conditions.Equals,
                Value = itemId
            },
            new ConditionColumn
            {
                TableName = GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName,
                ColumnName = GlobalAppSettings.DbColumns.DB_ItemVersion.IsActive,
                Condition = Conditions.Equals,
                LogicalOperator = LogicalOperators.AND,
                Value = true
            }
            });
            try
            {
                _result = _dataProvider.ExecuteNonQuery(_queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName, updateColumns, updatewhereColumns));
            }
            catch (Exception e)
            {
                LogExtension.LogError("Cannot update item current version", e, MethodBase.GetCurrentMethod(), " ItemId - " + itemId);
                return false;
            }
            return _result.Status;
        }

        public ItemVersion FindItemVersionByVerionId(Guid itemId, int version)
        {
            var itemVersionDetail = new ItemVersion();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemVersion.ItemId,
                    Condition = Conditions.Equals,
                    Value = itemId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemVersion.VersionNumber,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = version
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemVersion.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            try
            {
                var currentTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone());
                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(
                            GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName, whereColumns));

                itemVersionDetail = result.DataTable.AsEnumerable().Select(a => new ItemVersion
                {
                    ItemId = a.Field<Guid>(GlobalAppSettings.DbColumns.DB_ItemVersion.ItemId),
                    ItemName = a.Field<string>(GlobalAppSettings.DbColumns.DB_ItemVersion.ItemName),
                    VersionNumber = a.Field<int>(GlobalAppSettings.DbColumns.DB_ItemVersion.VersionNumber),
                    IsCurrentVersion = a.Field<bool>(GlobalAppSettings.DbColumns.DB_ItemVersion.IsCurrentVersion),
                    CreatedById = a.Field<int>(GlobalAppSettings.DbColumns.DB_ItemVersion.CreatedById),
                    IsActive = a.Field<bool>(GlobalAppSettings.DbColumns.DB_ItemVersion.IsActive),
                    Comment = a.Field<string>(GlobalAppSettings.DbColumns.DB_ItemVersion.Comment),
                    ItemTypeId = (ItemType)a.Field<int>(GlobalAppSettings.DbColumns.DB_ItemVersion.ItemTypeId),
                    CreatedDate =
                        TimeZoneInfo.ConvertTimeFromUtc(
                            a.Field<DateTime>(GlobalAppSettings.DbColumns.DB_ItemVersion.CreatedDate),
                            currentTimeZoneInfo)
                }).FirstOrDefault();

                itemVersionDetail.CreatedDateString =
                    itemVersionDetail.CreatedDate.ToString(GlobalAppSettings.SystemSettings.DateFormat + " hh:mm tt");
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in find version details", e, MethodBase.GetCurrentMethod(), " ItemId - " + itemId + " Version - " + version);
                return itemVersionDetail;
            }
            return itemVersionDetail;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool UpdateItemStatus(string itemId, bool status)
        {
            var updateColumns = new List<UpdateColumn>();
            var updatewhereColumns = new List<ConditionColumn>();
            updateColumns.Add(new UpdateColumn { ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive, Value = status });
            updatewhereColumns.AddRange(new List<ConditionColumn>{
            new ConditionColumn
            {
                ColumnName = GlobalAppSettings.DbColumns.DB_Item.Id,
                Condition = Conditions.Equals,
                Value = itemId
            }
            });
            try
            {
                _dataProvider.ExecuteNonQuery(_queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, updateColumns, updatewhereColumns));
                LogExtension.LogInfo("Item status has been updated successfully", MethodBase.GetCurrentMethod(), " ItemId - " + itemId + " Status - " + status);
            }
            catch (SqlException e)
            {
                LogExtension.LogError("Error in updating item status", e, MethodBase.GetCurrentMethod(), " ItemId - " + itemId + " Status - " + status);
                return false;
            }
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="itemObj"></param>
        /// <returns></returns>
        public Result PublishFile(ItemDetail itemObj)
        {
            var result = new Result();
            var guid = Guid.NewGuid();
            try
            {
                var values = new Dictionary<string, object>
                {
                    {GlobalAppSettings.DbColumns.DB_Item.Name, itemObj.Name},
                    {GlobalAppSettings.DbColumns.DB_Item.Description, itemObj.Description},
                    {GlobalAppSettings.DbColumns.DB_Item.CreatedById, itemObj.CreatedById},
                    {GlobalAppSettings.DbColumns.DB_Item.ModifiedById, itemObj.CreatedById},
                    {GlobalAppSettings.DbColumns.DB_Item.CreatedDate, itemObj.CreatedDate},
                    {GlobalAppSettings.DbColumns.DB_Item.ModifiedDate, itemObj.CreatedDate},
                    {GlobalAppSettings.DbColumns.DB_Item.ItemTypeId,(int) itemObj.ItemType},
                    {GlobalAppSettings.DbColumns.DB_Item.IsActive, true},
                    {GlobalAppSettings.DbColumns.DB_Item.Extension, itemObj.Extension}
                };

                if (itemObj.CategoryId != null)
                    values.Add(GlobalAppSettings.DbColumns.DB_Item.ParentId, itemObj.CategoryId);
                result = _dataProvider.ExecuteScalarQuery(
                    _queryBuilder.AddToTableWithGUID(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, values,
                        GlobalAppSettings.DbColumns.DB_Item.Id, guid), guid);
                result.ReturnValue = Guid.Parse(result.ReturnValue.ToString());
                LogExtension.LogInfo("Item has been published successfully", MethodBase.GetCurrentMethod(), " ItemName - " + itemObj.Name + " ItemDescription - " + itemObj.Description + " ItemTypeId - " + (int)itemObj.ItemType + " ItemExtension - " + itemObj.Extension + " CreatedById - " + itemObj.CreatedById + " ModifiedById - " + itemObj.CreatedById + " CreatedDate - " + itemObj.CreatedDate + " ModifiedDate - " + itemObj.CreatedDate + " CategoryId - " + itemObj.CategoryId + " Guid - " + guid);
            }
            catch (SqlException e)
            {
                LogExtension.LogError("Error in publishing Item", e, MethodBase.GetCurrentMethod(), " ItemName - " + itemObj.Name + " ItemDescription - " + itemObj.Description + " ItemTypeId - " + (int)itemObj.ItemType + " ItemExtension - " + itemObj.Extension + " CreatedById - " + itemObj.CreatedById + " ModifiedById - " + itemObj.CreatedById + " CreatedDate - " + itemObj.CreatedDate + " ModifiedDate - " + itemObj.CreatedDate + " CategoryId - " + itemObj.CategoryId + " Guid - " + guid);
                return result;
            }
            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reportname"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public bool IsItemNameAlreadyExists(string reportname, Guid? categoryId)
        {
            var result = new Result();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ParentId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = categoryId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = reportname
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = 1
                }
            };
            try
            {
                result =
                    _dataProvider.ExecuteReaderQuery(_queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, whereColumns));
            }
            catch (SqlException e)
            {
                LogExtension.LogError("Error in checking item name", e, MethodBase.GetCurrentMethod(), " ReportName - " + reportname + " CategoryId - " + categoryId);
                return false;
            }

            return result.DataTable.Rows.Count > 0;
        }

        public bool IsItemNameAlreadyExists(string reportname, Guid categoryId, ItemType itemType)
        {
            var result = new Result();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = reportname
                },
                 new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ParentId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = categoryId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = 1
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ItemTypeId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = (int)itemType
                }
            };
            try
            {
                result =
                    _dataProvider.ExecuteReaderQuery(_queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, whereColumns));
            }
            catch (SqlException e)
            {
                LogExtension.LogError("Error in checking item name", e, MethodBase.GetCurrentMethod(), " ReportName - " + reportname + " CategoryId - " + categoryId + " ItemType - " + (int)itemType);
                return false;
            }

            return result.DataTable.Rows.Count > 0;
        }

        public bool IsItemNameAlreadyExists(string reportname, ItemType itemType)
        {
            var result = new Result();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = reportname
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = 1
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ItemTypeId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = (int)itemType
                }
            };
            try
            {
                result =
                    _dataProvider.ExecuteReaderQuery(_queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, whereColumns));
            }
            catch (SqlException e)
            {
                LogExtension.LogError("Error in checking item name", e, MethodBase.GetCurrentMethod(), " ReportName - " + reportname + " ItemType - " + (int)itemType);
                return false;
            }

            return result.DataTable.Rows.Count > 0;
        }

        public ItemDetail GetItemDetailsFromItemId(Guid itemId, bool isActive = true)
        {
            var item = new ItemDetail();

            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Id,
                    Condition = Conditions.Equals,
                    Value = itemId
                }
            };
            if (isActive)
            {
                whereColumns.Add(new ConditionColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                        ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = isActive
                    });
            }
            try
            {
                item =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                            whereColumns)).DataTable.AsEnumerable().Select(r => new ItemDetail
                            {
                                Id = r.Field<Guid>(GlobalAppSettings.DbColumns.DB_Item.Id),
                                ItemType = r.Field<ItemType>(GlobalAppSettings.DbColumns.DB_Item.ItemTypeId),
                                Name = r.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Name),
                                Description = r.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Description),
                                CreatedById = r.Field<int>(GlobalAppSettings.DbColumns.DB_Item.CreatedById),
                                CloneOf = r.Field<Guid?>(GlobalAppSettings.DbColumns.DB_Item.CloneItemId),
                                CategoryId = r.Field<Guid?>(GlobalAppSettings.DbColumns.DB_Item.ParentId),
                                CategoryName =
                                    (r.Field<Guid?>(GlobalAppSettings.DbColumns.DB_Item.ParentId) != null)
                                        ? GetItemDetailsFromItemId(
                                            r.Field<Guid>(GlobalAppSettings.DbColumns.DB_Item.ParentId)).Name
                                        : null,
                                ItemCreatedDate = r.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.CreatedDate),
                                ItemModifiedDate = r.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.ModifiedDate),
                                CreatedDate =
                                    r.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.CreatedDate).ToString(),
                                ModifiedDate =
                                    r.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.ModifiedDate).ToString(),
                                ModifiedById = r.Field<int>(GlobalAppSettings.DbColumns.DB_Item.ModifiedById),
                                Extension = r.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Extension)
                            }).FirstOrDefault();
            }
            catch (Exception e)
            {
                LogExtension.LogError("Cannot find the report", e, MethodBase.GetCurrentMethod(), " ItemId - " + itemId);
            }
            return item;
        }

        public ItemDetail GetItemDetailsFromItemPath(string itemPath)
        {
            var item = new ItemDetail();
            var pathSplits = itemPath.Split('/').ToList();
            var selectColumn = new List<SelectedColumn>{
                new SelectedColumn{
                    TableName=GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName="*"
                }
            };
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    TableName=GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name,
                    Condition = Conditions.Equals,
                    Value = pathSplits[2]
                },new ConditionColumn
            {
                TableName=GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                Condition = Conditions.Equals,
                LogicalOperator = LogicalOperators.AND,
                Value = true
            },new ConditionColumn
            {
                TableName="ParentItem",
                ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name,
                Condition = Conditions.Equals,
                LogicalOperator = LogicalOperators.AND,
                Value = pathSplits[1]
            }
            };
            var joinSpecification = new JoinSpecification
            {
                Table = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                Column =
                    new List<JoinColumn>
                    {
                        new JoinColumn
                        {
                            TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                            JoinedColumn = GlobalAppSettings.DbColumns.DB_Item.Id,
                            Operation = Conditions.Equals,
                            ParentTableColumn = GlobalAppSettings.DbColumns.DB_Item.ParentId
                        }
                    },
                JoinType = JoinTypes.Inner,
                JoinTableAliasName = "ParentItem"
            };
            try
            {
                item =
                    _dataProvider.ExecuteReaderQuery(
                       _queryBuilder.ApplyWhereClause(_queryBuilder.ApplyJoins(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, selectColumn, joinSpecification), whereColumns)).DataTable.AsEnumerable().Select(r => new ItemDetail
                            {
                                Id = r.Field<Guid>(GlobalAppSettings.DbColumns.DB_Item.Id),
                                ItemType = r.Field<ItemType>(GlobalAppSettings.DbColumns.DB_Item.ItemTypeId),
                                Name = r.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Name),
                                Description = r.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Description),
                                CreatedById = r.Field<int>(GlobalAppSettings.DbColumns.DB_Item.CreatedById),
                                CloneOf = r.Field<Guid?>(GlobalAppSettings.DbColumns.DB_Item.CloneItemId),
                                CategoryId = r.Field<Guid?>(GlobalAppSettings.DbColumns.DB_Item.ParentId),
                                ItemCreatedDate = r.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.CreatedDate),
                                ItemModifiedDate = r.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.ModifiedDate),
                                CreatedDate =
                                    r.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.CreatedDate).ToString(),
                                ModifiedDate =
                                    r.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.ModifiedDate).ToString(),
                                ModifiedById = r.Field<int>(GlobalAppSettings.DbColumns.DB_Item.ModifiedById),
                                Extension = r.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Extension)
                            }).FirstOrDefault();
            }
            catch (Exception e)
            {
                LogExtension.LogError("Cannot find the report", e, MethodBase.GetCurrentMethod(), " ItemPath - " + itemPath);
            }
            return item;
        }

        public ItemDetail GetItemDetailsFromItemName(string itemName, ItemType? itemType = null)
        {
            var item = new ItemDetail();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    TableName=GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name,
                    Condition = Conditions.Equals,
                    Value = itemName
                },new ConditionColumn
            {
                TableName=GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                Condition = Conditions.Equals,
                LogicalOperator = LogicalOperators.AND,
                Value = true
            },
            };
            if (itemType != null)
            {
                whereColumns.Add(
                new ConditionColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                        ColumnName = GlobalAppSettings.DbColumns.DB_Item.ItemTypeId,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = (int)itemType
                    });
            }
            try
            {
                item =
                    _dataProvider.ExecuteReaderQuery(
                       _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, whereColumns)).DataTable.AsEnumerable().Select(r => new ItemDetail
                       {
                           Id = r.Field<Guid>(GlobalAppSettings.DbColumns.DB_Item.Id),
                           ItemType = r.Field<ItemType>(GlobalAppSettings.DbColumns.DB_Item.ItemTypeId),
                           Name = r.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Name),
                           Description = r.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Description),
                           CreatedById = r.Field<int>(GlobalAppSettings.DbColumns.DB_Item.CreatedById),
                           CloneOf = r.Field<Guid?>(GlobalAppSettings.DbColumns.DB_Item.CloneItemId),
                           CategoryId = r.Field<Guid?>(GlobalAppSettings.DbColumns.DB_Item.ParentId),
                           ItemCreatedDate = r.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.CreatedDate),
                           ItemModifiedDate = r.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.ModifiedDate),
                           CreatedDate =
                               r.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.CreatedDate).ToString(),
                           ModifiedDate =
                               r.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.ModifiedDate).ToString(),
                           ModifiedById = r.Field<int>(GlobalAppSettings.DbColumns.DB_Item.ModifiedById),
                           Extension = r.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Extension)
                       }).FirstOrDefault();
            }
            catch (Exception e)
            {
                LogExtension.LogError("Cannot find the report", e, MethodBase.GetCurrentMethod(), " ItemName - " + itemName);
            }
            return item;
        }

        public DataTable GetItemLog(Guid itemId)
        {


            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ItemLog.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemLog.ItemId,
                    Condition = Conditions.Equals,
                    Value = itemId
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ItemLog.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemLog.ParentId,
                    Condition = Conditions.IS,
                    LogicalOperator = LogicalOperators.AND,
                    Value = null
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ItemLog.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemLog.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            var FromCategoryAlias = "FromCategory";
            var ToCategoryAlias = "ToCategory";
            var CurrentUser = "CurrentUser";
            var TargetUser = "TargetUser";
            var selectColumns = new List<SelectedColumn>
            {
                new SelectedColumn {TableName = GlobalAppSettings.DbColumns.DB_ItemLog.DB_TableName, ColumnName = "*"},
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    AliasName="ItemName",
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name,
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ItemLog.DB_TableName,
                    AliasName="VersionNumber",
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemLog.ItemVersionId,
                },
                new SelectedColumn
                {
                    JoinAliasName = FromCategoryAlias,
                    AliasName="FromCategoryName",
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name,
                },
                new SelectedColumn
                {
                    JoinAliasName = ToCategoryAlias,
                    AliasName="ToCategoryName",
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name,
                },
                new SelectedColumn
                {
                    JoinAliasName = CurrentUser,
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.FirstName
                },
                new SelectedColumn
                {
                    JoinAliasName = CurrentUser,
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.LastName
                },
                new SelectedColumn
                {
                    JoinAliasName = CurrentUser,
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.DisplayName
                },
                
                new SelectedColumn
                {
                    JoinAliasName = TargetUser,
                    AliasName="TargetUserFullName",
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.DisplayName
                },
                new SelectedColumn
                {
                    JoinAliasName = CurrentUser,
                    AliasName="CurrentuserName",
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.UserName
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ItemLogType.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemLogType.Name
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
                                TableName = CurrentUser,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_User.Id,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_ItemLog.UpdatedUserId,
                                ParentTable = GlobalAppSettings.DbColumns.DB_ItemLog.DB_TableName,
                            }
                        },
                    JoinType = JoinTypes.Left,
                    JoinTableAliasName = CurrentUser
                },
                new JoinSpecification
                {
                    Table = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    Column =
                        new List<JoinColumn>
                        {
                            new JoinColumn
                            {
                                TableName = TargetUser,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_User.Id,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_ItemLog.UpdatedUserId,
                                ParentTable = GlobalAppSettings.DbColumns.DB_ItemLog.DB_TableName
                            }
                        },
                    JoinType = JoinTypes.Left,
                    JoinTableAliasName = TargetUser
                }
                ,
                new JoinSpecification
                {
                    Table = GlobalAppSettings.DbColumns.DB_ItemLogType.DB_TableName,
                    Column =
                        new List<JoinColumn>
                        {
                            new JoinColumn
                            {
                                TableName = GlobalAppSettings.DbColumns.DB_ItemLogType.DB_TableName,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_ItemLogType.Id,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_ItemLog.ItemLogTypeId,
                                ParentTable = GlobalAppSettings.DbColumns.DB_ItemLog.DB_TableName
                            }
                        },
                    JoinType = JoinTypes.Inner
                },
                new JoinSpecification
                {
                    Table = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    Column =
                        new List<JoinColumn>
                        {
                            new JoinColumn
                            {
                                TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_Item.Id,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_ItemLog.ItemId,
                                ParentTable = GlobalAppSettings.DbColumns.DB_ItemLog.DB_TableName
                            }
                        },
                    JoinType = JoinTypes.Left
                },
                new JoinSpecification
                {
                    Table = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    Column =
                        new List<JoinColumn>
                        {
                            new JoinColumn
                            {
                                TableName = FromCategoryAlias,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_Item.Id,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_ItemLog.FromCategoryId,
                                ParentTable = GlobalAppSettings.DbColumns.DB_ItemLog.DB_TableName
                            }
                        },
                    JoinType = JoinTypes.Left,
                    JoinTableAliasName = FromCategoryAlias
                },
                new JoinSpecification
                {
                    Table = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    Column =
                        new List<JoinColumn>
                        {
                            new JoinColumn
                            {
                                TableName = ToCategoryAlias,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_ItemLogType.Id,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_ItemLog.ToCategoryId,
                                ParentTable = GlobalAppSettings.DbColumns.DB_ItemLog.DB_TableName
                            }
                        },
                    JoinType = JoinTypes.Left,
                    JoinTableAliasName = ToCategoryAlias
                },
                new JoinSpecification
                {
                    Table = GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName,
                    Column =
                        new List<JoinColumn>
                        {
                            new JoinColumn
                            {
                                TableName = GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_ItemVersion.Id,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_ItemLog.ItemVersionId,
                                ParentTable = GlobalAppSettings.DbColumns.DB_ItemLog.DB_TableName
                            }
                        },
                    JoinType = JoinTypes.Left
                }
            };
            var dataTableOut = _dataProvider.ExecuteReaderQuery(_queryBuilder.ApplyWhereClause(
                _queryBuilder.ApplyMultipleJoins(GlobalAppSettings.DbColumns.DB_ItemLog.DB_TableName, selectColumns,
                    joinSpecification), whereColumns)).DataTable;
            return dataTableOut;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="categoryId"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public bool IsItemNameAlreadyExistsForUpdate(string itemName, Guid? categoryId, Guid itemId)
        {
            var result = new Result();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ParentId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = categoryId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = itemName
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Id,
                    Condition = Conditions.NotEquals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = itemId
                },
                new ConditionColumn
                {
                    ColumnName =  GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            try
            {
                result =
                    _dataProvider.ExecuteReaderQuery(_queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, whereColumns));
            }
            catch (SqlException e)
            {
                LogExtension.LogError("Error in checking item name to update", e, MethodBase.GetCurrentMethod(), " ItemName - " + itemName + " CategoryId - " + categoryId + " ItemId - " + itemId);
                return false;
            }

            return result.DataTable.Rows.Count > 0;
        }

        public bool IsItemNameAlreadyExistsForUpdate(string itemName, Guid categoryId, Guid itemId, ItemType itemType)
        {
            var result = new Result();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ParentId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = categoryId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = itemName
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Id,
                    Condition = Conditions.NotEquals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = itemId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ItemTypeId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = (int) itemType
                }
            };

            try
            {
                result =
                    _dataProvider.ExecuteReaderQuery(_queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, whereColumns));
            }
            catch (SqlException e)
            {
                LogExtension.LogError("Error in checking item name to update", e, MethodBase.GetCurrentMethod(), " ItemName - " + itemName + " CategoryId - " + categoryId + " ItemId - " + itemId + " ItemType - " + (int)itemType);
                return false;
            }

            return result.DataTable.Rows.Count > 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reportItemId"></param>
        /// <param name="dataSourceId"></param>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        public bool AddDataSourceWithRdl(string reportItemId, string dataSourceId, string dataSourceName)
        {
            try
            {
                var values = new Dictionary<string, object>
                {
                    {GlobalAppSettings.DbColumns.DB_ReportDataSource.ReportItemId, reportItemId},
                    {GlobalAppSettings.DbColumns.DB_ReportDataSource.DataSourceItemId, dataSourceId},
                    {
                        GlobalAppSettings.DbColumns.DB_ReportDataSource.ModifiedDate,
                        DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    },
                    {GlobalAppSettings.DbColumns.DB_ReportDataSource.Name, dataSourceName},
                    {GlobalAppSettings.DbColumns.DB_ReportDataSource.IsActive, true}
                };
                _dataProvider.ExecuteNonQuery(_queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_ReportDataSource.DB_TableName, values));
                LogExtension.LogInfo("Datasource has been added to report successfully", MethodBase.GetCurrentMethod(), " ReportItemId - " + reportItemId + " DataSourceId - " + dataSourceId + " DataSourceName - " + dataSourceName);
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in adding datasource with report", e, MethodBase.GetCurrentMethod(), " ReportItemId - " + reportItemId + " DataSourceId - " + dataSourceId + " DataSourceName - " + dataSourceName);
                return false;
            }
            return true;
        }

        public bool DisableDataSourceOfRdl(string reportItemId)
        {
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ReportDataSource.ReportItemId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = reportItemId
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ReportDataSource.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ReportDataSource.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            var updateColumns = new List<UpdateColumn>{
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ReportDataSource.IsActive,
                    Value = false
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ReportDataSource.ModifiedDate,
                    Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                }
            };
            try
            {
                _dataProvider.ExecuteNonQuery(_queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_ReportDataSource.DB_TableName, updateColumns, whereColumns));
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in disabling datasource of report", e, MethodBase.GetCurrentMethod(), " ReportItemId - " + reportItemId);
                return false;
            }
            return true;
        }

        public bool UpdateDataSourceOfRdl(string dataSourceName, string dataSourceId, string reportId)
        {
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ReportDataSource.ReportItemId,
                    Condition = Conditions.Equals,
                    Value = reportId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ReportDataSource.Name,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = dataSourceName
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ReportDataSource.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            var updateColumns = new List<UpdateColumn>{
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ReportDataSource.DataSourceItemId,
                    Value = dataSourceId
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ReportDataSource.ModifiedDate,
                    Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                }
            };
            try
            {
                _dataProvider.ExecuteNonQuery(_queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_ReportDataSource.DB_TableName, updateColumns, whereColumns));
                LogExtension.LogInfo("Datsource of report has been updated successfully", MethodBase.GetCurrentMethod(), " ReportId - " + reportId + " DataSourceName - " + dataSourceName + " DataSourceId - " + dataSourceId);
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in updating datasource of report", e, MethodBase.GetCurrentMethod(), " ReportId - " + reportId + " DataSourceName - " + dataSourceName + " DataSourceId - " + dataSourceId);
                return false;
            }
            return true;
        }

        /// <summary>
        /// From this method, we can get the list of datasource that we have mapped with the particular report id
        /// </summary>
        /// <param name="reportId">Id of the report</param>
        /// <returns>Return the datatable contains the datasource id corresponds to the report id.</returns>
        public List<Guid> GetDataSourceListbyReportId(Guid reportId)
        {
            var reportDataSourceList = new List<Guid>();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ReportDataSource.ReportItemId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = reportId
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ReportDataSource.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ReportDataSource.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            try
            {
                reportDataSourceList = _dataProvider.ExecuteReaderQuery(_queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_ReportDataSource.DB_TableName, whereColumns)).DataTable.AsEnumerable().Select(s => s.Field<Guid>(GlobalAppSettings.DbColumns.DB_ReportDataSource.DataSourceItemId)).ToList();
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in getting datasource of report", e, MethodBase.GetCurrentMethod(), " ReportId - " + reportId);
                return reportDataSourceList;
            }
            return reportDataSourceList;
        }

        public List<DataSourceMappingInfo> GetDataSourceDetailListbyReportId(Guid reportId)
        {
            var reportDataSourceList = new List<DataSourceMappingInfo>();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ReportDataSource.ReportItemId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = reportId
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ReportDataSource.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ReportDataSource.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            try
            {
                var result = _dataProvider.ExecuteReaderQuery(_queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_ReportDataSource.DB_TableName, whereColumns));
                if (result.Status)
                {
                    reportDataSourceList = result.DataTable.AsEnumerable().Select(s => new DataSourceMappingInfo
                    {
                        DataSourceId = s.Field<Guid>(GlobalAppSettings.DbColumns.DB_ReportDataSource.DataSourceItemId),
                        Name = s.Field<string>(GlobalAppSettings.DbColumns.DB_ReportDataSource.Name)
                    }).ToList();
                }
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in getting datasource of report", e, MethodBase.GetCurrentMethod(), " ReportId - " + reportId);
                return reportDataSourceList;
            }
            return reportDataSourceList;
        }

        /// <summary>
        /// This method is used to map password to datasource if the datasource has password
        /// </summary>
        /// <param name="dataSourceId">DataSourceId which is saved as an item id in database</param>
        /// <param name="passWord">Provided password of data source</param>
        /// <returns>Returns true if values added successfully, otherwise false</returns>
        public bool AddPasswordForDataSource(string dataSourceId, string passWord)
        {
            try
            {
                var values = new Dictionary<string, object>
                {
                    {GlobalAppSettings.DbColumns.DB_DataSourceDetail.DataSourceId, dataSourceId},
                    {GlobalAppSettings.DbColumns.DB_DataSourceDetail.Password, passWord},
                    {
                        GlobalAppSettings.DbColumns.DB_DataSourceDetail.ModifiedDate,
                        DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    },
                    {GlobalAppSettings.DbColumns.DB_DataSourceDetail.IsActive, true}
                };
                _dataProvider.ExecuteNonQuery(_queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_DataSourceDetail.DB_TableName, values));
                LogExtension.LogInfo("Password has been added to the datasource successfully", MethodBase.GetCurrentMethod(), " DataSourceId - " + dataSourceId + " Password - " + passWord);
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in adding password for datasource", e, MethodBase.GetCurrentMethod(), " DataSourceId - " + dataSourceId + " Password - " + passWord);
                return false;
            }
            return true;
        }

        public bool UpdatePasswordOfDataSource(Guid dataSourceId, string passWord)
        {
            var updatewhereColumns = new List<ConditionColumn>();
            updatewhereColumns.Add(new ConditionColumn
            {
                ColumnName = GlobalAppSettings.DbColumns.DB_DataSourceDetail.DataSourceId,
                Condition = Conditions.Equals,
                Value = dataSourceId
            });
            updatewhereColumns.Add(new ConditionColumn
            {
                ColumnName = GlobalAppSettings.DbColumns.DB_DataSourceDetail.IsActive,
                LogicalOperator=LogicalOperators.AND,
                Condition = Conditions.Equals,
                Value = true
            });
            var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_DataSourceDetail.DB_TableName,
                            updatewhereColumns));
            if (result.DataTable.Rows.Count == 0)
            {
                return AddPasswordForDataSource(dataSourceId.ToString(), passWord);
            }
            var updateColumns = new List<UpdateColumn>();
            updateColumns.Add(new UpdateColumn
            {
                ColumnName = GlobalAppSettings.DbColumns.DB_DataSourceDetail.ModifiedDate,
                Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
            });
            updateColumns.Add(new UpdateColumn
            {
                ColumnName = GlobalAppSettings.DbColumns.DB_DataSourceDetail.Password,
                Value = passWord
            });
            try
            {
                _dataProvider.ExecuteNonQuery(_queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName, updateColumns, updatewhereColumns));
                LogExtension.LogInfo("Password of datasource has been updated successfully", MethodBase.GetCurrentMethod(), " DataSourceId - " + dataSourceId + " Password - " + passWord);
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in updating password of datasource", e, MethodBase.GetCurrentMethod(), " DataSourceId - " + dataSourceId + " Password - " + passWord);
                return false;
            }
            return true;
        }

        public bool DisableDataSourcePassWord(Guid dataSourceId)
        {
            var updateColumns = new List<UpdateColumn>();
            var updatewhereColumns = new List<ConditionColumn>();
            updateColumns.Add(new UpdateColumn { ColumnName = GlobalAppSettings.DbColumns.DB_DataSourceDetail.IsActive, Value = false });
            updateColumns.Add(new UpdateColumn
            {
                ColumnName = GlobalAppSettings.DbColumns.DB_DataSourceDetail.ModifiedDate,
                Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
            });
            updatewhereColumns.Add(new ConditionColumn
            {
                ColumnName = GlobalAppSettings.DbColumns.DB_DataSourceDetail.DataSourceId,
                Condition = Conditions.Equals,
                Value = dataSourceId
            });
            updatewhereColumns.Add(new ConditionColumn
            {
                ColumnName = GlobalAppSettings.DbColumns.DB_DataSourceDetail.IsActive,
                Condition = Conditions.Equals,
                Value = true
            });
            try
            {
                _dataProvider.ExecuteNonQuery(_queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName, updateColumns, updatewhereColumns));
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in disabling datasource password", e, MethodBase.GetCurrentMethod(), " DataSourceId - " + dataSourceId);
                return false;
            }
            return true;
        }

        public string GetItemLocation(Guid itemId, ItemType fileType, string itemName = "", int versionId = 0)
        {
            var itemLocation = String.Empty;
            try
            {
                var cloneItemId = GetCloneOfItem(itemId);
                if (String.IsNullOrEmpty(cloneItemId.ToString()))
                    cloneItemId = itemId;

                if (versionId == 0)
                    versionId =
                        FindItemCurrentVersion(itemId)
                            .AsEnumerable()
                            .Select(a => a.Field<int>(GlobalAppSettings.DbColumns.DB_ItemVersion.VersionNumber))
                            .FirstOrDefault();

                var basePath = GlobalAppSettings.GetItemsPath() + cloneItemId + "\\" + versionId;

                if (fileType != ItemType.Dashboard)
                {
                    itemLocation = String.IsNullOrWhiteSpace(itemName)
                        ? Directory.GetFiles(basePath).FirstOrDefault().ToString()
                        : basePath + "\\" + itemName;
                }
                else
                    itemLocation = basePath;

                return itemLocation;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in getting item location", e, MethodBase.GetCurrentMethod(), " ItemId - " + itemId + " FileType - " + fileType + " ItemName - " + itemName + " VersionId - " + versionId + " ItemLocation - " + itemLocation);
                return itemLocation;
            }
        }

        public Guid GetCloneItemId(Guid itemId)
        {
            var cloneItemId = GetCloneOfItem(itemId);
            if (cloneItemId != null)
            {
                itemId = Guid.Parse(cloneItemId.ToString());
            }
            return itemId;
        }

        public Guid? GetCloneOfItem(Guid fileId)
        {
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Item.Id,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = fileId
                    }
                    // We have scenario to deleting the parent item if the clone item remains exist, so we no need to
                    // check for isactive field here
                };
                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                            whereColumns));
                if (result.DataTable.Rows.Count == 0)
                {
                    return null;
                }
                var itemId =
                    result.DataTable.AsEnumerable()
                        .Select(a => a.Field<Guid?>(GlobalAppSettings.DbColumns.DB_Item.CloneItemId))
                        .First();

                if (!String.IsNullOrEmpty(itemId.ToString()))
                {
                    return GetCloneOfItem(Guid.Parse(itemId.ToString()));
                }
                else
                {
                    itemId = fileId;
                    return itemId;
                }
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in getting Clone of item", e, MethodBase.GetCurrentMethod(), " FileId - " + fileId);
                return fileId;
            }

            return fileId;
        }

        public DataSourceDefinition GetDataSourceDefinition(Guid dataSourceId)
        {
            DataSourceDefinition dataSourceDef = null;
            try
            {
                var dataSourceLocation = GetItemLocation(dataSourceId, ItemType.Datasource);

                var xmlSerializer = new XmlSerializer(typeof(DataSourceDefinition));
                if (File.Exists(dataSourceLocation))
                {
                    using (var reader = new StreamReader(dataSourceLocation))
                    {
                        dataSourceDef = (DataSourceDefinition)xmlSerializer.Deserialize(reader);
                        reader.Close();
                    }
                    dataSourceDef.Password = GetDataSourcePassword(dataSourceId);
                    return dataSourceDef;
                }
                return dataSourceDef;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in getting datasource definition", e, MethodBase.GetCurrentMethod(), " DataSourceId - " + dataSourceId + " DataSourceDefinition - " + dataSourceDef);
                return dataSourceDef;
            }
        }

        public DataSourceMappingInfo FinddatasourceByDatasourceName(Guid? categoryId, string dataSourceName)
        {
            var result = new DataSourceMappingInfo();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ParentId,
                    Condition = Conditions.Equals,
                    Value = categoryId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value=dataSourceName
                }
            };
            try
            {
                result =
                    _dataProvider.ExecuteReaderQuery(_queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                        whereColumns)).DataTable.AsEnumerable().Select(x =>
                        new DataSourceMappingInfo()
                        {
                            DataSourceId = x.Field<Guid>(GlobalAppSettings.DbColumns.DB_Item.Id),
                            Name = x.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Name)
                        }).FirstOrDefault();
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in getting datasource with name", e, MethodBase.GetCurrentMethod(), " CategoryId - " + categoryId + " DataSourceName - " + dataSourceName);
                return result;
            }
            return result;
        }

        public List<DataSourceInfo> GetReportDatasourceList(Guid reportId)
        {
            var dataSourceList = new List<DataSourceInfo>();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ReportDataSource.ReportItemId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = reportId
                },
                new ConditionColumn
                {
                    TableName=GlobalAppSettings.DbColumns.DB_ReportDataSource.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ReportDataSource.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            var selected = new List<SelectedColumn>
            {
                new SelectedColumn {
             TableName=GlobalAppSettings.DbColumns.DB_ReportDataSource.DB_TableName,
             ColumnName=GlobalAppSettings.DbColumns.DB_ReportDataSource.DataSourceItemId
            },
             new SelectedColumn {
             TableName=GlobalAppSettings.DbColumns.DB_ReportDataSource.DB_TableName,
             ColumnName=GlobalAppSettings.DbColumns.DB_ReportDataSource.Name,
             AliasName = "DataSourceName"
            },
            new SelectedColumn {
             TableName=GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
             ColumnName=GlobalAppSettings.DbColumns.DB_Item.Name
            }
            };
            var joinSpecification = new JoinSpecification
            {
                Table = GlobalAppSettings.DbColumns.DB_ReportDataSource.DB_TableName,
                Column =
                    new List<JoinColumn>
                    {
                        new JoinColumn
                        {
                            TableName = GlobalAppSettings.DbColumns.DB_ReportDataSource.DB_TableName,
                            JoinedColumn = GlobalAppSettings.DbColumns.DB_ReportDataSource.DataSourceItemId,
                            Operation = Conditions.Equals,
                            ParentTableColumn = GlobalAppSettings.DbColumns.DB_Item.Id
                        }
                    },
                JoinType = JoinTypes.Inner
            };
            try
            {
                dataSourceList =
                    _dataProvider.ExecuteReaderQuery(_queryBuilder.ApplyWhereClause(_queryBuilder.ApplyJoins(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, selected, joinSpecification), whereColumns))
                        .DataTable.AsEnumerable()
                        .Select(x =>
                            new DataSourceInfo()
                            {
                                DataSourceId = x.Field<Guid>(GlobalAppSettings.DbColumns.DB_ReportDataSource.DataSourceItemId),
                                DataSourceName = x.Field<string>("DataSourceName"),
                                Name = x.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Name)
                            }).ToList();
            }
            catch
            {
                return dataSourceList;
            }
            return dataSourceList;
        }

        public List<ItemDetail> GetAllItemsOfUserFromItemTrash(int userId)
        {
            var result = new List<ItemDetail>();
            var whereColumns = new List<ConditionColumn>();
            var selectedColumns = new List<SelectedColumn> {
                new SelectedColumn
                {
                     TableName=GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                     ColumnName="*"
                },
                new SelectedColumn
                {
                     TableName=GlobalAppSettings.DbColumns.DB_ItemTrash.DB_TableName,
                     ColumnName="*"
                }
            };
            whereColumns.Add(new ConditionColumn
            {
                TableName = GlobalAppSettings.DbColumns.DB_ItemTrash.DB_TableName,
                ColumnName = GlobalAppSettings.DbColumns.DB_ItemTrash.TrashedById,
                Condition = Conditions.Equals,
                Value = userId
            });
            whereColumns.Add(new ConditionColumn
            {
                TableName = GlobalAppSettings.DbColumns.DB_ItemTrash.DB_TableName,
                ColumnName = GlobalAppSettings.DbColumns.DB_ItemTrash.IsActive,
                Condition = Conditions.Equals,
                LogicalOperator = LogicalOperators.AND,
                Value = 1
            });
            var joinSpecification = new JoinSpecification
            {
                Table = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                Column =
                    new List<JoinColumn>
                    {
                        new JoinColumn
                        {
                            TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                            JoinedColumn = GlobalAppSettings.DbColumns.DB_Item.Id,
                            Operation = Conditions.Equals,
                            ParentTableColumn = GlobalAppSettings.DbColumns.DB_ItemTrash.ItemId
                        }
                    },
                JoinType = JoinTypes.Inner
            };
            try
            {
                result =
                    _dataProvider.ExecuteReaderQuery(_queryBuilder.ApplyWhereClause(_queryBuilder.ApplyJoins(GlobalAppSettings.DbColumns.DB_ItemTrash.DB_TableName, selectedColumns, joinSpecification), whereColumns)).DataTable.AsEnumerable().Select(row => new ItemDetail()
                    {
                        Id = row.Field<Guid>(GlobalAppSettings.DbColumns.DB_Item.Id),
                        Name = row.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Name),
                        Description = row.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Description),
                        ItemType = (ItemType)row.Field<int>(GlobalAppSettings.DbColumns.DB_Item.ItemTypeId),
                        CreatedById = row.Field<int>(GlobalAppSettings.DbColumns.DB_Item.CreatedById),
                        CreatedByDisplayName =
                            row.Field<string>("CreatedByDisplayName"),
                        CreatedDate =
                            row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.CreatedDate)
                                .ToString(),
                        ModifiedById = row.Field<int>(GlobalAppSettings.DbColumns.DB_Item.ModifiedById),
                        ModifiedByFullName =
                            row.Field<string>("ModifiedByFirstName") + " " + row.Field<string>("ModifiedByLastName"),
                        ModifiedDate =
                            row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.ModifiedDate)
                                .ToString(),
                        IsCreatedByActive = row.Field<bool>("IsCreatedByActive"),
                        IsModifiedByActive = row.Field<bool>("IsModifiedByActive")
                    }).ToList();
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in geting reports of user from trash", e, MethodBase.GetCurrentMethod(), " UserId - " + userId);
                return result;
            }

            return result;
        }

        public bool RestoreItemWithItemId(Guid itemId, int userId)
        {
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Id,
                    Condition = Conditions.Equals,
                    Value = itemId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = false
                }
            };
            var updateColumns = new List<UpdateColumn>
            {
                new UpdateColumn {ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive, Value = true},
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ModifiedDate,
                    Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ModifiedById,
                    Value = userId
                }
            };

            try
            {
                _dataProvider.ExecuteNonQuery(_queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, updateColumns, whereColumns));
                LogExtension.LogInfo("Item has been restored successfully", MethodBase.GetCurrentMethod(), " ItemId - " + itemId + " UserId - " + userId);
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in restoring item", e, MethodBase.GetCurrentMethod(), " ItemId - " + itemId + " UserId - " + userId);
                return false;
            }
            return true;
        }

        public DataTable FindItemCurrentVersion(Guid itemId)
        {
            var result = new Result();
            var cloneItemId = GetCloneOfItem(itemId);
            if (String.IsNullOrEmpty(cloneItemId.ToString()))
                cloneItemId = itemId;

            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemVersion.ItemId,
                    Condition = Conditions.Equals,
                    Value = cloneItemId
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemVersion.IsCurrentVersion,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemVersion.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };

            result =
                _dataProvider.ExecuteReaderQuery(
                    _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName,
                        whereColumns));

            return result.DataTable;
        }

        public int FindItemPrevVersionCount(Guid itemId)
        {
            var itemVersionCount = 0;
            var selectedColumns = new List<SelectedColumn> {
                    new SelectedColumn
                    {
                     TableName=GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName,
                     ColumnName="*"
                    },
                    new SelectedColumn
                    {
                     TableName=GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName,
                     ColumnName=GlobalAppSettings.DbColumns.DB_ItemVersion.IsActive
                    },
                    new SelectedColumn
                    {
                     TableName=GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                     ColumnName=GlobalAppSettings.DbColumns.DB_User.FirstName
                    },
                    new SelectedColumn
                    {
                     TableName=GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                     ColumnName=GlobalAppSettings.DbColumns.DB_User.LastName
                    },
                    new SelectedColumn
                    {
                     TableName=GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                     ColumnName=GlobalAppSettings.DbColumns.DB_User.DisplayName
                    }
            };
            var joinSpecification = new JoinSpecification
            {
                Table = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                Column =
                    new List<JoinColumn>
                    {
                        new JoinColumn
                        {
                            TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            JoinedColumn = GlobalAppSettings.DbColumns.DB_User.Id,
                            Operation = Conditions.Equals,
                            ParentTableColumn = GlobalAppSettings.DbColumns.DB_ItemVersion.CreatedById
                        }
                    },
                JoinType = JoinTypes.Inner
            };
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemVersion.ItemId,
                    Condition = Conditions.Equals,
                    Value = itemId
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemVersion.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            itemVersionCount = _dataProvider.ExecuteReaderQuery(_queryBuilder.ApplyWhereClause(_queryBuilder.ApplyJoins(GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName, selectedColumns, joinSpecification), whereColumns)).DataTable.Rows.Count;

            return itemVersionCount;
        }

        public object AddItemLog(int versionId, ItemLogType logType, int currentUserId, Guid curReportId, int parentId,
            Guid? fromCategory = null, Guid? toCategory = null, DateTime? modifiedDate = null)
        {
            if (modifiedDate == null)
                modifiedDate = DateTime.UtcNow;
            var elements = new Dictionary<string, object>
            {
                {GlobalAppSettings.DbColumns.DB_ItemLog.ItemLogTypeId, (int) logType},
                {GlobalAppSettings.DbColumns.DB_ItemLog.UpdatedUserId, currentUserId},
                {GlobalAppSettings.DbColumns.DB_ItemLog.ItemId, curReportId},
                {GlobalAppSettings.DbColumns.DB_ItemLog.ItemVersionId, versionId},
                {GlobalAppSettings.DbColumns.DB_ItemLog.ModifiedDate, modifiedDate.Value.ToString(GlobalAppSettings.GetDateTimeFormat())},
                {GlobalAppSettings.DbColumns.DB_ItemLog.IsActive, true}
            };
            if (fromCategory != null)
                elements.Add(GlobalAppSettings.DbColumns.DB_ItemLog.FromCategoryId, fromCategory);
            if (toCategory != null)
                elements.Add(GlobalAppSettings.DbColumns.DB_ItemLog.ToCategoryId, toCategory);
            if (parentId != 0)
                elements.Add(GlobalAppSettings.DbColumns.DB_ItemLog.ParentId, parentId);

            var output = new List<string> { GlobalAppSettings.DbColumns.DB_ItemLog.Id };
            var result =
                _dataProvider.ExecuteScalarQuery(
                    _queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_ItemLog.DB_TableName, elements, output));
            return result.ReturnValue;
        }

        public List<ScheduleItem> GetSchedulesOfReport(Guid reportId)
        {
            var scheduledReports = new List<ScheduleItem>();
            try
            {
                var selectColumns = new List<SelectedColumn>
                {
                    new SelectedColumn
                    {
                        ColumnName = "*",
                        TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName
                    },
                    new SelectedColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name,
                        AliasName = "ReportName",
                        TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName
                    }
                };

                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ItemId,
                        Condition = Conditions.Equals,
                        Value = reportId,
                        TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName,
                        LogicalOperator = LogicalOperators.AND
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.IsActive,
                        Condition = Conditions.Equals,
                        Value = true,
                        TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName,
                        LogicalOperator = LogicalOperators.AND
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Item.Id,
                        Condition = Conditions.Equals,
                        Value = reportId,
                        TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                        LogicalOperator = LogicalOperators.AND
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                        Condition = Conditions.Equals,
                        Value = true,
                        TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                        LogicalOperator = LogicalOperators.AND
                    }
                };

                var joinSpecification = new List<JoinSpecification>
                {
                    new JoinSpecification
                    {
                        Table = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                        Column =
                        new List<JoinColumn>
                        {
                            new JoinColumn
                            {
                                TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_Item.Id,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ItemId,
                                ParentTable = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName
                            }
                        },
                        JoinType = JoinTypes.Inner
                    }
                };

                scheduledReports = _dataProvider.ExecuteReaderQuery(
                    _queryBuilder.ApplyWhereClause(
                        _queryBuilder.ApplyMultipleJoins(GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName,
                            selectColumns, joinSpecification), whereColumns))
                    .DataTable.AsEnumerable()
                    .Select(s => new ScheduleItem
                    {
                        ScheduleId = s.Field<Guid>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.ScheduleId),
                        ItemId = s.Field<Guid>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.ItemId),
                        Name = s.Field<string>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.Name),
                        RecurrenceTypeId = s.Field<int>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.RecurrenceTypeId),
                        RecurrenceInfo = s.Field<string>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.RecurrenceInfo),
                        StartDate = s.Field<DateTime>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.StartDate),
                        EndDate = s.Field<DateTime>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.EndDate),
                        EndAfter = s.Field<int>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.EndAfter),
                        NextSchedule = s.Field<DateTime>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.NextSchedule),
                        ExportTypeId = s.Field<int>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.ExportTypeId),
                        IsEnabled = s.Field<bool>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.IsEnabled),
                        ModifiedById = s.Field<int>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.ModifiedById),
                        ModifiedDate = s.Field<DateTime>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.ModifiedDate),
                        CreatedById = s.Field<int>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.CreatedById),
                        IsActive = s.Field<bool>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.IsActive)
                    }).ToList();

                return scheduledReports;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in getting schedules of report", e, MethodBase.GetCurrentMethod(), " ReportId - " + reportId);
                return scheduledReports;
            }
        }

        public bool CheckIsScheduledReport(Guid reportId, out List<string> scheduleNameList)
        {
            var scheduledReports = GetSchedulesOfReport(reportId);
            if (scheduledReports.Count > 0)
            {
                scheduleNameList = scheduledReports.Select(a => a.Name).ToList();
                return true;
            }
            scheduleNameList = null;
            return false;
        }

        public bool AddItemToTrash(ItemDetail itemObject, int currentUserId)
        {
            var values = new Dictionary<string, object>
            {
                {GlobalAppSettings.DbColumns.DB_ItemTrash.TrashedById, currentUserId},
                {
                    GlobalAppSettings.DbColumns.DB_ItemTrash.TrashedDate,
                    DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                },
                {GlobalAppSettings.DbColumns.DB_ItemTrash.ItemId, itemObject.Id},
                {GlobalAppSettings.DbColumns.DB_ItemTrash.IsActive, true}
            };
            try
            {
                _dataProvider.ExecuteNonQuery(_queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_ItemTrash.DB_TableName, values));
                LogExtension.LogInfo("Report has been Added to Trash successfully", MethodBase.GetCurrentMethod(), " ItemObject - " + itemObject + " CurrentUserId - " + currentUserId);
            }
            catch (SqlException e)
            {
                LogExtension.LogError("Error in Adding Report to Trash", e, MethodBase.GetCurrentMethod(), " ItemObject - " + itemObject + " CurrentUserId - " + currentUserId);
                return false;
            }
            return true;
        }

        public ItemTrash GetReportFromTrashByReportId(Guid reportId)
        {
            var result = new Result();
            var itemTrashDetail = new ItemTrash();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemTrash.ItemId,
                    Condition = Conditions.Equals,
                    Value = reportId
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ItemTrash.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemTrash.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            try
            {
                result = _dataProvider.ExecuteReaderQuery(_queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_ItemTrash.DB_TableName, whereColumns));
                itemTrashDetail = result.DataTable.AsEnumerable().Select(a => new ItemTrash
                {
                    Id = a.Field<int>(GlobalAppSettings.DbColumns.DB_ItemTrash.Id),
                    ItemId = a.Field<Guid>(GlobalAppSettings.DbColumns.DB_ItemTrash.ItemId),
                    TrashedById = a.Field<int>(GlobalAppSettings.DbColumns.DB_ItemTrash.TrashedById),
                    TrashedDate = a.Field<string>(GlobalAppSettings.DbColumns.DB_ItemTrash.TrashedDate),
                    IsActive = a.Field<bool>(GlobalAppSettings.DbColumns.DB_ItemTrash.IsActive)
                }).FirstOrDefault();
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in get report from trash", e, MethodBase.GetCurrentMethod(), " ReportId - " + reportId);
                return itemTrashDetail;
            }

            return itemTrashDetail;
        }

        public bool RemoveItemFromTrash(Guid reportId)
        {
            var result = new Result();
            var updateColumn = new List<UpdateColumn>
            {
                new UpdateColumn {
                 ColumnName=GlobalAppSettings.DbColumns.DB_ItemTrash.IsActive,
                 Value=false
                }
            };
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemTrash.ItemId,
                    Condition = Conditions.Equals,
                    Value = reportId
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ItemTrash.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemTrash.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            try
            {
                result = _dataProvider.ExecuteReaderQuery(_queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_ItemTrash.DB_TableName, updateColumn, whereColumns));
            }
            catch (SqlException e)
            {
                LogExtension.LogError("Report not removed from trash", e, MethodBase.GetCurrentMethod(), " ReportId - " + reportId);
                return false;
            }

            return result.Status;
        }

        public string GetDataSourcePassword(Guid dataSourceId)
        {
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
            {
                ColumnName = GlobalAppSettings.DbColumns.DB_DataSourceDetail.DataSourceId,
                Condition = Conditions.Equals,
                Value = dataSourceId
            },
             new ConditionColumn
            {
                ColumnName = GlobalAppSettings.DbColumns.DB_DataSourceDetail.IsActive,
                Condition = Conditions.Equals,
                LogicalOperator = LogicalOperators.AND,
                Value = true
            }
            };
            var result = _dataProvider.ExecuteReaderQuery(
                    _queryBuilder.SelectAllRecordsFromTable(
                        GlobalAppSettings.DbColumns.DB_DataSourceDetail.DB_TableName, whereColumns));
            if (result.Status && result.DataTable.Rows.Count > 0)
            {
                return result.DataTable.AsEnumerable()
                .Select(r => r.Field<string>(GlobalAppSettings.DbColumns.DB_DataSourceDetail.Password))
                .FirstOrDefault();
            }
            return string.Empty;
        }

        public DataTable ItemsfromCategory(List<string> categoryId, ItemType itemType)
        {
            const string joinAliasName = "User_modified";
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    Value = true
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ItemTypeId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = (int) itemType
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ParentId,
                    Condition = Conditions.IN,
                    LogicalOperator = LogicalOperators.AND,
                    Values = categoryId
                }
            };
            var selectColumns = new List<SelectedColumn>
            {
                new SelectedColumn {TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName, ColumnName = "*"},
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    AliasName="CreatedByFirstName",
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.FirstName
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    AliasName="CreatedByLastName",
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.LastName
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    AliasName="CreatedByDisplayName",
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.DisplayName
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    AliasName="IsCreatedByActive",
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.IsActive
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ParentId
                },
                new SelectedColumn {JoinAliasName = joinAliasName,ColumnName = GlobalAppSettings.DbColumns.DB_User.FirstName, AliasName = "ModifiedByFirstName"},
                new SelectedColumn {JoinAliasName = joinAliasName,ColumnName = GlobalAppSettings.DbColumns.DB_User.LastName, AliasName = "ModifiedByLastName"},
                new SelectedColumn {JoinAliasName = joinAliasName,ColumnName = GlobalAppSettings.DbColumns.DB_User.DisplayName, AliasName = "ModifiedByDisplayName"},
                new SelectedColumn {JoinAliasName = joinAliasName,ColumnName = GlobalAppSettings.DbColumns.DB_User.IsActive, AliasName = "IsModifiedByActive"}
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
                                TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_Item.CreatedById,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_User.Id,
                                ParentTable = GlobalAppSettings.DbColumns.DB_User.DB_TableName
                            }
                        },
                    JoinType = JoinTypes.Inner
                },
                new JoinSpecification
                {
                    Table = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    Column =
                        new List<JoinColumn>
                        {
                            new JoinColumn
                            {
                                TableName = joinAliasName,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_User.Id,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_Item.ModifiedById,
                                ParentTable = GlobalAppSettings.DbColumns.DB_Item.DB_TableName
                            }
                        },
                    JoinType = JoinTypes.Inner,
                    JoinTableAliasName = joinAliasName
                }
            };
            var result = _dataProvider.ExecuteReaderQuery(_queryBuilder.ApplyWhereClause(
                _queryBuilder.ApplyMultipleJoins(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, selectColumns,
                    joinSpecification), whereColumns));
            return result.DataTable;
        }

        public bool DeleteItem(Guid itemId, int userId)
        {
            var item = GetItemDetailsFromItemId(itemId);

            var addItemSuccess = false;
            if (item != null)
            {
                if ((ItemType)item.ItemType == ItemType.Report)
                {
                    DisableDataSourceOfRdl(item.Id.ToString());
                }
                addItemSuccess = AddItemToTrash(item, userId);
            }

            if (addItemSuccess)
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Item.Id,
                        Condition = Conditions.Equals,
                        Value = itemId
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = true
                    }
                };
                var updateColumns = new List<UpdateColumn>
                {
                    new UpdateColumn {ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive, Value = false},
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_Item.ModifiedDate,
                        Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    },
                    new UpdateColumn {ColumnName = GlobalAppSettings.DbColumns.DB_Item.ModifiedById, Value = userId}
                };

                try
                {
                    var result =
                        _dataProvider.ExecuteNonQuery(
                            _queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                                updateColumns, whereColumns));
                    return result.Status;
                }
                catch (Exception e)
                {
                    LogExtension.LogError("Cannot Delete item", e, MethodBase.GetCurrentMethod(), " ItemId - " + itemId + " UserId - " + userId);
                    return false;
                }
            }
            return false;
        }

        public void DeleteDirectory(string targetDirectory)
        {
            var files = Directory.GetFiles(targetDirectory);
            var dirs = Directory.GetDirectories(targetDirectory);

            foreach (var file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (var dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDirectory, false);
        }

        public bool UpdateItemFields(Dictionary<string, object> updateFields, Guid itemId)
        {
            var result = new Result();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Id,
                    Condition = Conditions.Equals,
                    Value = itemId
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            var updateColumns = new List<UpdateColumn>{
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ModifiedDate,
                    Value = updateFields[GlobalAppSettings.DbColumns.DB_Item.ModifiedDate]
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ModifiedById,
                    Value = updateFields[GlobalAppSettings.DbColumns.DB_Item.ModifiedById]
                }
        };
            if (updateFields.ContainsKey(GlobalAppSettings.DbColumns.DB_Item.Name))
            {
                updateColumns.Add(new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name,
                    Value = updateFields[GlobalAppSettings.DbColumns.DB_Item.Name].ToString()
                });
            }
            if (updateFields.ContainsKey(GlobalAppSettings.DbColumns.DB_Item.Description))
            {
                updateColumns.Add(new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Description,
                    Value = updateFields[GlobalAppSettings.DbColumns.DB_Item.Description].ToString()
                });
            }
            if (updateFields.ContainsKey(GlobalAppSettings.DbColumns.DB_Item.Extension))
            {
                updateColumns.Add(new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Extension,
                    Value = updateFields[GlobalAppSettings.DbColumns.DB_Item.Extension].ToString()
                });
            }
            if (updateFields.ContainsKey(GlobalAppSettings.DbColumns.DB_Item.ParentId))
            {
                updateColumns.Add(new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ParentId,
                    Value = updateFields[GlobalAppSettings.DbColumns.DB_Item.ParentId].ToString()
                });
            }
            try
            {
                result = _dataProvider.ExecuteNonQuery(_queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, updateColumns, whereColumns));
            }
            catch (Exception e)
            {
                LogExtension.LogError("Cannot update report fields", e, MethodBase.GetCurrentMethod(), " ItemId - " + itemId + " ModifiedDate - " + updateFields[GlobalAppSettings.DbColumns.DB_Item.ModifiedDate] + " ModifiedById - " + updateFields[GlobalAppSettings.DbColumns.DB_Item.ModifiedById] + " ItemName - " + updateFields[GlobalAppSettings.DbColumns.DB_Item.Name].ToString() + " ItemDescription - " + updateFields[GlobalAppSettings.DbColumns.DB_Item.Description].ToString() + " ItemExtension - " + updateFields[GlobalAppSettings.DbColumns.DB_Item.Extension].ToString() + " ParentId - " + updateFields[GlobalAppSettings.DbColumns.DB_Item.ParentId].ToString());
                return false;
            }
            return result.Status;
        }

        public bool UpdateItemType(Guid reportId, ItemType fileTypeId, int userId)
        {
            var result = new Result();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Id,
                    Condition = Conditions.Equals,
                    Value = reportId
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            var updateColumns = new List<UpdateColumn>
            {
                new UpdateColumn {ColumnName = GlobalAppSettings.DbColumns.DB_Item.ItemTypeId, Value = (int)fileTypeId},
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ModifiedById,
                    Value = userId
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ModifiedDate,
                    Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                }
            };
            try
            {
                result = _dataProvider.ExecuteNonQuery(_queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, updateColumns, whereColumns));
            }
            catch (Exception e)
            {
                LogExtension.LogError("Can't update item type", e, MethodBase.GetCurrentMethod(), " ReportId - " + reportId + " FileTypeId - " + fileTypeId + " UserId - " + userId);
                return false;
            }
            return result.Status;
        }

        public List<Guid> GetCloneHeriarchy(Guid itemId)
        {

            var value = itemId;
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.CloneItemId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = value
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };

            var parentId = _dataProvider.ExecuteReaderQuery(_queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, whereColumns)).DataTable.AsEnumerable().Select(a => a.Field<Guid>(GlobalAppSettings.DbColumns.DB_Item.Id)).ToList();
            for (var y = 0; y < parentId.Count(); y++)
            {
                cloneHeriarchy.Add(parentId[y]);
                GetCloneHeriarchy(Guid.Parse(parentId[y].ToString()));
            }

            return cloneHeriarchy;
        }

        public Result CopyItem(Guid itemId, Guid destinationCategoryId, int copiedByUserId, string itemName)
        {
            var actionResult = new Result();
            var cloneItemId = GetCloneOfItem(itemId);
            if (String.IsNullOrEmpty(cloneItemId.ToString()))
                cloneItemId = itemId;

            var itemDetails = GetItemDetailsFromItemId(Guid.Parse(cloneItemId.ToString()), false);

            if (itemDetails != null)
            {
                var values = new Dictionary<string, object>
                {
                    {GlobalAppSettings.DbColumns.DB_Item.Name, itemName},
                    {GlobalAppSettings.DbColumns.DB_Item.Description, itemDetails.Description},
                    {GlobalAppSettings.DbColumns.DB_Item.CreatedById,copiedByUserId },
                    {GlobalAppSettings.DbColumns.DB_Item.ModifiedById, copiedByUserId},
                    {GlobalAppSettings.DbColumns.DB_Item.CreatedDate, DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())},
                    {GlobalAppSettings.DbColumns.DB_Item.Extension, itemDetails.Extension},
                    {
                        GlobalAppSettings.DbColumns.DB_Item.ModifiedDate,
                        DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    },
                    {GlobalAppSettings.DbColumns.DB_Item.ParentId, destinationCategoryId},
                    {GlobalAppSettings.DbColumns.DB_Item.IsActive, false},
                    {GlobalAppSettings.DbColumns.DB_Item.ItemTypeId, (int)itemDetails.ItemType}
                };
                try
                {
                    var itemversion =
                        FindItemCurrentVersion(Guid.Parse(cloneItemId.ToString()))
                            .AsEnumerable()
                            .Select(a => a.Field<int>(GlobalAppSettings.DbColumns.DB_ItemVersion.VersionNumber))
                            .FirstOrDefault();
                    var filename = itemDetails.Name + itemDetails.Extension;
                    var fileLocation =
                        Path.Combine(GlobalAppSettings.GetItemsPath() + cloneItemId + "\\" + itemversion + "\\" +
                                     filename);
                    if (itemDetails.ItemType == ItemType.Report)
                    {
                        if (!IsItemNameAlreadyExists(itemName, destinationCategoryId, ItemType.Report) && File.Exists(fileLocation))
                        {
                            var guid = Guid.NewGuid();
                            var result =
                                _dataProvider.ExecuteScalarQuery(
                                    _queryBuilder.AddToTableWithGUID(GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                                        values, GlobalAppSettings.DbColumns.DB_Item.Id, guid), guid);
                            var publishedReportId = new Guid(result.ReturnValue.ToString());
                            actionResult.ReturnValue = publishedReportId;
                            var rootPath = Path.Combine(GlobalAppSettings.GetItemsPath() + publishedReportId + "\\1");
                            Directory.CreateDirectory(Path.Combine(rootPath));
                            string destdir;
                            var modifiedDate = DateTime.UtcNow;
                            try
                            {
                                var ext = Path.GetExtension(fileLocation);
                                destdir = Path.Combine(rootPath + "\\" + itemName + ext);
                                SaveItemVersion(publishedReportId, copiedByUserId, 0, 1, itemName + ext, String.Empty,
                                    itemDetails.ItemType, modifiedDate);
                                File.Copy(fileLocation, destdir);
                                if (itemDetails.ItemType == ItemType.Report)
                                {
                                    var copyDataSources = CopyDataSourcesOfReport(new Guid(cloneItemId.ToString()), publishedReportId);
                                    if (copyDataSources)
                                    {
                                        UpdateItemStatus(publishedReportId.ToString(), true);
                                    }
                                }
                                else
                                {
                                    UpdateItemStatus(publishedReportId.ToString(), true);
                                }
                            }
                            catch (Exception e)
                            {
                                LogExtension.LogError("Error in Copying item", e, MethodBase.GetCurrentMethod(),
                                    " ItemId - " + itemId + " DestinationCategoryId - " + destinationCategoryId +
                                    " CopiedByUserId - " + copiedByUserId + " ItemName - " + itemName +
                                    " ItemDescription - " + itemDetails.Description + " CreatedById - " +
                                    itemDetails.CreatedById + " ModifiedById - " + copiedByUserId + " CreatedDate - " +
                                    itemDetails.CreatedDate +
                                    " ItemExtension - " + itemDetails.Extension + " ItemTypeId - " +
                                    (int)itemDetails.ItemType + " ParentId - " + destinationCategoryId);
                                actionResult.Status = false;
                                actionResult.ReturnValue = "Exception";
                            }
                            AddItemLog(itemversion, ItemLogType.Copied, copiedByUserId, publishedReportId, 0, itemDetails.CategoryId,
                                destinationCategoryId, modifiedDate);
                            LogExtension.LogInfo("Report Copied successfully", MethodBase.GetCurrentMethod(),
                                " ItemId - " + itemId + " DestinationCategoryId - " + destinationCategoryId +
                                " CopiedByUserId - " + copiedByUserId + " ItemName - " + itemName +
                                " ItemDescription - " + itemDetails.Description + " CreatedById - " +
                                itemDetails.CreatedById + " ModifiedById - " + copiedByUserId + " CreatedDate - " +
                                itemDetails.CreatedDate +
                                " ItemExtension - " + itemDetails.Extension + " ItemTypeId - " +
                                (int)itemDetails.ItemType + " ParentId - " + destinationCategoryId);
                            actionResult = result;
                        }
                        else
                        {
                            LogExtension.LogInfo("Report Name already Exist", MethodBase.GetCurrentMethod(),
                                " ItemId - " + itemId + " DestinationCategoryId - " + destinationCategoryId +
                                " CopiedByUserId - " + copiedByUserId + " ItemName - " + itemName +
                                " ItemDescription - " + itemDetails.Description + " CreatedById - " +
                                itemDetails.CreatedById + " ModifiedById - " + copiedByUserId + " CreatedDate - " +
                                itemDetails.CreatedDate +
                                " ItemExtension - " + itemDetails.Extension + " ItemTypeId - " +
                                (int)itemDetails.ItemType + " ParentId - " + destinationCategoryId);
                            actionResult.Status = false;
                            actionResult.ReturnValue = "Name";
                        }
                    }
                    else
                    {
                        var direcoryLocation =
                        Path.Combine(GlobalAppSettings.GetItemsPath() + cloneItemId + "\\" + itemversion + "\\");
                        if (!IsItemNameAlreadyExists(itemName, destinationCategoryId, ItemType.Dashboard) && Directory.Exists(direcoryLocation))
                        {
                            var guid = Guid.NewGuid();
                            var result =
                                _dataProvider.ExecuteScalarQuery(
                                    _queryBuilder.AddToTableWithGUID(GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                                        values, GlobalAppSettings.DbColumns.DB_Item.Id, guid), guid);
                            var publishedReportId = new Guid(result.ReturnValue.ToString());
                            actionResult.ReturnValue = publishedReportId;
                            var rootPath = Path.Combine(GlobalAppSettings.GetItemsPath() + publishedReportId + "\\1");
                            Directory.CreateDirectory(Path.Combine(rootPath));
                            string destdir;
                            var modifiedDate = DateTime.UtcNow;
                            try
                            {
                                var tempDirectory = direcoryLocation;
                                destdir = Path.Combine(rootPath);
                                SaveItemVersion(publishedReportId, copiedByUserId, 0, 1, String.Empty, String.Empty,
                                    itemDetails.ItemType, modifiedDate);
                                DirectoryCopy(tempDirectory, destdir, true);
                                UpdateItemStatus(publishedReportId.ToString(), true);
                            }
                            catch (Exception e)
                            {
                                LogExtension.LogError("Error in Copying item", e, MethodBase.GetCurrentMethod(),
                                    " ItemId - " + itemId + " DestinationCategoryId - " + destinationCategoryId +
                                    " CopiedByUserId - " + copiedByUserId + " ItemName - " + itemName +
                                    " ItemDescription - " + itemDetails.Description + " CreatedById - " +
                                    itemDetails.CreatedById + " ModifiedById - " + copiedByUserId + " CreatedDate - " +
                                    itemDetails.CreatedDate +
                                    " ItemExtension - " + itemDetails.Extension + " ItemTypeId - " +
                                    (int)itemDetails.ItemType + " ParentId - " + destinationCategoryId);
                                actionResult.Status = false;
                                actionResult.ReturnValue = "Exception";
                            }
                            AddItemLog(itemversion, ItemLogType.Copied, copiedByUserId, publishedReportId, 0, itemDetails.CategoryId,
                                destinationCategoryId, modifiedDate);
                            LogExtension.LogInfo("Dashboard Copied successfully", MethodBase.GetCurrentMethod(),
                                " ItemId - " + itemId + " DestinationCategoryId - " + destinationCategoryId +
                                " CopiedByUserId - " + copiedByUserId + " ItemName - " + itemName +
                                " ItemDescription - " + itemDetails.Description + " CreatedById - " +
                                itemDetails.CreatedById + " ModifiedById - " + copiedByUserId + " CreatedDate - " +
                                itemDetails.CreatedDate +
                                " ItemExtension - " + itemDetails.Extension + " ItemTypeId - " +
                                (int)itemDetails.ItemType + " ParentId - " + destinationCategoryId);
                            actionResult = result;
                        }
                        else
                        {
                            LogExtension.LogInfo("Dashboard Name already Exist", MethodBase.GetCurrentMethod(),
                                " ItemId - " + itemId + " DestinationCategoryId - " + destinationCategoryId +
                                " CopiedByUserId - " + copiedByUserId + " ItemName - " + itemName +
                                " ItemDescription - " + itemDetails.Description + " CreatedById - " +
                                itemDetails.CreatedById + " ModifiedById - " + copiedByUserId + " CreatedDate - " +
                                itemDetails.CreatedDate +
                                " ItemExtension - " + itemDetails.Extension + " ItemTypeId - " +
                                (int)itemDetails.ItemType + " ParentId - " + destinationCategoryId);
                            actionResult.Status = false;
                            actionResult.ReturnValue = "Name";
                        }
                    }
                }
                catch (Exception e)
                {
                    LogExtension.LogError("Error in Copying the dashboard", e, MethodBase.GetCurrentMethod(),
                        " ItemId - " + itemId + " DestinationCategoryId - " + destinationCategoryId +
                        " CopiedByUserId - " + copiedByUserId + " ItemName - " + itemName + " ItemDescription - " +
                        itemDetails.Description + " CreatedById - " + itemDetails.CreatedById + " ModifiedById - " +
                        copiedByUserId + " CreatedDate - " +
                        itemDetails.CreatedDate + " ItemExtension - " +
                        itemDetails.Extension + " ItemTypeId - " + (int)itemDetails.ItemType + " ParentId - " +
                        destinationCategoryId);
                    actionResult.Status = false;
                    actionResult.ReturnValue = "Exception";
                }
            }
            return actionResult;
        }

        public bool CopyDataSourcesOfReport(Guid originalItemId, Guid copiedItemId)
        {
            var result = true;
            var dataSources = GetDataSourceDetailListbyReportId(originalItemId);
            try
            {
                for (var dataSource = 0; dataSource < dataSources.Count; dataSource++)
                {
                    if (result)
                    {
                        result = AddDataSourceWithRdl(copiedItemId.ToString(), dataSources[dataSource].DataSourceId.ToString(), dataSources[dataSource].Name);
                    }
                }
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in Copying data sources of report", e, MethodBase.GetCurrentMethod(), " OriginalItemId - " + originalItemId + " CopiedItemId - " + copiedItemId);
                return false;
            }
            return result;
        }

        public Result CloneItem(Guid itemId, Guid destinationCategoryId, int copiedByUserId, string itemName)
        {
            var actionResult = new Result();
            var itemDetails = GetItemDetailsFromItemId(itemId);
            if (itemDetails != null)
            {
                var values = new Dictionary<string, object>
                {
                    {GlobalAppSettings.DbColumns.DB_Item.Name, itemName},
                    {GlobalAppSettings.DbColumns.DB_Item.Description, itemDetails.Description},
                    {GlobalAppSettings.DbColumns.DB_Item.CreatedById, copiedByUserId},
                    {GlobalAppSettings.DbColumns.DB_Item.ModifiedById, copiedByUserId},
                    {GlobalAppSettings.DbColumns.DB_Item.CreatedDate, DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())},
                    {
                        GlobalAppSettings.DbColumns.DB_Item.ModifiedDate,
                        DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    },
                    {GlobalAppSettings.DbColumns.DB_Item.CloneItemId, itemId},
                    {GlobalAppSettings.DbColumns.DB_Item.ParentId, destinationCategoryId},
                    {GlobalAppSettings.DbColumns.DB_Item.IsActive, true},
                    {GlobalAppSettings.DbColumns.DB_Item.Extension, itemDetails.Extension},
                    {GlobalAppSettings.DbColumns.DB_Item.ItemTypeId, (int)itemDetails.ItemType}
                };
                try
                {
                    if (!IsItemNameAlreadyExists(itemName, destinationCategoryId, itemDetails.ItemType))
                    {
                        var itemVersion =
                            FindItemCurrentVersion(itemId)
                                .AsEnumerable()
                                .Select(a => a.Field<int>(GlobalAppSettings.DbColumns.DB_ItemVersion.VersionNumber))
                                .FirstOrDefault();
                        var filename = itemDetails.Name + itemDetails.Extension;
                        var fileLocation =
                            Path.Combine(GlobalAppSettings.GetItemsPath() + itemId + "\\" + itemVersion + "\\" +
                                         filename);
                        var extention = Path.GetExtension(fileLocation);
                        var modifiedDate = DateTime.UtcNow;
                        var guid = Guid.NewGuid();
                        var result =
                            _dataProvider.ExecuteScalarQuery(
                                _queryBuilder.AddToTableWithGUID(GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                                    values, GlobalAppSettings.DbColumns.DB_Item.Id, guid), guid);
                        var publishedReportId = new Guid(result.ReturnValue.ToString());
                        actionResult.ReturnValue = publishedReportId;
                        SaveItemVersion(publishedReportId, copiedByUserId, 0, 1, itemName + extention, String.Empty,
                            itemDetails.ItemType, modifiedDate);

                        AddItemLog(itemVersion, ItemLogType.Cloned, copiedByUserId, publishedReportId, 0, itemDetails.CategoryId,
                            destinationCategoryId, modifiedDate);
                        LogExtension.LogInfo("Report Copied successfully", MethodBase.GetCurrentMethod(),
                            " ItemId - " + itemId + " DestinationCategoryId - " + destinationCategoryId +
                            " CopiedByUserId - " + copiedByUserId + " ItemName - " + itemName + " ItemDescription - " +
                            itemDetails.Description + " CreatedById - " + itemDetails.CreatedById + " ModifiedById - " +
                            copiedByUserId + " CreatedDate - " + itemDetails.CreatedDate + " ItemExtension - " +
                            itemDetails.Extension + " ItemTypeId - " + (int) itemDetails.ItemType + " CloneItemId - " +
                            itemId + " ParentId - " + destinationCategoryId);
                        actionResult = result;
                    }
                    else
                    {
                        LogExtension.LogInfo("Report Name already exist", MethodBase.GetCurrentMethod(),
                            " ItemId - " + itemId + " DestinationCategoryId - " + destinationCategoryId +
                            " CopiedByUserId - " + copiedByUserId + " ItemName - " + itemName + " ItemDescription - " +
                            itemDetails.Description + " CreatedById - " + itemDetails.CreatedById + " ModifiedById - " +
                            copiedByUserId + " CreatedDate - " + itemDetails.CreatedDate + " ItemExtension - " +
                            itemDetails.Extension + " ItemTypeId - " + (int) itemDetails.ItemType + " CloneItemId - " +
                            itemId + " ParentId - " + destinationCategoryId);
                        actionResult.Status = false;
                        actionResult.ReturnValue = "Name";
                    }
                }
                catch (SqlException e)
                {
                    LogExtension.LogError("Error in copying the report", e, MethodBase.GetCurrentMethod(),
                        " ItemId - " + itemId + " DestinationCategoryId - " + destinationCategoryId +
                        " CopiedByUserId - " + copiedByUserId + " ItemName - " + itemName + " ItemDescription - " +
                        itemDetails.Description + " CreatedById - " + itemDetails.CreatedById + " ModifiedById - " +
                        copiedByUserId + " CreatedDate - " + itemDetails.CreatedDate + " ItemExtension - " +
                        itemDetails.Extension + " ItemTypeId - " + (int) itemDetails.ItemType + " CloneItemId - " +
                        itemId + " ParentId - " + destinationCategoryId);
                    actionResult.Status = false;
                    actionResult.ReturnValue = "Exception";
                }
            }
            return actionResult;
        }

        public Result MoveItem(Guid itemId, Guid destinationCategoryId, int userId, string itemName)
        {
            var actionResult = new Result();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Id,
                    Condition = Conditions.Equals,
                    Value = itemId
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            var updateColumns = new List<UpdateColumn>
            {
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ParentId,
                    Value = destinationCategoryId
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ModifiedDate,
                    Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ModifiedById,
                    Value = userId
                }
            };
            try
            {
                var itemVersion = FindItemCurrentVersion(itemId).AsEnumerable().Select(a => a.Field<int>(GlobalAppSettings.DbColumns.DB_ItemVersion.VersionNumber)).FirstOrDefault();
                var itemDetails = GetItemDetailsFromItemId(itemId);
                if (!IsItemNameAlreadyExists(itemName, destinationCategoryId, itemDetails.ItemType))
                {
                    var result =
                        _dataProvider.ExecuteNonQuery(
                            _queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                                updateColumns, whereColumns));
                    AddItemLog(itemVersion, ItemLogType.Moved, userId, itemId, 0, itemDetails.CategoryId,
                        destinationCategoryId);
                    LogExtension.LogInfo("Item moved successfully", MethodBase.GetCurrentMethod(), " ItemId - " + itemId + " DestinationCategoryId - " + destinationCategoryId + " UserId - " + userId);
                    actionResult = result;
                }
                else
                {
                    LogExtension.LogInfo("Report Name Already Exist", MethodBase.GetCurrentMethod(), " ItemId - " + itemId + " DestinationCategoryId - " + destinationCategoryId + " UserId - " + userId);
                    actionResult.Status = false;
                    actionResult.ReturnValue = "Name";

                }
            }
            catch (SqlException e)
            {
                LogExtension.LogError("Error in moving the item", e, MethodBase.GetCurrentMethod(), " ItemId - " + itemId + " DestinationCategoryId - " + destinationCategoryId + " UserId - " + userId);
                actionResult.Status = false;
                actionResult.ReturnValue = "Exception";
            }
            return actionResult;
        }

        public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            var dir = new DirectoryInfo(sourceDirName);
            var dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            if (copySubDirs)
            {
                foreach (var subdir in dirs)
                {
                    var temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, true);
                }
            }
        }

        public bool AddReportintrashDeletedTable(Guid itemId, int userId)
        {
            var result = new Result();
            var itemTrashid = GetReportFromTrashByReportId(itemId).Id;
            var values = new Dictionary<string, object>
            {
                {GlobalAppSettings.DbColumns.DB_ItemTrashDeleted.DeletedById, userId},
                {
                    GlobalAppSettings.DbColumns.DB_ItemTrashDeleted.DeletedDate,
                    DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                },
                {GlobalAppSettings.DbColumns.DB_ItemTrashDeleted.ItemId, itemId},
                {GlobalAppSettings.DbColumns.DB_ItemTrashDeleted.IsActive, true},
                {GlobalAppSettings.DbColumns.DB_ItemTrashDeleted.ItemTrashId, itemTrashid}
            };
            try
            {
                result = _dataProvider.ExecuteNonQuery(_queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_ItemTrashDeleted.DB_TableName, values));
            }
            catch (SqlException e)
            {
                LogExtension.LogError("Error in adding report in trash deleted table", e, MethodBase.GetCurrentMethod(), " ItemId - " + itemId + " UserId - " + userId + " ItemTrashId - " + itemTrashid);
                return false;
            }
            return result.Status;
        }

        public bool UpdateFileType(Guid itemId, ItemType itemType, int userId)
        {
            var result = new Result();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Id,
                    Condition = Conditions.Equals,
                    Value = itemId
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            var updateColumns = new List<UpdateColumn>
            {
                new UpdateColumn {ColumnName = GlobalAppSettings.DbColumns.DB_Item.ItemTypeId, Value = (int)itemType},
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ModifiedById,
                    Value = userId
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ModifiedDate,
                    Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                }
            };
            try
            {
                result = _dataProvider.ExecuteNonQuery(_queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, updateColumns, whereColumns));
            }
            catch (Exception e)
            {
                Logger.LogExtension.LogError("Can't update item type", e, MethodBase.GetCurrentMethod(), " ItemId - " + itemId + " ItemType - " + (int)itemType + " UserId - " + userId);
                return false;
            }
            return result.Status;
        }

        public List<string> GetChildItemofClone(List<string> fileIdList, Guid? cloneId)
        {
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.CloneItemId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = cloneId
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            var result = _dataProvider.ExecuteReaderQuery(_queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, whereColumns));
            if (result.DataTable.Rows.Count == 0)
            {
                return fileIdList;
            }
            var itemId = result.DataTable.AsEnumerable().Select(a => a.Field<Guid?>(GlobalAppSettings.DbColumns.DB_Item.Id)).ToList();

            if (String.IsNullOrEmpty(itemId.ToString()))
                return fileIdList;

            foreach (var id in itemId)
            {
                fileIdList.Add(id.ToString());
                GetChildItemofClone(fileIdList, Guid.Parse(id.ToString()));
            }

            return fileIdList;
        }

        public List<ItemDetail> GetAllItems(ItemType itemType)
        {
            const string joinAliasName = "User_modified";
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    Value = true
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ItemTypeId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = (int)itemType
                }
            };
            var selectColumns = new List<SelectedColumn>
            {
                new SelectedColumn {TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName, ColumnName = "*"},
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    AliasName="CreatedByFirstName",
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.FirstName
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    AliasName="CreatedByLastName",
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.LastName
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    AliasName="CreatedByDisplayName",
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.DisplayName
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    AliasName="IsCreatedByActive",
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.IsActive
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ParentId
                },
                new SelectedColumn {JoinAliasName = joinAliasName,ColumnName = GlobalAppSettings.DbColumns.DB_User.FirstName, AliasName = "ModifiedByFirstName"},
                new SelectedColumn {JoinAliasName = joinAliasName,ColumnName = GlobalAppSettings.DbColumns.DB_User.LastName, AliasName = "ModifiedByLastName"},
                new SelectedColumn {JoinAliasName = joinAliasName,ColumnName = GlobalAppSettings.DbColumns.DB_User.DisplayName, AliasName = "ModifiedByDisplayName"},
                new SelectedColumn {JoinAliasName = joinAliasName,ColumnName = GlobalAppSettings.DbColumns.DB_User.IsActive, AliasName = "IsModifiedByActive"}
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
                                TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_Item.CreatedById,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_User.Id,
                                ParentTable = GlobalAppSettings.DbColumns.DB_User.DB_TableName
                            }
                        },
                    JoinType = JoinTypes.Inner
                },
                new JoinSpecification
                {
                    Table = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    Column =
                        new List<JoinColumn>
                        {
                            new JoinColumn
                            {
                                TableName = joinAliasName,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_User.Id,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_Item.ModifiedById,
                                ParentTable = GlobalAppSettings.DbColumns.DB_Item.DB_TableName
                            }
                        },
                    JoinType = JoinTypes.Inner,
                    JoinTableAliasName = joinAliasName
                }
            };
            var result = _dataProvider.ExecuteReaderQuery(_queryBuilder.ApplyWhereClause(
                _queryBuilder.ApplyMultipleJoins(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, selectColumns,
                    joinSpecification), whereColumns));

            var itemDetails = result.DataTable.AsEnumerable()
                .Select(row =>
                    new ItemDetail
                    {
                        Id = row.Field<Guid>(GlobalAppSettings.DbColumns.DB_Item.Id),
                        Name = row.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Name),
                        ItemType = (ItemType)row.Field<int>(GlobalAppSettings.DbColumns.DB_Item.ItemTypeId),
                        CreatedById = row.Field<int>(GlobalAppSettings.DbColumns.DB_Item.CreatedById),
                        CreatedByDisplayName =
                            row.Field<string>("CreatedByDisplayName"),
                        CreatedDate =
                            row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.CreatedDate)
                                .ToString(),
                        ModifiedById = row.Field<int>(GlobalAppSettings.DbColumns.DB_Item.ModifiedById),
                        CategoryId = row.Field<Guid?>(GlobalAppSettings.DbColumns.DB_Item.ParentId),
                        ModifiedByFullName =
                            row.Field<string>("ModifiedByFirstName") + " " + row.Field<string>("ModifiedByLastName"),
                        ModifiedDate =
                            row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.ModifiedDate)
                                .ToString(),
                        IsCreatedByActive = row.Field<bool>("IsCreatedByActive"),
                        IsModifiedByActive = row.Field<bool>("IsModifiedByActive")
                    }).OrderBy(s => s.Name).ToList();
            return itemDetails;
        }

        public Dictionary<ItemType, bool> GetItemTypesWithCreateAccess(int userId)
        {
            var itemTypes = new Dictionary<ItemType, bool>();
            var permissions = new List<int>();
            var groupIds = _userManagement.GetAllGroupsOfUser(userId).Select(r => r.Id.ToString()).Distinct().ToList();
            var whereUserColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.UserId,
                    Condition = Conditions.Equals,
                    Value = userId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator=LogicalOperators.AND,
                    Value = true
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId,
                    Condition = Conditions.Equals,
                    LogicalOperator=LogicalOperators.AND,
                    Value = (int) PermissionAccess.Create
                }
            };
            var query = _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName, whereUserColumns);
            if (groupIds.Any())
            {
                var whereGroupColumns = new List<ConditionColumn>{
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId,
                    Condition = Conditions.IN,
                    Values = groupIds
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator=LogicalOperators.AND,
                    Value = true
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionAccessId,
                    Condition = Conditions.Equals,
                    LogicalOperator=LogicalOperators.AND,
                    Value = (int) PermissionAccess.Create
                }
            };
                query += " UNION " + _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName, whereGroupColumns);
            }
            var result = _dataProvider.ExecuteReaderQuery(query);
            if (result.Status)
            {
                permissions = result.DataTable.AsEnumerable().Select(r => r.Field<int>(GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId)).Distinct().ToList();
            }
            itemTypes.Add(ItemType.Category, (permissions.Contains((int)PermissionEntity.AllCategories)));
            itemTypes.Add(ItemType.Dashboard, (permissions.Contains((int)PermissionEntity.AllDashboards) || permissions.Contains((int)PermissionEntity.DashboardsInCategory)));
            itemTypes.Add(ItemType.Datasource, (permissions.Contains((int)PermissionEntity.AllDataSources)));
            itemTypes.Add(ItemType.Report, (permissions.Contains((int)PermissionEntity.AllReports) || permissions.Contains((int)PermissionEntity.ReportsInCategory)));
            itemTypes.Add(ItemType.File, (permissions.Contains((int)PermissionEntity.AllFiles)));

            return itemTypes;
        }

        public Guid SaveCategory(ItemDetail category)
        {
            var guid = Guid.NewGuid();
            var result = new Result();
            try
            {
                var elements = new Dictionary<string, object>
            {
                {GlobalAppSettings.DbColumns.DB_Item.ItemTypeId,Convert.ToInt32(category.ItemType)},
                {GlobalAppSettings.DbColumns.DB_Item.Name,category.Name},
                {GlobalAppSettings.DbColumns.DB_Item.Description,category.Description},
                {GlobalAppSettings.DbColumns.DB_Item.CreatedById,category.CreatedById},
                {GlobalAppSettings.DbColumns.DB_Item.CreatedDate, category.CreatedDate},
                {GlobalAppSettings.DbColumns.DB_Item.ModifiedById,category.ModifiedById},
                {GlobalAppSettings.DbColumns.DB_Item.ModifiedDate,category.ModifiedDate},
                {GlobalAppSettings.DbColumns.DB_Item.IsActive,true}
            };
                result =
                    _dataProvider.ExecuteScalarQuery(
                        _queryBuilder.AddToTableWithGUID(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, elements,
                            GlobalAppSettings.DbColumns.DB_Item.Id, guid), GlobalAppSettings.ConnectionString, guid);
            }
            catch (Exception e)
            {
                Logger.LogExtension.LogError("Error in add category ", e, MethodBase.GetCurrentMethod(), " ItemTypeId - " + Convert.ToInt32(category.ItemType) + " Name - " + category.Name + " Description - " + category.Description + " CreatedById - " + category.CreatedById + " CreatedDate - " + category.CreatedDate + " ModifiedById - " + category.ModifiedById + " ModifiedDate - " + category.ModifiedDate + " Guid - " + guid);
                return Guid.Empty;
            }

            return new Guid(result.ReturnValue.ToString());
        }

        public bool ItemDeleteValidation(Guid itemId, ItemType itemType)
        {
            var validation = true;
            var scheduleList = new List<string>();
            switch (itemType)
            {
                case ItemType.Category:
                    validation = !IsCategoryContainsChild(itemId);
                    break;

                case ItemType.Report:
                    validation = !CheckIsScheduledReport(itemId, out scheduleList);
                    break;

                case ItemType.Dashboard:
                    validation = !CheckIsScheduledReport(itemId, out scheduleList);
                    break;

                case ItemType.Datasource:
                    validation = !IsDataSourceInUse(itemId);
                    break;
            }
            return validation;
        }

        public bool IsCategoryContainsChild(Guid itemId)
        {
            var result = new Result();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ParentId,
                    Condition = Conditions.Equals,
                    Value = itemId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            try
            {
                result =
                    _dataProvider.ExecuteReaderQuery(_queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, whereColumns));
            }
            catch (SqlException e)
            {
                LogExtension.LogError("Error in checking category delete validation", e, MethodBase.GetCurrentMethod(), " ItemId - " + itemId);
                return false;
            }

            return result.DataTable.Rows.Count > 0;
        }

        public bool IsDataSourceInUse(Guid itemId)
        {
            var result = new Result();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ReportDataSource.DataSourceItemId,
                    Condition = Conditions.Equals,
                    Value = itemId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ReportDataSource.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            try
            {
                result =
                    _dataProvider.ExecuteReaderQuery(_queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_ReportDataSource.DB_TableName, whereColumns));
            }
            catch (SqlException e)
            {
                LogExtension.LogError("Error in checking delete data source validation", e, MethodBase.GetCurrentMethod(), " ItemId - " + itemId);
                return false;
            }

            return result.DataTable.Rows.Count > 0;
        }

        public bool IsCategoryNameExistAlready(string categoryname, int itemtypeId)
        {
            var result = new Result();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name,
                    Condition = Conditions.Equals,

                    Value = categoryname
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ItemTypeId,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = itemtypeId
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            try
            {
                result =
                    _dataProvider.ExecuteReaderQuery(_queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, whereColumns));
            }
            catch (SqlException e)
            {
                LogExtension.LogError("Error in checking category name", e, MethodBase.GetCurrentMethod(), " CategoryName - " + categoryname + " ItemTypeId - " + itemtypeId);
                return false;
            }

            return result.DataTable.Rows.Count > 0;
        }

        public List<ItemVersion> FindItemPrevVersion(Guid itemId, int userId, out int countValue, string orgItemId = null, int skip = 0, int take = 0)
        {
            var exItemId = Guid.Empty;
            if (orgItemId == null)
                exItemId = itemId;
            else
                exItemId = Guid.Parse(orgItemId);

            var cloneItemId = Guid.Parse(GetCloneOfItem(itemId).ToString());
            if (String.IsNullOrEmpty(cloneItemId.ToString()))
                cloneItemId = itemId;

            var _itemVersion = new List<ItemVersion>();
            var selectedColumns = new List<SelectedColumn> {
                    new SelectedColumn
                    {
                     TableName=GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName,
                     ColumnName="*"
                    },
                    new SelectedColumn
                    {
                     TableName=GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName,
                     ColumnName=GlobalAppSettings.DbColumns.DB_ItemVersion.IsActive
                    },
                    new SelectedColumn
                    {
                     TableName=GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                     ColumnName=GlobalAppSettings.DbColumns.DB_User.FirstName
                    },
                    new SelectedColumn
                    {
                     TableName=GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                     ColumnName=GlobalAppSettings.DbColumns.DB_User.LastName
                    },
                    new SelectedColumn
                    {
                     TableName=GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                     ColumnName=GlobalAppSettings.DbColumns.DB_User.DisplayName
                    }
            };
            var joinSpecification = new JoinSpecification
            {
                Table = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                Column =
                    new List<JoinColumn>
                    {
                        new JoinColumn
                        {
                            TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            JoinedColumn = GlobalAppSettings.DbColumns.DB_User.Id,
                            Operation = Conditions.Equals,
                            ParentTableColumn = GlobalAppSettings.DbColumns.DB_ItemVersion.CreatedById
                        }
                    },
                JoinType = JoinTypes.Inner
            };
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemVersion.ItemId,
                    Condition = Conditions.Equals,
                    Value = cloneItemId
                },
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ItemVersion.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                }
            };
            var itemDetails = GetItemDetailsFromItemId(cloneItemId, false);
            var itemsList = GetItems(userId, itemDetails.ItemType, null, null, "", 0, 10, exItemId);
            var CanWrite = false;
            if (itemsList.result.Any(a => a.Id == exItemId && a.CanWrite) == true)
                CanWrite = true;

            var currentTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone());

            var versions =
                _dataProvider.ExecuteReaderQuery(
                    _queryBuilder.ApplyWhereClause(
                        _queryBuilder.ApplyJoins(GlobalAppSettings.DbColumns.DB_ItemVersion.DB_TableName,
                            selectedColumns, joinSpecification), whereColumns)).DataTable.AsEnumerable().
                    Select(row => new ItemVersion
                    {
                        ItemId = itemId,
                        ItemName = row.Field<string>(GlobalAppSettings.DbColumns.DB_ItemVersion.ItemName),
                        ItemTypeId = (ItemType)row.Field<Int32>(GlobalAppSettings.DbColumns.DB_ItemVersion.ItemTypeId),
                        VersionNumber = row.Field<Int32>(GlobalAppSettings.DbColumns.DB_ItemVersion.VersionNumber),
                        CreatedByName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.DisplayName),
                        IsCurrentVersion = row.Field<bool>(GlobalAppSettings.DbColumns.DB_ItemVersion.IsCurrentVersion),
                        CreatedById = row.Field<Int32>(GlobalAppSettings.DbColumns.DB_ItemVersion.CreatedById),
                        IsActive = row.Field<bool>(GlobalAppSettings.DbColumns.DB_ItemVersion.IsActive),
                        Comment = row.Field<string>(GlobalAppSettings.DbColumns.DB_ItemVersion.Comment),
                        CreatedDate =
                            TimeZoneInfo.ConvertTimeFromUtc(
                                row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_ItemVersion.CreatedDate),
                                currentTimeZoneInfo),
                        CanWrite = CanWrite
                    }).OrderByDescending(a => a.VersionNumber);

            countValue = versions.ToList().Count();
            if (take != 0)
            {
                _itemVersion = versions.Skip(skip).Take(take).ToList();
            }
            else
            {
                _itemVersion = versions.ToList();
            }
            _itemVersion.ForEach(f => f.CreatedDateString =
                f.CreatedDate.ToString(GlobalAppSettings.SystemSettings.DateFormat + " hh:mm tt"));

            return _itemVersion;
        }

        public List<ItemDetail> GetAllItems(int userId, List<int> groupIds, ItemType? itemType = null, List<Permission> parentIdPermissions = null, List<SortCollection> sortCollection = null, List<FilterCollection> filterSettings = null, string search = "", List<string> searchDescriptor = null, Guid? itemId = null)
        {
            var parentIdString = string.Empty;
            var query =
                "SELECT UnionResult.PermissionAccessId,UnionResult.PermissionEntityId,UnionResult.IsSpecificItemPermission,[" +
                GlobalAppSettings.DbColumns.DB_Item.DB_TableName +
                "].*,ParentItemTable.Name AS [CategoryName],ParentItemTable.Id AS [CategoryId],ParentItemTable.Description AS [CategoryDescription], [User].FirstName AS [CreatedByFirstName],[User].LastName AS [CreatedByLastName],[User].DisplayName AS [CreatedByDisplayName],[User].IsActive AS [IsCreatedByActive],User_modified.FirstName AS [ModifiedByFirstName],User_modified.LastName AS [ModifiedByLastName],User_modified.DisplayName AS [ModifiedByDisplayName],User_modified.IsActive AS [IsModifiedByActive] FROM (SELECT [" +
                GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" + GlobalAppSettings.DbColumns.DB_Item.Id +
                "],[" + GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId + "],[" +
                GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId +
                "],'True' as IsSpecificItemPermission FROM [" +
                GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "]" +
                " LEFT JOIN [" + GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "] ON [" +
                GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                GlobalAppSettings.DbColumns.DB_UserPermission.ItemId + "]=[" +
                GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" + GlobalAppSettings.DbColumns.DB_Item.Id + "]" +
                " WHERE [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                GlobalAppSettings.DbColumns.DB_Item.IsActive + "]='True' ";
            if (itemType != null)
            {
                query += " AND [" +
                 GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                 GlobalAppSettings.DbColumns.DB_Item.ItemTypeId + "]=" + (int)itemType;
                if (itemType == ItemType.Category)
                {
                    query += " AND [" +
                 GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                 GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId + "] NOT IN (" + (int)PermissionEntity.ReportsInCategory + "," + (int)PermissionEntity.DashboardsInCategory + ")";
                }
            }
            else
            {
                query += " AND [" +
                 GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                 GlobalAppSettings.DbColumns.DB_Item.ItemTypeId + "]!=" + (int)ItemType.Category;
            }
            if (itemId != null)
            {
                query += " AND [" +
                 GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                 GlobalAppSettings.DbColumns.DB_Item.Id + "]=" + "'" + (Guid)itemId + "'";
            }
            query += " AND [" +
                 GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                 GlobalAppSettings.DbColumns.DB_UserPermission.UserId + "]=" + userId +
                 " AND [" + GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                 GlobalAppSettings.DbColumns.DB_UserPermission.IsActive + "]='True'";
            if (parentIdPermissions != null && parentIdPermissions.Count != 0)
            {
                for (var permission = 0; permission < parentIdPermissions.Count; permission++)
                {
                    query += " UNION" +
                    " SELECT [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                    GlobalAppSettings.DbColumns.DB_Item.Id + "],[" +
                    GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                    GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId + "],[" +
                    GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                    GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId +
                    "],'False' as IsSpecificItemPermission FROM [" +
                    GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "]" +
                    " LEFT JOIN [" + GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "] ON [" +
                    GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                    GlobalAppSettings.DbColumns.DB_UserPermission.ItemId + "]=[" +
                    GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" + GlobalAppSettings.DbColumns.DB_Item.ParentId +
                    "]" +
                    " WHERE [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                    GlobalAppSettings.DbColumns.DB_Item.IsActive + "]='True' AND [" +
                    GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                    GlobalAppSettings.DbColumns.DB_Item.ItemTypeId + "]=" + (int)parentIdPermissions[permission].ItemType +
                    " AND [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                                 GlobalAppSettings.DbColumns.DB_Item.ParentId +
                                 "]='" + parentIdPermissions[permission].ItemId +
                                 "' AND [" + GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                             GlobalAppSettings.DbColumns.DB_UserPermission.UserId + "]=" + userId +
                             " AND [" + GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                             GlobalAppSettings.DbColumns.DB_UserPermission.IsActive + "]='True'";
                }
            }
            if (groupIds.Count > 0)
            {
                var groupString = string.Empty;
                for (var i = 0; i < groupIds.Count; i++)
                {
                    if (i != 0)
                    {
                        groupString += ",";
                    }
                    groupString += groupIds[i];
                }
                query += " UNION SELECT [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                         GlobalAppSettings.DbColumns.DB_Item.Id + "],[" +
                         GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                         GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionAccessId + "],[" +
                         GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                         GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId +
                         "],'True' as IsSpecificItemPermission FROM [" +
                         GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "]" +
                         " LEFT JOIN [" + GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "] ON [" +
                         GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                         GlobalAppSettings.DbColumns.DB_GroupPermission.ItemId + "]=[" +
                         GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                         GlobalAppSettings.DbColumns.DB_Item.Id + "]" +
                         " WHERE [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                         GlobalAppSettings.DbColumns.DB_Item.IsActive + "]='True'";
                if (itemType != null)
                {
                    query += " AND [" +
                         GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                         GlobalAppSettings.DbColumns.DB_Item.ItemTypeId + "]=" + (int)itemType;
                    if (itemType == ItemType.Category)
                    {
                        query += " AND [" +
                     GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                     GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId + "] NOT IN (" + (int)PermissionEntity.ReportsInCategory + "," + (int)PermissionEntity.DashboardsInCategory + ")";
                    }
                }
                else
                {
                    query += " AND [" +
                     GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                     GlobalAppSettings.DbColumns.DB_Item.ItemTypeId + "]!=" + (int)ItemType.Category;
                }
                query += " AND [" +
                         GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                         GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId + "] IN (" + groupString + ")" +
                         " AND [" + GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                         GlobalAppSettings.DbColumns.DB_GroupPermission.IsActive + "]='True'";
                if (parentIdPermissions != null && parentIdPermissions.Count != 0)
                {
                    for (var permission = 0; permission < parentIdPermissions.Count; permission++)
                    {
                        query += " UNION" +
                        " SELECT [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                        GlobalAppSettings.DbColumns.DB_Item.Id + "],[" +
                        GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                        GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionAccessId + "],[" +
                        GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                        GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId +
                        "],'False' as IsSpecificItemPermission FROM [" +
                        GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "]" +
                        " LEFT JOIN [" + GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "] ON [" +
                        GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                        GlobalAppSettings.DbColumns.DB_GroupPermission.ItemId + "]=[" +
                        GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" + GlobalAppSettings.DbColumns.DB_Item.ParentId +
                        "]" +
                        " WHERE [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                        GlobalAppSettings.DbColumns.DB_Item.IsActive + "]='True' AND [" +
                        GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                        GlobalAppSettings.DbColumns.DB_Item.ItemTypeId + "]=" + (int)parentIdPermissions[permission].ItemType +
                        " AND [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                                     GlobalAppSettings.DbColumns.DB_Item.ParentId +
                                     "]='" + parentIdPermissions[permission].ItemId +
                                     "' AND [" + GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                                 GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId + "] IN (" + groupString +
                                 ") AND [" + GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                                 GlobalAppSettings.DbColumns.DB_GroupPermission.IsActive + "]='True'";
                    }
                }
            }
            query += ") as UnionResult " +
                     " INNER JOIN [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "] ON [" +
                     GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" + GlobalAppSettings.DbColumns.DB_Item.Id +
                     "]=UnionResult.Id" +
                     " INNER JOIN [" + GlobalAppSettings.DbColumns.DB_User.DB_TableName + "] ON [" +
                     GlobalAppSettings.DbColumns.DB_User.DB_TableName + "].[" + GlobalAppSettings.DbColumns.DB_User.Id +
                     "]=[" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                     GlobalAppSettings.DbColumns.DB_Item.CreatedById + "]" +
                     " INNER JOIN [" + GlobalAppSettings.DbColumns.DB_User.DB_TableName + "]  AS User_modified ON [" +
                     GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                     GlobalAppSettings.DbColumns.DB_Item.ModifiedById + "]=User_modified.[" +
                     GlobalAppSettings.DbColumns.DB_User.Id + "]" +
                     " LEFT JOIN [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "] AS ParentItemTable ON [" +
                     GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" + GlobalAppSettings.DbColumns.DB_Item.ParentId +
                     "]=ParentItemTable.[Id]";

            var result = _dataProvider.ExecuteReaderQuery(FilteringHelper(sortCollection, filterSettings, query, search, searchDescriptor));
            var canCreateSchedule = (itemType == ItemType.Report || itemType == ItemType.Dashboard) && (GetUserItemsForSpecificPermission(userId, PermissionEntity.AllSchedules, PermissionAccess.Create).DataTable.AsEnumerable().ToList().Count > 0 ? true : false);
            var itemsList = result.DataTable.AsEnumerable().Select(row => new
            {
                Id = row.Field<Guid>(GlobalAppSettings.DbColumns.DB_Item.Id),
                Name = row.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Name),
                CategoryName = row.Field<string>("CategoryName"),
                CategoryDescription = row.Field<string>("CategoryDescription"),
                CategoryId = row.Field<Guid?>("CategoryId"),
                Description = row.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Description),
                ItemType = (ItemType)row.Field<int>(GlobalAppSettings.DbColumns.DB_Item.ItemTypeId),
                CreatedById = row.Field<int>(GlobalAppSettings.DbColumns.DB_Item.CreatedById),
                CreatedByDisplayName =
                    row.Field<string>("CreatedByDisplayName"),
                CreatedDate =
                    row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.CreatedDate)
                        .ToString(),
                ModifiedById = row.Field<int>(GlobalAppSettings.DbColumns.DB_Item.ModifiedById),
                ModifiedByFullName =
                    row.Field<string>("ModifiedByFirstName") + " " + row.Field<string>("ModifiedByLastName"),
                ModifiedDate =
                    row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.ModifiedDate)
                        .ToString(),
                IsCreatedByActive = row.Field<bool>("IsCreatedByActive"),
                IsModifiedByActive = row.Field<bool>("IsModifiedByActive"),
                PermissionEntityId =
                    row.Field<object>("PermissionEntityId") == null
                        ? 0
                        : row.Field<int>("PermissionEntityId"),
                PermissionAccessId =
                    row.Field<object>("PermissionAccessId") == null
                        ? 0
                        : row.Field<int>("PermissionAccessId"),
                IsSpecicPermission = row.Field<string>("IsSpecificItemPermission") == "True",
            }).ToList();

            var groupedItems =
                itemsList.GroupBy(x => x.Id).Select(grp => new { ItemId = grp.Key, Details = grp.ToList() }).ToList();

            var items = groupedItems.Select(r => new ItemDetail
            {
                Id = r.ItemId,
                Name = r.Details[0].Name,
                Description = r.Details[0].Description,
                ItemType = r.Details[0].ItemType,
                CreatedById = r.Details[0].CreatedById,
                CreatedByDisplayName = r.Details[0].CreatedByDisplayName,
                CreatedDate = r.Details[0].CreatedDate,
                CategoryName = r.Details[0].CategoryName,
                CategoryDescription = r.Details[0].CategoryDescription,
                CategoryId = r.Details[0].CategoryId,
                ModifiedById = r.Details[0].ModifiedById,
                ModifiedByFullName = r.Details[0].ModifiedByFullName,
                ModifiedDate = r.Details[0].ModifiedDate,
                IsCreatedByActive = r.Details[0].IsCreatedByActive,
                IsModifiedByActive = r.Details[0].IsModifiedByActive,
                CanMove = false,
                CanCopy = false,
                CanClone = false,
                CanRead =
                    r.Details.Any(
                        s =>
                            ((s.Id == r.ItemId) &&
                             ((((s.ItemType == ItemType.Report)
                                 ? (s.PermissionEntityId == (int)PermissionEntity.ReportsInCategory)
                                 : (s.ItemType == ItemType.Dashboard)
                                         ? (s.PermissionEntityId == (int)PermissionEntity.DashboardsInCategory)
                                         : (s.PermissionEntityId == 0)) &&
                              (s.PermissionAccessId == (int)PermissionAccess.Read ||
                               s.PermissionAccessId == (int)PermissionAccess.ReadWrite ||
                               s.PermissionAccessId == (int)PermissionAccess.ReadWriteDelete)) ||
                              (s.IsSpecicPermission && (s.PermissionAccessId == (int)PermissionAccess.Read ||
                                                        s.PermissionAccessId == (int)PermissionAccess.ReadWrite ||
                                                        s.PermissionAccessId == (int)PermissionAccess.ReadWriteDelete))))),
                CanWrite = r.Details.Any(
                    s =>
                        ((s.Id == r.ItemId) &&
                         ((((s.ItemType == ItemType.Report)
                                 ? (s.PermissionEntityId == (int)PermissionEntity.ReportsInCategory)
                                 : (s.ItemType == ItemType.Dashboard)
                                         ? (s.PermissionEntityId == (int)PermissionEntity.DashboardsInCategory)
                                         : (s.PermissionEntityId == 0)) &&
                          (s.PermissionAccessId == (int)PermissionAccess.ReadWrite ||
                           s.PermissionAccessId == (int)PermissionAccess.ReadWriteDelete)) ||
                          (s.IsSpecicPermission && (s.PermissionAccessId == (int)PermissionAccess.ReadWrite ||
                                                    s.PermissionAccessId == (int)PermissionAccess.ReadWriteDelete))))),
                CanDelete =
                    r.Details.Any(
                        s =>
                            ((s.Id == r.ItemId) &&
                             ((((s.ItemType == ItemType.Report)
                                 ? (s.PermissionEntityId == (int)PermissionEntity.ReportsInCategory)
                                 : (s.ItemType == ItemType.Dashboard)
                                         ? (s.PermissionEntityId == (int)PermissionEntity.DashboardsInCategory)
                                         : (s.PermissionEntityId == 0)) &&
                              (s.PermissionAccessId == (int)PermissionAccess.ReadWriteDelete)) ||
                              (s.IsSpecicPermission && (s.PermissionAccessId == (int)PermissionAccess.ReadWriteDelete))))),
                CanOpen = true,
                CanSchedule = canCreateSchedule
            }).ToList();
            items = items.Distinct(new ItemComparer()).ToList();
            return items;
        }

        public List<ItemDetail> ItemsView(int userId, List<int> groupIds, int globalPermissionAccessId, int globalPermissionEntityId, ItemType? itemType = null, List<Permission> parentIdPermissions = null, List<SortCollection> sortCollection = null, List<FilterCollection> filterSettings = null, string search = "", List<string> searchDescriptor = null, Guid? itemId = null)
        {
            var parentIdString = string.Empty;
            var query =
                "SELECT UnionResult.PermissionAccessId,UnionResult.PermissionEntityId,UnionResult.IsSpecificItemPermission,[" +
                GlobalAppSettings.DbColumns.DB_Item.DB_TableName +
                "].*,ParentItemTable.Name AS [CategoryName],ParentItemTable.Id AS [CategoryId],ParentItemTable.Description AS [CategoryDescription], [User].FirstName AS [CreatedByFirstName],[User].LastName AS [CreatedByLastName],[User].DisplayName AS [CreatedByDisplayName],[User].IsActive AS [IsCreatedByActive],User_modified.FirstName AS [ModifiedByFirstName],User_modified.LastName AS [ModifiedByLastName],User_modified.DisplayName AS [ModifiedByDisplayName],User_modified.IsActive AS [IsModifiedByActive] FROM (SELECT [" +
                GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" + GlobalAppSettings.DbColumns.DB_Item.Id +
                "],[" + GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId + "],[" +
                GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId +
                "],'True' as IsSpecificItemPermission FROM [" +
                GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "]" +
                " LEFT JOIN [" + GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "] ON [" +
                GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                GlobalAppSettings.DbColumns.DB_UserPermission.ItemId + "]=[" +
                GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" + GlobalAppSettings.DbColumns.DB_Item.Id + "]" +
                " WHERE [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                GlobalAppSettings.DbColumns.DB_Item.IsActive + "]='True' ";
            if (itemType != null)
            {
                query += " AND [" +
                 GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                 GlobalAppSettings.DbColumns.DB_Item.ItemTypeId + "]=" + (int)itemType;
                if (itemType == ItemType.Category)
                {
                    query += " AND [" +
                 GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                 GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId + "]!=" + (int)PermissionEntity.ReportsInCategory;
                }
            }
            else
            {
                query += " AND [" +
                 GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                 GlobalAppSettings.DbColumns.DB_Item.ItemTypeId + "]!=" + (int)ItemType.Category;
            }
            if (itemId != null)
            {
                query += " AND [" +
                 GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                 GlobalAppSettings.DbColumns.DB_Item.Id + "]=" + "'" + (Guid)itemId + "'";
            }
            query += " AND [" +
                 GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                 GlobalAppSettings.DbColumns.DB_UserPermission.UserId + "]=" + userId +
                 " AND [" + GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                 GlobalAppSettings.DbColumns.DB_UserPermission.IsActive + "]='True'";


            query += " UNION SELECT [" +
                GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" + GlobalAppSettings.DbColumns.DB_Item.Id +
                "]," + globalPermissionAccessId + " as " +
                GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId + "," + globalPermissionEntityId + " as " +
                GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId +
                ",'True' as IsSpecificItemPermission FROM [" +
                GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "]" +
                " WHERE [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                GlobalAppSettings.DbColumns.DB_Item.IsActive + "]='True' ";
            if (itemType != null)
            {
                query += " AND [" +
                 GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                 GlobalAppSettings.DbColumns.DB_Item.ItemTypeId + "]=" + (int)itemType;
            }
            else
            {
                query += " AND [" +
                 GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                 GlobalAppSettings.DbColumns.DB_Item.ItemTypeId + "]!=" + (int)ItemType.Category;
            }
            if (itemId != null)
            {
                query += " AND [" +
                 GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                 GlobalAppSettings.DbColumns.DB_Item.Id + "]=" + "'" + (Guid)itemId + "'";
            }

            if (parentIdPermissions != null && parentIdPermissions.Count != 0)
            {
                for (var permission = 0; permission < parentIdPermissions.Count; permission++)
                {
                    query += " UNION" +
                    " SELECT [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                    GlobalAppSettings.DbColumns.DB_Item.Id + "],[" +
                    GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                    GlobalAppSettings.DbColumns.DB_UserPermission.PermissionAccessId + "],[" +
                    GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                    GlobalAppSettings.DbColumns.DB_UserPermission.PermissionEntityId +
                    "],'False' as IsSpecificItemPermission FROM [" +
                    GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "]" +
                    " LEFT JOIN [" + GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "] ON [" +
                    GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                    GlobalAppSettings.DbColumns.DB_UserPermission.ItemId + "]=[" +
                    GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" + GlobalAppSettings.DbColumns.DB_Item.ParentId +
                    "]" +
                    " WHERE [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                    GlobalAppSettings.DbColumns.DB_Item.IsActive + "]='True' AND [" +
                    GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                    GlobalAppSettings.DbColumns.DB_Item.ItemTypeId + "]=" + (int)parentIdPermissions[permission].ItemType +
                    " AND [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                                 GlobalAppSettings.DbColumns.DB_Item.ParentId +
                                 "]='" + parentIdPermissions[permission].ItemId +
                                 "' AND [" + GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                             GlobalAppSettings.DbColumns.DB_UserPermission.UserId + "]=" + userId +
                             " AND [" + GlobalAppSettings.DbColumns.DB_UserPermission.DB_TableName + "].[" +
                             GlobalAppSettings.DbColumns.DB_UserPermission.IsActive + "]='True'";
                }
            }
            if (groupIds.Count > 0)
            {
                var groupString = string.Empty;
                for (var i = 0; i < groupIds.Count; i++)
                {
                    if (i != 0)
                    {
                        groupString += ",";
                    }
                    groupString += groupIds[i];
                }
                query += " UNION SELECT [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                         GlobalAppSettings.DbColumns.DB_Item.Id + "],[" +
                         GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                         GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionAccessId + "],[" +
                         GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                         GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId +
                         "],'True' as IsSpecificItemPermission FROM [" +
                         GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "]" +
                         " LEFT JOIN [" + GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "] ON [" +
                         GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                         GlobalAppSettings.DbColumns.DB_GroupPermission.ItemId + "]=[" +
                         GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                         GlobalAppSettings.DbColumns.DB_Item.Id + "]" +
                         " WHERE [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                         GlobalAppSettings.DbColumns.DB_Item.IsActive + "]='True'";
                if (itemType != null)
                {
                    query += " AND [" +
                         GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                         GlobalAppSettings.DbColumns.DB_Item.ItemTypeId + "]=" + (int)itemType;
                    if (itemType == ItemType.Category)
                    {
                        query += " AND [" +
                     GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                     GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId + "]!=" + (int)PermissionEntity.ReportsInCategory;
                    }
                }
                else
                {
                    query += " AND [" +
                     GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                     GlobalAppSettings.DbColumns.DB_Item.ItemTypeId + "]!=" + (int)ItemType.Category;
                }
                query += " AND [" +
                         GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                         GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId + "] IN (" + groupString + ")" +
                         " AND [" + GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                         GlobalAppSettings.DbColumns.DB_GroupPermission.IsActive + "]='True'";
                if (parentIdPermissions != null && parentIdPermissions.Count != 0)
                {
                    for (var permission = 0; permission < parentIdPermissions.Count; permission++)
                    {
                        query += " UNION" +
                        " SELECT [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                        GlobalAppSettings.DbColumns.DB_Item.Id + "],[" +
                        GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                        GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionAccessId + "],[" +
                        GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                        GlobalAppSettings.DbColumns.DB_GroupPermission.PermissionEntityId +
                        "],'False' as IsSpecificItemPermission FROM [" +
                        GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "]" +
                        " LEFT JOIN [" + GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "] ON [" +
                        GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                        GlobalAppSettings.DbColumns.DB_GroupPermission.ItemId + "]=[" +
                        GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" + GlobalAppSettings.DbColumns.DB_Item.ParentId +
                        "]" +
                        " WHERE [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                        GlobalAppSettings.DbColumns.DB_Item.IsActive + "]='True' AND [" +
                        GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                        GlobalAppSettings.DbColumns.DB_Item.ItemTypeId + "]=" + (int)parentIdPermissions[permission].ItemType +
                        " AND [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                                     GlobalAppSettings.DbColumns.DB_Item.ParentId +
                                     "]='" + parentIdPermissions[permission].ItemId +
                                     "' AND [" + GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                                 GlobalAppSettings.DbColumns.DB_GroupPermission.GroupId + "] IN (" + groupString +
                                 ") AND [" + GlobalAppSettings.DbColumns.DB_GroupPermission.DB_TableName + "].[" +
                                 GlobalAppSettings.DbColumns.DB_GroupPermission.IsActive + "]='True'";
                    }
                }
            }
            query += ") as UnionResult " +
                     " INNER JOIN [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "] ON [" +
                     GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" + GlobalAppSettings.DbColumns.DB_Item.Id +
                     "]=UnionResult.Id" +
                     " INNER JOIN [" + GlobalAppSettings.DbColumns.DB_User.DB_TableName + "] ON [" +
                     GlobalAppSettings.DbColumns.DB_User.DB_TableName + "].[" + GlobalAppSettings.DbColumns.DB_User.Id +
                     "]=[" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                     GlobalAppSettings.DbColumns.DB_Item.CreatedById + "]" +
                     " INNER JOIN [" + GlobalAppSettings.DbColumns.DB_User.DB_TableName + "]  AS User_modified ON [" +
                     GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" +
                     GlobalAppSettings.DbColumns.DB_Item.ModifiedById + "]=User_modified.[" +
                     GlobalAppSettings.DbColumns.DB_User.Id + "]" +
                     " LEFT JOIN [" + GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "] AS ParentItemTable ON [" +
                     GlobalAppSettings.DbColumns.DB_Item.DB_TableName + "].[" + GlobalAppSettings.DbColumns.DB_Item.ParentId +
                     "]=ParentItemTable.[Id]";

            var result = _dataProvider.ExecuteReaderQuery(FilteringHelper(sortCollection, filterSettings, query, search, searchDescriptor));
            var canCreateSchedule = (itemType == ItemType.Report || itemType == ItemType.Dashboard) && (GetUserItemsForSpecificPermission(userId, PermissionEntity.AllSchedules, PermissionAccess.Create).DataTable.AsEnumerable().ToList().Count > 0 ? true : false);
            var itemsList = result.DataTable.AsEnumerable().Select(row => new
            {
                Id = row.Field<Guid>(GlobalAppSettings.DbColumns.DB_Item.Id),
                Name = row.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Name),
                CategoryName = row.Field<string>("CategoryName"),
                CategoryDescription = row.Field<string>("CategoryDescription"),
                CategoryId = row.Field<Guid?>("CategoryId"),
                Description = row.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Description),
                ItemType = (ItemType)row.Field<int>(GlobalAppSettings.DbColumns.DB_Item.ItemTypeId),
                CreatedById = row.Field<int>(GlobalAppSettings.DbColumns.DB_Item.CreatedById),
                CreatedByDisplayName =
                    row.Field<string>("CreatedByDisplayName"),
                CreatedDate =
                    row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.CreatedDate)
                        .ToString(),
                ModifiedById = row.Field<int>(GlobalAppSettings.DbColumns.DB_Item.ModifiedById),
                ModifiedByFullName =
                    row.Field<string>("ModifiedByFirstName") + " " + row.Field<string>("ModifiedByLastName"),
                ModifiedDate =
                    row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_Item.ModifiedDate)
                        .ToString(),
                IsCreatedByActive = row.Field<bool>("IsCreatedByActive"),
                IsModifiedByActive = row.Field<bool>("IsModifiedByActive"),
                PermissionEntityId =
                    row.Field<object>("PermissionEntityId") == null
                        ? 0
                        : row.Field<int>("PermissionEntityId"),
                PermissionAccessId =
                    row.Field<object>("PermissionAccessId") == null
                        ? 0
                        : row.Field<int>("PermissionAccessId"),
                IsSpecicPermission = row.Field<string>("IsSpecificItemPermission") == "True",
            }).ToList();

            var groupedItems =
                itemsList.GroupBy(x => x.Id).Select(grp => new { ItemId = grp.Key, Details = grp.ToList() }).ToList();

            var items = groupedItems.Select(r => new ItemDetail
            {
                Id = r.ItemId,
                Name = r.Details[0].Name,
                Description = r.Details[0].Description,
                ItemType = r.Details[0].ItemType,
                CreatedById = r.Details[0].CreatedById,
                CreatedByDisplayName = r.Details[0].CreatedByDisplayName,
                CreatedDate = r.Details[0].CreatedDate,
                CategoryName = r.Details[0].CategoryName,
                CategoryDescription = r.Details[0].CategoryDescription,
                CategoryId = r.Details[0].CategoryId,
                ModifiedById = r.Details[0].ModifiedById,
                ModifiedByFullName = r.Details[0].ModifiedByFullName,
                ModifiedDate = r.Details[0].ModifiedDate,
                IsCreatedByActive = r.Details[0].IsCreatedByActive,
                IsModifiedByActive = r.Details[0].IsModifiedByActive,
                CanMove = false,
                CanCopy = false,
                CanClone = false,
                CanRead =
                    r.Details.Any(
                        s =>
                            ((s.Id == r.ItemId) &&
                             ((((s.ItemType == ItemType.Report)
                                 ? (s.PermissionEntityId == (int)PermissionEntity.ReportsInCategory)
                                 : (s.ItemType == ItemType.Dashboard)
                                         ? (s.PermissionEntityId == (int)PermissionEntity.DashboardsInCategory)
                                         : (s.PermissionEntityId == 0)) &&
                              (s.PermissionAccessId == (int)PermissionAccess.Read ||
                               s.PermissionAccessId == (int)PermissionAccess.ReadWrite ||
                               s.PermissionAccessId == (int)PermissionAccess.ReadWriteDelete)) ||
                              (s.IsSpecicPermission && (s.PermissionAccessId == (int)PermissionAccess.Read ||
                                                        s.PermissionAccessId == (int)PermissionAccess.ReadWrite ||
                                                        s.PermissionAccessId == (int)PermissionAccess.ReadWriteDelete))))),
                CanWrite = r.Details.Any(
                    s =>
                        ((s.Id == r.ItemId) &&
                         ((((s.ItemType == ItemType.Report)
                                 ? (s.PermissionEntityId == (int)PermissionEntity.ReportsInCategory)
                                 : (s.ItemType == ItemType.Dashboard)
                                         ? (s.PermissionEntityId == (int)PermissionEntity.DashboardsInCategory)
                                         : (s.PermissionEntityId == 0)) &&
                          (s.PermissionAccessId == (int)PermissionAccess.ReadWrite ||
                           s.PermissionAccessId == (int)PermissionAccess.ReadWriteDelete)) ||
                          (s.IsSpecicPermission && (s.PermissionAccessId == (int)PermissionAccess.ReadWrite ||
                                                    s.PermissionAccessId == (int)PermissionAccess.ReadWriteDelete))))),
                CanDelete =
                    r.Details.Any(
                        s =>
                            ((s.Id == r.ItemId) &&
                             ((((s.ItemType == ItemType.Report)
                                 ? (s.PermissionEntityId == (int)PermissionEntity.ReportsInCategory)
                                 : (s.ItemType == ItemType.Dashboard)
                                         ? (s.PermissionEntityId == (int)PermissionEntity.DashboardsInCategory)
                                         : (s.PermissionEntityId == 0)) &&
                              (s.PermissionAccessId == (int)PermissionAccess.ReadWriteDelete)) ||
                              (s.IsSpecicPermission && (s.PermissionAccessId == (int)PermissionAccess.ReadWriteDelete))))),
                CanOpen = true,
                CanSchedule = canCreateSchedule
            }).ToList();
            items = items.Distinct(new ItemComparer()).ToList();
            return items;
        }

        public string FilteringHelper(List<SortCollection> sortCollection, List<FilterCollection> filterCollection, string query, string searchKey, List<string> searchDescriptor)
        {
            var sortKey = "ModifiedDate";
            var sortValue = "desc";
            if (sortCollection != null && sortCollection.Any())
            {
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
                         ") ItemTable ";

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