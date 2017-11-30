using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Encryption;
using Grout.Base.Item;
using Grout.Base.Logger;

namespace Grout.UMP.Models
{
    public class Designer
    {
        private readonly IItemManagement _itemManagement = new ItemManagement();
        private readonly IUserManagement _userManagement = new UserManagement();
        private readonly IGroupManagement _groupManagement = new GroupManagement();
        private readonly TokenCryptography _tokenCryptography = new TokenCryptography();
        private readonly Cryptography _cryptography = new Cryptography();
        private readonly Item _item = new Item();
        private readonly DataSourceManager _dataSourceManager = new DataSourceManager();
        private readonly PermissionSet _permissionBase = new PermissionSet();

        public ItemResponse DownloadItem(ItemRequest itemRequest)
        {
            var apiResponse = new ItemResponse();
            try
            {
                var itemDetail = _itemManagement.GetItemDetailsFromItemId(itemRequest.ItemId);
                
                if (itemDetail != null)
                {
                    var itemLocation = _itemManagement.GetItemLocation(itemDetail.Id, ItemType.Report);
                    apiResponse = new ItemResponse
                    {
                        Status = true,
                        FileContent = GetResponseBytes(itemLocation),
                        ItemName = itemDetail.Name
                    };
                }
            }
            catch (Exception e)
            {
                apiResponse.Status = false;
                apiResponse.StatusMessage = "File not found";
                LogExtension.LogInfo("Item Version has been saved successfully", MethodBase.GetCurrentMethod(), " ItemId - " + itemRequest.ItemId);
                return apiResponse;
            }
            return apiResponse;
        }

        public byte[] GetResponseBytes(string filePath)
        {
            byte[] result = null;
            try
            {
                result = File.ReadAllBytes(filePath);
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while reading bytes from file location", e, MethodBase.GetCurrentMethod(), " FilePath - " + filePath);
                return result;
            }
            return result;
        }

        /// <summary>
        /// Add RDL report in Reports Server
        /// </summary>
        /// <param name="itemRequest">ItemRequest object which is sent as request</param>
        /// <returns>The response is sent as ItemResponse object</returns>
        public ItemResponse PublishReport(ItemRequest itemRequest)
        {
            ItemResponse apiResponse;
            var userName = itemRequest.UserName;
            var currentUserId = _userManagement.GetUserId(userName);
            itemRequest.Description=(String.IsNullOrWhiteSpace(itemRequest.Description)) ? string.Empty : itemRequest.Description.Trim();
            var publishedFileId = _item.AddItem(itemRequest.Name, itemRequest.Description, itemRequest.CategoryId,
                    currentUserId, ItemType.Report, ".rdl");
            DataResponse addVersion;
            var modifiedDate = DateTime.UtcNow;
            try
            {
                var rootPath =
                    Path.Combine(GlobalAppSettings.GetItemsPath() + publishedFileId +
                                 "\\1\\");
                if (!Directory.Exists(Path.Combine(rootPath)))
                    Directory.CreateDirectory(Path.Combine(rootPath));
                var extractedPath = Path.Combine(rootPath + itemRequest.Name + ".rdl");
                File.WriteAllBytes(extractedPath, itemRequest.ItemContent);
                
                addVersion = _itemManagement.SaveItemVersion(publishedFileId, currentUserId, 0, 1,
                    itemRequest.Name + ".rdl", String.Empty,
                    ItemType.Report, modifiedDate);
                if (itemRequest.DataSourceMappingInfo != null)
                {
                    foreach (var dataSource in itemRequest.DataSourceMappingInfo)
                    {
                        _dataSourceManager.AddDataSourceWithReport(dataSource.Name, dataSource.DataSourceId.ToString(),
                            publishedFileId.ToString());
                    }
                }
                var permissionValue = new Permission
                {
                    IsUserPermission = true,
                    ItemId = publishedFileId,
                    PermissionAccess = PermissionAccess.ReadWriteDelete,
                    PermissionEntity = PermissionEntity.SpecificReports,
                    TargetId = currentUserId
                };
                _permissionBase.AddPermissionToUser(permissionValue);
            }
            catch (Exception ex)
            {
                LogExtension.LogError("Error while adding report from report designer", ex, MethodBase.GetCurrentMethod(), " UserName - " + userName + " UserId - " + currentUserId + " ItemRequestName - " + itemRequest.Name + " ItemRequestDescription - " + itemRequest.Description + " ItemRequestCategoryId - " + itemRequest.CategoryId);
                _itemManagement.UpdateItemStatus(publishedFileId.ToString(), false);
                apiResponse = new ItemResponse
                {
                    Status = false,
                    StatusMessage = ex.Message
                };
                return apiResponse;
            }
            apiResponse = new ItemResponse
            {
                Status = true,
                PublishedItemId = publishedFileId,
                StatusMessage = "Report has been added successfully."
            };
            try
            {
                _itemManagement.AddItemLog(Convert.ToInt32(addVersion.Value.ToString()), ItemLogType.Added,
                    currentUserId, publishedFileId, 0, null, null, modifiedDate);
            }
            catch (Exception ex)
            {
                LogExtension.LogError("Error while adding report log", ex, MethodBase.GetCurrentMethod(), " UserName - " + userName + " UserId - " + currentUserId + " ItemRequestName - " + itemRequest.Name + " ItemRequestDescription - " + itemRequest.Description + " ItemRequestCategoryId - " + itemRequest.CategoryId);
                return apiResponse;
            }
            return apiResponse;
        }

