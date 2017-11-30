using System;
using System.ComponentModel;

namespace Grout.Base.DataClasses
{
    public class Permission
    {
        public int PermissionId { get; set; }
        public PermissionAccess PermissionAccess { get; set; }
        public PermissionEntity PermissionEntity { get; set; }
        public string PermissionAccessDescription { get; set; }
        public string PermissionEntityDescription { get; set; }
        public Guid? ItemId { get; set; }
        public string ItemName { get; set; }
        public ItemType ItemType { get; set; }
        public bool IsUserPermission { get; set; }
        public int TargetId { get; set; }
        public string GroupName { get; set; }
    }
}
