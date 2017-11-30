using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Grout.Base.DataClasses;

namespace Grout.Base.Item
{
    public interface IItemManagement
    {
        DataResponse SaveItemVersion(Guid itemId, int createdById, int rollBackVersionNo, int prevVersions, string itemName, string comment, ItemType itemType,DateTime modifiedDateTime);
        bool UpdateNameInVersionTable(Guid itemId, int itemVersion, string itemName);
        bool UpdateItemCurrentVersion(Guid itemId);
        ItemVersion FindItemVersionByVerionId(Guid itemId, int version);
        string GetItemLocation(Guid itemId, ItemType fileType, string itemName = "", int versionId = 0);
        DataTable FindItemCurrentVersion(Guid itemId);
        int FindItemPrevVersionCount(Guid itemId);

        object AddItemLog(int versionId, ItemLogType logType, int currentUserId, Guid currentItemId, int parentId,
            Guid? fromCategory = null, Guid? toCategory = null, DateTime? modifiedDate = null);
        ItemDetail GetItemDetailsFromItemId(Guid itemId, bool isActive = true);
        List<ScheduleItem> GetSchedulesOfReport(Guid reportId);
        bool CheckIsScheduledReport(Guid reportId, out List<string> scheduleNameList);
        bool AddItemToTrash(ItemDetail reportObj, int currentUserId);
        ItemTrash GetReportFromTrashByReportId(Guid reportId);
        bool RemoveItemFromTrash(Guid reportId);
        List<ItemDetail> GetAllItemsOfUserFromItemTrash(int userId);
        bool IsItemNameAlreadyExistsForUpdate(string reportname, Guid? categoryId, Guid reportId);
        bool IsItemNameAlreadyExists(string reportname, Guid? categoryId);
        bool IsItemNameAlreadyExists(string reportname, ItemType itemType);
        DataTable GetItemLog(Guid itemId);
        bool UpdateItemStatus(string itemId, bool status);
        bool AddDataSourceWithRdl(string reportItemId, string dataSourceId, string dataSourceName);
        bool DisableDataSourceOfRdl(string reportItemId);
        bool UpdateDataSourceOfRdl(string dataSourceName, string dataSourceId, string reportId);
        bool AddPasswordForDataSource(string dataSourceId, string passWord);
        bool UpdatePasswordOfDataSource(Guid dataSourceId, string passWord);
        bool DisableDataSourcePassWord(Guid dataSourceId);
        List<Guid> GetDataSourceListbyReportId(Guid reportId);
        Guid? GetCloneOfItem(Guid itemId);
        Guid GetCloneItemId(Guid itemId);
        DataSourceDefinition GetDataSourceDefinition(Guid dataSourceId);
        DataSourceMappingInfo FinddatasourceByDatasourceName(Guid? categoryId, string dataSourceName);
        List<DataSourceInfo> GetReportDatasourceList(Guid reportId);
        bool RestoreItemWithItemId(Guid itemId, int userId);
        string GetDataSourcePassword(Guid dataSourceId);
        List<Permission> GetUserPermissions(int userId, PermissionEntity permissionEntity, List<string> itemId);

        EntityData<ItemDetail> GetItems(int userId, ItemType itemType, List<SortCollection> sortCollection = null, List<FilterCollection> filterSettings = null, string search = "",
            int? skip = 0, int? take = 10, Guid? itemId = null, bool isAllCategorySearch = false);

        Dictionary<PermissionAccess, bool> GetGlobalPermissionOfItem(int userId, ItemType itemType);

        List<Permission> GetCategories(int userId, List<int> groupList, ItemType? itemType = null, string categoryId = null);

        bool DeleteItem(Guid itemId, int userId);

        void DeleteDirectory(string targetDirectory);

        bool UpdateItemFields(Dictionary<string, object> updateFields, Guid itemId);

        bool UpdateItemType(Guid reportId, ItemType fileTypeId, int userId);

        List<Guid> GetCloneHeriarchy(Guid itemId);

        Result CloneItem(Guid itemId, Guid destiationCategoryId, int copiedByUserId, string itemName);
        Result CopyItem(Guid itemId, Guid destiationCategoryId, int copiedByUserId, string itemName);
        Result MoveItem(Guid itemId, Guid destiationCategoryId, int userId, string itemName);

        Result PublishFile(ItemDetail itemObj);

        bool AddReportintrashDeletedTable(Guid itemId, int userId);

        bool UpdateFileType(Guid itemId, ItemType itemType, int userId);

        void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs);

        List<string> GetChildItemofClone(List<string> fileIdList, Guid? cloneId);

        List<ItemDetail> GetAllItems(ItemType itemType);

        List<ItemDetail> CreateAccessCategoriesOfItem(int userId, ItemType itemType);

        bool HasGlobalCreatePermissionOnItem(int userId, int permissionEntityValue, List<string> groupIds);

        List<ItemDetail> GetAllCategoriesOfSystem();

        Dictionary<ItemType, bool> GetItemTypesWithCreateAccess(int userId);

        bool IsCategoryNameExistAlready(string categoryname, int itemtypeId);

        Guid SaveCategory(ItemDetail category);

        bool ItemDeleteValidation(Guid itemId, ItemType itemType);

        List<ItemVersion> FindItemPrevVersion(Guid itemId, int userId, out int countValue, string orgItemId = null, int skip = 0, int take = 0);

        List<ItemDetail> GetAllItems(int userId, List<int> groupIds, ItemType? itemType = null, List<Permission> parentIdPermissions = null, List<SortCollection> sortCollection = null, List<FilterCollection> filterSettings = null, string search = "", List<string> searchDescriptor = null, Guid? itemId = null);

        ItemDetail GetItemDetailsFromItemPath(string itemPath);
    }
}
