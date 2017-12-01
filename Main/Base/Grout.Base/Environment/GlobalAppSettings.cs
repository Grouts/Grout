using Grout.Base.Data;
using Grout.Base.DataClasses;
using Grout.Base.Encryption;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using Grout.Base.Logger;
using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;

namespace Grout.Base
{
    public class GlobalAppSettings
    {
        public GlobalAppSettings()
        {
            GroutColumns = new DB_Grout();
        }

        public static DataBaseType DbSupport { get; set; }

        public static string ConnectionString { get; set; }

        public static IQueryBuilder QueryBuilder { get; set; }

        public static IRelationalDataProvider DataProvider { get; set; }

        public static JavaScriptSerializer Serializer { get; set; }

        public static SystemSettings SystemSettings { get; set; }

        public static DateTime LocalSystemTime { get; set; }

        public static DB_SyncUMP DbColumns { get; set; }

        public static DB_Grout GroutColumns { get; set; }

        public static bool IsLatestVersion { get; set; }

        /// <summary>
        /// Get all the system setting properties from database.
        /// </summary>
        /// <returns></returns>
        public Result GetSystemSettings()
        {
            var result = new Result();
            try
            {
                result =
                    DataProvider.ExecuteReaderQuery(
                        QueryBuilder.SelectAllRecordsFromTable(DbColumns.DB_SystemSettings.DB_TableName));
            }
            catch (SqlException e)
            {
                LogExtension.LogError("Error while getting system settings properties", e, MethodBase.GetCurrentMethod());
                return result;
            }
            return result;
        }

        public static string GetDescription(Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            DescriptionAttribute attribute
                = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                    as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static string GetConfigFilepath()
        {
            return Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                    "\\Grout\\" + WebConfigurationManager.AppSettings["ApplicationName"] +
                                    "\\Configuration\\");
        }

        public static string GetItemsPath()
        {
            return
                Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                 "\\Grout\\" +
                                 WebConfigurationManager.AppSettings["ApplicationName"] +
                                 "\\Resources\\");
        }

