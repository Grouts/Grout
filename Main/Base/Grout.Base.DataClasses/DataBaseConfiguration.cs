namespace Grout.Base.DataClasses
{
    public class DataBaseConfiguration
    {
        public string ConnectionString { get; set; }

        public DataBaseType ServerType { get; set; }

        public AuthenticationType AuthenticationType { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string DataBaseName { get; set; }

        public string ServerName { get; set; }

        public bool IsWindowsAuthentication { get; set; }
    }
}
