using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Grout.Base.DataClasses;
using Grout.Base.Item;

namespace Grout.UMP.Models
{
    public class ReportModel
    {
        //public ReportDefinition GetReportDefinition(Guid reportId)
        //{
        //    var reportLocation = new ItemManagement().GetItemLocation(reportId, ItemType.Report);

        //    ReportDefinition reportDef = null;

        //    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        //    ns.Add("rd", "http://schemas.microsoft.com/SQLServer/reporting/reportdesigner");
        //    ns.Add("cl", "http://schemas.microsoft.com/sqlserver/reporting/2010/01/componentdefinition");
        //    ns.Add("", "http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition");

        //    var xmlSerializer = new XmlSerializer(typeof(ReportDefinition),
        //        "http://schemas.microsoft.com/sqlserver/reporting/2010/01/reportdefinition");
        //    if (File.Exists(reportLocation))
        //    {
        //        using (var xtr = new XmlTextReader(reportLocation) { Namespaces = true })
        //        using (var reader = new StreamReader(reportLocation))
        //        {
        //            reportDef = (ReportDefinition)xmlSerializer.Deserialize(reader);
        //            reader.Close();
        //        }
        //        return reportDef;
        //    }

        //    return reportDef;
        //}
    }
}