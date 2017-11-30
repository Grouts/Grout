using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Http;
using System.Web.Script.Serialization;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Item;
using Grout.UMP.ActionFilters;
using Grout.UMP.Models;

namespace Grout.UMP.Controllers
{
    [ApiAuthenticationActionFilter]
    public class ReportServerApiController : ApiController
    {
        private readonly Designer designerApi = new Designer();
        private readonly ItemManagement itemManagement = new ItemManagement(GlobalAppSettings.QueryBuilder,
            GlobalAppSettings.DataProvider);
        private readonly UserManagement userManagement = new UserManagement(GlobalAppSettings.QueryBuilder,
            GlobalAppSettings.DataProvider);

        [HttpPost]
        [ApiItemAccessActionFilter(ItemType = ItemType.Report)]
        public ItemResponse DownloadItem(ItemRequest itemRequest)
        {
            return designerApi.DownloadItem(itemRequest);
        }

        /// <summary>
        /// Add RDL report in Reports Server
        /// </summary>
        /// <param name="itemRequest">ItemRequest object which is sent as request</param>
        /// <returns>The response is sent as ItemResponse object</returns>
        [HttpPost]
        [ApiItemAddActionFilter(ItemType = ItemType.Report)]
        public ItemResponse AddReport(ItemRequest itemRequest)
        {
            return designerApi.PublishReport(itemRequest);
        }

        /// <summary>
        /// Add data source in Reports Server
        /// </summary>
        /// <param name="itemRequest">ItemRequest object which is sent as request</param>
        /// <returns>The response is sent as ItemResponse object</returns>
        [HttpPost]
        [ApiItemAddActionFilter(ItemType = ItemType.Datasource)]
        public ItemResponse AddDataSource(ItemRequest itemRequest)
        {
            return designerApi.PublishDataSource(itemRequest);
        }

        /// <summary>
        /// Update RDL report in Reports Server
        /// </summary>
        /// <param name="itemRequest">ItemRequest object which is sent as request</param>
        /// <returns>The response is sent as ItemResponse object</returns>
        [HttpPost]
        [ApiItemEditActionFilter(ItemType = ItemType.Report)]
        public ItemResponse UpdateReport(ItemRequest itemRequest)
        {
            return designerApi.UpdateReport(itemRequest);
        }

        /// <summary>
        /// Update data source in Reports Server
        /// </summary>
        /// <param name="itemRequest">ItemRequest object which is sent as request</param>
        /// <returns>The response is sent as ItemResponse object</returns>
        [HttpPost]
        [ApiItemEditActionFilter(ItemType = ItemType.Datasource)]
        public ItemResponse UpdateDataSource(ItemRequest itemRequest)
        {
            return designerApi.UpdateDataSource(itemRequest);
        }

        /// <summary>
        /// Get data source definition by data source guid id
        /// </summary>
        /// <param name="itemRequest"></param>
        /// <returns>Class type ItemReponse</returns>
        [HttpPost]
        [ApiItemAccessActionFilter(ItemType = ItemType.Datasource)]
        public ItemResponse GetDataSource(ItemRequest itemRequest)
        {
            if (itemRequest.ItemId != Guid.Empty)
            {
                return designerApi.GetDataSourceDefinitionById(itemRequest);
            }
            else
            {
                return new ItemResponse
                {
                    Status = false,
                    StatusMessage = "Invalid Values"
                };
            }
        }

        /// <summary>
        /// Get file by ItemRequest
        /// </summary>
        /// <param name="itemRequest">Request object from report designer</param>
        /// <returns>Class type ItemReponse</returns>
        [HttpPost]
        [ApiItemAccessActionFilter(ItemType = ItemType.File)]
        public ItemResponse GetFile(ItemRequest itemRequest)
        {
            if (itemRequest.ItemId != Guid.Empty)
            {
                return designerApi.DownloadFileById(itemRequest);
            }
            else
            {
                return new ItemResponse
                {
                    Status = false,
                    StatusMessage = "Invalid Values"
                };
            }
        }

        /// <summary>
        /// The method will get the data source location and convert the xml to bytes
        /// </summary>        
        /// <param name="itemRequest"></param>
        /// <returns>Class type ItemResponse</returns>
        [HttpPost]
        [ApiItemAccessActionFilter(ItemType = ItemType.Datasource)]
        public ItemResponse DownloadDataSource(ItemRequest itemRequest)
        {
            if (itemRequest.ItemId != Guid.Empty)
            {
                return designerApi.DownloadDataSourceFileById(itemRequest);
            }
            else
            {
                return new ItemResponse
                {
                    Status = false,
                    StatusMessage = "Invalid Values"
                };
            }
        }

        [HttpPost]
        public HttpResponseMessage GetItems(ItemRequest itemRequest)
        {
            var filterCollection=new List<FilterCollection>();
            if (itemRequest.ItemType == ItemType.Report)
            {
                var pathSplits = itemRequest.ServerPath.Split('/').ToList();
                itemRequest.ItemType = ItemType.Report;
                var categoryName = pathSplits[1];
                filterCollection = new List<FilterCollection>{
                    new FilterCollection{
                        PropertyName="CategoryName",
                        FilterType="equal",
                        FilterKey=categoryName
                    }
                };
            }
            try
            {
                var items = itemManagement.GetItems(
                    userManagement.GetUserId(itemRequest.UserName), itemRequest.ItemType, null, filterCollection);

                var ser = new DataContractJsonSerializer(typeof(ItemDetailsApiResponse));
                var mem = new MemoryStream();
                ser.WriteObject(mem, new ItemDetailsApiResponse
                {
                    ApiStatus = true,
                    Data = items
                });
                var stream = Encoding.UTF8.GetString(mem.ToArray(), 0, (int)mem.Length);
                
                return new HttpResponseMessage()
                {
                    Content = new StringContent(
                        stream,
                        Encoding.UTF8,
                        "text/html"
                    )
                };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}