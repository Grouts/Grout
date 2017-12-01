using Grout.Base;
using System;
using System.IO;
using Grout.Base.Data;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Grout.Base.DataClasses;
using Grout.Base.Logger;

namespace Grout.UMP.Models
{
    public class SystemStartUpPageModel
    {
        private readonly IRelationalDataProvider dataProvider;
        private readonly IQueryBuilder queryBuilder;

        public SystemStartUpPageModel()
        {
            if (GlobalAppSettings.DbSupport == DataBaseType.MSSQLCE)
            {
                dataProvider = new SqlCeRelationalDataAdapter(Connection.ConnectionString);
                queryBuilder = new SqlCeQueryBuilder();
            }
            else
            {
                dataProvider = new SqlRelationalDataAdapter(Connection.ConnectionString);
                queryBuilder = new SqlQueryBuilder();
            }
        }

        /// <summary>
        /// Serialize the System Settings Properties
        /// </summary>
        /// <param name="systemSettingsProperties"></param>
        public static void SetSystemProperties(SystemSettings systemSettingsProperties)
        {
            var serializer = new SystemSettingsSerializer();

            var configurationFolderPath = GlobalAppSettings.GetConfigFilepath();

            if (Directory.Exists(Path.GetFullPath(configurationFolderPath)) == false)
            {
                Directory.CreateDirectory(Path.GetFullPath(configurationFolderPath));
            }

            serializer.Serialize(systemSettingsProperties, configurationFolderPath + ServerSetup.Configuration);
        }

        /// <summary>
        /// Add System Admin 
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="firstName">Full name of user</param>
        /// <param name="lastName">Last name of user</param>
        /// <param name="emailId">Email Id</param>
        /// <param name="password">Password</param>
        public static void AddSystemAdmin(string userName, string firstName, string lastName, string emailId,
            string password)
        {
            LogExtension.LogInfo("Creating system admin",
                MethodBase.GetCurrentMethod());
            var encrypt = new Cryptography();
            var umpUser = new User();
            var userManagement = new UserManagement(GlobalAppSettings.QueryBuilder, GlobalAppSettings.DataProvider);
            var groupManagement = new GroupManagement(GlobalAppSettings.QueryBuilder, GlobalAppSettings.DataProvider);
            umpUser.Password = Convert.ToBase64String(encrypt.Encryption(password));
            umpUser.CreatedDate = DateTime.UtcNow;
            umpUser.ModifiedDate = DateTime.UtcNow;
            umpUser.IsActive = true;
            umpUser.IsDeleted = false;
            umpUser.ResetPasswordCode = "default";
            umpUser.ActivationCode = "default";
            umpUser.UserName = userName;
            umpUser.FirstName = firstName.Trim();
            umpUser.LastName = lastName.Trim();
            umpUser.DisplayName = (umpUser.FirstName.Trim() + " " + umpUser.LastName.Trim()).Trim();
            umpUser.Email = emailId;
            umpUser.IsActivated = true;
            var activationCode = String.Empty;
            var activationExpirationDate = new DateTime();

            LogExtension.LogInfo("Adding user in user table", MethodBase.GetCurrentMethod());
            var result = userManagement.AddUser(umpUser, out activationExpirationDate, out activationCode);

            if (result.Status)
            {
                LogExtension.LogInfo("Adding user in user table succesful", MethodBase.GetCurrentMethod());
                LogExtension.LogInfo("Adding user in super admin group table", MethodBase.GetCurrentMethod());
                var userGroup = groupManagement.AddUserInGroup(Convert.ToInt32(result.ReturnValue), 1);
                LogExtension.LogInfo("Is user added in super admin?" + userGroup, MethodBase.GetCurrentMethod());

                //var permissionSet = new PermissionSet();

                //permissionSet.AddPermissionToGroup(new Permission
                //{
                //    PermissionAccess = PermissionAccess.Create,
                //    PermissionEntity = PermissionEntity.AllCategories,
                //    TargetId = 1
                //});

                //permissionSet.AddPermissionToGroup(new Permission
                //{
                //    PermissionAccess = PermissionAccess.Create,
                //    PermissionEntity = PermissionEntity.AllReports,
                //    TargetId = 1
                //});

                //permissionSet.AddPermissionToGroup(new Permission
                //{
                //    PermissionAccess = PermissionAccess.Create,
                //    PermissionEntity = PermissionEntity.AllSchedules,
                //    TargetId = 1
                //});

                //permissionSet.AddPermissionToGroup(new Permission
                //{
                //    PermissionAccess = PermissionAccess.Create,
                //    PermissionEntity = PermissionEntity.AllDataSources,
                //    TargetId = 1
                //});

                //permissionSet.AddPermissionToGroup(new Permission
                //{
                //    PermissionAccess = PermissionAccess.Create,
                //    PermissionEntity = PermissionEntity.AllFiles,
                //    TargetId = 1
                //});
            }
            else
            {
                LogExtension.LogInfo("Error in adding user in user table", MethodBase.GetCurrentMethod());
            }

        }

