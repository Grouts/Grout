using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Data;
using System.Web.Configuration;
using System.Xml.Serialization;
using Grout.Base.DataClasses;
using Grout.Base.Logger;

namespace Grout.Base.Utilities
{
    public class DatabaseSchemaUpdater
    {
        public DatabaseSchemaUpdater()
        {
            string versionNumber = GetLatestVersion();
            var installedVersionNumber = GetReportServerVersionOfInstalledMachine();

            if (String.Compare(versionNumber, installedVersionNumber) != 1) return;

            var versionList = Deserialize();

            var scriptToRun = GetScriptToExecute(versionList.Version, installedVersionNumber);
            foreach (var script in scriptToRun)
            {
                var retrycount = 0;
            Retry:
                var status = RunScript(script.Value, script.Key, installedVersionNumber);
                retrycount++;
                if (!status && retrycount < 3)
                    goto Retry;
                else if (retrycount == 3 && !status)
                    return;
            }

            UpdateLaterversionToServerVersion(versionNumber);
        }
        public static string GetReportServerVersionOfInstalledMachine()
        {
            string version;
            try
            {
                var result = GlobalAppSettings.DataProvider.ExecuteReaderQuery(GlobalAppSettings.QueryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_ServerVersion.DB_TableName));
                if (result != null && result.DataTable != null && result.DataTable.Rows.Count > 0)
                {
                    version = result.DataTable.Rows[0].Field<string>("VersionNumber").ToLower();
                }
                else
                {
                    version = "1.1.0.1";
                }
            }
            catch (Exception ex)
            {
                version = null;
                LogExtension.LogError("Error in getting ReportServer version of installed machine", ex, MethodBase.GetCurrentMethod());
            }
            return version;
        }

        public static IOrderedEnumerable<KeyValuePair<string, string>> GetScriptToExecute(List<Version> versionList, string installedVersionNumber)
        {
            var scriptToInstall = new Dictionary<string, string>();
            try
            {
                foreach (var version in versionList)
                {
                    if (String.Compare(version.VersionNumber, installedVersionNumber) == 0)
                    {
                        break;
                    }

                    scriptToInstall.Add(version.VersionNumber, version.ScriptName);
                }
            }
            catch (Exception ex)
            {
                LogExtension.LogError("Error in getting script to excute", ex, MethodBase.GetCurrentMethod());
            }

            return scriptToInstall.OrderBy(k => k.Key);
        }

        public static bool RunScript(string scriptName, string version, string installedVersion)
        {

            try
            {
                if (String.IsNullOrWhiteSpace(scriptName))
                    return true;
                var scriptFileInfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + WebConfigurationManager.AppSettings["SystemConfigurationPath"] + "\\VersionedScripts\\" + scriptName);
                var script = scriptFileInfo.OpenText().ReadToEnd();

                if (GlobalAppSettings.DbSupport == DataBaseType.MSSQLCE)
                {
                    var splitter = new[] { ";" };
                    var commandTexts = script.Split(splitter, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var commandText in commandTexts)
                    {
                        GlobalAppSettings.DataProvider.ExecuteNonQuery(commandText);
                    }
                }
                else
                {
                    GlobalAppSettings.DataProvider.ExecuteNonQuery(script);

                }
                UpdateLaterversionToServerVersion(version);

                return true;
            }
            catch (Exception ex)
            {
                LogExtension.LogError("Error while running script", ex, MethodBase.GetCurrentMethod());
                return false;
            }
        }

        public static void UpdateLaterversionToServerVersion(string version)
        {
            try
            {
                var query = new StringBuilder();
                var installedVersion = GetReportServerVersionOfInstalledMachine();
                if(String.IsNullOrWhiteSpace(installedVersion))
                    return;
                if (installedVersion == "1.1.0.24")
                {
                    var values = new Dictionary<string, object>
                    {
                        {GlobalAppSettings.DbColumns.DB_ServerVersion.VersionNumber, version}
                    };
                    query.Append(GlobalAppSettings.QueryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_ServerVersion.DB_TableName,
                        values));
                }
                else
                {
                    var updateColumns = new List<UpdateColumn>
                {
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_ServerVersion.VersionNumber,
                        Value = version
                    }
                };
                    var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_ServerVersion.VersionNumber,
                        Condition = Conditions.Equals,
                        Value = installedVersion
                    }
                };
                    query.Append(GlobalAppSettings.QueryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_ServerVersion.DB_TableName, updateColumns, whereColumns));
                }

                GlobalAppSettings.DataProvider.ExecuteNonQuery(query.ToString());
            }
            catch (Exception ex)
            {
                LogExtension.LogError("Error while updatig the version to server version table", ex, MethodBase.GetCurrentMethod());
            }
        }
        public static string GetLatestVersion()
        {
            var versionData = Deserialize();
            return versionData.Version.Select(k => k.VersionNumber).FirstOrDefault();
        }
        public static Versions Deserialize()
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + WebConfigurationManager.AppSettings["SystemConfigurationPath"] + "\\VersionedScripts\\Versions.xml";
            Versions data = null;
            var xmlSerializer = new XmlSerializer(typeof(Versions));
            if (File.Exists(filePath))
            {
                using (var reader = new StreamReader(filePath))
                {
                    data = (Versions)xmlSerializer.Deserialize(reader);
                    reader.Close();
                }
                return data;
            }
            return null;
        }
        public static bool IsLatestVersion()
        {
            var installedVersion = GetReportServerVersionOfInstalledMachine();
            var versionData = Deserialize();
            return !versionData.Version.Any(k => k.VersionNumber == installedVersion);
        }
    }
    public class Versions
    {
        [XmlElement("Version")]
        public List<Version> Version { get; set; }
    }

    public class Version
    {
        [XmlText]
        public string VersionNumber { get; set; }

        [XmlAttribute("file")]
        public string ScriptName { get; set; }
    }


}