using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.UMP.ActionFilters;
using Grout.UMP.Models;
using System.Web;
using System.Linq;

namespace Grout.UMP.Controllers
{
    [Authorize(Order = 1)]
    [AdminActionFilter(Order = 2)]
    public class GroupController : Controller
    {
        private readonly GroupModels groupModel = new GroupModels();
        private readonly GroupManagement groupManagement = new GroupManagement();
        private readonly UserManagement userManagement = new UserManagement();
        readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public ActionResult Group()
        {
            var groupList = groupManagement.GetAllActiveGroups();
            ViewBag.groupCount = groupList.Count;
            return View();
        }


        public PartialViewResult AddGroupView()
        {
            return PartialView("_AddGroup");
        }

        public PartialViewResult DeleteGroupView(string message, string group)
        {
            ViewBag.DeleteMessage = message;
            var t = Request["group"];
            ViewBag.id = group;
            return PartialView("_DeleteConfirm");
        }

        public PartialViewResult DeleteGroupUserView(string userId, string groupId, string userName)
        {
            ViewBag.id = userId;
            ViewBag.name = userName;
            ViewBag.groupId = groupId;
            return PartialView("_DeleteGroupUser");
        }

        public JsonResult RefreshGroup(int? skip, int? take, string searchKey, List<SortCollection> sorted, List<FilterCollection> filterCollection)
        {
            var skipValue = skip.HasValue ? skip.Value : 0;
            var takeValue = take.HasValue ? take.Value : 10;
            var result = groupModel.GetActiveGroups(sorted, skipValue, takeValue, searchKey, filterCollection);

            return Json(result);
        }

        public int? AddGroup(string groupName, string groupDescription, string groupColor = "#ffffff")
        {
            var group = new Group();
            group.GroupName = groupName;
            group.GroupDescription = groupDescription;
            group.GroupColor = groupColor;

            return groupManagement.AddGroup(group);
        }

        public ActionResult EditGroup(int groupId)
        {
            try
            {
                var groupUsers = groupModel.GetUsersOfGroup(groupId, null, null, null).result;
                var allUsers = userManagement.GetAllActiveInactiveUsers();
                ViewBag.groupDetails = GlobalAppSettings.Serializer.Serialize(groupModel.GetGroupById(groupId));
                if (ViewBag.groupDetails == "null")
                    throw new HttpException(404, "Page Not Found");
                foreach (var groupUser in groupUsers)
                {
                    allUsers.RemoveAll(x => x.UserId == groupUser.UserId);
                }
                ViewBag.allUser = GlobalAppSettings.Serializer.Serialize(allUsers);
                ViewBag.CurrentUserId = HttpContext.User.Identity.Name;
                return View();
            }
            catch (HttpException e)
            {
                throw new HttpException(e.GetHttpCode(), e.Message);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public JsonResult RefreshGroupUsers(string groupId, int? skip, int? take, List<SortCollection> sorted, string searchKey, List<FilterCollection> filterCollection)
        {
            var skipValue = skip.HasValue ? skip.Value : 0;
            var takeValue = take.HasValue ? take.Value : 10;
            return Json(groupModel.GetUsersOfGroup(Convert.ToInt32(groupId), sorted, skipValue, takeValue, searchKey, filterCollection));
        }

        public bool CheckGroupname(string groupName)
        {
            return groupModel.CheckGroupName(groupName);
        }

        [HttpPost]
        public JsonResult SaveGroupSettings()
        {
            var groupInfo = GlobalAppSettings.Serializer.Deserialize<Group>(Request.Form["groupInfo"]);
            var currentGroup = groupModel.GetGroupById(groupInfo.GroupId);
            if (currentGroup.GroupName != groupInfo.GroupName)
            {
                var isNameExists = groupModel.CheckGroupName(groupInfo.GroupName);

                if (isNameExists)
                    return Json(new { status = false, key = "name", value = " Group Name already exist" });
            }
            var updateSuccess = groupModel.EditGroup(groupInfo);

            if (updateSuccess)
                return Json(new { status = true, key = "success", value = "Group has been updated successfully." });
            else
                return Json(new { status = false, key = "error", value = "There is an error in updating this group. Please reload the page and try again" });
        }

        public bool AddUserInGroup(string userId, string groupId)
        {
            var userList = _serializer.Deserialize<List<int>>(userId);
            return groupModel.AddUserinGroup(userList, Convert.ToInt32(groupId));
        }

        public bool DeleteUserFromGroup(string userId, string groupId)
        {
            return groupManagement.DeleteUserFromGroup(Convert.ToInt32(userId), Convert.ToInt32(groupId));
        }

        [HttpPost]
        public bool DeleteGroup(int groupId)
        {
            return groupModel.DeleteGroup(groupId);
        }
    }
}
