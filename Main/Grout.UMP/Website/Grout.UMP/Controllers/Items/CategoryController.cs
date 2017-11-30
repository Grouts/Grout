using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Item;
using Grout.UMP.Models;

namespace Grout.UMP.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly IUserManagement _userManagement = new UserManagement();
        private readonly IItemManagement _itemManagement = new ItemManagement();
        private readonly Item _items = new Item();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>        
        public ActionResult Category()
        {
            return View();
        }

        public JsonResult GetItems(string searchKey, int? skip, int? take, List<SortCollection> sorted, List<FilterCollection> filterCollection)
        {
            var itemList = _itemManagement.GetItems(Convert.ToInt32(HttpContext.User.Identity.Name), ItemType.Category, sorted, filterCollection, searchKey, skip, take);
            return Json(itemList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCategoryDetails()
        {
            var itemDetail = _itemManagement.GetItemDetailsFromItemId(Guid.Parse(Request["itemId"]));
            return PartialView("_EditCategory", itemDetail);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult AddCategory()
        {
            return PartialView("_AddCategory");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult EditCategory()
        {
            var updateFields = new Dictionary<string, object>
            {
                {
                    GlobalAppSettings.DbColumns.DB_Item.ModifiedById,
                    Convert.ToInt32(HttpContext.User.Identity.Name)
                },
            };
            if (Convert.ToBoolean(Request["IsNameChanged"]))
            {
                var nameExistInFolder = _itemManagement.IsCategoryNameExistAlready(Request["Name"],
                    Convert.ToInt32(ItemType.Category));
                if (nameExistInFolder)
                {

                    return Json(new
                    {
                        Status = false,
                        NameExists = true
                    });
                }
                updateFields.Add(GlobalAppSettings.DbColumns.DB_Item.Name, Request["Name"]);
            }
            if (Convert.ToBoolean(Request["IsDescriptionChanged"]))
            {
                updateFields.Add(GlobalAppSettings.DbColumns.DB_Item.Description, Request["Description"].ToString().Trim());
            }
            var updateResult = _items.UpdateItem(updateFields, new Guid(Request["itemId"]), ItemType.Category);
            return Json(new {Status = updateResult.Status, NameExists = false});
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryname"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddCategory(string categoryname, string description = null)
        {
            var category = new ItemDetail()
            {
                Name = categoryname,
                Description =description!=null? description.ToString().Trim():description,
                CreatedById = Convert.ToInt32(HttpContext.User.Identity.Name),
                CreatedDate = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat()),
                ItemType = ItemType.Category,
                ModifiedById = Convert.ToInt32(HttpContext.User.Identity.Name),
                ModifiedDate = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
            };
            ViewBag.IsSuccess = _items.AddNewCategory(category);
            ViewBag.CategoryName = categoryname;
            return View("_AddCategory");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult IsCategoryExist(string categoryName)
        {
            return Json(new {Data = _items.IsCategoryNameExistAlready(categoryName, Convert.ToInt32(ItemType.Category))});
        }
    }
}