        /// <summary>
        /// Add data source in Reports Server
        /// </summary>
        /// <param name="itemRequest">ItemRequest object which is sent as request</param>
        /// <returns>The response is sent as ItemResponse object</returns>
        public ItemResponse PublishDataSource(ItemRequest itemRequest)
        {
            ItemResponse apiResponse;
            var userName = itemRequest.UserName;
            var currentUserId = _userManagement.GetUserId(userName);
            itemRequest.Description = (String.IsNullOrWhiteSpace(itemRequest.Description)) ? string.Empty : itemRequest.Description.Trim();
            var publishedFileId = _item.AddItem(itemRequest.Name, itemRequest.Description, itemRequest.CategoryId,
                currentUserId, ItemType.Datasource, ".rds");
            DataResponse addVersion;
            var modifiedDate = DateTime.UtcNow;
            try
            {
                var rootPath =
                    Path.Combine(GlobalAppSettings.GetItemsPath() + publishedFileId +
                                 "\\1\\");
                if (!Directory.Exists(Path.Combine(rootPath)))
                    Directory.CreateDirectory(Path.Combine(rootPath));
                var extractedPath = Path.Combine(rootPath + itemRequest.Name + ".rds");
                var xmlserializer = new XmlSerializer(typeof(DataSourceDefinition));
                using (var writer = new StreamWriter(extractedPath))
                {
                    xmlserializer.Serialize(writer, itemRequest.DataSourceDefinition);
                    writer.Close();
                }
                addVersion = _itemManagement.SaveItemVersion(publishedFileId, currentUserId, 0, 1,
                    itemRequest.Name + ".rds", String.Empty,
                    ItemType.Datasource, modifiedDate);
                var permissionValue = new Permission
                {
                    IsUserPermission = true,
                    ItemId = publishedFileId,
                    PermissionAccess = PermissionAccess.ReadWriteDelete,
                    PermissionEntity = PermissionEntity.SpecificDataSource,
                    TargetId = currentUserId
                };
                _permissionBase.AddPermissionToUser(permissionValue);
            }
            catch (Exception ex)
            {
                LogExtension.LogError("Error while adding data source from report designer", ex, MethodBase.GetCurrentMethod(), " UserName - " + userName + " UserId - " + currentUserId + " ItemRequestName - " + itemRequest.Name + " ItemRequestDescription - " + itemRequest.Description + " ItemRequestCategoryId - " + itemRequest.CategoryId);
                _itemManagement.UpdateItemStatus(publishedFileId.ToString(), false);
                apiResponse = new ItemResponse
                {
                    Status = false,
                    StatusMessage = ex.Message
                };
                return apiResponse;
            }
            apiResponse = new ItemResponse
            {
                Status = true,
                PublishedItemId = publishedFileId,
                StatusMessage = "Data source has been added successfully."
            };
            try
            {
                _itemManagement.AddItemLog(Convert.ToInt32(addVersion.Value.ToString()), ItemLogType.Added,
                    currentUserId, publishedFileId, 0, null, null, modifiedDate);
            }
            catch (Exception ex)
            {
                LogExtension.LogError("Error while adding data source log", ex, MethodBase.GetCurrentMethod(), " UserName - " + userName + " UserId - " + currentUserId + " ItemRequestName - " + itemRequest.Name + " ItemRequestDescription - " + itemRequest.Description + " ItemRequestCategoryId - " + itemRequest.CategoryId);
                return apiResponse;
            }
            return apiResponse;
        }

