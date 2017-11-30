using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AnalysisServices.AdomdClient;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Encryption;
using Grout.Base.Item;

namespace Grout.UMP.Models
{
    public class DataSourceManager
    {
        private readonly Item _item = new Item();
        private readonly IItemManagement _itemManagement = new ItemManagement();
        private readonly IUserManagement _userManagement = new UserManagement();
        private readonly IGroupManagement _groupManagement = new GroupManagement();
        private readonly TokenCryptography _tokenCryptography = new TokenCryptography();
        private readonly PermissionSet _permissionBase = new PermissionSet();

        /// <summary>
        /// This method will add rdl report with shared data sources and map the appropriate data sources with selected data sources
        /// </summary>
        /// <param name="file">Http request elements which is used to add report item in data base</param>
        /// <param name="temporaryFileName">File name of the saved file in temporary folder</param>
        /// <param name="selectedDataSources">List of selected folders for appropriate data sources of rdl</param>
        /// <returns>Status of operation</returns>
        public ItemResponse AddRdlReport(ItemDetail file, string temporaryFileName, List<DataSourceMappingInfo> selectedDataSources,int currentUserId)
        {
            var filePublishStatus = new ItemResponse();
            var publishedFileId = _item.AddItem(file.Name, file.Description, file.CategoryId,
                currentUserId, file.ItemType, file.Extension);
            filePublishStatus.PublishedItemId = publishedFileId;
            var itemName = file.Name + file.Extension;
            var rootPath =
                Path.Combine(GlobalAppSettings.GetItemsPath() + publishedFileId + "\\1\\");
            var modifiedDate = DateTime.UtcNow;
            var version = _itemManagement.SaveItemVersion(publishedFileId, currentUserId, 0, 1, itemName, string.Empty,
                file.ItemType, modifiedDate);
            _itemManagement.AddItemLog(Convert.ToInt32(version.Value.ToString()), ItemLogType.Added, currentUserId, publishedFileId, 0, null, null, modifiedDate);

            var temporaryDirectory = Path.Combine(GlobalAppSettings.GetItemsPath() + "Temporary_Files\\");

            if (Directory.Exists(temporaryDirectory) == false)
            {
                Directory.CreateDirectory(temporaryDirectory);
            }

            var temporaryXmlRootPath = Path.Combine(temporaryDirectory + temporaryFileName);

            try
            {
                var xmlDocument = new XmlDocument();
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
                            sharedDataSource = selectedDataSources.Where(r => r.Name == dataSourceName).First();
                            xmlChildLinkedNode.InnerText = sharedDataSource.DataSourceId.ToString();
                        }
                    }
                }
                xmlDocument.Save(temporaryXmlRootPath);