        /// <summary>
        /// Save all the system setting properties into the Systemsettings table
        /// </summary>
        /// <param name="data"></param>
        /// <param name="connectionString"></param>
        public void InsertSystemSettings(SystemSettings data, string connectionString)
        {
            var listValues = new List<Dictionary<string, object>>
            {
                {
                    new Dictionary<string, object>
                    {
                        {
                            GlobalAppSettings.DbColumns.DB_SystemSettings.Key,
                            EnumClass.SystemSettingKeys.OrganizationName.ToString()
                        },
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.Value, data.OrganizationName},
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.IsActive, true}
                    }
                },
                {
                    new Dictionary<string, object>
                    {
                        {
                            GlobalAppSettings.DbColumns.DB_SystemSettings.Key,
                            EnumClass.SystemSettingKeys.LoginLogo.ToString()
                        },
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.Value, data.LoginLogo},
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.IsActive, true}
                    }
                },
                {
                    new Dictionary<string, object>
                    {
                        {
                            GlobalAppSettings.DbColumns.DB_SystemSettings.Key,
                            EnumClass.SystemSettingKeys.MainScreenLogo.ToString()
                        },
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.Value, data.MainScreenLogo},
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.IsActive, true}
                    }
                },
                {
                    new Dictionary<string, object>
                    {
                        {
                            GlobalAppSettings.DbColumns.DB_SystemSettings.Key,
                            EnumClass.SystemSettingKeys.FavIcon.ToString()
                        },
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.Value, data.FavIcon},
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.IsActive, true}
                    }
                },
                {
                    new Dictionary<string, object>
                    {
                        {
                            GlobalAppSettings.DbColumns.DB_SystemSettings.Key,
                            EnumClass.SystemSettingKeys.WelcomeNoteText.ToString()
                        },
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.Value,data.WelcomeNoteText},
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.IsActive, true}
                    }
                },
                {
                    new Dictionary<string, object>
                    {
                        {
                            GlobalAppSettings.DbColumns.DB_SystemSettings.Key,
                            EnumClass.SystemSettingKeys.Language.ToString()
                        },
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.Value, data.Language},
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.IsActive, true}
                    }
                },
                {
                    new Dictionary<string, object>
                    {
                        {
                            GlobalAppSettings.DbColumns.DB_SystemSettings.Key,
                            EnumClass.SystemSettingKeys.TimeZone.ToString()
                        },
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.Value, data.TimeZone},
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.IsActive, true}
                    }
                },
                {
                    new Dictionary<string, object>
                    {
                        {
                            GlobalAppSettings.DbColumns.DB_SystemSettings.Key,
                            EnumClass.SystemSettingKeys.DateFormat.ToString()
                        },
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.Value, data.DateFormat},
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.IsActive, true}
                    }
                },
                {
                    new Dictionary<string, object>
                    {
                        {
                            GlobalAppSettings.DbColumns.DB_SystemSettings.Key,
                            EnumClass.SystemSettingKeys.ActivationExpirationDays.ToString()
                        },
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.Value, data.ActivationExpirationDays.ToString()},
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.IsActive, true}
                    }
                },
                {
                    new Dictionary<string, object>
                    {
                        {
                            GlobalAppSettings.DbColumns.DB_SystemSettings.Key,
                            EnumClass.SystemSettingKeys.ReportCount.ToString()
                        },
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.Value, data.ReportCount.ToString()},
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.IsActive, true}
                    }
                },
                {
                    new Dictionary<string, object>
                    {
                        {
                            GlobalAppSettings.DbColumns.DB_SystemSettings.Key,
                            EnumClass.SystemSettingKeys.MailSettingsAddress.ToString()
                        },
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.Value, String.Empty},
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.IsActive, true}
                    }
                },
                {
                    new Dictionary<string, object>
                    {
                        {
                            GlobalAppSettings.DbColumns.DB_SystemSettings.Key,
                            EnumClass.SystemSettingKeys.MailSettingsPassword.ToString()
                        },
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.Value, String.Empty},
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.IsActive, true}
                    }
                },
                {
                    new Dictionary<string, object>
                    {
                        {
                            GlobalAppSettings.DbColumns.DB_SystemSettings.Key,
                            EnumClass.SystemSettingKeys.MailSettingsHost.ToString()
                        },
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.Value, String.Empty},
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.IsActive, true}
                    }
                },
                {
                    new Dictionary<string, object>
                    {
                        {
                            GlobalAppSettings.DbColumns.DB_SystemSettings.Key,
                            EnumClass.SystemSettingKeys.MailSettingsSenderName.ToString()
                        },
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.Value, String.Empty},
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.IsActive, true}
                    }
                },
                {
                    new Dictionary<string, object>
                    {
                        {
                            GlobalAppSettings.DbColumns.DB_SystemSettings.Key,
                            EnumClass.SystemSettingKeys.MailSettingsPort.ToString()
                        },
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.Value, "0"},
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.IsActive, true}
                    }
                },
                {
                    new Dictionary<string, object>
                    {
                        {
                            GlobalAppSettings.DbColumns.DB_SystemSettings.Key,
                            EnumClass.SystemSettingKeys.MailSettingsIsSecureAuthentication.ToString()
                        },
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.Value, false},
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.IsActive, true}
                    }
                },
                {
                    new Dictionary<string, object>
                    {
                        {
                            GlobalAppSettings.DbColumns.DB_SystemSettings.Key,
                            EnumClass.SystemSettingKeys.BaseUrl.ToString()
                        },
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.Value, data.BaseUrl},
                        {GlobalAppSettings.DbColumns.DB_SystemSettings.IsActive, true}
                    }
                }
            };
            var query = String.Empty;
            for (var t = 0; t < listValues.Count; t++)
            {
                query += queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_SystemSettings.DB_TableName,
                    listValues[t]);
                if (t != listValues.Count - 1)
                    query += "; ";
            }

            dataProvider.ExecuteBulkQuery(query, connectionString);
        }

        public static void InsertSampleReports()
        {
            try
            {
                //var itemManagement = new ItemManagement();
                //var item = new Item();
                var userManagement = new UserManagement();

                var userDetail = userManagement.FindUserByUserId(1);
                var baseUrl = new UriBuilder(HttpContext.Current.Request.Url.Scheme,
                    HttpContext.Current.Request.Url.Host, HttpContext.Current.Request.Url.Port).ToString();
                var dataSourceId = Guid.Empty;

                //#region AddCategory

                //var category = new ItemDetail()
                //{
                //    Name = "Sample Reports",
                //    Description = "Check our sample reports in this category",
                //    CreatedById = userDetail.UserId,
                //    CreatedDate = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat()),
                //    ItemType = ItemType.Category,
                //    ModifiedById = userDetail.UserId,
                //    ModifiedDate = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                //};
                //item.AddNewCategory(category);

                //#endregion

                //#region Add Data Sources

                //List<FileInfo> dataSourceList =
                //   new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\SampleReports").GetFiles("*.rds").ToList();

                //dataSourceList = dataSourceList.OrderByDescending(o => o.Name).ToList();

                

                //foreach (var dataSource in dataSourceList)
                //{
                //    DataSourceDefinition dataSourceDefinition;
                //    var xmlSerializer = new XmlSerializer(typeof(DataSourceDefinition));
                //    using (var reader = new StreamReader(dataSource.FullName))
                //    {
                //        dataSourceDefinition = (DataSourceDefinition)xmlSerializer.Deserialize(reader);
                //        reader.Close();
                //    }
                //    var itemRequest = new ItemRequest
                //    {
                //        Description = "This is a sample data source.",
                //        DataSourceDefinition = dataSourceDefinition,
                //        ItemType = ItemType.Datasource,
                //        Name = Path.GetFileNameWithoutExtension(dataSource.Name),
                //        UserName = userDetail.UserName,
                //        Password = userDetail.Password
                //    };

                //    using (var webclient = new WebClient())
                //    {
                //        var serializer = new DataContractJsonSerializer(typeof(ItemRequest));
                //        var memoryStream = new MemoryStream();

                //        serializer.WriteObject(memoryStream, itemRequest);

                //        var data = Encoding.UTF8.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Length);

                //        webclient.Headers["Content-type"] = "application/json";
                //        webclient.Encoding = Encoding.UTF8;

                //        var apiResult = webclient.UploadString(baseUrl.TrimEnd('/') + "/api/reportserverapi/add-data-source", "POST", data);

                //        var itemResponse = JsonConvert.DeserializeObject<ItemResponse>(apiResult);
                //        dataSourceId = itemResponse.PublishedItemId;
                //    }
                //}

                //#endregion

                //#region AddReports

                //var temporaryDirectory = Path.Combine(GlobalAppSettings.GetItemsPath() + "Temporary_Files");

                //if (Directory.Exists(temporaryDirectory) == false)
                //{
                //    Directory.CreateDirectory(temporaryDirectory);
                //}

                //List<FileInfo> reportList =
                //    new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "\\SampleReports").GetFiles("*.rdl").ToList();

                //reportList = reportList.OrderByDescending(o => o.Name).ToList();

                //foreach (var report in reportList)
                //{
                //    var xmlDocument = new XmlDocument();
                //    xmlDocument.Load(report.FullName);

                //    var dataSourceNodes = xmlDocument.GetElementsByTagName("DataSource");
                    
                //    foreach (var dataSourceNode in dataSourceNodes)
                //    {
                //        var xmlLinkedNode = dataSourceNode as XmlLinkedNode;
                //        foreach (var childNodes in xmlLinkedNode.ChildNodes)
                //        {
                //            var xmlChildLinkedNode = childNodes as XmlLinkedNode;
                //            if (xmlChildLinkedNode.Name == "DataSourceReference")
                //            {
                //                xmlChildLinkedNode.InnerText = dataSourceId.ToString();
                //            }
                //        }
                //    }
                //    var tempReportName = temporaryDirectory + "\\" + report.Name;
                //    xmlDocument.Save(tempReportName);

                //    var itemRequest = new ItemRequest
                //    {
                //        CategoryId = itemManagement.GetItemDetailsFromItemName(category.Name, ItemType.Category).Id,
                //        DataSourceMappingInfo = new List<DataSourceMappingInfo>
                //        {
                //            new DataSourceMappingInfo
                //            {
                //                DataSourceId = dataSourceId,
                //                Name = Path.GetFileNameWithoutExtension(dataSourceList.FirstOrDefault().Name)
                //            }
                //        },
                //        Description = "This is a sample report.",
                //        ItemContent = File.ReadAllBytes(tempReportName),
                //        ItemType = ItemType.Report,
                //        Name = Path.GetFileNameWithoutExtension(tempReportName),
                //        UserName = userDetail.UserName,
                //        Password = userDetail.Password
                //    };

                //    using (var webclient = new WebClient())
                //    {
                //        var serializer = new DataContractJsonSerializer(typeof(ItemRequest));
                //        var memoryStream = new MemoryStream();

                //        serializer.WriteObject(memoryStream, itemRequest);

                //        var data = Encoding.UTF8.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Length);

                //        webclient.Headers["Content-type"] = "application/json";
                //        webclient.Encoding = Encoding.UTF8;

                //        var apiResult = webclient.UploadString(baseUrl.TrimEnd('/') + "/api/reportserverapi/add-report", "POST", data);

                //        var itemResponse = JsonConvert.DeserializeObject<ItemResponse>(apiResult);
                //    }
                //}

                //LogExtension.LogInfo("Sample reports has been added successfully.", MethodBase.GetCurrentMethod());

                //#endregion
            }
            catch (Exception ex)
            {
                LogExtension.LogError("Error in adding sample reports.", ex, MethodBase.GetCurrentMethod());
            }
        }
    }
}