        /// <summary>
        /// Update RDL report in Reports Server
        /// </summary>
        /// <param name="itemRequest">ItemRequest object which is sent as request</param>
        /// <returns>The response is sent as ItemResponse object</returns>
        public ItemResponse UpdateReport(ItemRequest itemRequest)
        {
            var apiResponse = new ItemResponse();
            var userName = itemRequest.UserName;
            var currentUserId = _userManagement.GetUserId(userName);
            var modifiedDate = DateTime.UtcNow;
            var versionInfo = new DataResponse();

            try
            {
                var orgItemId = itemRequest.ItemId;
                string targetPath;
                itemRequest.ItemId = GetCloneItemId(itemRequest.ItemId);
                var itemDetails = _itemManagement.GetItemDetailsFromItemId(itemRequest.ItemId);
                var previousItemName = itemDetails.Name;
                var itemName = ((itemRequest.ItemId == orgItemId) && (!String.IsNullOrWhiteSpace(itemRequest.Name)))
                                    ? itemRequest.Name
                                    : previousItemName;
                var previousVersion = _itemManagement.FindItemPrevVersionCount(itemRequest.ItemId);
                var previousItemExtension = itemDetails.Extension;
                var rootPath = Path.Combine(GlobalAppSettings.GetItemsPath() + itemRequest.ItemId);

                var updateFields = new Dictionary<string, object>
                {
                    {GlobalAppSettings.DbColumns.DB_Item.ModifiedById, currentUserId},
                    {GlobalAppSettings.DbColumns.DB_Item.ModifiedDate,DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())}
                };
                if (!String.IsNullOrWhiteSpace(itemRequest.Name))
                {
                    updateFields.Add(GlobalAppSettings.DbColumns.DB_Item.Name, itemRequest.Name);
                }
                if (!String.IsNullOrWhiteSpace(itemRequest.Description))
                {
                    updateFields.Add(GlobalAppSettings.DbColumns.DB_Item.Description, itemRequest.Description.Trim());
                }

                if (itemRequest.ItemContent != null)
                {
                    targetPath = Path.Combine(rootPath + "\\" + (previousVersion + 1) + "\\");
                    _itemManagement.UpdateItemCurrentVersion(itemRequest.ItemId);
                    if (Directory.Exists(Path.Combine(targetPath)))
                        _itemManagement.DeleteDirectory(Path.Combine(targetPath));
                    Directory.CreateDirectory(targetPath);
                    itemName += ".rdl";
                    try
                    {
                        var extractedPath = Path.Combine(targetPath + itemName);
                        File.WriteAllBytes(extractedPath, itemRequest.ItemContent);
                        _itemManagement.DisableDataSourceOfRdl(itemRequest.ItemId.ToString());
                        if (itemRequest.DataSourceMappingInfo != null)
                        {
                            foreach (var dataSource in itemRequest.DataSourceMappingInfo)
                            {
                                _dataSourceManager.AddDataSourceWithReport(dataSource.Name, dataSource.DataSourceId.ToString(),
                                    itemRequest.ItemId.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogExtension.LogError("Error while updating report in report designer", ex, MethodBase.GetCurrentMethod(), " UserName - " + userName + " UserId - " + currentUserId + " ItemRequestName - " + itemRequest.Name + " ItemRequestDescription - " + itemRequest.Description + " ItemRequestItemId - " + itemRequest.ItemId);
                        apiResponse.Status = false;
                        apiResponse.StatusMessage = ex.Message;
                        return apiResponse;
                    }
                    var versionComment = !String.IsNullOrWhiteSpace(itemRequest.VersionComment) ? itemRequest.VersionComment : string.Empty;
                    versionInfo = _itemManagement.SaveItemVersion(itemRequest.ItemId, currentUserId, 0,
                        previousVersion + 1,
                        itemName, versionComment, ItemType.Report, modifiedDate);
                }
                else
                {
                    if (itemRequest.DataSourceMappingInfo != null)
                    {
                        foreach (var dataSource in itemRequest.DataSourceMappingInfo)
                        {
                            _itemManagement.UpdateDataSourceOfRdl(dataSource.Name, dataSource.DataSourceId.ToString(), itemRequest.ItemId.ToString());
                        }
                    }
                    if (!String.IsNullOrWhiteSpace(itemRequest.Name))
                    {
                        var sourcePath = Path.Combine(rootPath + "\\" + previousVersion + "\\" + previousItemName + "." + previousItemExtension);
                        targetPath = Path.Combine(rootPath + "\\" + previousVersion + "\\" + itemName + "." + previousItemExtension);
                        File.Move(sourcePath, targetPath);
                        _itemManagement.UpdateNameInVersionTable(itemRequest.ItemId, previousVersion, itemName + "." + previousItemExtension);
                    }
                }
                if (orgItemId == itemRequest.ItemId)
                {
                    _itemManagement.UpdateItemFields(updateFields, orgItemId);
                }
                else
                {
                    _itemManagement.UpdateItemFields(updateFields, orgItemId);
                    var parentUpdateFields = new Dictionary<string, object>
                    {
                    {GlobalAppSettings.DbColumns.DB_Item.ModifiedById,updateFields[GlobalAppSettings.DbColumns.DB_Item.ModifiedById]},
                    {GlobalAppSettings.DbColumns.DB_Item.ModifiedDate,updateFields[GlobalAppSettings.DbColumns.DB_Item.ModifiedDate]}
                    };
                    _itemManagement.UpdateItemFields(parentUpdateFields, itemRequest.ItemId);
                }
                var reportOwner = itemDetails.CreatedById;
                var currentVersion = Convert.ToInt32(versionInfo.Value);
                if (currentVersion == 0)
                    currentVersion =
                        _itemManagement.FindItemCurrentVersion(itemRequest.ItemId).AsEnumerable()
                            .Select(
                                a =>
                                    a.Field<int>(
                                        GlobalAppSettings.DbColumns.DB_ItemVersion.VersionNumber))
                            .FirstOrDefault();
                _itemManagement.AddItemLog(currentVersion, ItemLogType.Edited,
                    currentUserId,
                    orgItemId, 0, null, null, modifiedDate);
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while updating log in updating report from report designer", e, MethodBase.GetCurrentMethod(), " UserName - " + userName + " UserId - " + currentUserId + " ItemRequestName - " + itemRequest.Name + " ItemRequestDescription - " + itemRequest.Description + " ItemRequestItemId - " + itemRequest.ItemId + " CurrentVersion - " + (int)versionInfo.Value);
                apiResponse.Status = false;
                apiResponse.StatusMessage = e.Message;
                return apiResponse;
            }
            apiResponse.Status = true;
            apiResponse.StatusMessage = "Report has been updated successfully.";
            return apiResponse;
        }

        /// <summary>
        /// Update data source in Reports Server
        /// </summary>
        /// <param name="itemRequest">ItemRequest object which is sent as request</param>
        /// <returns>The response is sent as ItemResponse object</returns>
        public ItemResponse UpdateDataSource(ItemRequest itemRequest)
        {
            var apiResponse = new ItemResponse();
            var userName = itemRequest.UserName;
            var currentUserId = _userManagement.GetUserId(userName);
            var versionInfo = new DataResponse();
            var modifiedDate = DateTime.UtcNow;
            try
            {
                var orgItemId = itemRequest.ItemId;
                string targetPath;
                itemRequest.ItemId = GetCloneItemId(itemRequest.ItemId);
                var itemDetails = _itemManagement.GetItemDetailsFromItemId(itemRequest.ItemId);
                var previousItemName = itemDetails.Name;
                var itemName = ((itemRequest.ItemId == orgItemId) && (!String.IsNullOrWhiteSpace(itemRequest.Name)))
                                    ? itemRequest.Name
                                    : previousItemName;
                var previousVersion = _itemManagement.FindItemPrevVersionCount(itemRequest.ItemId);
                var previousItemExtension = itemDetails.Extension;
                var rootPath = Path.Combine(GlobalAppSettings.GetItemsPath() + itemRequest.ItemId);

                var updateFields = new Dictionary<string, object>
                {
                    {GlobalAppSettings.DbColumns.DB_Item.ModifiedById, currentUserId}
                };
                if (!String.IsNullOrWhiteSpace(itemRequest.Name))
                {
                    updateFields.Add(GlobalAppSettings.DbColumns.DB_Item.Name, itemRequest.Name);
                }
                if (!String.IsNullOrWhiteSpace(itemRequest.Description))
                {
                    updateFields.Add(GlobalAppSettings.DbColumns.DB_Item.Description, itemRequest.Description.Trim());
                }
                if (itemRequest.DataSourceDefinition != null)
                {

                    targetPath = Path.Combine(rootPath + "\\" + (previousVersion + 1) + "\\");
                    _itemManagement.UpdateItemCurrentVersion(itemRequest.ItemId);
                    if (Directory.Exists(Path.Combine(targetPath)))
                        _itemManagement.DeleteDirectory(Path.Combine(targetPath));
                    Directory.CreateDirectory(targetPath);
                    itemName += ".rds";
                    try
                    {
                        var dataSourceDefinition = (DataSourceDefinition)itemRequest.DataSourceDefinition;
                        var extractedPath = Path.Combine(targetPath + itemName);
                        var xmlSerializer = new XmlSerializer(typeof(DataSourceDefinition));

                        if (!string.IsNullOrEmpty(dataSourceDefinition.Password))
                        {
                            var _tokenCryptography = new TokenCryptography();
                            var encryptedPassword = _tokenCryptography.DoEncryption(dataSourceDefinition.Password);
                            dataSourceDefinition.Password = null;
                            _itemManagement.UpdatePasswordOfDataSource(itemRequest.ItemId, encryptedPassword);
                        }
                        using (var writer = new StreamWriter(extractedPath))
                        {
                            xmlSerializer.Serialize(writer, dataSourceDefinition);
                            writer.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogExtension.LogError("Error while updating log in updating report from report designer", ex, MethodBase.GetCurrentMethod(), " UserName - " + userName + " UserId - " + currentUserId + " ItemRequestName - " + itemRequest.Name + " ItemRequestDescription - " + itemRequest.Description + " ItemRequestItemId - " + itemRequest.ItemId);
                        apiResponse.Status = false;
                        apiResponse.StatusMessage = ex.Message;
                        return apiResponse;
                    }
                    updateFields.Add(GlobalAppSettings.DbColumns.DB_Item.ModifiedDate, DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat()));
                    var versionComment = !String.IsNullOrWhiteSpace(itemRequest.VersionComment) ? itemRequest.VersionComment : string.Empty;
                    versionInfo = _itemManagement.SaveItemVersion(itemRequest.ItemId, currentUserId, 0,
                        previousVersion + 1,
                        itemName, versionComment, ItemType.Datasource, modifiedDate);

                }
                else
                {
                    if (itemRequest.DataSourceMappingInfo != null)
                    {
                        foreach (var dataSource in itemRequest.DataSourceMappingInfo)
                        {
                            _itemManagement.UpdateDataSourceOfRdl(dataSource.Name, dataSource.DataSourceId.ToString(), itemRequest.ItemId.ToString());
                        }
                    }
                    if (!String.IsNullOrWhiteSpace(itemRequest.Name))
                    {
                        var sourcePath = Path.Combine(rootPath + "\\" + previousVersion + "\\" + previousItemName + "." + previousItemExtension);
                        targetPath = Path.Combine(rootPath + "\\" + previousVersion + "\\" + itemName + "." + previousItemExtension);
                        File.Move(sourcePath, targetPath);
                        _itemManagement.UpdateNameInVersionTable(itemRequest.ItemId, previousVersion, itemName + "." + previousItemExtension);
                    }
                }
                if (orgItemId == itemRequest.ItemId)
                {
                    _itemManagement.UpdateItemFields(updateFields, orgItemId);
                }
                else
                {
                    _itemManagement.UpdateItemFields(updateFields, orgItemId);
                    var parentUpdateFields = new Dictionary<string, object>
                    {
                    {GlobalAppSettings.DbColumns.DB_Item.ModifiedById,updateFields[GlobalAppSettings.DbColumns.DB_Item.ModifiedById]},
                    {GlobalAppSettings.DbColumns.DB_Item.ModifiedDate,updateFields[GlobalAppSettings.DbColumns.DB_Item.ModifiedDate]}
                    };
                    _itemManagement.UpdateItemFields(parentUpdateFields, itemRequest.ItemId);
                }
                var reportOwner = itemDetails.CreatedById;
                var currentVersion = Convert.ToInt32(versionInfo.Value);
                if (currentVersion == 0)
                    currentVersion =
                        _itemManagement.FindItemCurrentVersion(itemRequest.ItemId).AsEnumerable()
                            .Select(
                                a =>
                                    a.Field<int>(
                                        GlobalAppSettings.DbColumns.DB_ItemVersion.VersionNumber))
                            .FirstOrDefault();
                _itemManagement.AddItemLog(currentVersion, ItemLogType.Edited,
                    currentUserId,
                    orgItemId, 0, null, null, modifiedDate);
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while updating log in updating data source from report designer", e, MethodBase.GetCurrentMethod(), " UserName - " + userName + " UserId - " + currentUserId + " ItemRequestName - " + itemRequest.Name + " ItemRequestDescription - " + itemRequest.Description + " ItemRequestItemId - " + itemRequest.ItemId + " CurrentVersion - " + (int)versionInfo.Value);
                apiResponse.Status = false;
                apiResponse.StatusMessage = e.Message;
                return apiResponse;
            }
            apiResponse.Status = true;
            apiResponse.StatusMessage = "Datasource has been updated successfully.";
            return apiResponse;
        }

        public Guid GetCloneItemId(Guid itemId)
        {
            var cloneItemId = _itemManagement.GetCloneOfItem(itemId);
            if (cloneItemId != null)
            {
                itemId = Guid.Parse(cloneItemId.ToString());
            }
            return itemId;
        }

        public ApiResponse Login(string userName, string passWord)
        {
            var dataResponse = new ApiResponse { ApiStatus = true };
            var userDetail = _userManagement.FindUserByUserName(userName);

            if (userDetail != null)
            {
                var encryptedPassword = String.Empty;
                try
                {
                    encryptedPassword =
                         userDetail.Password;
                }
                catch (Exception ex)
                {
                    dataResponse.Data = new ApiData
                    {
                        Success = false,
                        Message = ex.Message,
                        Value = passWord
                    };
                    return dataResponse;
                }

                bool isActive;
                bool isDeleted;
                try
                {
                    isActive = userDetail.Status == UserStatus.Active;
                }
                catch (Exception ex)
                {
                    dataResponse.Data = new ApiData
                    {
                        Success = false,
                        Message = ex.Message,
                        Value = passWord
                    };
                    return dataResponse;
                }

                try
                {
                    isDeleted = userDetail.IsDeleted;
                }
                catch (Exception ex)
                {
                    dataResponse.Data = new ApiData
                    {
                        Success = false,
                        Message = ex.Message,
                        Value = passWord
                    };
                    return dataResponse;
                }

                if (isActive && !isDeleted)
                {
                    if (passWord == encryptedPassword)
                    {
                        dataResponse.Data = new ApiData
                        {
                            Success = true,
                            Message = "Authentication successfull",
                            Value = passWord
                        };
                        return dataResponse;
                    }
                    dataResponse.Data = new ApiData
                    {
                        Success = false,
                        Message = "Invalid password",
                        Value = passWord
                    };
                    return dataResponse;
                }

                if (!isActive)
                {
                    dataResponse.Data = new ApiData
                    {
                        Success = false,
                        Message = "Deactivated user",
                        Value = passWord
                    };
                    return dataResponse;
                }
                dataResponse.Data = new ApiData
                {
                    Success = false,
                    Message = "Deleted user",
                    Value = passWord
                };
                return dataResponse;
            }
            dataResponse.Data = new ApiData
            {
                Success = false,
                Message = "User not found",
                Value = passWord
            };
            return dataResponse;
        }

        /// <summary>
        /// Get the datasource definition by folderid and data source name
        /// </summary>
        /// <param name="path">The path container the folder id and data source</param>
        /// <returns>Class type ItemResponse</returns>
        public ItemResponse GetDataSourceDefinitionByName(ItemRequest itemRequest)
        {
            var apiResponse = new ItemResponse();

            byte[] datasourceByteValue = null;
            try
            {
                var categoryId = itemRequest.CategoryId;
                var datasourceName = itemRequest.DatasourceDetails.Name;
                var datasourcedetails = _itemManagement.FinddatasourceByDatasourceName(categoryId, datasourceName);
                if (datasourcedetails != null)
                {
                    var datasourceDefinition = _itemManagement.GetDataSourceDefinition(datasourcedetails.DataSourceId);
                    var datasourceStream = new MemoryStream();
                    var dataSourceSerializer = new XmlSerializer(typeof(DataSourceDefinition));
                    var datasourceStreamWriter = new XmlTextWriter(datasourceStream, Encoding.UTF8);
                    dataSourceSerializer.Serialize(datasourceStreamWriter, datasourceDefinition);
                    datasourceStream = (MemoryStream)datasourceStreamWriter.BaseStream;
                    datasourceByteValue = datasourceStream.ToArray();
                    apiResponse = new ItemResponse
                    {
                        FileContent = datasourceByteValue,
                        Status = true,
                        ItemName = datasourcedetails.Name
                    };
                    return apiResponse;
                }
                else
                {
                    apiResponse = new ItemResponse
                    {
                        StatusMessage = "Data source does not exist in database",
                        Status = false
                    };
                    return apiResponse;
                }

            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while downloading data source from report designer", e, MethodBase.GetCurrentMethod(), " CategoryId - " + itemRequest.CategoryId + " DatasourceName - " + itemRequest.DatasourceDetails.Name);
                var response = new ItemResponse
                {
                    StatusMessage = e.Message,
                    Status = false
                };
                return response;
            }
        }

        /// <summary>
        /// Get data source definition by data source guid id
        /// </summary>
        /// <param name="id">Data source guid id requested as string</param>
        /// <returns>Class type ItemReponse</returns>
        public ItemResponse GetDataSourceDefinitionById(ItemRequest itemRequest)
        {
            var apiResponse = new ItemResponse();

            byte[] datasourceByteValue = null;
            try
            {
                var datasourceId = itemRequest.ItemId;
                var datasourcedetails = _itemManagement.GetItemDetailsFromItemId(datasourceId);
                if (datasourcedetails != null)
                {
                    var datasourceDefinition = _itemManagement.GetDataSourceDefinition(datasourceId);
                    datasourceDefinition.Enabled = Convert.ToBoolean(datasourceDefinition.Enabled.ToString().ToLower());
                    var datasourceStream = new MemoryStream();
                    var dataSourceSerializer = new XmlSerializer(typeof(DataSourceDefinition));
                    var datasourceStreamWriter = new XmlTextWriter(datasourceStream, Encoding.UTF8);
                    dataSourceSerializer.Serialize(datasourceStreamWriter, datasourceDefinition);
                    datasourceStream = (MemoryStream)datasourceStreamWriter.BaseStream;
                    datasourceByteValue = datasourceStream.ToArray();
                    apiResponse = new ItemResponse
                    {
                        FileContent = datasourceByteValue,
                        Status = true,
                        ItemName = datasourcedetails.Name
                    };
                    return apiResponse;
                }
                else
                {
                    apiResponse = new ItemResponse
                    {
                        StatusMessage = "Data source does not exist in database",
                        Status = false
                    };
                    return apiResponse;
                }

            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while downloading data source from report designer", e, MethodBase.GetCurrentMethod(), " DatasourceId - " + itemRequest.ItemId);
                var response = new ItemResponse
                {
                    StatusMessage = e.Message,
                    Status = false
                };
                return response;
            }

        }


        /// <summary>
        /// The method will get the data source location and convert the xml to bytes
        /// </summary>
        /// <param name="path">The path container the folder id and data source</param>
        /// <returns>Class type ItemResponse</returns>
        public ItemResponse DownloadDataSourceFileById(ItemRequest itemRequest)
        {
            var apiResponse = new ItemResponse ();
            try
            {
                var itemId = itemRequest.ItemId;
                var datasourcedetails = _itemManagement.GetItemDetailsFromItemId(itemId);
                if (datasourcedetails != null)
                {
                    var reportLocation = _itemManagement.GetItemLocation(datasourcedetails.Id,
                        ItemType.Datasource);
                    var passwordOfDataSource = _dataSourceManager.GetDataSourcePassword(datasourcedetails.Id);
                    if (String.IsNullOrWhiteSpace(passwordOfDataSource))
                    {
                        apiResponse = new ItemResponse
                        {
                            PublishedItemId = itemRequest.ItemId,
                            FileContent = File.ReadAllBytes(reportLocation),
                            Status = true,
                            ItemName = datasourcedetails.Name
                        };
                    }
                    else
                    {
                        var dataSourceDefinition = new DataSourceDefinition();
                        var stringwriter = new StringWriter();
                        var xmlSerializer = new XmlSerializer(typeof(DataSourceDefinition));
                        using (var reader = new StreamReader(reportLocation))
                        {
                            dataSourceDefinition = (DataSourceDefinition)xmlSerializer.Deserialize(reader);
                            reader.Close();
                        }
                        dataSourceDefinition.Password = passwordOfDataSource;
                        xmlSerializer.Serialize(stringwriter, dataSourceDefinition);
                        var xmlContent = stringwriter.ToString();
                        var bytes = new byte[xmlContent.Length * sizeof(char)];
                        Buffer.BlockCopy(xmlContent.ToCharArray(), 0, bytes, 0, bytes.Length);
                        apiResponse = new ItemResponse
                        {
                            PublishedItemId = itemRequest.ItemId,
                            FileContent = bytes,
                            Status = true,
                            ItemName = datasourcedetails.Name
                        };
                    }
                    return apiResponse;
                }
                else
                {
                    apiResponse = new ItemResponse
                    {
                        StatusMessage = "Data source does not exist in database",
                        Status = false
                    };
                    return apiResponse;
                }

            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while downloading data source from from report designer", e, MethodBase.GetCurrentMethod(), " DatasourceId - " + itemRequest.ItemId);
                var response = new ItemResponse
                {
                    StatusMessage = e.Message,
                    Status = false
                };
                return response;
            }

        }

        /// <summary>
        /// The method will get the file and convert to bytes as response
        /// </summary>
        /// <param name="itemRequest">Request object from report designer</param>
        /// <returns>Class type ItemResponse</returns>
        public ItemResponse DownloadFileById(ItemRequest itemRequest)
        {
            var apiResponse = new ItemResponse();
            try
            {
                var itemDetail = _itemManagement.GetItemDetailsFromItemId(itemRequest.ItemId);

                if (itemDetail != null)
                {
                    var itemLocation = _itemManagement.GetItemLocation(itemDetail.Id, ItemType.File);
                    apiResponse = new ItemResponse
                    {
                        PublishedItemId=itemRequest.ItemId,
                        Status = true,
                        FileContent = GetResponseBytes(itemLocation),
                        ItemName = itemDetail.Name
                    };
                }
                else
                {
                    apiResponse.Status = false;
                    apiResponse.StatusMessage = "Invalid ItemId";
                    LogExtension.LogInfo("Error while reading file from Grout API", MethodBase.GetCurrentMethod(), " ItemId - " + itemRequest.ItemId);
                    return apiResponse;
                }
            }
            catch (Exception e)
            {
                apiResponse.Status = false;
                apiResponse.StatusMessage = "File not found";
                LogExtension.LogInfo("Error while reading file from Grout API", MethodBase.GetCurrentMethod(), " ItemId - " + itemRequest.ItemId);
                return apiResponse;
            }
            return apiResponse;
        }

        /// <summary>
        /// The method will get the data source location and convert the xml to bytes
        /// </summary>
        /// <param name="path">The path container the folder id and data source</param>
        /// <returns>Class type ItemResponse</returns>
        public ItemResponse DownloadDataSourceFileByName(ItemRequest itemRequest)
        {
            var apiResponse = new ItemResponse();

            try
            {
                var folderId = itemRequest.CategoryId;
                var datasourceName = itemRequest.DatasourceDetails.Name;
                var datasourcedetails = _itemManagement.FinddatasourceByDatasourceName(folderId, datasourceName);
                if (datasourcedetails != null)
                {
                    var reportLocation = _itemManagement.GetItemLocation(datasourcedetails.DataSourceId,
                        ItemType.Datasource);
                    apiResponse = new ItemResponse
                    {
                        FileContent = File.ReadAllBytes(reportLocation),
                        Status = true,
                        ItemName = datasourcedetails.Name
                    };
                    return apiResponse;
                }
                else
                {
                    apiResponse = new ItemResponse
                    {
                        StatusMessage = "Data source does not exist in database",
                        Status = false
                    };
                    return apiResponse;
                }

            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while downloading data source from from report designer", e, MethodBase.GetCurrentMethod(), " FolderId - " + itemRequest.CategoryId + " DatasourceName - " + itemRequest.DatasourceDetails.Name);
                apiResponse = new ItemResponse
                {
                    StatusMessage = e.Message,
                    Status = false
                };
                return apiResponse;
            }

        }


    }
}