        public static string GetApplicationImagesPath()
        {
            return Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                    "\\Grout\\" + WebConfigurationManager.AppSettings["ApplicationName"] +
                                    "\\Content\\Images\\Application\\");
        }

        public static string GetProfilePicturesPath()
        {
            return Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                    "\\Grout\\" + WebConfigurationManager.AppSettings["ApplicationName"] +
                                    "\\Content\\Images\\ProfilePictures\\");
        }

        public static string GetUploadedFilesPath()
        {
            return Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                    "\\Grout\\" + WebConfigurationManager.AppSettings["ApplicationName"] +
                                    "\\Content\\UploadedFiles\\");
        }

        public static string GetAppDataFolderPath()
        {
            return Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                    "\\Grout\\" + WebConfigurationManager.AppSettings["ApplicationName"] +
                                    "\\App_Data\\");
        }

        public static string GetSchedulerExportPath()
        {
            return Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                    "\\Grout\\" + WebConfigurationManager.AppSettings["ApplicationName"] +
                                    "\\SchedulerService\\");
        }

        /// <summary>
        /// Initialize the Settings for the Server.
        /// </summary>
        /// <param name="path">Path of the configuration file</param>
        public static void InitializeSystemSettings(string path)
        {
            var tokenCryptography = new TokenCryptography();
            try
            {
                SystemSettings = new SystemSettingsSerializer().Deserialize(path);
                if (SystemSettings != null)
                {
                    SystemSettings.SqlConfiguration.ConnectionString =
                        tokenCryptography.Decrypt(SystemSettings.SqlConfiguration.ConnectionString);
                    ConnectionString = SystemSettings.SqlConfiguration.ConnectionString;
                    DbSupport = SystemSettings.SqlConfiguration.ServerType;
                    if (DbSupport == DataBaseType.MSSQLCE)
                    {
                        DataProvider = new SqlCeRelationalDataAdapter(ConnectionString);
                        QueryBuilder = new SqlCeQueryBuilder();
                    }
                    else
                    {
                        QueryBuilder = new SqlQueryBuilder();
                        DataProvider = new SqlRelationalDataAdapter(ConnectionString);
                    }
                    var globalSettings = new GlobalAppSettings().GetSystemSettings().DataTable.AsEnumerable()
                        .Select(a => new
                        {
                            Key = a.Field<string>(DbColumns.DB_SystemSettings.Key),
                            Value = a.Field<string>(DbColumns.DB_SystemSettings.Value)
                        }
                        ).ToDictionary(a => a.Key, a => a.Value);

                    SystemSettings.OrganizationName =
                        globalSettings[SystemSettingKeys.OrganizationName.ToString()];
                    SystemSettings.LoginLogo = globalSettings[SystemSettingKeys.LoginLogo.ToString()];
                    SystemSettings.MainScreenLogo =
                        globalSettings[SystemSettingKeys.MainScreenLogo.ToString()];
                    SystemSettings.FavIcon = globalSettings[SystemSettingKeys.FavIcon.ToString()];
                    SystemSettings.WelcomeNoteText =
                        globalSettings[SystemSettingKeys.WelcomeNoteText.ToString()];
                    SystemSettings.Language = globalSettings[SystemSettingKeys.Language.ToString()];
                    SystemSettings.TimeZone = globalSettings[SystemSettingKeys.TimeZone.ToString()];
                    SystemSettings.DateFormat = globalSettings[SystemSettingKeys.DateFormat.ToString()];
                    SystemSettings.BaseUrl = globalSettings[SystemSettingKeys.BaseUrl.ToString()];
                    SystemSettings.ActivationExpirationDays =
                        Convert.ToInt32(globalSettings[SystemSettingKeys.ActivationExpirationDays.ToString()]);
                    SystemSettings.MailSettingsAddress =
                        globalSettings[SystemSettingKeys.MailSettingsAddress.ToString()];
                    SystemSettings.MailSettingsHost =
                        globalSettings[SystemSettingKeys.MailSettingsHost.ToString()];
                    SystemSettings.MailSettingsSenderName =
                        globalSettings[SystemSettingKeys.MailSettingsSenderName.ToString()];
                    SystemSettings.MailSettingsPassword =
                        tokenCryptography.Decrypt(
                            globalSettings[SystemSettingKeys.MailSettingsPassword.ToString()]);
                    SystemSettings.MailSettingsPort =
                        Convert.ToInt32(globalSettings[SystemSettingKeys.MailSettingsPort.ToString()]);
                    SystemSettings.MailSettingsIsSecureAuthentication =
                        Convert.ToBoolean(
                            globalSettings[SystemSettingKeys.MailSettingsIsSecureAuthentication.ToString()]);
                    Serializer = new JavaScriptSerializer();
                }
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception while deserializing system settings", e, MethodBase.GetCurrentMethod(),
                    " Path - " + path + " ConnectionString - " + ConnectionString + " OrganizationName - " +
                    SystemSettings.OrganizationName + " LoginLogo - " + SystemSettings.LoginLogo + " MainScreenLogo - " +
                    SystemSettings.MainScreenLogo + " FavIcon - " + SystemSettings.FavIcon + " WelcomeNoteText - " +
                    SystemSettings.WelcomeNoteText + " Language - " + SystemSettings.Language + " TimeZone - " +
                    SystemSettings.TimeZone + " DateFormat - " + SystemSettings.DateFormat + " BaseUrl - " +
                    SystemSettings.BaseUrl + " ActivationExpirationDays - " + SystemSettings.ActivationExpirationDays +
                    " MailSettingsAddress - " + SystemSettings.MailSettingsAddress + " MailSettingsHost - " +
                    SystemSettings.MailSettingsHost + " MailSettingsSenderName - " +
                    SystemSettings.MailSettingsSenderName + " MailSettingsPassword - " +
                    SystemSettings.MailSettingsPassword + " MailSettingsPort - " + SystemSettings.MailSettingsPort +
                    " MailSettingsIsSecureAuthentication - " + SystemSettings.MailSettingsIsSecureAuthentication);
            }
        }

        /// <summary>
        /// Checks whether the given user id is an admin or not
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>True if the user is an admin</returns>
        public static bool IsAdmin(int userId)
        {
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = DbColumns.DB_User.Id,
                    TableName = DbColumns.DB_User.DB_TableName,
                    Value = userId,
                    Condition = Conditions.Equals
                },
                new ConditionColumn
                {
                    ColumnName = DbColumns.DB_User.IsActive,
                    TableName = DbColumns.DB_User.DB_TableName,
                    Value = true,
                    LogicalOperator = LogicalOperators.AND,
                    Condition = Conditions.Equals
                },
                new ConditionColumn
                {
                    ColumnName = DbColumns.DB_Group.Id,
                    TableName = DbColumns.DB_Group.DB_TableName,
                    Value = 1,
                    LogicalOperator = LogicalOperators.AND,
                    Condition = Conditions.Equals
                },
                new ConditionColumn
                {
                    ColumnName = DbColumns.DB_Group.IsActive,
                    TableName = DbColumns.DB_Group.DB_TableName,
                    Value = true,
                    LogicalOperator = LogicalOperators.AND,
                    Condition = Conditions.Equals
                },
                new ConditionColumn
                {
                    ColumnName = DbColumns.DB_UserGroup.IsActive,
                    TableName = DbColumns.DB_UserGroup.DB_TableName,
                    Value = true,
                    LogicalOperator = LogicalOperators.AND,
                    Condition = Conditions.Equals
                }
            };

            var joinSpecificationList = new List<JoinSpecification>
            {
                new JoinSpecification
                {
                    Table = DbColumns.DB_UserGroup.DB_TableName,
                    Column =
                        new List<JoinColumn>
                        {
                            new JoinColumn
                            {
                                TableName = DbColumns.DB_User.DB_TableName,
                                JoinedColumn = DbColumns.DB_User.Id,
                                Operation = Conditions.Equals,
                                ParentTableColumn = DbColumns.DB_UserGroup.UserId,
                                ParentTable = DbColumns.DB_UserGroup.DB_TableName
                            }
                        },
                    JoinType = JoinTypes.Inner
                },
                new JoinSpecification
                {
                    Table = DbColumns.DB_Group.DB_TableName,
                    Column =
                        new List<JoinColumn>
                        {
                            new JoinColumn
                            {
                                TableName = DbColumns.DB_UserGroup.DB_TableName,
                                JoinedColumn = DbColumns.DB_UserGroup.GroupId,
                                Operation = Conditions.Equals,
                                ParentTableColumn = DbColumns.DB_Group.Id,
                                ParentTable = DbColumns.DB_Group.DB_TableName
                            }
                        },
                    JoinType = JoinTypes.Inner
                }
            };

            var selectColumn = new List<SelectedColumn>
            {
                new SelectedColumn
                {
                    TableName = DbColumns.DB_User.DB_TableName,
                    ColumnName = DbColumns.DB_User.Id
                }
            };

            var result =
                DataProvider.ExecuteReaderQuery(
                    QueryBuilder.ApplyWhereClause(
                        QueryBuilder.ApplyMultipleJoins(DbColumns.DB_User.DB_TableName, selectColumn,
                            joinSpecificationList), whereColumns));

            return result.DataTable.Rows.Count > 0;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public static void InitializeDataBaseColumns(string path)
        {
            DbColumns = new DbColumnDeSerializer().DeserializeTables(path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static DateTime GetGlobalTime()
        {
            var timeUtc = DateTime.UtcNow;
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById(SystemSettings.TimeZone);
            return TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toAddress"></param>
        /// <param name="body"></param>
        /// <param name="subject"></param>
        public static void SendCustomMail(string toAddress, string body, string subject)
        {
            new MailSender().SendEmail(toAddress, subject, body);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="timeZone"></param>
        /// <returns></returns>
        public static DateTime GetCovertedTimeZone(DateTime dateTime, string timeZone)
        {
            if (String.IsNullOrWhiteSpace(timeZone) == false && timeZone != SystemSettings.TimeZone)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime, TimeZoneInfo.FindSystemTimeZoneById(timeZone));
            }

            return TimeZoneInfo.ConvertTimeFromUtc(dateTime,
                TimeZoneInfo.FindSystemTimeZoneById(SystemSettings.TimeZone));
        }

        public static string GetFormattedTimeZone(DateTime dateTime, int userId)
        {
            var timeZone = new UserManagement().GetUserPreferTimeZone(userId);
            var convertedDate = GetCovertedTimeZone(dateTime, timeZone);
            var formattedString = convertedDate.ToString(SystemSettings.DateFormat + " hh:mm:ss tt");
            return formattedString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        public static void SetTimeZone(int userId = 0)
        {
            if (userId == 0 && !String.IsNullOrWhiteSpace(HttpContext.Current.User.Identity.Name))
            {
                userId = Convert.ToInt32(HttpContext.Current.User.Identity.Name);
            }
            else if (userId == 0 && HttpContext.Current.Session != null && HttpContext.Current.Session["UserId"] != null)
            {
                userId = Convert.ToInt32(HttpContext.Current.Session["UserId"]);
            }

            var timeZone = new UserManagement().GetUserPreferTimeZone(userId);

            if (HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session["TimeZone"] = !String.IsNullOrWhiteSpace(timeZone)
                    ? timeZone
                    : SystemSettings.TimeZone;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetSessionTimeZone()
        {
            SetTimeZone();

            return HttpContext.Current.Session["TimeZone"].ToString();
        }


        public static string GetDateTimeFormat()
        {
            if (DbSupport == DataBaseType.MSSQLCE)
            {
                return "yyyy/MM/dd HH:mm:ss";
            }

            var format = "";

            var result =
                DataProvider.ExecuteReaderQuery("dbcc useroptions");

            if (result != null && result.DataTable != null && result.DataTable.Rows.Count > 0)
            {
                var dbFormat = result.DataTable.AsEnumerable()
                                    .Select(a => new
                                    {
                                        Key = a.Field<string>("Set Option"),
                                        Value = a.Field<string>("Value")
                                    }).ToDictionary(a => a.Key, a => a.Value)["dateformat"];
               
                if (dbFormat == "mdy")
                {
                    format = "MM/dd/yyyy HH:mm:ss";
                }
                else if (dbFormat == "dmy")
                {
                    format = "dd/MM/yyyy HH:mm:ss";
                }
                else if (dbFormat == "ymd")
                {
                    format = "yyyy/MM/dd HH:mm:ss";
                }
            }
            return format;
        }
    }
}