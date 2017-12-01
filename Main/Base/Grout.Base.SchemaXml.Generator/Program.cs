using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace Grout.Base.SchemaXml.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            var dbGrout = new DB_Grout();
            var connectionString = "Data Source=.;Initial Catalog=Grout;user id=sa;password=Admin@123";
            var dbCreationScript = File.ReadAllText("ColumnDetector.sql");
            var connection = new SqlConnection(connectionString);

            var result = new DataTable();

            var adapter = new SqlDataAdapter(dbCreationScript, connection);
            try
            {
                adapter.Fill(result);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                adapter.Dispose();
                connection.Close();
            }

            var dataClasses = result.AsEnumerable().Select(r => r.Field<string>("query")).FirstOrDefault();
            using (TextWriter w = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "../../../Grout.Base.Data/DataBase/DB_GruntUMP.cs"))
            {
                w.WriteLine(dataClasses);
            }
        }
    }
}
