using Syncfusion.Server.Base.DataClasses;
using Syncfusion.Server.Base.Encryption;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Syncfusion.Server.Base.Database.Uninstaller
{
    class Program
    {
        static string errorTextFilePath;

        static void Main(string[] args)
        {
            var rootPath = string.Empty;
            var processSuccess = true;
            var errorLogFileName = string.Empty;
            var errorLogFolder=string.Empty;
            if (args.Length != 0)
            {
                string syncfusionServerType;
                if (args[0].ToLower() == "reportserver")
                {
                    syncfusionServerType = "Report Server";
                    errorLogFileName = "ReportServer_Errorlog_" + GetAssemblyVersion() + "_" + DateTime.Now.ToString("MMddyyyyHHmmssfff") + ".txt";
                }
                else
                {
                    syncfusionServerType = "Dashboard Server";
                    errorLogFileName = "DashboardServer_Errorlog_" + GetAssemblyVersion() + "_" + DateTime.Now.ToString("MMddyyyyHHmmssfff") + ".txt";
                }
                errorLogFolder=Path.Combine(Path.GetTempPath(), "Syncfusion " + syncfusionServerType);
                errorTextFilePath = Path.Combine(errorLogFolder,errorLogFileName);
                rootPath = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                                 "\\Syncfusion\\" + syncfusionServerType);
                SystemSettings data = null;
                var xmlSerializer = new XmlSerializer(typeof(SystemSettings));
                if (File.Exists(rootPath + "\\Configuration\\Config.xml"))
                {
                    using (var reader = new StreamReader(rootPath + "\\Configuration\\Config.xml"))
                    {
                        data = (SystemSettings)xmlSerializer.Deserialize(reader);
                        reader.Close();
                    }
                }
                if (data != null)
                {
                    switch (data.SqlConfiguration.ServerType)
                    {
                        case DataBaseType.MSSQL:
                            var tokenCryptography = new TokenCryptography();
                            var connectionString = tokenCryptography.Decrypt(data.SqlConfiguration.ConnectionString);
                            var builder = new SqlConnectionStringBuilder(connectionString);
                            var isDatabaseExist = CheckDatabaseExists(connectionString, builder.InitialCatalog);
                            if (isDatabaseExist)
                            {
                                var dbDropDatabaseScript = "use master; ALTER DATABASE [" + builder.InitialCatalog + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; drop database [" + builder.InitialCatalog + "];";
                                var deleteAction = DeleteSQLDatabase(dbDropDatabaseScript, connectionString);
                                if (!deleteAction.Success)
                                {
                                    processSuccess = false;
                                    if (!Directory.Exists(errorLogFolder))
                                    {
                                        Directory.CreateDirectory(errorLogFolder);
                                    }
                                    TextWriter tw = File.CreateText(errorTextFilePath);
                                    tw.WriteLine("Syncfusion " + syncfusionServerType + " - Uninstaller Error log.");
                                    tw.WriteLine("Error in dropping the database. Please find the below exception thrown from the SQL Server.");
                                    tw.WriteLine(deleteAction.Value.ToString());
                                    tw.Close();
                                }
                            }
                            break;
                    }
                    var deleteDirectory = DeleteDirectory(rootPath);
                    if (!deleteDirectory.Success)
                    {
                        processSuccess = false;
                        var errorFileExist = File.Exists(errorTextFilePath);
                        if (!errorFileExist)
                        {
                            if (!Directory.Exists(errorLogFolder))
                            {
                                Directory.CreateDirectory(errorLogFolder);
                            }
                            TextWriter tw = File.CreateText(errorTextFilePath);
                            tw.WriteLine("Syncfusion " + syncfusionServerType + " - Uninstaller Error log.");
                            tw.WriteLine("Error in deleting directories.");
                            tw.WriteLine(deleteDirectory.Value.ToString());
                            tw.Close();
                        }
                        else
                        {
                            TextWriter tw = new StreamWriter(errorTextFilePath);
                            tw.WriteLine("Error in deleting directories.");
                            tw.WriteLine(deleteDirectory.Value.ToString());
                            tw.Close();
                        }
                    }
                    var statusMessage = (processSuccess) ? "success" : "failure;" + errorLogFileName;
                    Console.WriteLine(statusMessage);
                }
            }
        }

        static DataResponse DeleteDirectory(string targetDirectory)
        {
            var dataResponse = new DataResponse();
            try
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
                dataResponse.Success = true;
            }
            catch (Exception e)
            {
                dataResponse.Success = false;
                dataResponse.Value = e.Message;
                return dataResponse;
            }
            return dataResponse;
        }

        static bool CheckDatabaseExists(string connectionString, string databaseName)
        {
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
                        var databaseId = (int)sqlCmd.ExecuteScalar();
                        tmpConn.Close();

                        result = (databaseId > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        static DataResponse DeleteSQLDatabase(string script, string connectionString)
        {
            var tryCount = 0;
            var dataResponse = new DataResponse();
            var errorMessage = string.Empty;
            do
            {
                var connection = new SqlConnection(connectionString);
                try
                {
                    var command = new SqlCommand(script, connection);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    tryCount++;
                    if (tryCount == 3)
                    {
                        errorMessage = ex.Message;
                    }
                }
                finally
                {
                    connection.Close();
                }
            } while (tryCount != 0 && tryCount < 3);
            dataResponse.Success = !(tryCount >= 3);
            dataResponse.Value = errorMessage;
            return dataResponse;
        }

        static string GetAssemblyVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
