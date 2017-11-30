namespace Grout.UMP.Models
{
    public class CsvUserImport
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string DisplayMessage { get; set; }

        public bool IsExist { get; set; }
    }
}