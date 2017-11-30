using Newtonsoft.Json;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Encryption;
using Grout.UMP.Models;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using Grout.Base.Logger;
using Grout.Base.Utilities;

namespace Grout.UMP.Controllers
{
    public class SystemStartUpPageController : Controller
    {
        readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();
        readonly TokenCryptography _tokenCryptography = new TokenCryptography();

        public ActionResult Startup()
        {
            FormsAuthentication.SignOut();
            var settings = new SystemSettingsSerializer().Deserialize(GlobalAppSettings.GetConfigFilepath() + ServerSetup.Configuration);
            if (settings != null)
            {
                return new RedirectResult("/reports");
            }
            var listTimeZone = TimeZoneInfo.GetSystemTimeZones().ToList();
            ViewBag.listTimeZone = listTimeZone;
            if (Request.Url != null)
                ViewBag.DefaultUrl = Request.Url.AbsoluteUri.Replace(Request.Url.AbsolutePath, "").Replace("http://", "");
            return View("System");
        }

        public ActionResult SystemRedirectToSystemSettings()
        {
            return RedirectToAction("System", "SystemStartUpPage");
        }

        [HttpPost]
        public void SetSystemSettings(string globalAdminDetails, string systemSettingsData)
        {
            LogExtension.LogInfo("Initializes system settings at first time", MethodBase.GetCurrentMethod());
            var systemSettingsProperties = JsonConvert.DeserializeObject<SystemSettings>(systemSettingsData);
            var globalAdminDetail = JsonConvert.DeserializeObject<User>(globalAdminDetails);

            systemSettingsProperties.SqlConfiguration.AuthenticationType = 0;

            GlobalAppSettings.DbSupport = systemSettingsProperties.SqlConfiguration.ServerType;

            var systemSettingsModel = new SystemStartUpPageModel();
            SystemStartUpPageModel.SetSystemProperties(systemSettingsProperties);
            Connection.ConnectionString = _tokenCryptography.Decrypt(systemSettingsProperties.SqlConfiguration.ConnectionString);

            var systemSettings = new SystemSettings
            {
                OrganizationName = "Grout",
                WelcomeNoteText = "Welcome to Grout",
                ActivationExpirationDays = 3,
                DateFormat = "MM/dd/yyyy",
                FavIcon = "Grout_Favicon.png",
                LoginLogo = "Grout_Login_Logo.png",
                MainScreenLogo = "Grout_Main_Logo.png",
                ReportCount = 20,
                TimeZone = TimeZoneInfo.Local.Id,
                Language = "en-US",
                BaseUrl = new UriBuilder(HttpContext.Request.Url.Scheme, HttpContext.Request.Url.Host, HttpContext.Request.Url.Port).ToString().TrimEnd('/')
            };

            //Insert System Settings
            systemSettingsModel.InsertSystemSettings(systemSettings, _tokenCryptography.Decrypt(systemSettingsProperties.SqlConfiguration.ConnectionString));

            //Initialize System Settings
            GlobalAppSettings.InitializeSystemSettings(GlobalAppSettings.GetConfigFilepath() + ServerSetup.Configuration);

            //Add System Administrator
            SystemStartUpPageModel.AddSystemAdmin(globalAdminDetail.UserName.ToLower(), CultureInfo.CurrentCulture.TextInfo.ToTitleCase(globalAdminDetail.FirstName), CultureInfo.CurrentCulture.TextInfo.ToTitleCase(globalAdminDetail.LastName),
                globalAdminDetail.Email.ToLower(), globalAdminDetail.Password);
            
            DatabaseSchemaUpdater.UpdateLaterversionToServerVersion(DatabaseSchemaUpdater.GetLatestVersion());
            GlobalAppSettings.IsLatestVersion = false;

            //Add Sample Reports
            SystemStartUpPageModel.InsertSampleReports();
            
        }

