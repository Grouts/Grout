using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Item;
using Grout.UMP.Models;

namespace Grout.UMP.Controllers
{
    [Authorize]
    public class FilesController : Controller
    {
        private readonly Item _items = new Item();
        private readonly ItemManagement _itemManagement = new ItemManagement();
        private readonly PermissionSet _permissionBase = new PermissionSet();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>        
        public ActionResult Files(string username)
        {
            ViewBag.ItemAddOptions =
                _itemManagement.GetItemTypesWithCreateAccess(Convert.ToInt32(HttpContext.User.Identity.Name));
            return View();
        }

        public JsonResult GetItems(string searchKey, int? skip, int? take, List<SortCollection> sorted, List<FilterCollection> filterCollection)
        {
            var itemList = _itemManagement.GetItems(Convert.ToInt32(HttpContext.User.Identity.Name), ItemType.File, sorted, filterCollection, searchKey, skip, take);
            return Json(itemList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult AddFile()
        {
            return PartialView("_AddFile");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult IsFileExist(string filename)
        {
            return Json(new {Data = _items.IsItemNameExistAlready(filename, ItemType.File)});
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddFile(string fileName, string description = null)
        {
            Stream fileStream = new MemoryStream();
            var file = new ItemDetail
            {
                Name = fileName,
                Description =description!=null?description.Trim(): description,
                CreatedByDisplayName = HttpContext.User.Identity.Name,
            };
            if (fileName != String.Empty)
            {
                for (var i = 0; i < HttpContext.Request.Files.Count; i++)
                {
                    var httpPostedFileBase = HttpContext.Request.Files[i];
                    if (httpPostedFileBase == null) continue;
                    fileStream = httpPostedFileBase.InputStream as Stream;
                    file.Extension = Path.GetExtension(httpPostedFileBase.FileName);
                }
                file.ItemType = ItemType.File;
                var response = _items.PublishFile(file, Convert.ToInt32(HttpContext.User.Identity.Name), fileStream);
                var permissionValue = new Permission
                {
                    IsUserPermission = true,
                    ItemId = response.PublishedItemId,
                    PermissionAccess = PermissionAccess.ReadWriteDelete,
                    PermissionEntity = PermissionEntity.SpecificFiles,
                    TargetId = Convert.ToInt32(HttpContext.User.Identity.Name)
                };
                _permissionBase.AddPermissionToUser(permissionValue);
            }
            ViewBag.IsSuccess = true;
            return View("_AddFile");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult GetFileDetails()
        {
            var itemDetail = _itemManagement.GetItemDetailsFromItemId(Guid.Parse(Request["itemId"]));
            ViewData["IsSuccess"] = false;
            ViewData["IsValid"] = true;
            return View("_EditFile", itemDetail);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditFile()
        {
            var itemDetail = new ItemDetail();
            var updateFields = new Dictionary<string, object>
            {
                {
                    GlobalAppSettings.DbColumns.DB_Item.ModifiedById,
                    Convert.ToInt32(HttpContext.User.Identity.Name)
                },
            };
            if (Convert.ToBoolean(Request["IsNameChanged"]))
            {
                var nameExistInCategory = _itemManagement.IsItemNameAlreadyExists(Request["Name"], ItemType.File);
                if (nameExistInCategory)
                {
                    ViewData["IsSuccess"] = false;
                    ViewData["IsValid"] = false;
                    ViewData["ErrorMessage"] = "A file with the same name already exists";
                    itemDetail = _itemManagement.GetItemDetailsFromItemId(Guid.Parse(Request["itemId"]));
                    return View("_EditFile", itemDetail);
                }
                updateFields.Add(GlobalAppSettings.DbColumns.DB_Item.Name, Request["Name"]);
               
            }
            if (Convert.ToBoolean(Request["IsDescriptionChanged"]))
            {
                updateFields.Add(GlobalAppSettings.DbColumns.DB_Item.Description, Request["Description"].ToString().Trim());
            }
            Stream fileStream = new MemoryStream();
            var extension = string.Empty;
            for (var i = 0; i < HttpContext.Request.Files.Count; i++)
            {
                var httpPostedFileBase = HttpContext.Request.Files[i];
                if (httpPostedFileBase == null) continue;
                fileStream = httpPostedFileBase.InputStream;
                extension = Path.GetExtension(httpPostedFileBase.FileName);
                if (!String.IsNullOrEmpty(extension))
                {
                    updateFields.Add(GlobalAppSettings.DbColumns.DB_Item.Extension, extension);
                }
            }
            updateFields.Add("fileStream", fileStream);
            updateFields.Add("isSourceChanged", Request["isSourceChanged"]);
            if (Convert.ToBoolean(Request["isSourceChanged"]))
            {
                updateFields.Add("versionComment", Request["versionComment"]);
            }
            var updateResult = _items.UpdateItem(updateFields, new Guid(Request["itemId"]), ItemType.File);
            ViewData["IsSuccess"] = updateResult.Status;
            if (updateResult.Status == false)
            {
                ViewData["IsValid"] = false;
                ViewData["ErrorMessage"] = "Internal server error. Please try again.";
                itemDetail = _itemManagement.GetItemDetailsFromItemId(Guid.Parse(Request["itemId"]));
            }
            return View("_EditFile", itemDetail);
        }
    }
}
