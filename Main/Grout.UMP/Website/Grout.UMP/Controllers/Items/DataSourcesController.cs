using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Item;
using Grout.UMP.Models;

namespace Grout.UMP.Controllers
{
    [Authorize]
    public class DataSourcesController : Controller
    {
        private readonly IItemManagement itemManagement = new ItemManagement();
        private readonly Item item = new Item();
        private readonly DataSourceManager dataSourceManager = new DataSourceManager();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public ActionResult DataSources(string username)
        {
            ViewBag.ItemAddOptions =
                itemManagement.GetItemTypesWithCreateAccess(Convert.ToInt32(HttpContext.User.Identity.Name));
            return View();
        }

        public JsonResult GetItems(string searchKey, int? skip, int? take, List<SortCollection> sorted, List<FilterCollection> filterCollection)
        {
            var itemList = itemManagement.GetItems(Convert.ToInt32(HttpContext.User.Identity.Name), ItemType.Datasource, sorted, filterCollection, searchKey, skip, take);
            return Json(itemList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult AddDataSource()
        {
            return PartialView("_AddDataSource");
        }

        /// <summary>
        /// Used to create data source from application
        /// </summary>
        /// <returns></returns>
        public JsonResult CreateDataSource()
        {
            var file = new ItemDetail
            {
                Name = HttpContext.Request["Name"],
                Description = HttpContext.Request["Description"].ToString().Trim(),
                Extension = ".rds",
                ItemType = ItemType.Datasource
            };
            var dataSourceUploadStatus = new DataSourceUploadStatus();
            var nameExistInFolder = item.IsItemNameExistAlready(file.Name, file.ItemType);
            if (nameExistInFolder)
            {
                dataSourceUploadStatus.IsNameExist = true;
                return Json(new {result = dataSourceUploadStatus});
            }
            var dataSourceDefinition = new DataSourceDefinition
            {
                Extension = HttpContext.Request["DataSourceType"],
                EnabledSpecified = true,
                Enabled = true
            };
            var connectionType = HttpContext.Request["ConnectUsing"];
            dataSourceDefinition.ConnectString = HttpContext.Request["ConnectionString"];
            switch (connectionType.ToLower())
            {
                case "prompt":
                    dataSourceDefinition.CredentialRetrieval = CredentialRetrievalEnum.Prompt;
                    dataSourceDefinition.Prompt = HttpContext.Request["PromptText"];
                    dataSourceDefinition.WindowsCredentialsSpecified = true;
                    dataSourceDefinition.WindowsCredentials =
                        Convert.ToBoolean(HttpContext.Request["EnablePromptWindowsAuth"]);
                    break;
                case "store":
                    dataSourceDefinition.CredentialRetrieval = CredentialRetrievalEnum.Store;
                    dataSourceDefinition.UserName = HttpContext.Request["UserName"];
                    dataSourceDefinition.Password = HttpContext.Request["Password"];
                    dataSourceDefinition.WindowsCredentialsSpecified = true;
                    dataSourceDefinition.WindowsCredentials =
                        Convert.ToBoolean(HttpContext.Request["EnableStoredWindowsAuth"]);
                    dataSourceDefinition.ImpersonateUserSpecified = true;
                    dataSourceDefinition.ImpersonateUser = Convert.ToBoolean(HttpContext.Request["ImpersonateUser"]);
                    break;
                case "integrated":
                    dataSourceDefinition.CredentialRetrieval = CredentialRetrievalEnum.Integrated;
                    break;
                default:
                    dataSourceDefinition.CredentialRetrieval = CredentialRetrievalEnum.None;
                    break;
            }
            dataSourceUploadStatus = dataSourceManager.OnTestConnectionString(dataSourceDefinition.Extension,
                dataSourceDefinition.ConnectString);
            if (dataSourceUploadStatus.ConnectionStringStatus == false)
            {
                return Json(new {result = dataSourceUploadStatus});
            }
            return
                Json(
                    new
                    {
                        result =
                            dataSourceManager.AddDataSource(file, dataSourceDefinition,
                                Convert.ToInt32(HttpContext.User.Identity.Name))
                    });
        }

        /// <summary>
        /// This will test the connection status for the provided details
        /// </summary>
        /// <returns></returns>
        public JsonResult TestDataSourceConnection()
        {
            var dataSourceDefinition = new DataSourceDefinition();
            var connectionType = HttpContext.Request["ConnectUsing"];
            dataSourceDefinition.ConnectString = HttpContext.Request["ConnectionString"];
            dataSourceDefinition.Extension = HttpContext.Request["DataSourceType"];
            switch (connectionType.ToLower())
            {
                case "prompt":
                    dataSourceDefinition.CredentialRetrieval = CredentialRetrievalEnum.Prompt;
                    dataSourceDefinition.Prompt = HttpContext.Request["PromptText"];
                    dataSourceDefinition.WindowsCredentials =
                        Convert.ToBoolean(HttpContext.Request["EnablePromptWindowsAuth"]);
                    break;
                case "store":
                    dataSourceDefinition.CredentialRetrieval = CredentialRetrievalEnum.Store;
                    dataSourceDefinition.UserName = HttpContext.Request["UserName"];
                    dataSourceDefinition.Password = HttpContext.Request["Password"];
                    dataSourceDefinition.WindowsCredentials =
                        Convert.ToBoolean(HttpContext.Request["EnableStoredWindowsAuth"]);
                    dataSourceDefinition.ImpersonateUserSpecified = true;
                    dataSourceDefinition.ImpersonateUser = Convert.ToBoolean(HttpContext.Request["ImpersonateUser"]);
                    break;
                case "integrated":
                    dataSourceDefinition.CredentialRetrieval = CredentialRetrievalEnum.Integrated;
                    break;
                default:
                    dataSourceDefinition.CredentialRetrieval = CredentialRetrievalEnum.None;
                    break;
            }
            return Json(new {result = dataSourceManager.OnTestDataSourceConnection(dataSourceDefinition)});
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult EditDataSourceView()
        {
            ViewData["dataSourceInfo"] = itemManagement.GetItemDetailsFromItemId(Guid.Parse(Request["itemId"]));
            ViewData["dataSourceDefinition"] =
                dataSourceManager.GetDataSourceDefinitionWithoutPassword(Guid.Parse(Request["itemId"]));
            return View("_EditDataSource");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JsonResult EditDataSource()
        {
            var dataSourceUploadStatus = new DataSourceUploadStatus();
            var updateFields = new Dictionary<string, object>
            {
                {
                    GlobalAppSettings.DbColumns.DB_Item.ModifiedById,
                    Convert.ToInt32(HttpContext.User.Identity.Name)
                },
                {"isSourceChanged", Convert.ToBoolean(Request["IsDataSourceDefinitionChanged"])}
            };
            if (Convert.ToBoolean(Request["IsNameChanged"]))
            {
                var nameExistInFolder = itemManagement.IsItemNameAlreadyExists(Request["Name"], ItemType.Datasource);
                if (nameExistInFolder)
                {
                    dataSourceUploadStatus.IsNameExist = true;
                    return Json(new {result = dataSourceUploadStatus});
                }
                updateFields.Add(GlobalAppSettings.DbColumns.DB_Item.Name, Request["Name"]);
            }
            else
            {
                dataSourceUploadStatus.IsNameExist = false;
            }
            if (Convert.ToBoolean(Request["IsDescriptionChanged"]))
            {
                updateFields.Add(GlobalAppSettings.DbColumns.DB_Item.Description, Request["Description"].ToString().Trim());
            }
            if (Convert.ToBoolean(Request["IsDataSourceDefinitionChanged"]))
            {
                var dataSourceDefinition = new DataSourceDefinition
                {
                    Extension = HttpContext.Request["DataSourceType"],
                    EnabledSpecified = true,
                    Enabled = true
                };
                var connectionType = HttpContext.Request["ConnectUsing"];
                dataSourceDefinition.ConnectString = HttpContext.Request["ConnectionString"];
                switch (connectionType.ToLower())
                {
                    case "prompt":
                        dataSourceDefinition.CredentialRetrieval = CredentialRetrievalEnum.Prompt;
                        dataSourceDefinition.Prompt = HttpContext.Request["PromptText"];
                        dataSourceDefinition.WindowsCredentialsSpecified = true;
                        dataSourceDefinition.WindowsCredentials =
                            Convert.ToBoolean(HttpContext.Request["EnablePromptWindowsAuth"]);
                        break;
                    case "store":
                        dataSourceDefinition.CredentialRetrieval = CredentialRetrievalEnum.Store;
                        dataSourceDefinition.UserName = HttpContext.Request["UserName"];
                        dataSourceDefinition.Password = HttpContext.Request["Password"];
                        dataSourceDefinition.WindowsCredentialsSpecified = true;
                        dataSourceDefinition.WindowsCredentials =
                            Convert.ToBoolean(HttpContext.Request["EnableStoredWindowsAuth"]);
                        dataSourceDefinition.ImpersonateUserSpecified = true;
                        dataSourceDefinition.ImpersonateUser = Convert.ToBoolean(HttpContext.Request["ImpersonateUser"]);
                        break;
                    case "integrated":
                        dataSourceDefinition.CredentialRetrieval = CredentialRetrievalEnum.Integrated;
                        break;
                    default:
                        dataSourceDefinition.CredentialRetrieval = CredentialRetrievalEnum.None;
                        break;
                }
                dataSourceUploadStatus = dataSourceManager.OnTestConnectionString(dataSourceDefinition.Extension,
                    dataSourceDefinition.ConnectString);
                if (dataSourceUploadStatus.ConnectionStringStatus == false)
                {
                    return Json(new {result = dataSourceUploadStatus});
                }
                updateFields.Add("dataSourceDefinition", dataSourceDefinition);
                if (!String.IsNullOrWhiteSpace(Request["versionComment"]))
                {
                    updateFields.Add("versionComment", Request["versionComment"]);
                }
            }
            else
            {
                dataSourceUploadStatus.ConnectionStringStatus = true;
            }
            var updateResult = item.UpdateItem(updateFields, new Guid(Request["itemId"]), ItemType.Datasource);
            dataSourceUploadStatus.Status = updateResult.Status;
            dataSourceUploadStatus.Message = updateResult.StatusMessage != null ? updateResult.StatusMessage : "";
            return Json(new {result = dataSourceUploadStatus});
        }
    }
}