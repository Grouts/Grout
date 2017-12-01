using Grout.Base.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Grout.Base.SchemaXml.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            var dbGrout = new DB_Grout();
            var connectionString = "Data Source=.;Initial Catalog=Grout;user id=sa;password=Admin@123";
            var dbCreationScript = File.ReadAllText("ColumnDetector.txt");
            var connection = new SqlConnection(connectionString);

            var dataprovider = new SqlRelationalDataAdapter(connectionString);

            var result = dataprovider.ExecuteReaderQuery(dbCreationScript);
            var obj = result.DataTable.AsEnumerable().Select(r => r.Field<string>("query")).FirstOrDefault();
            using (TextWriter w = File.CreateText("schema.xml"))
            {
                w.WriteLine(obj);
            }
        }
    }
}
