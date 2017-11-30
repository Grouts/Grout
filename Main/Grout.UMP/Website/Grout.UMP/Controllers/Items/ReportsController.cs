using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Item;
using Grout.UMP.Models;

namespace Grout.UMP.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly Item _item = new Item();
        private readonly ItemManagement _itemManagement = new ItemManagement();
        private readonly DataSourceManager _dataSourceManager = new DataSourceManager();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>        
        public ActionResult Reports(string username)
        {
            var settings = new SystemSettingsSerializer().Deserialize(GlobalAppSettings.GetConfigFilepath() + ServerSetup.Configuration);

            if (settings == null)
            {
                FormsAuthentication.SignOut();
                return Redirect("/startup");
            }

            var itemList = _itemManagement.GetItems(Convert.ToInt32(HttpContext.User.Identity.Name), ItemType.Report,
                null, null, "", null, null).result.GroupBy(s => s.CategoryName).Select(t => new CategoryDetail()
                {
                    Id = t.FirstOrDefault() != null ? t.FirstOrDefault().CategoryId : null,
                    Description = t.FirstOrDefault() != null ? t.FirstOrDefault().CategoryDescription : "",
                    Name = t.Key,
                    ReportsCount = t.Count()
                }).ToList();

            var categoryList = _itemManagement.GetItems(Convert.ToInt32(HttpContext.User.Identity.Name),
                ItemType.Category,
                null, null, "", null, null).result.Select(t => new CategoryDetail()
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    ReportsCount = 0,
                    CanOpen = t.CanOpen,
                    CanRead = t.CanRead,
                    CanWrite = t.CanWrite,
                    CanDelete = t.CanDelete
                }).ToList();


            foreach (var category in categoryList)
            {
                var _category = itemList.Find(f => f.Id == category.Id);
                if (_category != null)
                {
                    itemList.Find(f => f.Id == category.Id).CanRead = category.CanRead;
                    itemList.Find(f => f.Id == category.Id).CanWrite = category.CanWrite;
                    itemList.Find(f => f.Id == category.Id).CanDelete = category.CanDelete;
                    itemList.Find(f => f.Id == category.Id).CanOpen = category.CanOpen;
                }
                else
                {
                    itemList.Add(category);
                }
            }
            if (!String.IsNullOrEmpty(HttpContext.Request.QueryString["categoryName"]) && itemList.Find(s => s.Name == HttpContext.Request.QueryString["categoryName"]) == null)
            {
                return RedirectToAction("Reports");
            }

            ViewBag.ItemAddOptions =
                _itemManagement.GetItemTypesWithCreateAccess(Convert.ToInt32(HttpContext.User.Identity.Name));

            return View(itemList.OrderByDescending(s => s.ReportsCount).ThenBy(s => s.Name).ToList());
        }

        public JsonResult GetItems(string searchKey, int? skip, int? take, List<SortCollection> sorted, List<FilterCollection> filterCollection, bool? isAllCategorySearch)
        {
            var itemList = _itemManagement.GetItems(Convert.ToInt32(HttpContext.User.Identity.Name), ItemType.Report, sorted, filterCollection, searchKey, skip, take, null, isAllCategorySearch.HasValue ? isAllCategorySearch.Value : false);
            return Json(itemList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult AddReport()
        {
            var selectedCategories =
                _itemManagement.CreateAccessCategoriesOfItem(Convert.ToInt32(HttpContext.User.Identity.Name),
                    ItemType.Report).OrderBy(o => o.Name).ToList();

            return PartialView("_AddReport", selectedCategories);
        }

        public ActionResult UploadReportView()
        {
            ViewBag.IsUploaded = false;
            ViewBag.FileName = "Choose file";
            return View("_ReportUpload", new ReportUploadResponse());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult UploadReport()
        {
            var previousFileName = Request["previousFileName"];
            if (previousFileName.ToLower() != "none")
            {
                _dataSourceManager.DeleteTemporaryFile(previousFileName);
            }
            Stream fileStream = new MemoryStream();
            for (var i = 0; i < HttpContext.Request.Files.Count; i++)
            {
                var httpPostedFileBase = HttpContext.Request.Files[i];
                if (httpPostedFileBase == null) continue;
                fileStream = httpPostedFileBase.InputStream as Stream;
            }
            var sharedDataSourceConnections = _dataSourceManager.UploadRdlFile(fileStream,
                Convert.ToInt32(HttpContext.User.Identity.Name));
            
            ViewBag.IsUploaded = true;
            ViewBag.FileName = Request["uploadedFileName"];
            return View("_ReportUpload", sharedDataSourceConnections);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JsonResult GetAvailableDataSourcesOfServer()
        {
            return
                Json(
                    new
                    {
                        result =
                            _dataSourceManager.GetAllDataSourceListOfUser(Convert.ToInt32(HttpContext.User.Identity.Name))
                    });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult ReportSelectDataSource(string sourceName,string selectedDataSource)
        {
            var result = _dataSourceManager.GetAllDataSourceListOfUser(Convert.ToInt32(HttpContext.User.Identity.Name));
            ViewData["sourceName"] = sourceName;
            ViewData["selectedDataSource"] = selectedDataSource;
            return PartialView("_ReportSelectDataSource",result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public JsonResult IsItemExistInSameCategory(Guid categoryId, string itemName)
        {
            return Json(new { Data = _itemManagement.IsItemNameAlreadyExists(itemName, categoryId) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult AddSharedRdlReport()
        {
            var item = new ItemDetail
            {
                Name = HttpContext.Request["fileName"],
                Description = HttpContext.Request["description"].ToString().Trim(),
                CategoryId = Guid.Parse(HttpContext.Request["selectedCategoryId"]),
                ItemType = ItemType.Report,
                Extension = ".rdl"
            };
            var dataSourceList = JsonConvert.DeserializeObject<List<DataSourceMappingInfo>>(Request["dataSourceList"]);
            var temporaryFileName = HttpContext.Request["temporaryFileName"];
            var rdlPublishResult = _dataSourceManager.AddRdlReport(item, temporaryFileName, dataSourceList,
                Convert.ToInt32(HttpContext.User.Identity.Name));
            return Json(new { result = rdlPublishResult });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JsonResult AddEmbeddedRdlReport()
        {
            var file = new ItemDetail
            {
                Name = HttpContext.Request["fileName"],
                Description = HttpContext.Request["description"].ToString().Trim(),
                CategoryId = Guid.Parse(HttpContext.Request["selectedCategoryId"]),
                ItemType = ItemType.Report,
                Extension = ".rdl"
            };
            var temporaryFileName = HttpContext.Request["temporaryFileName"];
            var rdlPublishResult = _dataSourceManager.AddEmbeddedRdlReport(file, temporaryFileName,
                Convert.ToInt32(HttpContext.User.Identity.Name));
            return Json(new { result = rdlPublishResult });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public JsonResult GetItemInfo(Guid itemId)
        {
            return Json(new { Data = _itemManagement.GetItemDetailsFromItemId(itemId) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JsonResult GetRdlReportInfo()
        {
            var itemDetail =
                _itemManagement.GetItemDetailsFromItemId(Guid.Parse(HttpContext.Request["itemId"]));
            var reportInfo =
                new
                {
                    ReportId = itemDetail.Id,
                    FolderId = itemDetail.CategoryId,
                    ReportName = itemDetail.Name,
                    ReportDescription = itemDetail.Description,
                    ReportFileName = itemDetail.Name + itemDetail.Extension,
                    DataSources = _dataSourceManager.GetDataSourceDetailsbyReportId(itemDetail.Id)
                };

            return Json(new { Data = reportInfo });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JsonResult EditReport()
        {
            var updateFields = new Dictionary<string, object>
            {
                {
                    GlobalAppSettings.DbColumns.DB_Item.ModifiedById,
                    Convert.ToInt32(HttpContext.User.Identity.Name)
                }
            };
            if (Convert.ToBoolean(Request["isReportNameChanged"]))
            {
                updateFields.Add(GlobalAppSettings.DbColumns.DB_Item.Name, Request["fileName"]);
            }
            if (Convert.ToBoolean(Request["isCategoryChanged"]))
            {
                updateFields.Add(GlobalAppSettings.DbColumns.DB_Item.ParentId, Request["categoryId"]);
            }
            if (Convert.ToBoolean(Request["isReportDescriptionChanged"]))
            {
                updateFields.Add(GlobalAppSettings.DbColumns.DB_Item.Description, Request["fileDescription"].ToString().Trim());
            }
            updateFields.Add("isSourceChanged", Convert.ToBoolean(Request["isFileChanged"]));
            if (Convert.ToBoolean(Request["isFileChanged"]))
            {
                updateFields.Add("temporaryFileName", Request["temporaryFileName"]);
                if (!String.IsNullOrWhiteSpace(Request["versionComment"]))
                {
                    updateFields.Add("versionComment", Request["versionComment"]);
                }
            }
            updateFields.Add("isDataSourceChanged", Convert.ToBoolean(Request["isDataSourceChanged"]));
            var dataSourceList = JsonConvert.DeserializeObject<List<DataSourceMappingInfo>>(Request["dataSourceList"]);
            return
                Json(
                    new
                    {
                        result =
                            _item.UpdateItem(updateFields, new Guid(Request["reportId"]), ItemType.Report,
                                dataSourceList)
                    });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult EditReportView()
        {
            var selectedCategories =
                _itemManagement.CreateAccessCategoriesOfItem(Convert.ToInt32(HttpContext.User.Identity.Name),
                    ItemType.Report);
            var itemDetail =
                _itemManagement.GetItemDetailsFromItemId(Guid.Parse(HttpContext.Request["itemId"]));
            var reportInfo =
                new
                {
                    ReportId = itemDetail.Id,
                    CategoryId = itemDetail.CategoryId.ToString().ToLower(),
                    ReportName = itemDetail.Name,
                    ReportDescription = itemDetail.Description,
                    ReportFileName = itemDetail.Name + itemDetail.Extension,
                    DataSources = _dataSourceManager.GetDataSourceDetailsbyReportId(itemDetail.Id)
                };
            var categoryCount = selectedCategories.Where(a=>a.Id==itemDetail.CategoryId).ToList().Count();
            if (categoryCount == 0)
            {
                var categoryDetails = _itemManagement.GetItemDetailsFromItemId(Guid.Parse(itemDetail.CategoryId.ToString()));
                selectedCategories.Add(new ItemDetail { Id = categoryDetails.Id, Name = categoryDetails.Name });
            }
            ViewData["reportInfo"] = reportInfo;
            return PartialView("_EditReport", selectedCategories);
        }

        /// <summary>
        /// If the popup is closed without any changes the uploaded file will be deleted from temporary files
        /// </summary>
        public void DeleteTemporaryRDLReport()
        {
            var previousFileName = Request["fileName"];
            _dataSourceManager.DeleteTemporaryFile(previousFileName);
        }

        public JsonResult RefreshCategoryList()
        {
            var itemList = _itemManagement.GetItems(Convert.ToInt32(HttpContext.User.Identity.Name), ItemType.Report,
                null, null, "", null, null).result.GroupBy(s => s.CategoryName).Select(t => new CategoryDetail()
                {
                    Id = t.FirstOrDefault() != null ? t.FirstOrDefault().CategoryId : null,
                    Name = t.Key,
                    Description = t.FirstOrDefault() != null ? t.FirstOrDefault().CategoryDescription : "",
                    ReportsCount = t.Count()
                }).ToList();

            var categoryList = _itemManagement.GetItems(Convert.ToInt32(HttpContext.User.Identity.Name),
                ItemType.Category,
                null, null, "", null, null).result.Select(t => new CategoryDetail()
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    ReportsCount = 0,
                    CanOpen = t.CanOpen,
                    CanRead = t.CanRead,
                    CanWrite = t.CanWrite,
                    CanDelete = t.CanDelete
                }).ToList();


            foreach (var category in categoryList)
            {
                var _category = itemList.Find(f => f.Id == category.Id);
                if (_category != null)
                {
                    itemList.Find(f => f.Id == category.Id).CanRead = category.CanRead;
                    itemList.Find(f => f.Id == category.Id).CanWrite = category.CanWrite;
                    itemList.Find(f => f.Id == category.Id).CanDelete = category.CanDelete;
                    itemList.Find(f => f.Id == category.Id).CanOpen = category.CanOpen;
                }
                else
                {
                    itemList.Add(category);
                }
            }

            return Json(itemList.OrderByDescending(s => s.ReportsCount).ThenBy(s => s.Name).ToList());
        }
    }
}
