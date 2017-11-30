namespace Grout.Base.DataClasses
{
    public class DataSourceUploadStatus
    {
        public bool Status { get; set; }

        public string Message { get; set; }

        public bool ConnectionStringStatus { get; set; }

        public bool IsNameExist { get; set; }

        public string PublishedDataSourceId { get; set; }
    }
}