        [HttpPost]
        public JsonResult ConnectDatabase(string data)
        {
            LogExtension.LogInfo("Testing connection info of provided SQL server", MethodBase.GetCurrentMethod());
            var databaseCredentials = JsonConvert.DeserializeObject<DataBaseConfiguration>(data);
            string connectionString;
            if (!databaseCredentials.IsWindowsAuthentication)
                connectionString = "Data Source=" + databaseCredentials.ServerName + ";user id=" +
                                    databaseCredentials.UserName + ";password=" + databaseCredentials.Password;
            else
                connectionString = "Server=" + databaseCredentials.ServerName + "; Integrated Security=yes;";
            var connect = JsonConvert.DeserializeObject<Dictionary<string, object>>(IsValidConnection(connectionString));
            if (!Convert.ToBoolean(connect["result"]))
            {
                var result = new { key = false, value = connect["Message"] };
                return Json(new { Data = result });
            }
            else
            {
                var result = new { key = true, value = "Connected successfully." };
                return Json(new { Data = result });
            }

        }

        [HttpPost]
        public JsonResult GenerateDatabase(string data)
        {
            LogExtension.LogInfo("Generating database", MethodBase.GetCurrentMethod());
            var i = 0;

            var databaseCredentials = JsonConvert.DeserializeObject<DataBaseConfiguration>(data);

            var isSql = databaseCredentials.ServerType.ToString();

            object result;
            if (String.Equals(isSql, "MSSQL", StringComparison.OrdinalIgnoreCase))
            {

                string connectionString;
                if (!databaseCredentials.IsWindowsAuthentication)
                {
                    connectionString =
                        "Data Source=" + databaseCredentials.ServerName + ";user id=" + databaseCredentials.UserName +
                        ";password=" +
                        databaseCredentials.Password;

                }
                else
                {
                    connectionString = "Server=" + databaseCredentials.ServerName + "; Integrated Security=yes;";

                }

                var sqldbScript = new FileInfo(AppDomain.CurrentDomain.BaseDirectory +
                                                WebConfigurationManager.AppSettings["SystemConfigurationPath"] +
                                                ServerSetup.SqlTables);

                var dbCreationScript = "USE [master]; CREATE DATABASE [" + databaseCredentials.DataBaseName + "];";

                var isDatabaseExist = CheckDatabaseExists(connectionString, databaseCredentials.DataBaseName);


                if (isDatabaseExist)
                {

                    var failResult = new { key = false, value = "Database name is already exist" };
                    return Json(new { Data = failResult });
                }

                var connection = new SqlConnection(connectionString);

                #region Create Database

                var isDatabaseCreated = false;

                try
                {
                    LogExtension.LogInfo("Creating database in SQL server", MethodBase.GetCurrentMethod());
                    var command = new SqlCommand(dbCreationScript, connection);
                    connection.Open();
                    command.ExecuteNonQuery();
                    isDatabaseCreated = true;
                }
                catch (SqlException ex)
                {
                    isDatabaseCreated = false;
                    LogExtension.LogInfo("Error in creating SQL Database", MethodBase.GetCurrentMethod());
                    LogExtension.LogError("Error in creating SQL Database", ex, MethodBase.GetCurrentMethod());
                    var failResult = new { key = false, value = ex.Message};
                    return Json(new { Data = failResult });
                }
                finally
                {
                    connection.Close();
                }
                LogExtension.LogInfo("Is database created?"+isDatabaseCreated.ToString(), MethodBase.GetCurrentMethod());
                #endregion

                if (isDatabaseCreated)
                {
                    var tabelCreationScript = "USE [" + databaseCredentials.DataBaseName + "]; " +
                                              sqldbScript.OpenText().ReadToEnd();

                    try
                    {
                        LogExtension.LogInfo("Creating database tables in SQL server", MethodBase.GetCurrentMethod());
                        var command = new SqlCommand(tabelCreationScript, connection);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        LogExtension.LogInfo("Error in creating SQL Database tables", MethodBase.GetCurrentMethod());
                        LogExtension.LogError("Error in creating SQL Database", ex, MethodBase.GetCurrentMethod());
                        var failResult = new { key = false, value = ex.Message };
                        return Json(new { Data = failResult });
                    }
                    finally
                    {
                        connection.Close();
                    }
                    LogExtension.LogInfo("SQL database tables created successfully.", MethodBase.GetCurrentMethod());

                    if (!databaseCredentials.IsWindowsAuthentication)
                    {
                        connectionString =
                            "Data Source=" + databaseCredentials.ServerName + ";Initial Catalog=" +
                            databaseCredentials.DataBaseName + ";user id=" + databaseCredentials.UserName + ";password=" +
                            databaseCredentials.Password;
                    }
                    else
                    {
                        connectionString = "Server=" + databaseCredentials.ServerName + ";Initial Catalog=" +
                                           databaseCredentials.DataBaseName + "; Integrated Security=yes;";
                    }
                }

                result = new { key = true, value = _tokenCryptography.DoEncryption(connectionString) };
            }
            else
            {

                var sqlcedbScript = new FileInfo(AppDomain.CurrentDomain.BaseDirectory +
                                                 WebConfigurationManager.AppSettings["SystemConfigurationPath"] +
                                                 ServerSetup.SqlTables);


                var appDataFolderPath = GlobalAppSettings.GetAppDataFolderPath();

                if (Directory.Exists(appDataFolderPath) == false)
                {
                    Directory.CreateDirectory(appDataFolderPath);
                }
                else
                {
                    Array.ForEach(Directory.GetFiles(appDataFolderPath), System.IO.File.Delete);
                }


                var connStr = "Data Source = " + appDataFolderPath + "ReportServer.sdf; Password = reportserver";

                using (var engine = new SqlCeEngine(connStr))
                {
                    LogExtension.LogInfo("Creating SQLCE database", MethodBase.GetCurrentMethod());
                    engine.CreateDatabase();
                }


                var script = sqlcedbScript.OpenText().ReadToEnd();

                SqlCeConnection conn = null;

                try
                {
                    conn = new SqlCeConnection(connStr);

                    conn.Open();

                    var cmd = conn.CreateCommand();

                    var splitter = new[] { ";" };

                    var commandTexts = script.Split(splitter, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string commandText in commandTexts)
                    {
                        cmd.CommandText = commandText;
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    LogExtension.LogInfo("Error in creating SQL CE Database", MethodBase.GetCurrentMethod());
                    LogExtension.LogError("Error in creating SQL CE Database", ex, MethodBase.GetCurrentMethod());
                }
                finally
                {
                    if (conn != null) conn.Close();
                }

                result =
                    new
                    {
                        key = true,
                        value = _tokenCryptography.DoEncryption(connStr)
                    };
            }

            return Json(new { Data = result });
        }

        private string IsValidConnection(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    LogExtension.LogInfo("SQL server connected successfully", MethodBase.GetCurrentMethod());
                    return _serializer.Serialize(new { result = true, Message = "Success" });
                }
                catch (SqlException ex)
                {
                    LogExtension.LogInfo("Invalid connection properties", MethodBase.GetCurrentMethod());
                    LogExtension.LogError("Error in checking wheteher a valid conection exists", ex,
                        MethodBase.GetCurrentMethod());
                    var outMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    return _serializer.Serialize(new { result = false, Message = outMessage });
                }
            }
        }

        private bool CheckDatabaseExists(string connectionString, string databaseName)
        {
            LogExtension.LogInfo("Testing database existance of SQL server", MethodBase.GetCurrentMethod());
            bool result;
            try
            {
                var tmpConn = new SqlConnection(connectionString);

                var sqlCreateDbQuery = string.Format("SELECT database_id FROM sys.databases WHERE Name = '{0}'",
                    databaseName);

                using (tmpConn)
                {
                    using (var sqlCmd = new SqlCommand(sqlCreateDbQuery, tmpConn))
                    {
                        tmpConn.Open();
                        var databaseId = (int) sqlCmd.ExecuteScalar();
                        tmpConn.Close();

                        result = (databaseId > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError("Error in checking whether a database exists", ex, MethodBase.GetCurrentMethod());
                result = false;
            }
            LogExtension.LogInfo("Is database with the name exists?:" + result.ToString(), MethodBase.GetCurrentMethod());
            return result;
        }
    }
}