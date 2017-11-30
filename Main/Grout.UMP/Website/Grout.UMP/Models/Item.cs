using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Encryption;
using Grout.Base.Item;

namespace Grout.UMP.Models
{
    public class Item
    {
        private readonly IItemManagement _itemManagement = new ItemManagement();
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();
        private readonly IUserManagement _userManagement = new UserManagement();
        private readonly IGroupManagement _groupManagement = new GroupManagement();
        private readonly PermissionSet _permissionBase = new PermissionSet();


        public bool FolderFileCopy(Guid itemId, Guid destinationCategoryId, int sharedById, string itemName)
        {
            var output = _itemManagement.CopyItem(itemId, destinationCategoryId, sharedById, itemName);
            var reportDetails = _itemManagement.GetItemDetailsFromItemId(itemId);
            var categoryId = Guid.Parse(reportDetails.CategoryId.ToString());
            var currentItemVersion = _itemManagement.FindItemCurrentVersion(itemId).AsEnumerable()
                .Select(
                    a =>
                        a.Field<int>(
                            GlobalAppSettings.DbColumns.DB_ItemVersion.VersionNumber))
                .FirstOrDefault();
            _itemManagement.AddItemLog(currentItemVersion, ItemLogType.Cloned, sharedById, itemId, 0, categoryId, destinationCategoryId);
            return output.Status;
        }

        public bool FolderFileClone(Guid itemId, Guid destinationCategoryId, int sharedById, string itemName)
        {
            var output = _itemManagement.CloneItem(itemId, destinationCategoryId, sharedById, itemName);
            var reportDetails = _itemManagement.GetItemDetailsFromItemId(itemId);
            var categoryId = Guid.Parse(reportDetails.CategoryId.ToString());
            var currentItemVersion = _itemManagement.FindItemCurrentVersion(itemId).AsEnumerable()
                .Select(
                    a =>
                        a.Field<int>(
                            GlobalAppSettings.DbColumns.DB_ItemVersion.VersionNumber))
                .FirstOrDefault();
            _itemManagement.AddItemLog(currentItemVersion, ItemLogType.Copied, sharedById, itemId, 0, categoryId, destinationCategoryId);
            return output.Status;
        }

        public UserPreferenceSort GetUserPreferenceSort(int userId)
        {
            var xmlserializer = new XmlSerializer(typeof(UserPreferenceSort));
            var str = _userManagement.GetUserPreferentSort(userId);
            if (str != "") return (UserPreferenceSort)xmlserializer.Deserialize(new StringReader(str));
            var result = new UserPreferenceSort { SortAttribute = "SharedDate", SortValue = "dsc" };
            return result;
        }

        public List<ItemDetail> GetTrashedReports(int userId)
        {
            return _itemManagement.GetAllItemsOfUserFromItemTrash(userId);
        }

        public void RestoreDashboard(Guid itemId, int userId)
        {
            _itemManagement.RemoveItemFromTrash(itemId);
            _itemManagement.RestoreItemWithItemId(itemId, userId);
            var currentItemVersion = _itemManagement.FindItemCurrentVersion(itemId).AsEnumerable()
                .Select(
                    a =>
                        a.Field<int>(
                            GlobalAppSettings.DbColumns.DB_ItemVersion.VersionNumber))
                .FirstOrDefault();
            _itemManagement.AddItemLog(currentItemVersion, ItemLogType.Restored, userId, itemId, 0);
        }

        public void DeleteReportFromReportTrash(Guid itemId, int currentUserId)
        {
            _itemManagement.RemoveItemFromTrash(itemId);
            _itemManagement.AddReportintrashDeletedTable(itemId, currentUserId);
            var currentItemVersion = _itemManagement.FindItemCurrentVersion(itemId).AsEnumerable()
                .Select(
                    a =>
                        a.Field<int>(
                            GlobalAppSettings.DbColumns.DB_ItemVersion.VersionNumber))
                .FirstOrDefault();
            _itemManagement.AddItemLog(currentItemVersion, ItemLogType.Deleted, currentUserId, itemId, 0);
        }

