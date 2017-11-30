using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Syncfusion.UMP.Base;

namespace Syncfusion.UMP.Backup.Base
{
    public class BackUpandRestore
    {
        private UserManagement userManagement = new UserManagement();
        private GlobalPermissions Globalpermission = new GlobalPermissions();

        
        //Commented as we are not going to Ship Backup features for Beta release
        //public void UMPBackUp(string path, string BackupBy, int backupEnum)
        //{
        //    var bs = BackUpSettings.GetBackupSettings(path);
        //    var BackupDate = DateTime.UtcNow;
        //    var Backupstring = DateTime.UtcNow.ToString("dd-MM-yyyy_hh-mm-ss");
        //    var BackupDBCl = new BackupDB();
        //    using (var connection = new SqlConnection(bs.DataSource))
        //    {
        //        try
        //        {

        //            var destination = bs.Destination + "bak_" + BackupDate.ToString("MM-dd-yyyy");
        //            var Filename = "bak_" + BackupDate.ToString("ddMMyyyyhhmmss");
        //            if (!Directory.Exists(destination))
        //            {
        //                Directory.CreateDirectory(destination);
        //            }
        //            //System.IO.Compression.ZipFile.CreateFromDirectory(bs.Source, destination + "\\Source.zip");
        //            var QueryForDBbackup = "Backup database SyncUMP to disk='" + destination;
        //            QueryForDBbackup += "\\" + Filename + ".bak'";
        //            connection.Open();
        //            var command = new SqlCommand(QueryForDBbackup, connection);
        //            command.ExecuteNonQuery();
        //            bs.Destination = destination;
        //            BackupDBCl.BackupName = "bak_" + BackupDate.ToString("ddMMyyyyhhmmss");
        //            BackupDBCl.CreatedDate = BackupDate;
        //            BackupDBCl.TakenBy = BackupBy;
        //            BackupDBCl.IsActive = true;
        //            BackupDBCl.FolderName = "bak_" + BackupDate.ToString("MM-dd-yyyy");
        //            //userManagement.AddBackupFile(BackupDBCl);
        //            //userManagement.AddDatabaseLog(backupEnum, BackupBy, "bak_" + BackupDate.ToString("ddMMyyyyhhmmss"),BackupBy);
        //        }
        //        catch (Exception ex)
        //        {
        //            throw ex;
        //        }
        //        finally
        //        {
        //            connection.Close();
        //        }
        //    }
        //}

        public DateTime? LastModifiedBackUp()
        {
            return DateTime.Now;
        }

        public void Restore()
        {
            var bs = BackUpSettings.GetBackupSettings("D:\\Configuration\\new");
            using (var connection = new SqlConnection("Data Source=(local);Integrated Security=true"))
            {
                try
                {
                    connection.Open();
                    var destination = bs.Destination + DateTime.Now.Date.ToShortDateString().Replace('/', '-');
                    var command =
                        new SqlCommand("RESTORE DATABASE SyncUMP FROM DISK = '" + destination + "\\" + "SyncUMP.bak'",
                            connection);
                    command.ExecuteNonQuery();
                    if (Directory.Exists(bs.Source))
                        System.IO.Directory.Delete(bs.Source, true);
                    connection.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    connection.Close();

                }
            }
        }
    }
}