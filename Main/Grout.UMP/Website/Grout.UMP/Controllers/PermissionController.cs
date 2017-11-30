using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.UMP.Models;
using Grout.UMP.ActionFilters;

namespace Grout.UMP.Controllers
{
    [Authorize]
    [AdminActionFilter]
    public class PermissionController : Controller
    {
        private readonly PermissionSet permissionBase = new PermissionSet();
        private readonly PermissionModel permissionModel = new PermissionModel();
        private readonly JavaScriptSerializer javascriptSerializer = new JavaScriptSerializer();
        private readonly GroupManagement groupManagement = new GroupManagement();
        private readonly UserManagement userManagement = new UserManagement();

        public ActionResult UserPermission()
        {
            var userId = Convert.ToInt32(Request["userid"]);
            var userDetail = userManagement.FindUserByUserId(userId);
            if (userDetail != null)
            {
                ViewBag.DisplayName = userDetail.DisplayName;
                ViewBag.UserName = userDetail.UserName;
                return View();
            }

            return RedirectToAction("Index", "UserManagement");
        }

        public ActionResult GroupPermission()
        {
            var groupId = Convert.ToInt32(Request["groupid"]);
            var groupDetail = groupManagement.GetGroupById(groupId);
            if (groupDetail != null)
            {
                ViewBag.groupName = groupDetail.GroupName;

                return View();
            }
            return RedirectToAction("Group", "Group");
        }

        public PartialViewResult AddGroupPermissionView(string groupId)
        {
            ViewBag.groupId = groupId;
            ViewBag.PermissionEntity = permissionModel.GetPermissionEntity();
            return PartialView("_AddGroupPermission");
        }

        public PartialViewResult AddUserPermissionView(string userId)
        {
            ViewBag.userId = userId;
            ViewBag.PermissionEntity = permissionModel.GetPermissionEntity();
            return PartialView("_AddUserPermission");
        }

        public JsonResult GetAllGroupPermission(int groupId, int? skip, int? take, bool requiresCounts, List<SortCollection> sorted)
        {
            var permissions = permissionBase.GetGroupPermission(groupId).Select(
                    a => new Permission
                    {
                        PermissionAccessDescription = GlobalAppSettings.GetDescription(a.PermissionAccess),
                        PermissionEntityDescription = GlobalAppSettings.GetDescription(a.PermissionEntity),
                        TargetId = a.TargetId,
                        ItemId = a.ItemId,
                        ItemName = a.ItemName,
                        IsUserPermission = a.IsUserPermission,
                        PermissionId = a.PermissionId
                    }).ToList();

            var permissionCount = permissions.Count();

            if (sorted != null && sorted.Any())
            {
                var sortKey = sorted.ElementAt(0).Direction.ToLower();
                switch (sorted.ElementAt(0).Name.ToLower())
                {
                    case "permissionaccessdescription":
                        permissions = sortKey == "descending" ? permissions.OrderByDescending(s => s.PermissionAccessDescription).ToList() : permissions.OrderBy(s => s.PermissionAccessDescription).ToList();
                        break;

                    case "permissionentitydescription":
                        permissions = sortKey == "descending" ? permissions.OrderByDescending(s => s.PermissionEntityDescription).ToList() : permissions.OrderBy(s => s.PermissionEntityDescription).ToList();
                        break;

                    case "itemname":
                        permissions = sortKey == "descending" ? permissions.OrderByDescending(s => s.ItemName).ToList() : permissions.OrderBy(s => s.ItemName).ToList();
                        break;

                    default:
                        permissions = sortKey == "descending" ? permissions.OrderByDescending(s => s.PermissionAccessDescription).ToList() : permissions.OrderBy(s => s.PermissionAccessDescription).ToList();
                        break;
                }
            }
            else
            {
                permissions = permissions = permissions.OrderBy(s => s.PermissionEntityDescription).ToList();
            }

            if (requiresCounts)
            {
                return Json(new EntityData<Permission>
                {
                    result =
                        permissionCount > skip.GetValueOrDefault()
                            ? permissions.Skip(skip.GetValueOrDefault()).Take(take.GetValueOrDefault()).ToList()
                            : permissions.Skip(permissionCount != 0
                                ? (permissionCount % 10 != 0 ? ((permissionCount / 10) * 10) : (((permissionCount / 10) - 1) * 10))
                                : 0).Take(take.GetValueOrDefault()).ToList(),
                    count = permissionCount
                });
            }
            else
            {
                return Json(new EntityData<Permission>
                {
                    result = permissions,
                    count = permissionCount
                });
            }
        }

