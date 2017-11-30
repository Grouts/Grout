namespace Grout.Base.DataClasses
{
    public static class ServerSetup
    {
        public static string DbSchema
        {
            get { return "DB_Schema.xml"; }
        }

        public static string Configuration
        {
            get { return "Config.xml"; }
        }

        public static string SqlTables
        {
            get { return "sql_tables.sql"; }
        }
    }
}
