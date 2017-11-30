namespace Grout.Base.DataClasses
{
    public class ReportViewerDefinition
    {
        public bool Status { get; set; }
        public string StatusMessage { get; set; }
        public byte[] DatasourceDefinition { get; set; }
        public bool HasDatasources { get; set; }
        public byte[] ReportDefinition { get; set; }
    }
}