        public bool AddNewGroupPermission(int mode, int entity, Guid? scopeId, int groupId)
        {
            var permissionVal = new Permission
            {
                IsUserPermission = true,
                ItemType = ItemType.Category,
                ItemId = scopeId,
                PermissionAccess = (PermissionAccess)mode,
                PermissionEntity = (PermissionEntity)entity,
                TargetId = groupId
            };

            var isPermissionExist = permissionBase.IsGroupPermissionExist(permissionVal);
            var result = false;

            if (!isPermissionExist)
                result = permissionBase.AddPermissionToGroup(permissionVal);

            return result;
        }

        public bool DeleteGroupPermission(int permissionId)
        {
            var permissionIdList = new List<Permission>
            {
                new Permission {PermissionId = permissionId}
            };

            return permissionBase.RemovePermissionFromGroup(permissionIdList);
        }

        public string GetPermissionEntity(int accessMode)
        {
            return permissionModel.GetPermissionEntity(accessMode);
        }

        public JsonResult GetAllUserPermission(int userId, int? skip, int? take, bool requiresCounts, List<SortCollection> sorted)
        {
            var permissions = permissionBase.GetUserPermission(userId).Select(
                a => new Permission
                {
                    PermissionAccessDescription = GlobalAppSettings.GetDescription(a.PermissionAccess),
                    PermissionEntityDescription = GlobalAppSettings.GetDescription(a.PermissionEntity),
                    TargetId = a.TargetId,
                    ItemId = a.ItemId,
                    ItemName = a.ItemName,
                    IsUserPermission = a.IsUserPermission,
                    PermissionId = a.PermissionId,
                    GroupName = a.GroupName
                }).ToList();

            var permissionCount = permissions.Count();

            if (sorted != null && sorted.Any())
            {
                var sortKey = sorted.ElementAt(0).Direction.ToLower();
                switch (sorted.ElementAt(0).Name.ToLower())
                {
                    case "permissionaccessdescription":
                        permissions = sortKey == "descending" ? permissions.OrderByDescending(s => s.PermissionAccessDescription).ToList() : permissions.OrderBy(s => s.PermissionAccessDescription).ToList();
                        break;

                    case "permissionentitydescription":
                        permissions = sortKey == "descending" ? permissions.OrderByDescending(s => s.PermissionEntityDescription).ToList() : permissions.OrderBy(s => s.PermissionEntityDescription).ToList();
                        break;

                    case "itemname":
                        permissions = sortKey == "descending" ? permissions.OrderByDescending(s => s.ItemName).ToList() : permissions.OrderBy(s => s.ItemName).ToList();
                        break;

                    default:
                        permissions = sortKey == "descending" ? permissions.OrderByDescending(s => s.PermissionAccessDescription).ToList() : permissions.OrderBy(s => s.PermissionAccessDescription).ToList();
                        break;
                }
            }
            else
            {
                permissions = permissions.OrderByDescending(s => s.IsUserPermission).ThenBy(s => s.PermissionEntityDescription).ToList();
            }

            if (requiresCounts)
            {
                return Json(new EntityData<Permission>
                {
                    result =
                        permissionCount > skip.GetValueOrDefault()
                            ? permissions.Skip(skip.GetValueOrDefault()).Take(take.GetValueOrDefault()).ToList()
                            : permissions.Skip(permissionCount != 0
                                ? (permissionCount % 10 != 0 ? ((permissionCount / 10) * 10) : (((permissionCount / 10) - 1) * 10))
                                : 0).Take(take.GetValueOrDefault()).ToList(),
                    count = permissionCount
                });
            }
            else
            {
                return Json(new EntityData<Permission>
                {
                    result = permissions,
                    count = permissionCount
                });
            }
        }

        public string GetItemScope(int entityId)
        {
            return permissionModel.GetItemScope((PermissionEntity)entityId);
        }

        public bool AddNewPermission(int mode, int entity, Guid? scopeId, int userId)
        {
            var permissionVal = new Permission
                                {
                                    IsUserPermission = true,
                                    ItemType = ItemType.Category,
                                    ItemId = scopeId,
                                    PermissionAccess = (PermissionAccess)mode,
                                    PermissionEntity = (PermissionEntity)entity,
                                    TargetId = userId
                                };
            var isPermissionExist = permissionBase.IsUserPermissionExist(permissionVal);
            var result = false;

            if (!isPermissionExist)
                result = permissionBase.AddPermissionToUser(permissionVal);

            return result;
        }

        public bool DeleteUserPermission(int permissionId)
        {
            var permissionIdList = new List<Permission> {
                new Permission
                {
                    PermissionId = permissionId
                }
            };

            return permissionBase.RemovePermissionFromUser(permissionIdList);
        }
    }
}