        public bool SaveSortOrder(int userId, string sortAttribute, string sortValue)
        {
            var previousSort = GetUserPreferenceSort(userId);
            if (sortAttribute == previousSort.SortAttribute && sortValue == previousSort.SortValue)
            {
                return true;
            }
            var xmlserializer = new XmlSerializer(typeof(UserPreferenceSort));
            var obj = new UserPreferenceSort();
            obj.SortAttribute = sortAttribute;
            obj.SortValue = sortValue;
            using (var stream = new MemoryStream())
            {
                var builder = new StringBuilder();
                var settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                var write = XmlWriter.Create(builder, settings);
                xmlserializer.Serialize(write, obj);
                stream.Close();
                return _userManagement.UpdateUserSortPreference(builder.ToString(), userId);
            }
        }

        public ItemResponse PublishFile(ItemDetail file, int currentUserId, Stream fileStream)
        {
            var filePublishStatus = new ItemResponse();
            var publishedFileId = AddItem(file.Name, file.Description, file.CategoryId,
                currentUserId, file.ItemType, file.Extension);

            var itemName = file.Name + file.Extension;

            int versionId;
            var modifiedDate = DateTime.UtcNow;
            if (file.ItemType == ItemType.Dashboard)
            {
                var rootPath =
                    Path.Combine(GlobalAppSettings.GetItemsPath() + publishedFileId + "\\1\\");

                versionId =
                    (int)_itemManagement.SaveItemVersion(publishedFileId, currentUserId, 0, 1, itemName, String.Empty,
                        file.ItemType, modifiedDate).Value;
                _itemManagement.AddItemLog(versionId, ItemLogType.Added, currentUserId, publishedFileId, 0, null, null, modifiedDate);
                var temporaryFolder = Path.Combine(GlobalAppSettings.GetItemsPath() + "Temporary_files\\");

                if (Directory.Exists(temporaryFolder) == false)
                {
                    Directory.CreateDirectory(temporaryFolder);
                }

                var zipFilePath = temporaryFolder + publishedFileId + ".crpt";

                try
                {

                    if (!Directory.Exists(Path.Combine(rootPath)))
                        Directory.CreateDirectory(Path.Combine(rootPath));

                    using (var outputFileSteam = new FileStream(zipFilePath, FileMode.Create))
                    {
                        const int bufferSize = 65536;
                        var buffer = new Byte[bufferSize];
                        var bytesRead = fileStream.Read(buffer, 0, bufferSize);
                        while (bytesRead > 0)
                        {
                            outputFileSteam.Write(buffer, 0, bytesRead);
                            bytesRead = fileStream.Read(buffer, 0, bufferSize);
                        }
                        outputFileSteam.Close();
                    }

                    ZipManager.ExtractZipToDirectory(zipFilePath, Path.Combine(rootPath));

                    if (File.Exists(rootPath + "PreviewIcon.png"))
                    {
                        var previewImage = Image.FromFile(rootPath + "PreviewIcon.png");
                        if (previewImage.Width != 285 || previewImage.Height != 185)
                        {
                            var resizedImage = ImageManager.ResizeImage(previewImage, 285, 185);
                            previewImage.Dispose();
                            File.Delete(rootPath + "PreviewIcon.png");
                            resizedImage.Save(rootPath + "PreviewIcon.png", ImageFormat.Png);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _itemManagement.UpdateItemStatus(publishedFileId.ToString(), false);
                    filePublishStatus.Status = false;
                    filePublishStatus.StatusMessage = "Error while adding file";
                    return filePublishStatus;
                }
                finally
                {
                    File.Delete(zipFilePath);
                }
            }
            else if (file.ItemType != ItemType.Category)
            {
                var rootPath =
                    Path.Combine(GlobalAppSettings.GetItemsPath() + publishedFileId + "\\1\\");

                versionId =
                    Convert.ToInt32(_itemManagement.SaveItemVersion(publishedFileId, currentUserId, 0, 1, itemName, String.Empty,
                        file.ItemType, modifiedDate).Value);
                _itemManagement.AddItemLog(versionId, ItemLogType.Added, currentUserId, publishedFileId, 0, null, null, modifiedDate);
                try
                {

                    if (!Directory.Exists(Path.Combine(rootPath)))
                        Directory.CreateDirectory(Path.Combine(rootPath));
                    var extractedPath = Path.Combine(rootPath + file.Name + file.Extension);
                    using (var outputFile = new FileStream(extractedPath, FileMode.Create))
                    {
                        const int bufferSize = 65536;
                        var buffer = new Byte[bufferSize];
                        var bytesRead = fileStream.Read(buffer, 0, bufferSize);
                        while (bytesRead > 0)
                        {
                            outputFile.Write(buffer, 0, bytesRead);
                            bytesRead = fileStream.Read(buffer, 0, bufferSize);
                        }
                        outputFile.Close();
                    }
                }
                catch (Exception ex)
                {
                    _itemManagement.UpdateItemStatus(publishedFileId.ToString(), false);
                    filePublishStatus.Status = false;
                    filePublishStatus.StatusMessage = "Error while adding file";
                    return filePublishStatus;
                }
            }
            filePublishStatus.Status = true;
            filePublishStatus.PublishedItemId = publishedFileId;
            return filePublishStatus;
        }

        public void ItemRollback(Guid itemId, int versionId)
        {
            var reportVersions = _itemManagement.FindItemPrevVersionCount(itemId);
            _itemManagement.UpdateItemCurrentVersion(itemId);

            var versiondetails = _itemManagement.FindItemVersionByVerionId(itemId, versionId);
            var itemName = versiondetails.ItemName;
            var itemType = versiondetails.ItemTypeId;
            var itemVId =
                versiondetails.VersionNumber;
            var versionComment = "Rolled back to version " + versionId;
            var modifiedDate = DateTime.UtcNow;
            var addVersion = _itemManagement.SaveItemVersion(itemId,
                Convert.ToInt32(HttpContext.Current.User.Identity.Name), versionId,
                reportVersions + 1, itemName, versionComment, itemType, modifiedDate);
            var itemdetails = _itemManagement.GetItemDetailsFromItemId(itemId, false);
            var itemCreatedById =
                itemdetails.CreatedById;
            var sourceFolder = Path.Combine(GlobalAppSettings.GetItemsPath() + itemId + "\\" + versionId);
            var destinationFolder = Path.Combine(GlobalAppSettings.GetItemsPath() + itemId + "\\" + (reportVersions + 1));

            Directory.CreateDirectory(destinationFolder);
            _itemManagement.DirectoryCopy(sourceFolder, destinationFolder, true);

            destinationFolder =
                Path.Combine(GlobalAppSettings.GetItemsPath() + itemId + "\\" + (reportVersions + 1)) + "\\" +
                itemName;

            _itemManagement.AddItemLog(itemVId, ItemLogType.Rollbacked,
                Convert.ToInt32(HttpContext.Current.User.Identity.Name),
                itemId, 0, null, null, modifiedDate);

            var cloneIdList = new List<string>();
            cloneIdList = _itemManagement.GetChildItemofClone(cloneIdList, itemId);

            for (var t = 0; t < cloneIdList.Count; t++)
                _itemManagement.AddItemLog(itemVId, ItemLogType.Rollbacked,
                   Convert.ToInt32(HttpContext.Current.User.Identity.Name), Guid.Parse(cloneIdList[t]), 0);
        }

        public EntityData<ItemLogs> GetItemLog(Guid itemId, int skip = 0, int take = 0)
        {
            var itemLogs = _itemManagement.GetItemLog(itemId).AsEnumerable().Select(a =>
                new ItemLogs
                {
                    ItemLogId = a.Field<int>(GlobalAppSettings.DbColumns.DB_ItemLog.Id),
                    DisplayName = a.Field<string>(GlobalAppSettings.DbColumns.DB_User.DisplayName),
                    UserName = a.Field<string>("CurrentUserName"),
                    TargetUserFullName = a.Field<string>("TargetUserFullName"),
                    ReportLogType = a.Field<string>(GlobalAppSettings.DbColumns.DB_ItemLogType.Name),
                    UpdatedDate =
                        a.Field<DateTime>(GlobalAppSettings.DbColumns.DB_ItemLog.ModifiedDate),
                    UpdatedDateString =
                        GlobalAppSettings.GetFormattedTimeZone(
                            a.Field<DateTime>(GlobalAppSettings.DbColumns.DB_ItemLog.ModifiedDate),
                            Convert.ToInt32(HttpContext.Current.User.Identity.Name)),
                    //IsToday =
                    //    (a.Field<DateTime>(GlobalAppSettings.DbColumns.DB_ItemLog.ModifiedDate)
                    //        .ToString(GlobalAppSettings.SystemSettings.DateFormat) ==
                    //     DateTime.UtcNow.ToString(GlobalAppSettings.SystemSettings.DateFormat)),
                    //FormattedTime =
                    //    a.Field<DateTime>(GlobalAppSettings.DbColumns.DB_ItemLog.ModifiedDate)
                    //        .ToString(GlobalAppSettings.SystemSettings.DateFormat),
                    //DaysAgo =
                    //    Math.Round(
                    //        DateTime.UtcNow.Subtract(
                    //            a.Field<DateTime>(GlobalAppSettings.DbColumns.DB_ItemLog.ModifiedDate)).TotalDays),
                    //MinutesAgo =
                    //    Math.Round(
                    //        DateTime.UtcNow.Subtract(
                    //            a.Field<DateTime>(GlobalAppSettings.DbColumns.DB_ItemLog.ModifiedDate)).TotalMinutes),
                    DisplayText =
                        (a.Field<Int32>(GlobalAppSettings.DbColumns.DB_ItemLog.ItemLogTypeId) ==
                         (int)ItemLogType.Edited)
                            ? "Updated"
                            : (a.Field<Int32>(GlobalAppSettings.DbColumns.DB_ItemLog.ItemLogTypeId) ==
                               (int)ItemLogType.Deleted)
                                ? "Deleted"
                                : (a.Field<Int32>(GlobalAppSettings.DbColumns.DB_ItemLog.ItemLogTypeId) ==
                                   (int)ItemLogType.Trashed)
                                    ? "Moved to Trash"
                                    : (a.Field<Int32>(GlobalAppSettings.DbColumns.DB_ItemLog.ItemLogTypeId) ==
                                       (int)ItemLogType.Restored)
                                        ? "Restored from Trash"
                                        : (a.Field<Int32>(GlobalAppSettings.DbColumns.DB_ItemLog.ItemLogTypeId) ==
                                           (int)ItemLogType.Added)
                                            ? "Added"
                                            : (a.Field<Int32>(
                                                GlobalAppSettings.DbColumns.DB_ItemLog.ItemLogTypeId) ==
                                               (int)ItemLogType.Moved)
                                                ? "Moved from " + a.Field<string>("FromCategoryName") + " to " +
                                                  a.Field<string>("ToCategoryName")
                                                : (a.Field<Int32>(
                                                    GlobalAppSettings.DbColumns.DB_ItemLog.ItemLogTypeId) ==
                                                   (int)ItemLogType.Copied)
                                                    ? "Copied from " + a.Field<string>("FromCategoryName") +
                                                      " to " + a.Field<string>("ToCategoryName")
                                                    : (a.Field<Int32>(
                                                        GlobalAppSettings.DbColumns.DB_ItemLog.ItemLogTypeId) ==
                                                       (int)ItemLogType.Cloned)
                                                        ? "Cloned from " + a.Field<string>("FromCategoryName") +
                                                          " to " + a.Field<string>("ToCategoryName")
                                                        : (a.Field<Int32>(
                                                            GlobalAppSettings.DbColumns.DB_ItemLog.ItemLogTypeId) ==
                                                           (int)ItemLogType.Rollbacked)
                                                            ? "Rollbacked to version " +
                                                              (a.Field<Int32>(
                                                                  GlobalAppSettings.DbColumns.DB_ItemVersion
                                                                      .VersionNumber))
                                                            : ""
                }).OrderByDescending(a => a.ItemLogId);
            var enitemLog = new List<ItemLogs>();
            var countValue = itemLogs.ToList().Count();
            if (take != 0)
            {
                enitemLog = itemLogs.Skip(skip).Take(take).ToList();
            }
            else
            {
                enitemLog = itemLogs.ToList();
            }

            return new EntityData<ItemLogs>
            {
                result = enitemLog,
                count = countValue
            };
        }

        public Guid AddItem(string name, string description, Guid? categoryId, int currentUserId, ItemType fileType,
            string extension)
        {
            var item = new ItemDetail
            {
                CreatedById = currentUserId,
                ModifiedById = currentUserId,
                Name = name,
                Description = description,
                CategoryId = categoryId,
                ItemType = fileType,
                CreatedDate = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat()),
                Extension = extension
            };
            var result = _itemManagement.PublishFile(item);
            return Guid.Parse(result.ReturnValue.ToString());
        }

        public bool IsItemNameExistAlready(string reportname, ItemType itemTypeId)
        {
            return _itemManagement.IsItemNameAlreadyExists(reportname, itemTypeId);
        }

        public bool IsItemNameExistAlreadyforupdate(string reportname, Guid categoryId, Guid reportId)
        {
            return _itemManagement.IsItemNameAlreadyExistsForUpdate(reportname, categoryId, reportId);
        }

        public bool UpdateDataSources(string dataSourceName, string dataSourceId,
            string reportId)
        {
            return _itemManagement.UpdateDataSourceOfRdl(dataSourceName, dataSourceId, reportId);

        }

        public ItemResponse UpdateItem(Dictionary<string, object> updateFields,
             Guid itemId, ItemType itemType, List<DataSourceMappingInfo> dataSourceList = null)
        {
            var _itemResponse = new ItemResponse();
            try
            {
                var versionInfo = new DataResponse();
                var orgItemId = itemId;
                string targetPath;
                itemId = _itemManagement.GetCloneItemId(itemId);
                var itemDetails = _itemManagement.GetItemDetailsFromItemId(itemId, false);
                var previousItemName = itemDetails.Name;

                var itemName = ((itemId == orgItemId) &&
                                (updateFields.ContainsKey(GlobalAppSettings.DbColumns.DB_Item.Name)))
                    ? updateFields[GlobalAppSettings.DbColumns.DB_Item.Name].ToString()
                    : previousItemName;
                var previousVersion = _itemManagement.FindItemPrevVersionCount(itemId);
                var previousItemExtension = itemDetails.Extension;
                var rootPath = Path.Combine(GlobalAppSettings.GetItemsPath() + itemId);
                updateFields.Add(GlobalAppSettings.DbColumns.DB_Item.ModifiedDate,
                    DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat()));
                var modifiedDate = DateTime.UtcNow;

                if (itemType != ItemType.Category)
                {
                    if (Convert.ToBoolean(updateFields["isSourceChanged"]))
                    {
                        switch (itemType)
                        {

                            case ItemType.Report:
                                targetPath = Path.Combine(rootPath + "\\" + (previousVersion + 1) + "\\");
                                _itemManagement.UpdateItemCurrentVersion(itemId);
                                if (Directory.Exists(Path.Combine(targetPath)))
                                    _itemManagement.DeleteDirectory(Path.Combine(targetPath));
                                Directory.CreateDirectory(targetPath);
                                itemName += ".rdl";
                                var sharedDataSources = dataSourceList;
                                var temporaryXmlRootPath =
                                    Path.Combine(GlobalAppSettings.GetItemsPath() + "Temporary_Files\\" +
                                                 updateFields["temporaryFileName"]);
                                try
                                {
                                    XmlDocument xmlDocument = new XmlDocument();
                                    xmlDocument.Load(temporaryXmlRootPath);
                                    var dataSourceNodes = xmlDocument.GetElementsByTagName("DataSource");
                                    string dataSourceName;
                                    DataSourceMappingInfo sharedDataSource;
                                    foreach (var dataSourceNode in dataSourceNodes)
                                    {
                                        var xmlLinkedNode = dataSourceNode as XmlLinkedNode;
                                        foreach (var childNodes in xmlLinkedNode.ChildNodes)
                                        {
                                            var xmlChildLinkedNode = childNodes as XmlLinkedNode;
                                            if (xmlChildLinkedNode.Name == "DataSourceReference")
                                            {
                                                dataSourceName = xmlLinkedNode.Attributes["Name"].Value;
                                                sharedDataSource = dataSourceList.First(r => r.Name == dataSourceName);
                                                xmlChildLinkedNode.InnerText = sharedDataSource.DataSourceId.ToString();
                                            }
                                        }
                                    }
                                    xmlDocument.Save(temporaryXmlRootPath);

                                    var extractedPath = Path.Combine(targetPath + itemName);
                                    File.Copy(
                                        GlobalAppSettings.GetItemsPath() + "Temporary_Files\\" +
                                        updateFields["temporaryFileName"], extractedPath);
                                    File.Delete(GlobalAppSettings.GetItemsPath() + "Temporary_Files\\" +
                                                updateFields["temporaryFileName"]);
                                    _itemManagement.DisableDataSourceOfRdl(itemId.ToString());
                                    if (dataSourceList != null)
                                    {
                                        foreach (var dataSource in sharedDataSources)
                                        {
                                            var mapStatus = _itemManagement.AddDataSourceWithRdl(itemId.ToString(),
                                                dataSource.DataSourceId.ToString(), dataSource.Name);
                                            if (!mapStatus)
                                            {
                                                _itemResponse.StatusMessage = "Error while adding data source";
                                            }
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    _itemResponse.Status = false;
                                    _itemResponse.StatusMessage = ex.Message;
                                    return _itemResponse;
                                }
                                break;

                            case ItemType.Datasource:
                                targetPath = Path.Combine(rootPath + "\\" + (previousVersion + 1) + "\\");
                                _itemManagement.UpdateItemCurrentVersion(itemId);
                                if (Directory.Exists(Path.Combine(targetPath)))
                                    _itemManagement.DeleteDirectory(Path.Combine(targetPath));
                                Directory.CreateDirectory(targetPath);
                                itemName += ".rds";
                                try
                                {
                                    var dataSourceDefinition =
                                        (DataSourceDefinition)updateFields["dataSourceDefinition"];
                                    var extractedPath = Path.Combine(targetPath + itemName);
                                    var xmlSerializer = new XmlSerializer(typeof(DataSourceDefinition));

                                    if (!string.IsNullOrEmpty(dataSourceDefinition.Password))
                                    {
                                        var _tokenCryptography = new TokenCryptography();
                                        var encryptedPassword =
                                            _tokenCryptography.DoEncryption(dataSourceDefinition.Password);
                                        dataSourceDefinition.Password = null;
                                        _itemManagement.UpdatePasswordOfDataSource(itemId, encryptedPassword);
                                    }
                                    dataSourceDefinition.CredentialRetrieval = (dataSourceDefinition.CredentialRetrieval == CredentialRetrievalEnum.Store) ? CredentialRetrievalEnum.Prompt : dataSourceDefinition.CredentialRetrieval;
                                    using (var writer = new StreamWriter(extractedPath))
                                    {
                                        xmlSerializer.Serialize(writer, dataSourceDefinition);
                                        writer.Close();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _itemResponse.Status = false;
                                    _itemResponse.StatusMessage = ex.Message;
                                    return _itemResponse;
                                }
                                break;

                            case ItemType.Dashboard:
                                targetPath = Path.Combine(rootPath + "\\" + (previousVersion + 1) + "\\");
                                var zipFilePath =
                                    Path.Combine(GlobalAppSettings.GetItemsPath() + "Temporary_Files\\" + itemId +
                                                 ".crpt");

                                var temporaryDirectory =
                                    Path.Combine(GlobalAppSettings.GetItemsPath() + "Temporary_Files\\");
                                try
                                {
                                    var fileStream = (Stream)updateFields["fileStream"];
                                    using (var outputFile = new FileStream(zipFilePath, FileMode.Create))
                                    {
                                        const int bufferSize = 65536;
                                        var buffer = new Byte[bufferSize];
                                        var bytesRead = fileStream.Read(buffer, 0, bufferSize);
                                        while (bytesRead > 0)
                                        {
                                            outputFile.Write(buffer, 0, bytesRead);
                                            bytesRead = fileStream.Read(buffer, 0, bufferSize);
                                        }
                                    }
                                    if (!Directory.Exists(targetPath))
                                        Directory.CreateDirectory(targetPath);
                                    ZipManager.ExtractZipToDirectory(zipFilePath, Path.Combine(targetPath));
                                    if (File.Exists(targetPath + "PreviewIcon.png"))
                                    {
                                        var previewImage = Image.FromFile(targetPath + "PreviewIcon.png");
                                        if (previewImage.Width != 285 || previewImage.Height != 185)
                                        {
                                            var resizedImage = ImageManager.ResizeImage(previewImage, 285, 185);
                                            previewImage.Dispose();
                                            File.Delete(rootPath + "PreviewIcon.png");
                                            resizedImage.Save(rootPath + "PreviewIcon.png", ImageFormat.Png);
                                        }
                                    }
                                    if (itemType != itemDetails.ItemType)
                                    {
                                        _itemManagement.UpdateFileType(itemId, itemType,
                                            Convert.ToInt32(HttpContext.Current.User.Identity.Name));
                                        var clones = _itemManagement.GetCloneHeriarchy(itemId);
                                        for (var i = 0; i < clones.Count; i++)
                                        {
                                            _itemManagement.UpdateFileType(Guid.Parse(clones[i].ToString()), itemType,
                                                Convert.ToInt32(HttpContext.Current.User.Identity.Name));
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _itemResponse.Status = false;
                                    _itemResponse.StatusMessage = ex.Message;
                                    return _itemResponse;
                                }
                                finally
                                {
                                    File.Delete(zipFilePath);
                                }
                                break;


                            case ItemType.File:
                                if (itemType != itemDetails.ItemType)
                                {
                                    _itemManagement.UpdateFileType(itemId, itemType,
                                        Convert.ToInt32(HttpContext.Current.User.Identity.Name));
                                    var clones = _itemManagement.GetCloneHeriarchy(itemId);
                                    for (var i = 0; i < clones.Count; i++)
                                    {
                                        _itemManagement.UpdateFileType(Guid.Parse(clones[i].ToString()), itemType,
                                            Convert.ToInt32(HttpContext.Current.User.Identity.Name));
                                    }
                                }
                                try
                                {
                                    targetPath = Path.Combine(rootPath + "\\" + (previousVersion + 1) + "\\");
                                    _itemManagement.UpdateItemCurrentVersion(itemId);
                                    if (Directory.Exists(Path.Combine(targetPath)))
                                        _itemManagement.DeleteDirectory(Path.Combine(targetPath));
                                    Directory.CreateDirectory(targetPath);
                                    itemName += updateFields[GlobalAppSettings.DbColumns.DB_Item.Extension];
                                    var fileStream = (Stream)updateFields["fileStream"];
                                    using (var outputFile = new FileStream(targetPath + itemName, FileMode.Create))
                                    {
                                        const int bufferSize = 65536;
                                        var buffer = new Byte[bufferSize];
                                        var bytesRead = fileStream.Read(buffer, 0, bufferSize);
                                        while (bytesRead > 0)
                                        {
                                            outputFile.Write(buffer, 0, bytesRead);
                                            bytesRead = fileStream.Read(buffer, 0, bufferSize);
                                        }
                                        outputFile.Close();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _itemResponse.Status = false;
                                    _itemResponse.StatusMessage = ex.Message;
                                    return _itemResponse;
                                }
                                break;
                        }
                        var versionComment = updateFields.ContainsKey("versionComment")
                            ? updateFields["versionComment"].ToString()
                            : string.Empty;
                        versionInfo = _itemManagement.SaveItemVersion(itemId,
                            Convert.ToInt32(HttpContext.Current.User.Identity.Name), 0,
                            previousVersion + 1,
                            itemName, versionComment, itemType, modifiedDate);

                    }
                    else
                    {
                        if (updateFields.ContainsKey(GlobalAppSettings.DbColumns.DB_Item.Name) &&
                            itemName != previousItemName)
                        {
                            var sourcePath =
                                Path.Combine(rootPath + "\\" + previousVersion + "\\" + previousItemName +
                                             previousItemExtension);
                            targetPath =
                                Path.Combine(rootPath + "\\" + previousVersion + "\\" + itemName + previousItemExtension);
                            File.Move(sourcePath, targetPath);
                            _itemManagement.UpdateNameInVersionTable(itemId, previousVersion,
                                itemName + previousItemExtension);
                        }
                        if (itemType == ItemType.Report && (bool)updateFields["isDataSourceChanged"])
                        {
                            if (dataSourceList != null)
                            {
                                var sharedDataSources = dataSourceList;
                                var temporaryXmlRootPath = _itemManagement.GetItemLocation(itemId, ItemType.Report);
                                XmlDocument xmlDocument = new XmlDocument();
                                xmlDocument.Load(temporaryXmlRootPath);
                                var dataSourceNodes = xmlDocument.GetElementsByTagName("DataSource");
                                string dataSourceName;
                                ItemDetail dataSourceDetails;
                                DataSourceMappingInfo sharedDataSource;
                                foreach (var dataSourceNode in dataSourceNodes)
                                {
                                    var xmlLinkedNode = dataSourceNode as XmlLinkedNode;
                                    foreach (var childNodes in xmlLinkedNode.ChildNodes)
                                    {
                                        var xmlChildLinkedNode = childNodes as XmlLinkedNode;
                                        if (xmlChildLinkedNode.Name == "DataSourceReference")
                                        {
                                            dataSourceName = xmlLinkedNode.Attributes["Name"].Value;
                                            try
                                            {
                                                sharedDataSource =
                                                    sharedDataSources.Where(r => r.Name == dataSourceName).First();
                                                if (sharedDataSource != null)
                                                {
                                                    xmlChildLinkedNode.InnerText = sharedDataSource.DataSourceId.ToString();
                                                }
                                            }
                                            catch
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                }
                                xmlDocument.Save(temporaryXmlRootPath);
                                foreach (var dataSource in sharedDataSources)
                                {
                                    var mapStatus = _itemManagement.UpdateDataSourceOfRdl(dataSource.Name,
                                        dataSource.DataSourceId.ToString(), itemId.ToString());
                                    if (!mapStatus)
                                    {
                                        _itemResponse.StatusMessage = "Error while adding data source";
                                    }
                                }
                            }
                        }
                    }
                }


                _itemManagement.UpdateItemFields(updateFields, orgItemId);

                var currentVersion = Convert.ToInt32(versionInfo.Value);
                if (Convert.ToInt32(versionInfo.Value) == 0)
                    currentVersion =
                        _itemManagement.FindItemCurrentVersion(itemId).AsEnumerable()
                            .Select(
                                a =>
                                    a.Field<int>(
                                        GlobalAppSettings.DbColumns.DB_ItemVersion.VersionNumber))
                            .FirstOrDefault();
                _itemManagement.AddItemLog(currentVersion, ItemLogType.Edited,
                    Convert.ToInt32(HttpContext.Current.User.Identity.Name),
                    orgItemId, 0, null, null, modifiedDate);
                if (itemType == ItemType.Report && Convert.ToBoolean(updateFields["isSourceChanged"]))
                {
                    var cloneitemList = _itemManagement.GetCloneHeriarchy(itemId);
                    cloneitemList.Add(itemId);
                    for (var t = 0; t < cloneitemList.Count(); t++)
                    {
                        if (orgItemId != cloneitemList[t])
                        {
                            _itemManagement.AddItemLog(currentVersion, ItemLogType.Edited,
                                   Convert.ToInt32(HttpContext.Current.User.Identity.Name),
                                   cloneitemList[t], 0, null, null, modifiedDate);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _itemResponse.Status = false;
                _itemResponse.StatusMessage = e.Message;
                return _itemResponse;
            }
            _itemResponse.Status = true;
            return _itemResponse;
        }

        public bool IsCategoryNameExistAlready(string categoryname, int itemtype)
        {
            return _itemManagement.IsCategoryNameExistAlready(categoryname, itemtype);
        }

        public bool AddNewCategory(ItemDetail item)
        {
            var publishedId = _itemManagement.SaveCategory(item);
            if (publishedId != Guid.Empty)
            {
                var permissionValue = new Permission
                {
                    IsUserPermission = true,
                    ItemId = publishedId,
                    PermissionAccess = PermissionAccess.ReadWriteDelete,
                    PermissionEntity = PermissionEntity.SpecificCategory,
                    TargetId = item.CreatedById
                };

                _permissionBase.AddPermissionToUser(permissionValue);
                _itemManagement.SaveItemVersion(publishedId, item.CreatedById, 0, 1, item.Name, String.Empty,
                    item.ItemType, DateTime.UtcNow);

                return true;
            }
            return false;
        }


    }
}