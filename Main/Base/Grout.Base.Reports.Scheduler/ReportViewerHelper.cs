using System;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using Newtonsoft.Json;
using Grout.Base.DataClasses;
using Grout.Base.Item;
using System.Linq;

namespace Grout.Base.Reports.Scheduler
{
    public class UMPServer
    {
        //    private readonly IItemManagement _itemManagement = new ItemManagement();

        //    private T Deserialize<T>(string json)
        //    {
        //        using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
        //        {
        //            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
        //            return (T)serializer.ReadObject(ms);
        //        }
        //    }

        //    T DeseralizeObj<T>(Stream str)
        //    {
        //        XmlSerializer serializer = new XmlSerializer(typeof(T));
        //        XmlReader reader = XmlReader.Create(str);
        //        return (T)serializer.Deserialize(reader);
        //    }

        //    Grout.EJ.RDL.ServerProcessor.DataSourceDefinition GetDataSourceDefinition(ItemResponse response)
        //    {
        //        var _rptDefinition = new Grout.EJ.RDL.ServerProcessor.DataSourceDefinition();
        //        var _datasourceStream = this.GetFileToStream(response.FileContent);
        //        var _umpDefinition = this.DeseralizeObj<Grout.Base.DataClasses.DataSourceDefinition>(_datasourceStream);
        //        _rptDefinition.ConnectString = _umpDefinition.ConnectString;
        //        _rptDefinition.CredentialRetrieval = (Grout.EJ.RDL.ServerProcessor.CredentialRetrievalEnum)Enum.Parse(typeof(Grout.EJ.RDL.ServerProcessor.CredentialRetrievalEnum), _umpDefinition.CredentialRetrieval.ToString(), true);
        //        _rptDefinition.Enabled = _umpDefinition.Enabled;
        //        _rptDefinition.EnabledSpecified = _umpDefinition.EnabledSpecified;
        //        _rptDefinition.Extension = _umpDefinition.Extension;
        //        _rptDefinition.ImpersonateUser = _umpDefinition.ImpersonateUser;
        //        _rptDefinition.ImpersonateUserSpecified = _umpDefinition.ImpersonateUserSpecified;
        //        _rptDefinition.OriginalConnectStringExpressionBased = _umpDefinition.OriginalConnectStringExpressionBased;
        //        _rptDefinition.Password = _umpDefinition.Password;
        //        _rptDefinition.Prompt = _umpDefinition.Prompt;
        //        _rptDefinition.UseOriginalConnectString = _umpDefinition.UseOriginalConnectString;
        //        _rptDefinition.UserName = _umpDefinition.UserName;
        //        _rptDefinition.WindowsCredentials = _umpDefinition.WindowsCredentials;
        //        return _rptDefinition;
        //    }

        //    public override Grout.EJ.RDL.ServerProcessor.DataSourceDefinition GetDataSourceDefinition(string dataSource)
        //    {
        //        var _credential = this.ReportServerCredential as System.Net.NetworkCredential;
        //        try
        //        {
        //            Guid itemId;
        //            ItemRequest itemRequest = new ItemRequest();
        //            itemRequest.UserName = _credential.UserName;
        //            itemRequest.Password = _credential.Password;
        //            itemRequest.ItemType = ItemType.Datasource;

        //            if (Guid.TryParse(dataSource, out itemId))
        //            {
        //                itemRequest.ItemId = itemId;
        //            }
        //            else
        //            {
        //                itemRequest.Name = dataSource.TrimStart('/');
        //            }

        //            using (var proxy = new CustomWebClient())
        //            {
        //                var ser = new DataContractJsonSerializer(typeof(ItemRequest));
        //                var mem = new MemoryStream();
        //                ser.WriteObject(mem, itemRequest);
        //                proxy.Headers["Content-type"] = "application/json";
        //                proxy.Encoding = Encoding.UTF8;
        //                var data = Encoding.UTF8.GetString(mem.ToArray(), 0, (int)mem.Length);

        //                var rdata = proxy.UploadString(new Uri(this.ReportServerUrl + "/download-data-source"),
        //                    "POST", data);

        //                var result = JsonConvert.DeserializeObject<ItemResponse>(rdata);
        //                return this.GetDataSourceDefinition(result);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //        }
        //        return null;
        //    }

        //    public override byte[] GetItemDefinition(string itemName)
        //    {
        //        var _credential = this.ReportServerCredential as System.Net.NetworkCredential;

        //        try
        //        {
        //            ItemRequest itemRequest = new ItemRequest();
        //            itemRequest.UserName = _credential.UserName;
        //            itemRequest.Password = _credential.Password;
        //            itemRequest.ItemType = ItemType.File;
        //            itemRequest.Name = itemName;

        //            using (var proxy = new CustomWebClient())
        //            {
        //                var ser = new DataContractJsonSerializer(typeof(ItemRequest));
        //                var mem = new MemoryStream();
        //                ser.WriteObject(mem, itemRequest);
        //                proxy.Headers["Content-type"] = "application/json";
        //                proxy.Encoding = Encoding.UTF8;
        //                var data = Encoding.UTF8.GetString(mem.ToArray(), 0, (int)mem.Length);

        //                var rdata = proxy.UploadString(new Uri(this.ReportServerUrl + "/api/reportserverapi/get-file"),
        //                    "POST", data);

        //                var result = JsonConvert.DeserializeObject<ItemResponse>(rdata);
        //                return result.FileContent;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //        }

        //        return null;
        //    }


        //    public override System.IO.Stream GetReport()
        //    {
        //        var _credential = this.ReportServerCredential as System.Net.NetworkCredential;
        //        var pathSplits = this.ReportPath.Split('/').ToList();
        //        try
        //        {
        //            var itemId = Guid.Parse(pathSplits[0]);
        //            var version = 0;
        //            if (pathSplits.Count > 1)
        //            {
        //                version = Convert.ToInt32(pathSplits[1]);
        //            }
        //            var apiResponse = new ItemResponse();
        //            try
        //            {
        //                var itemDetail = _itemManagement.GetItemDetailsFromItemId(itemId);

        //                if (itemDetail != null)
        //                {
        //                    var itemLocation = _itemManagement.GetItemLocation(itemDetail.Id, ItemType.Report, String.Empty, version);
        //                    apiResponse = new ItemResponse
        //                    {
        //                        Status = true,
        //                        FileContent = File.ReadAllBytes(itemLocation),
        //                        ItemName = itemDetail.Name
        //                    };
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                throw new Exception("Report is incorrect Format");
        //            }
        //            return this.GetFileToStream(apiResponse.FileContent);
        //        }
        //        catch (Exception ex)
        //        {
        //        }
        //        return null;
        //    }

        //    private Stream GetFileToStream(byte[] _fileContent)
        //    {
        //        MemoryStream memStream = new MemoryStream();
        //        memStream.Write(_fileContent, 0, _fileContent.Length);
        //        memStream.Seek(0, SeekOrigin.Begin);
        //        return memStream;
        //    }
        //}

        //class CustomWebClient : WebClient
        //{
        //    protected override WebRequest GetWebRequest(Uri uri)
        //    {
        //        var request = base.GetWebRequest(uri);
        //        request.Timeout = 4 * 60 * 1000; //Increase time out
        //        return request;
        //    }
        //}
    }
}