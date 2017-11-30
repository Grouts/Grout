using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Syncfusion.UMP.Backup.Base
{
    public class BackUP
    {
        public void UMPBackUp()
        {
            try
            {

                var bs = BackUpSettings.GetBackupSettings("D:\\Configuration\\new"); 
                var destination = bs.Destination+DateTime.Now.Date.ToShortDateString().Replace('/','-');               
                if (!Directory.Exists(destination))
                {
                    Directory.CreateDirectory(destination);
                }
                else
                {
                    Directory.Delete(destination, true);
                    Directory.CreateDirectory(destination);
                }
                
                var QueryForDBbackup = "Backup database SyncUMP to disk='" + destination;
                QueryForDBbackup += "\\syncUMP.bak'";
                var connection = new SqlConnection(bs.DataSource);
                connection.Open();
                var command = new SqlCommand(QueryForDBbackup, connection);
                command.ExecuteNonQuery();
                connection.Close();

               
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
    }
}