                if (!Directory.Exists(Path.Combine(rootPath)))
                    Directory.CreateDirectory(Path.Combine(rootPath));
                var extractedPath = Path.Combine(rootPath + file.Name + file.Extension);
                File.Copy(GlobalAppSettings.GetItemsPath() + "Temporary_Files\\" + temporaryFileName, extractedPath);
                File.Delete(GlobalAppSettings.GetItemsPath() + "Temporary_Files\\" + temporaryFileName);
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
            catch (Exception)
            {
                _itemManagement.UpdateItemStatus(publishedFileId.ToString(), false);
                filePublishStatus.Status = false;
                filePublishStatus.StatusMessage = "Error while adding file";
                return filePublishStatus;
            }
            filePublishStatus.Status = true;
            foreach (var dataSource in selectedDataSources)
            {
                var mapStatus = AddDataSourceWithReport(dataSource.Name, dataSource.DataSourceId.ToString(), publishedFileId.ToString());
                if (mapStatus)
                {
                    filePublishStatus.StatusMessage = "Error while adding data source";
                }
            }
            return filePublishStatus;
        }



        /// <summary>
        /// This method is used to add embedded rdl report in server
        /// </summary>
        /// <param name="file">Http request elements which is used to add report item in data base</param>
        /// <param name="temporaryFileName">File name of the saved file in temporary folder</param>
        /// <returns>Status of operation</returns>
        public ItemResponse AddEmbeddedRdlReport(ItemDetail file, string temporaryFileName,int currentUserId)
        {
            var filePublishStatus = new ItemResponse();
            var publishedFileId = _item.AddItem(file.Name, file.Description, file.CategoryId,
               currentUserId,file.ItemType, file.Extension);
            filePublishStatus.PublishedItemId = publishedFileId;
            var itemName = file.Name + file.Extension;
            var rootPath =
                Path.Combine(GlobalAppSettings.GetItemsPath() + publishedFileId + "\\1\\");

            var modifiedDate = DateTime.UtcNow;
            var versionId =
                Convert.ToInt32(
                    _itemManagement.SaveItemVersion(publishedFileId, currentUserId, 0, 1, itemName, string.Empty,
                        file.ItemType, modifiedDate).Value.ToString());
            _itemManagement.AddItemLog(versionId, ItemLogType.Added, currentUserId, publishedFileId, 0, null, null, modifiedDate);
            try
            {
                if (!Directory.Exists(Path.Combine(rootPath)))
                    Directory.CreateDirectory(Path.Combine(rootPath));
                var extractedPath = Path.Combine(rootPath + file.Name + file.Extension);
                File.Copy(GlobalAppSettings.GetItemsPath() + "Temporary_Files\\" + temporaryFileName, extractedPath);
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
            catch (Exception)
            {
                _itemManagement.UpdateItemStatus(publishedFileId.ToString(), false);
                filePublishStatus.Status = false;
                filePublishStatus.StatusMessage = "Error while adding file";
                return filePublishStatus;
            }
            filePublishStatus.Status = true;
            return filePublishStatus;
        }

        /// <summary>
        /// This method is used to upload the rdl file for data source processing. The uploaded file will be saved in temporary folder.
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public ReportUploadResponse UploadRdlFile(Stream fileStream,int currentUserId)
        {
            var reportUploadResponse = new ReportUploadResponse();
            var itemName = currentUserId + "_" + DateTime.UtcNow.ToString("ddMMyyyy_hhmmss") + ".rdl";
            var rootPath = Path.Combine(GlobalAppSettings.GetItemsPath() + "Temporary_Files\\");

            string extractedPath;
            try
            {

                if (!Directory.Exists(Path.Combine(rootPath)))
                    Directory.CreateDirectory(Path.Combine(rootPath));
                extractedPath = Path.Combine(rootPath + itemName);
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
            catch (Exception)
            {
                reportUploadResponse.Status = false;
                return reportUploadResponse;
            }
            reportUploadResponse.UploadedReportName = itemName;
            reportUploadResponse.Status = true;
            try
            {
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(extractedPath);
                var xnList = xmlDocument.GetElementsByTagName("DataSource");
                reportUploadResponse.IsShared = false;
                var sharedDataSource = new List<string>();
                var value = string.Empty;
                foreach (var item in xnList)
                {
                    var xmlLinkedNode = item as XmlLinkedNode;
                    foreach (var childNodes in xmlLinkedNode.ChildNodes)
                    {
                        var xmlChildLinkedNode = childNodes as XmlLinkedNode;
                        if (xmlChildLinkedNode.Name == "DataSourceReference")
                        {
                            value = xmlLinkedNode.Attributes["Name"].Value;
                            reportUploadResponse.IsShared = true;
                        }
                    }
                    if (value != string.Empty)
                    {
                        sharedDataSource.Add(value);
                        value = string.Empty;
                    }
                }
                if (reportUploadResponse.IsShared)
                {
                    reportUploadResponse.SharedDataSourceList = sharedDataSource;
                }
            }
            catch
            {
                return reportUploadResponse;
            }
            return reportUploadResponse;
        }

        /// <summary>
        /// If the popup is closed without any changes the uploaded file will be deleted from temporary files
        /// </summary>
        /// <param name="fileName">Name of the target file</param>
        public void DeleteTemporaryFile(string fileName)
        {
            try
            {
                File.Delete(GlobalAppSettings.GetItemsPath() + "Temporary_Files\\" + fileName);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// This method is used to give all the accessible data sources of the user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<ItemDetail> GetAllDataSourceListOfUser(int userId)
        {
            var dataSourceList = _itemManagement.GetItems(userId, ItemType.Datasource,null,null,string.Empty,0,0);
            return dataSourceList.result.ToList();
        }

        
        /// <summary>
        /// This method is used to add data source in server
        /// </summary>
        /// <param name="file">Request elements of the uploaded datasources</param>
        /// <param name="dataSourceDefinition">Datasource definition for the data source</param>
        /// <returns>Status of the operation</returns>
        public DataSourceUploadStatus AddDataSource(ItemDetail file, DataSourceDefinition dataSourceDefinition,int currentUserId)
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus();
            try
            {
                var publishedFileId = _item.AddItem(file.Name, file.Description, file.CategoryId,
                    currentUserId, file.ItemType, ".rds");
                dataSourceUploadStatus.PublishedDataSourceId = publishedFileId.ToString();
                var itemName = file.Name + file.Extension;
                var rootPath =
                    Path.Combine(GlobalAppSettings.GetItemsPath() + publishedFileId + "\\1\\");
                var modifiedDate = DateTime.UtcNow;
                var addVersion = _itemManagement.SaveItemVersion(publishedFileId, currentUserId, 0, 1, itemName,
                    string.Empty,
                    file.ItemType, modifiedDate);

                if (!Directory.Exists(Path.Combine(rootPath)))
                {
                    Directory.CreateDirectory(Path.Combine(rootPath));
                }

                var extractedPath = Path.Combine(rootPath + file.Name + file.Extension);
                var xmlSerializer = new XmlSerializer(typeof(DataSourceDefinition));

                if (!string.IsNullOrEmpty(dataSourceDefinition.Password))
                {
                    var encryptedPassword = _tokenCryptography.DoEncryption(dataSourceDefinition.Password);
                    dataSourceDefinition.Password = null;
                    _itemManagement.AddPasswordForDataSource(publishedFileId.ToString(), encryptedPassword);
                }
                dataSourceDefinition.CredentialRetrieval = (dataSourceDefinition.CredentialRetrieval == CredentialRetrievalEnum.Store) ? CredentialRetrievalEnum.Prompt : dataSourceDefinition.CredentialRetrieval;
                using (var writer = new StreamWriter(extractedPath))
                {
                    xmlSerializer.Serialize(writer, dataSourceDefinition);
                    writer.Close();
                }

                _itemManagement.AddItemLog(Convert.ToInt32(addVersion.Value.ToString()), ItemLogType.Added,
                    currentUserId, publishedFileId, 0,null,null,modifiedDate);
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
                dataSourceUploadStatus.Message = ex.Message;
                return dataSourceUploadStatus;
            }
            dataSourceUploadStatus.Status = true;
            dataSourceUploadStatus.ConnectionStringStatus = true;
            return dataSourceUploadStatus;
        }

        /// <summary>
        /// This method is used to map data source of the rdl with selected data sources
        /// </summary>
        /// <param name="dataSourceName">Name of the data source provided in rdl file</param>
        /// <param name="dataSourceId">Seleceted data source for the rdl report</param>
        /// <param name="targetFileId">Item id of the uploaded rdl file</param>
        /// <param name="dataSourceIdentity">Source identity of the data source</param>
        /// <returns>Status of the operation</returns>
        public bool AddDataSourceWithReport(string dataSourceName, string dataSourceId, string targetFileId)
        {
            var result = _itemManagement.AddDataSourceWithRdl(targetFileId, dataSourceId, dataSourceName);
            return result;
        }

        /// <summary>
        ///  This will test the connection status for the provided details
        /// </summary>
        /// <param name="dataSourceDefinition">Request elements which is saved as object</param>
        /// <returns>Status of the operation</returns>
        public DataSourceUploadStatus OnTestDataSourceConnection(DataSourceDefinition dataSourceDefinition)
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus();
            string connectionString;
            switch (dataSourceDefinition.Extension)
            {
                case "SQL":
                    switch (dataSourceDefinition.CredentialRetrieval)
                    {
                        case CredentialRetrievalEnum.Integrated:
                            connectionString = dataSourceDefinition.ConnectString + "Integrated Security=SSPI;";
                            break;
                        case CredentialRetrievalEnum.Store:
                            connectionString = dataSourceDefinition.ConnectString + "user id=" + dataSourceDefinition.UserName + ";password=" + dataSourceDefinition.Password + ";Integrated Security=false;";
                            break;
                        default:
                            connectionString = dataSourceDefinition.ConnectString + ";Integrated Security=false;";
                            break;
                    }
                    dataSourceUploadStatus = IsValidSqlConnectionString(dataSourceDefinition.ConnectString);
                    if (dataSourceUploadStatus.ConnectionStringStatus)
                    {
                        dataSourceUploadStatus = IsValidSqlConnection(connectionString);
                    }
                    break;
                case "SQLCe":
                    if (dataSourceDefinition.CredentialRetrieval == CredentialRetrievalEnum.Store)
                    {
                        connectionString = dataSourceDefinition.ConnectString + "user id=" + dataSourceDefinition.UserName + ";password=" + dataSourceDefinition.Password + ";";
                    }
                    else
                    {
                        connectionString = dataSourceDefinition.ConnectString + ";";
                    }
                    dataSourceUploadStatus = IsValidSqlCeConnectionString(dataSourceDefinition.ConnectString);
                    if (dataSourceUploadStatus.ConnectionStringStatus)
                    {
                        dataSourceUploadStatus = IsValidSqlCeConnection(connectionString);
                    }
                    break;
                case "OLEDB":
                    switch (dataSourceDefinition.CredentialRetrieval)
                    {
                        case CredentialRetrievalEnum.Integrated:
                            connectionString = dataSourceDefinition.ConnectString + "Integrated Security=SSPI;";
                            break;
                        case CredentialRetrievalEnum.Store:
                            connectionString = dataSourceDefinition.ConnectString + "user id=" + dataSourceDefinition.UserName + ";password=" + dataSourceDefinition.Password + ";Integrated Security=false;";
                            break;
                        default:
                            connectionString = dataSourceDefinition.ConnectString + ";Integrated Security=false;";
                            break;
                    }
                    dataSourceUploadStatus = IsValidOledbConnectionString(dataSourceDefinition.ConnectString);
                    if (dataSourceUploadStatus.ConnectionStringStatus)
                    {
                        dataSourceUploadStatus = IsValidOledbConnection(connectionString);
                    }
                    break;
                case "ODBC":
                    if (dataSourceDefinition.CredentialRetrieval == CredentialRetrievalEnum.Store)
                    {
                        connectionString = dataSourceDefinition.ConnectString + "uid=" + dataSourceDefinition.UserName + ";pwd=" + dataSourceDefinition.Password + ";";
                    }
                    else
                    {
                        connectionString = dataSourceDefinition.ConnectString + ";";
                    }
                    dataSourceUploadStatus = IsValidOdbcConnectionString(dataSourceDefinition.ConnectString);
                    if (dataSourceUploadStatus.ConnectionStringStatus)
                    {
                        dataSourceUploadStatus = IsValidOdbcConnection(connectionString);
                    }
                    break;
                case "ORACLE":
                    switch (dataSourceDefinition.CredentialRetrieval)
                    {
                        case CredentialRetrievalEnum.Integrated:
                            connectionString = dataSourceDefinition.ConnectString + "Integrated Security=yes;";
                            break;
                        case CredentialRetrievalEnum.Store:
                            connectionString = dataSourceDefinition.ConnectString + "user id=" + dataSourceDefinition.UserName + ";password=" + dataSourceDefinition.Password + ";Integrated Security=false;";
                            break;
                        default:
                            connectionString = dataSourceDefinition.ConnectString + ";Integrated Security=false;";
                            break;
                    }
                    dataSourceUploadStatus = IsValidOracleConnectionString(dataSourceDefinition.ConnectString);
                    if (dataSourceUploadStatus.ConnectionStringStatus)
                    {
                        dataSourceUploadStatus = IsValidOracleConnection(connectionString);
                    }
                    break;
            }
            return dataSourceUploadStatus;
        }

        /// <summary>
        /// This method is used to test the connection string for the data source type and connection string
        /// </summary>
        /// <param name="dataSourceExtension">data source type</param>
        /// <param name="connectionString">connection string provided in form element</param>
        /// <returns>Status of the operation</returns>
        public DataSourceUploadStatus OnTestConnectionString(string dataSourceExtension, string connectionString)
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus();
            switch (dataSourceExtension)
            {
                case "SQL":
                    dataSourceUploadStatus = IsValidSqlConnectionString(connectionString);
                    break;
                case "SQLCe":
                    dataSourceUploadStatus = IsValidSqlCeConnectionString(connectionString);
                    break;
                case "OLEDB":
                    dataSourceUploadStatus = IsValidOledbConnectionString(connectionString);
                    break;
                case "ODBC":
                    dataSourceUploadStatus = IsValidOdbcConnectionString(connectionString);
                    break;
                case "ORACLE":
                    dataSourceUploadStatus = IsValidOracleConnectionString(connectionString);
                    break;
                case "XML":
                    dataSourceUploadStatus.ConnectionStringStatus = true;
                    break;
            }
            return dataSourceUploadStatus;
        }

        /// <summary>
        /// This method is used to test SQL Connection
        /// </summary>
        /// <param name="connectionString">connection string provided in form element</param>
        /// <returns>Status of the operation</returns>
        public DataSourceUploadStatus IsValidSqlConnection(string connectionString)
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus { ConnectionStringStatus = true };
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    dataSourceUploadStatus.Status = true;
                    dataSourceUploadStatus.Message = "Connection has been created successfully.";
                }
                catch (SqlException e)
                {
                    dataSourceUploadStatus.Status = false;
                    dataSourceUploadStatus.Message = e.Message;
                }
                finally
                {
                    connection.Close();
                }
                return dataSourceUploadStatus;
            }
        }

        /// <summary>
        /// This method is used to test SQLCE Connection
        /// </summary>
        /// <param name="connectionString">connection string provided in form element</param>
        /// <returns>Status of the operation</returns>
        public DataSourceUploadStatus IsValidSqlCeConnection(string connectionString)
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus { ConnectionStringStatus = true };
            using (var connection = new SqlCeConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    dataSourceUploadStatus.Status = true;
                    dataSourceUploadStatus.Message = "Connection has been created successfully.";
                }
                catch (SqlCeException e)
                {
                    dataSourceUploadStatus.Status = false;
                    dataSourceUploadStatus.Message = e.Message;
                }
                finally
                {
                    connection.Close();
                }
                return dataSourceUploadStatus;
            }
        }

        /// <summary>
        /// This method is used to test OLEDB Connection
        /// </summary>
        /// <param name="connectionString">connection string provided in form element</param>
        /// <returns>Status of the operation</returns>
        public DataSourceUploadStatus IsValidOledbConnection(string connectionString)
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus { ConnectionStringStatus = true };
            using (var connection = new OleDbConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    dataSourceUploadStatus.Status = true;
                    dataSourceUploadStatus.Message = "Connection has been created successfully";
                }
                catch (OleDbException e)
                {
                    dataSourceUploadStatus.Status = false;
                    dataSourceUploadStatus.Message = e.Message;
                }
                finally
                {
                    connection.Close();
                }
                return dataSourceUploadStatus;
            }
        }

        /// <summary>
        /// This method is used to test OBDC Connection
        /// </summary>
        /// <param name="connectionString">connection string provided in form element</param>
        /// <returns>Status of the operation</returns>
        public DataSourceUploadStatus IsValidOdbcConnection(string connectionString)
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus { ConnectionStringStatus = true };
            using (var connection = new OdbcConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    dataSourceUploadStatus.Status = true;
                    dataSourceUploadStatus.Message = "Connection has been created successfully.";
                }
                catch (OdbcException e)
                {
                    dataSourceUploadStatus.Status = false;
                    dataSourceUploadStatus.Message = e.Message;
                }
                finally
                {
                    connection.Close();
                }
                return dataSourceUploadStatus;
            }
        }

        /// <summary>
        /// This method is used to test SSAS Connection.
        /// </summary>
        /// <param name="connectionString">connection string provided in form element</param>
        /// <returns>Status of the operation</returns>
        //Currently this method is not used in Grout since the data source type is not available. This method will be used in future.
        public DataSourceUploadStatus IsValidSsasConnection(string connectionString)
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus { ConnectionStringStatus = true };
            using (var connection = new AdomdConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    dataSourceUploadStatus.Status = true;
                    dataSourceUploadStatus.Message = "Connection has been created successfully.";
                }
                catch (AdomdException e)
                {
                    dataSourceUploadStatus.Status = false;
                    dataSourceUploadStatus.Message = e.Message;
                }
                finally
                {
                    connection.Close();
                }
                return dataSourceUploadStatus;
            }
        }

        /// <summary>
        /// This method is used to test Oracle Connection
        /// </summary>
        /// <param name="connectionString">connection string provided in form element</param>
        /// <returns>Status of the operation</returns>
        public DataSourceUploadStatus IsValidOracleConnection(string connectionString)
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus { ConnectionStringStatus = true };
            using (var connection = new OracleConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    dataSourceUploadStatus.Status = true;
                    dataSourceUploadStatus.Message = "Connection has been created successfully.";
                }
                catch (OracleException e)
                {
                    dataSourceUploadStatus.Status = false;
                    dataSourceUploadStatus.Message = e.Message;
                }
                finally
                {
                    connection.Close();
                }
                return dataSourceUploadStatus;
            }
        }

        /// <summary>
        /// This method is used to test connection string for SQL Connection
        /// </summary>
        /// <param name="connectionString">connection string provided in form element</param>
        /// <returns>Status of the operation</returns>
        public DataSourceUploadStatus IsValidSqlConnectionString(string connectionString)
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus();
            try
            {
                var sqlConnectionStringBuilder =
                        new SqlConnectionStringBuilder(connectionString);
            }
            catch (Exception exception)
            {
                dataSourceUploadStatus.ConnectionStringStatus = false;
                dataSourceUploadStatus.Message = exception.Message;
                return dataSourceUploadStatus;
            }
            dataSourceUploadStatus.ConnectionStringStatus = true;
            return dataSourceUploadStatus;
        }

        /// <summary>
        /// This method is used to test connection string for SQLCE Connection
        /// </summary>
        /// <param name="connectionString">connection string provided in form element</param>
        /// <returns>Status of the operation</returns>
        public DataSourceUploadStatus IsValidSqlCeConnectionString(string connectionString)
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus();
            try
            {
                var sqlConnectionStringBuilder =
                        new SqlCeConnectionStringBuilder(connectionString);
            }
            catch (Exception exception)
            {
                dataSourceUploadStatus.ConnectionStringStatus = false;
                dataSourceUploadStatus.Message = exception.Message;
                return dataSourceUploadStatus;
            }
            dataSourceUploadStatus.ConnectionStringStatus = true;
            return dataSourceUploadStatus;
        }

        /// <summary>
        /// This method is used to test connection string for OLEDB Connection
        /// </summary>
        /// <param name="connectionString">connection string provided in form element</param>
        /// <returns>Status of the operation</returns>
        public DataSourceUploadStatus IsValidOledbConnectionString(string connectionString)
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus();
            try
            {
                var sqlConnectionStringBuilder =
                        new OleDbConnectionStringBuilder(connectionString);
            }
            catch (Exception exception)
            {
                dataSourceUploadStatus.ConnectionStringStatus = false;
                dataSourceUploadStatus.Message = exception.Message;
                return dataSourceUploadStatus;
            }
            dataSourceUploadStatus.ConnectionStringStatus = true;
            return dataSourceUploadStatus;
        }

        /// <summary>
        /// This method is used to test connection string for OBDC Connection
        /// </summary>
        /// <param name="connectionString">connection string provided in form element</param>
        /// <returns>Status of the operation</returns>
        public DataSourceUploadStatus IsValidOdbcConnectionString(string connectionString)
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus();
            try
            {
                var sqlConnectionStringBuilder =
                        new OdbcConnectionStringBuilder(connectionString);
            }
            catch (Exception exception)
            {
                dataSourceUploadStatus.ConnectionStringStatus = false;
                dataSourceUploadStatus.Message = exception.Message;
                return dataSourceUploadStatus;
            }
            dataSourceUploadStatus.ConnectionStringStatus = true;
            return dataSourceUploadStatus;
        }

        /// <summary>
        /// This method is used to test connection string for Oracle Connection
        /// </summary>
        /// <param name="connectionString">connection string provided in form element</param>
        /// <returns>Status of the operation</returns>
        public DataSourceUploadStatus IsValidOracleConnectionString(string connectionString)
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus();
            try
            {
                var sqlConnectionStringBuilder =
                        new OracleConnectionStringBuilder(connectionString);
            }
            catch (Exception exception)
            {
                dataSourceUploadStatus.ConnectionStringStatus = false;
                dataSourceUploadStatus.Message = exception.Message;
                return dataSourceUploadStatus;
            }
            dataSourceUploadStatus.ConnectionStringStatus = true;
            return dataSourceUploadStatus;
        }

        /// <summary>
        /// This method is used to test connection string for SSAS Connection
        /// </summary>
        /// <param name="connectionString">connection string provided in form element</param>
        /// <returns>Status of the operation</returns>
        //Currently this method is not used in Grout since the data source type is not available. This method will be used in future.
        public DataSourceUploadStatus IsValidSsasConnectionString(string connectionString)
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus();
            var fields = connectionString.ToLower().Split(';').ToList();
            var keys = new List<string>();
            for (var field = 0; field < fields.Count(); field++)
            {
                if (fields[field] != "")
                {
                    keys.Add(fields[field].Split('=').ToList()[0]);
                }
            }
            var isContainMandateFields = (keys.Contains("data source") || keys.Contains("datasource")) && (keys.Contains("catalog") || keys.Contains("initial catalog"));
            if (isContainMandateFields)
            {
                var allPatterns = new string[] { 
                                    "data source",
                                    "datasource",
                                    "initical catalog",
                                    "cube",
                                    "effectiveusername",
                                    "encrypt password",
                                    "encryption password",
                                    "impersonation level",
                                    "Integrated security",
                                    "persist encrypted",
                                    "persist security info",
                                    "roles",
                                    "sspi",
                                    "use encryption for data",
                                    "user id",
                                    "password", 
                                    "catalog", 
                                    "protectionlevel",
                                    "application name",
                                    "autoSyncperiod",
                                    "character encoding",
                                    "comparecasesensitivestringflags",
                                    "compression level",
                                    "connect timeout",
                                    "mdx compatibility",
                                    "mdx missing member mode",
                                    "mode",
                                    "optimize response",
                                    "packet size",
                                    "protocol format",
                                    "real time olap",
                                    "safety options",
                                    "sqlquerymode",
                                    "timeout",
                                    "transport compression",
                                    "useexistingfile",
                                    "visualmode"
                               }.ToList();
                var inValidFields = keys.Except(allPatterns).ToList();
                if (inValidFields.Count != 0)
                {
                    dataSourceUploadStatus.ConnectionStringStatus = false;
                    dataSourceUploadStatus.Message = "Keyword not supported: ";
                    for (var inValidField = 0; inValidField < inValidFields.Count; inValidField++)
                    {

                        dataSourceUploadStatus.Message += inValidFields[inValidField];
                        if (inValidField != inValidFields.Count - 1)
                        {
                            dataSourceUploadStatus.Message += ", ";
                        }
                    }
                }
                else
                {
                    dataSourceUploadStatus.ConnectionStringStatus = true;
                }
            }
            else
            {
                dataSourceUploadStatus.ConnectionStringStatus = false;
                dataSourceUploadStatus.Message = "Connection string should contain data source and catalog fields";
            }
            return dataSourceUploadStatus;
        }

        /// <summary>
        /// This method is used to test connection string for SharePointList Connection
        /// </summary>
        /// <param name="connectionString">connection string provided in form element</param>
        /// <returns>Status of the operation</returns>
        //Currently this method is not used in Grout since the data source type is not available. This method will be used in future.
        public DataSourceUploadStatus IsValidSharePointListConnectionString(string connectionString)
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus();
            var fields = connectionString.ToLower().Split(';').ToList();
            var keys = new List<string>();
            for (var field = 0; field < fields.Count(); field++)
            {
                if (fields[field] != "")
                {
                    keys.Add(fields[field].Split('=').ToList()[0]);
                }
            }
            var isContainMandateFields = keys.Contains("server") && keys.Contains("database") && keys.Contains("domain");
            if (isContainMandateFields)
            {
                var allPatterns = new string[] { 
                                    "source",
                                    "database",
                                    "domain",
                                    "user",
                                    "password",
                                    "ssl",
                                    "authentication"
                               }.ToList();
                var inValidFields = keys.Except(allPatterns).ToList();
                if (inValidFields.Count != 0)
                {
                    dataSourceUploadStatus.ConnectionStringStatus = false;
                    dataSourceUploadStatus.Message = "Keyword not supported: ";
                    for (var inValidField = 0; inValidField < inValidFields.Count; inValidField++)
                    {

                        dataSourceUploadStatus.Message += inValidFields[inValidField];
                        if (inValidField != inValidFields.Count - 1)
                        {
                            dataSourceUploadStatus.Message += ", ";
                        }
                    }
                }
                else
                {
                    dataSourceUploadStatus.ConnectionStringStatus = true;
                    dataSourceUploadStatus.Status = true;
                    dataSourceUploadStatus.Message = "Connection has been created successfully.";
                }
            }
            else
            {
                dataSourceUploadStatus.ConnectionStringStatus = false;
                dataSourceUploadStatus.Message = "Connection string should contain server, database and domain fields";
            }
            return dataSourceUploadStatus;
        }

        /// <summary>
        /// This method is used to test connection string for SAPW Connection
        /// </summary>
        /// <param name="connectionString">connection string provided in form element</param>
        /// <returns>Status of the operation</returns>
        //Currently this method is not used in Grout since the data source type is not available. This method will be used in future.
        public DataSourceUploadStatus IsValidSapbwConnectionString(string connectionString)
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus();
            var fields = connectionString.ToLower().Split(';').ToList();
            var keys = new List<string>();
            for (var field = 0; field < fields.Count(); field++)
            {
                if (fields[field] != "")
                {
                    keys.Add(fields[field].Split('=').ToList()[0]);
                }
            }
            bool isContainMandateFields = keys.Contains("datasource");
            if (isContainMandateFields)
            {
                var allPatterns = new string[] { 
                                    "datasource"
                               }.ToList();
                var inValidFields = keys.Except(allPatterns).ToList();
                if (inValidFields.Count != 0)
                {
                    dataSourceUploadStatus.ConnectionStringStatus = false;
                    dataSourceUploadStatus.Message = "Keyword not supported: ";
                    for (var inValidField = 0; inValidField < inValidFields.Count; inValidField++)
                    {
                        dataSourceUploadStatus.Message += inValidFields[inValidField];
                        if (inValidField != inValidFields.Count - 1)
                        {
                            dataSourceUploadStatus.Message += ", ";
                        }
                    }
                }
                else
                {
                    dataSourceUploadStatus.ConnectionStringStatus = true;
                    dataSourceUploadStatus.Status = true;
                    dataSourceUploadStatus.Message = "Connection has been created successfully.";
                }
            }
            else
            {
                dataSourceUploadStatus.ConnectionStringStatus = false;
                dataSourceUploadStatus.Message = "Connection string should contain datasource";
            }
            return dataSourceUploadStatus;
        }

        /// <summary>
        /// This method is used to test connection string for ESSBASE Connection
        /// </summary>
        /// <param name="connectionString">connection string provided in form element</param>
        /// <returns>Status of the operation</returns>
        //Currently this method is not used in Grout since the data source type is not available. This method will be used in future.
        public DataSourceUploadStatus IsValidEssbaseConnectionString(string connectionString)
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus();
            var fields = connectionString.ToLower().Split(';').ToList();
            var keys = new List<string>();
            for (var field = 0; field < fields.Count(); field++)
            {
                if (fields[field] != "")
                {
                    keys.Add(fields[field].Split('=').ToList()[0]);
                }
            }
            bool isContainMandateFields = (keys.Contains("data source") || keys.Contains("datasource")) && (keys.Contains("catalog") || keys.Contains("initial catalog"));
            if (isContainMandateFields)
            {
                var allPatterns = new string[] { 
                                    "datasource",
                                    "data source",
                                    "catalog",
                                    "initial catalog",
                                    "datasourceinfo"
                               }.ToList();
                var inValidFields = keys.Except(allPatterns).ToList();
                if (inValidFields.Count != 0)
                {
                    dataSourceUploadStatus.ConnectionStringStatus = false;
                    dataSourceUploadStatus.Message = "Keyword not supported: ";
                    for (var inValidField = 0; inValidField < inValidFields.Count; inValidField++)
                    {

                        dataSourceUploadStatus.Message += inValidFields[inValidField];
                        if (inValidField != inValidFields.Count - 1)
                        {
                            dataSourceUploadStatus.Message += ", ";
                        }
                    }
                }
                else
                {
                    dataSourceUploadStatus.ConnectionStringStatus = true;
                    dataSourceUploadStatus.Status = true;
                    dataSourceUploadStatus.Message = "Connection has been created successfully.";
                }
            }
            else
            {
                dataSourceUploadStatus.ConnectionStringStatus = false;
                dataSourceUploadStatus.Message = "Connection string should contain datasource and initial catalog fields";
            }
            return dataSourceUploadStatus;
        }

        /// <summary>
        /// This method will return a password of the data source if the data source has passwrord
        /// </summary>
        /// <param name="dataSourceId">DataSourceId which is saved as an item id in database</param>
        /// <returns>Decrypted password</returns>
        public string GetDataSourcePassword(Guid dataSourceId)
        {
            try
            {
                var encryptedPassword = _itemManagement.GetDataSourcePassword(dataSourceId);
                if (String.IsNullOrWhiteSpace(encryptedPassword))
                {
                    return String.Empty;
                }
                var decryptedPasswrord = string.Empty;
                decryptedPasswrord = _tokenCryptography.Decrypt(encryptedPassword);
                return decryptedPasswrord;
            }
            catch(Exception e)
            {
                return String.Empty;
            }
            
        }


        public List<DataSourceInfo> GetDataSourceDetailsbyReportId(Guid reportId)
        {
            var cloneItemId = _itemManagement.GetCloneOfItem(reportId);
            if (cloneItemId != null)
                reportId = Guid.Parse(cloneItemId.ToString());
            return _itemManagement.GetReportDatasourceList(reportId);
        }

        public DataSourceDefinition GetDataSourceDefinition(Guid dataSourceId)
        {
            var dataSourceLocation = _itemManagement.GetItemLocation(dataSourceId, ItemType.Datasource);
            DataSourceDefinition dataSourceDefinition = null;
            var xmlSerializer = new XmlSerializer(typeof(DataSourceDefinition));
            if (File.Exists(dataSourceLocation))
            {
                using (var reader = new StreamReader(dataSourceLocation))
                {
                    dataSourceDefinition = (DataSourceDefinition)xmlSerializer.Deserialize(reader);
                    reader.Close();
                }
                dataSourceDefinition.Password = GetDataSourcePassword(dataSourceId);
                return dataSourceDefinition;
            }
            return dataSourceDefinition;
        }

        public DataSourceDefinition GetDataSourceDefinitionWithoutPassword(Guid dataSourceId)
        {
            var dataSourceLocation = _itemManagement.GetItemLocation(dataSourceId, ItemType.Datasource);
            DataSourceDefinition dataSourceDefinition = null;
            var xmlSerializer = new XmlSerializer(typeof(DataSourceDefinition));
            if (File.Exists(dataSourceLocation))
            {
                using (var reader = new StreamReader(dataSourceLocation))
                {
                    dataSourceDefinition = (DataSourceDefinition)xmlSerializer.Deserialize(reader);
                    reader.Close();
                }
                return dataSourceDefinition;
            }
            return dataSourceDefinition;
        }
    }
}