using System;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Item;
using Grout.UMP.Models;

namespace Grout.UMP.Controllers.Items
{
    [Authorize]
    public class ItemsController : Controller
    {
        private readonly Item _item = new Item();
        private readonly ItemManagement _itemManagement = new ItemManagement();
        private readonly JavaScriptSerializer _javaScriptSerializer = new JavaScriptSerializer();
        private readonly PermissionSet _permissionBase = new PermissionSet();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public PartialViewResult ItemVersion(string itemId)
        {
            var itemName = _itemManagement.GetItemDetailsFromItemId(Guid.Parse(itemId)).Name;
            ViewBag.ItemName = itemName.Length > 48 ? itemName.Substring(0, 48) + "..." : itemName;
            ViewBag.FullItemName = itemName;
            return PartialView("_ItemVersion");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public JsonResult ItemLogs(string itemId,int? skip,int? take)
        {
            var skipValue = 0;
            var takeValue = 10;

            if (skip == null) { skipValue = 0; } else { skipValue = Convert.ToInt32(skip.ToString()); }
            if (take == null) { takeValue = 10; } else { takeValue = Convert.ToInt32(take.ToString()); }

            return Json(_item.GetItemLog(Guid.Parse(itemId), skipValue, takeValue));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="itemAction"></param>
        /// <returns></returns>
        public PartialViewResult MoveViewItem(string itemId, string itemAction)
        {
            var itemDetails = _itemManagement.GetItemDetailsFromItemId(Guid.Parse(itemId));
            var itemName = itemDetails.Name;
            ViewBag.ItemName = itemName.Length > 33 ? itemName.Substring(0, 33) + "..." : itemName;
            ViewBag.FullItemName = itemName;
            ViewBag.ItemId = itemDetails.Id;
            ViewBag.Action = itemAction;
            ViewBag.CurrentCategoryName = itemDetails.CategoryName;
            var itemList =
                _itemManagement.CreateAccessCategoriesOfItem(Convert.ToInt32(HttpContext.User.Identity.Name),
                    itemDetails.ItemType).OrderBy(o => o.Name).ToList();

            itemList.Remove(itemList.Find(f => f.Id == itemDetails.CategoryId));

            ViewBag.CategoryDetails = itemList;
            if (!itemList.Any())
                TempData["ErrorMessage"] = "You do not have permission to create reports in a category. So you could not move reports in any category ";
            return PartialView("_MoveItem");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="itemAction"></param>
        /// <returns></returns>
        public PartialViewResult CopyViewItem(string itemId, string itemAction)
        {
            var itemDetails = _itemManagement.GetItemDetailsFromItemId(Guid.Parse(itemId));
            var itemName = itemDetails.Name;
            ViewBag.ItemName = itemName.Length > 33 ? itemName.Substring(0, 33) + "..." : itemName;
            ViewBag.FullItemName = itemName;
            ViewBag.ItemId = itemDetails.Id;
            ViewBag.Action = itemAction;
            ViewBag.CurrentCategoryName = itemDetails.CategoryName;
            var itemList =
                _itemManagement.CreateAccessCategoriesOfItem(Convert.ToInt32(HttpContext.User.Identity.Name),
                    itemDetails.ItemType).OrderBy(o => o.Name).ToList();
            ViewBag.CategoryDetails = itemList;
            if (!itemList.Any())
            {
                if (itemAction == "Copy")
                    TempData["ErrorMessage"] =
                        "You do not have permission to create reports in a category. So you could not copy reports in any category ";
                else if (itemAction == "Clone")
                    TempData["ErrorMessage"] =
                        "You do not have permission to create reports in a category. So you could not clone reports in any category ";
            }
            return PartialView("_CopyItem");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="toCategoryId"></param>
        /// <returns></returns>
        public JsonResult MoveItem(string itemId, string toCategoryId, string itemName)
        {
            var userId = Convert.ToInt32(HttpContext.User.Identity.Name);
            var itemIdGuid=Guid.Parse(itemId);
            var result= _itemManagement.MoveItem(itemIdGuid, Guid.Parse(toCategoryId), userId, itemName);
            if (result.Status)
            {
                var itemsList = _itemManagement.GetItems(userId, ItemType.Report, null, null, null, null, null, itemIdGuid);
                var permissionAccess = (itemsList.result.Any(a => a.Id == itemIdGuid && a.CanDelete) == true) ? PermissionAccess.ReadWriteDelete : (itemsList.result.Any(a => a.Id == itemIdGuid && a.CanWrite) == true) ? PermissionAccess.ReadWrite : PermissionAccess.Read;
                var permissionValue = new Permission
                {
                    IsUserPermission = true,
                    ItemId = itemIdGuid,
                    PermissionAccess = permissionAccess,
                    PermissionEntity = PermissionEntity.SpecificReports,
                    TargetId = userId
                };
                var isPermissionExist = _permissionBase.IsUserPermissionExist(permissionValue);
                if(!isPermissionExist)
                _permissionBase.AddPermissionToUser(permissionValue);

                var itemDetail = _itemManagement.GetItemDetailsFromItemId(itemIdGuid);
                if (itemName != itemDetail.Name)
                {
                    var updateFields = new Dictionary<string, object>
                        {
                            {
                                GlobalAppSettings.DbColumns.DB_Item.ModifiedById,
                                Convert.ToInt32(HttpContext.User.Identity.Name)
                            }
                        };
                    updateFields.Add("isSourceChanged", false);
                    updateFields.Add("isDataSourceChanged", false);
                    updateFields.Add(GlobalAppSettings.DbColumns.DB_Item.Name, itemName);
                    var updateResult = _item.UpdateItem(updateFields, itemIdGuid, ItemType.Report);
                }
           
            }
            return  Json(new{
                status=result.Status,
                isNameExist=result.ReturnValue.ToString().ToLower()=="name"?true:false,
                isException=result.ReturnValue.ToString().ToLower()=="exception"?true:false
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="toCategoryId"></param>
        /// <param name="userid"></param>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public JsonResult CopyItem(string itemId, string toCategoryId, string userid, string itemName)
        {
            var result= _itemManagement.CopyItem(Guid.Parse(itemId), Guid.Parse(toCategoryId), Convert.ToInt32(userid),
                itemName);
            if (result.Status)
            {
                var permissionValue = new Permission
                {
                    IsUserPermission = true,
                    ItemId =new Guid(result.ReturnValue.ToString()),
                    PermissionAccess = PermissionAccess.ReadWriteDelete,
                    PermissionEntity = PermissionEntity.SpecificReports,
                    TargetId = Convert.ToInt32(userid)
                };
                _permissionBase.AddPermissionToUser(permissionValue);
            }
            return Json(new
            {
                status = result.Status,
                isNameExist = result.ReturnValue.ToString().ToLower() == "name" ? true : false,
                isException = result.ReturnValue.ToString().ToLower() == "exception" ? true : false
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="toCategoryId"></param>
        /// <param name="userid"></param>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public JsonResult CloneItem(string itemId, string toCategoryId, string userid, string itemName)
        {
            var userId = Convert.ToInt32(userid);
            var itemIdGuid = Guid.Parse(itemId);
            var result= _itemManagement.CloneItem(Guid.Parse(itemId), Guid.Parse(toCategoryId), Convert.ToInt32(userid),
                itemName);
            if (result.Status)
            {
                var itemsList = _itemManagement.GetItems(userId, ItemType.Report, null, null, null, null, null, itemIdGuid);
                var permissionAccess = (itemsList.result.Any(a => a.Id == itemIdGuid && a.CanDelete) == true) ? PermissionAccess.ReadWriteDelete : (itemsList.result.Any(a => a.Id == itemIdGuid && a.CanWrite) == true) ? PermissionAccess.ReadWrite : PermissionAccess.Read;
                var permissionValue = new Permission
                {
                    IsUserPermission = true,
                    ItemId = new Guid(result.ReturnValue.ToString()),
                    PermissionAccess = permissionAccess,
                    PermissionEntity = PermissionEntity.SpecificReports,
                    TargetId = userId
                };
                _permissionBase.AddPermissionToUser(permissionValue);
            }
            return Json(new
            {
                status = result.Status,
                isNameExist = result.ReturnValue.ToString().ToLower() == "name" ? true : false,
                isException = result.ReturnValue.ToString().ToLower() == "exception" ? true : false
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public bool RollbackItem(Guid itemId, int versionId)
        {
            var cloneId = _itemManagement.GetCloneItemId(itemId);
            _item.ItemRollback(cloneId, versionId);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public JsonResult VersionItems(Guid itemId,int? skip,int? take)
        {
            var skipValue = 0;
            var takeValue = 10;
            if (skip == null) { skipValue = 0; } else { skipValue = Convert.ToInt32(skip.ToString()); }
            if (take == null) { takeValue = 10; } else { takeValue = Convert.ToInt32(take.ToString()); }
            var userId = Convert.ToInt32(HttpContext.User.Identity.Name);
            int countValue;
            var result = _itemManagement.FindItemPrevVersion(itemId, userId, out countValue, null, skipValue, takeValue);
            return Json(
                new EntityData<ItemVersion>
                {
                    result = result,
                    count = countValue
                });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult ItemAddOptions()
        {
            var createPermissions =
                _itemManagement.GetItemTypesWithCreateAccess(Convert.ToInt32(HttpContext.User.Identity.Name));
            return View("_ItemAddOptions", createPermissions);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteConfirmation()
        {
            var itemType = (ItemType) Convert.ToInt32(Request["itemTypeId"]);
            var validation = _itemManagement.ItemDeleteValidation(Guid.Parse(Request["itemId"]), itemType);
            ViewBag.ItemName = Request["itemName"];
            ViewBag.ItemId = Request["itemId"];
            return View("_DeleteItem", new DataResponse {Success = validation, Value = itemType});
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JsonResult DeleteItem()
        {
            return
                Json(_itemManagement.DeleteItem(Guid.Parse(Request["itemId"]),
                    Convert.ToInt32(HttpContext.User.Identity.Name)));
        }
    }
}
