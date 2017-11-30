using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Item;

namespace Grout.UMP.Models.API
{
    public class Viewer
    {
        private readonly UserManagement _userManagement = new UserManagement();
        private readonly GroupManagement _groupManagement = new GroupManagement();
        private readonly ItemManagement _items = new ItemManagement();

        /// <summary>
        /// Get the report definition and list of data source definition
        /// </summary>
        /// <param name="itemRequest"></param>
        /// <returns>Class type ViewerReportApi</returns>
        //public ReportViewerDefinition GetReportById(ItemRequest itemRequest)
        //{
        //    try
        //    {
        //        var datasourceList = _items.GetDataSourceListbyReportId(itemRequest.ItemId);
        //        var datasourceDefinitionList = new List<DataSourceDefinition>();
        //        for (var i = 0; i < datasourceList.Count; i++)
        //        {
        //            datasourceDefinitionList.Add(_items.GetDataSourceDefinition(datasourceList[i]));
        //        }

        //        byte[] datasourceByteValue = null;
        //        byte[] reportByteValue = null;
        //        if (datasourceDefinitionList.Any())
        //        {
        //            var datasourceStream = new MemoryStream();
        //            var dataSourceSerializer = new XmlSerializer(typeof(List<DataSourceDefinition>));
        //            var datasourceStreamWriter = new XmlTextWriter(datasourceStream, Encoding.UTF8);
        //            dataSourceSerializer.Serialize(datasourceStreamWriter, datasourceDefinitionList);
        //            datasourceStream = (MemoryStream)datasourceStreamWriter.BaseStream;
        //            datasourceByteValue = datasourceStream.ToArray();
        //        }

        //        var reportStream = new MemoryStream();
        //        var reportDefinition = new ReportModel().GetReportDefinition(itemRequest.ItemId);
        //        if (reportDefinition != null)
        //        {
        //            var reportSerializer = new XmlSerializer(typeof(ReportDefinition));
        //            var reportStreamWriter = new XmlTextWriter(reportStream, Encoding.UTF8);
        //            reportSerializer.Serialize(reportStreamWriter, reportDefinition);
        //            reportStream = (MemoryStream)reportStreamWriter.BaseStream;
        //            reportByteValue = reportStream.ToArray();
        //        }
        //        var reportResponse = new ReportViewerDefinition
        //        {
        //            DatasourceDefinition = datasourceByteValue,
        //            HasDatasources = (datasourceList.Count > 0),
        //            ReportDefinition = reportByteValue,
        //            Status = true
        //        };
        //        return reportResponse;
        //    }
        //    catch (Exception e)
        //    {
        //        var reportResponse = new ReportViewerDefinition
        //        {
        //            Status = false,
        //            StatusMessage = e.Message
        //        };
        //        return reportResponse;
        //    }
        //}
    }
}