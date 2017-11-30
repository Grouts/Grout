using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Grout.Base.DataClasses;

namespace Grout.Base
{
    interface IPermissionSet
    {
        bool AddPermissionToUser(Permission permission);
        bool AddPermissionToGroup(Permission permission);        
        bool RemovePermissionFromUser(List<Permission> permission);
        bool RemovePermissionFromGroup(List<Permission> permission);
        List<Permission> GetGroupPermission(int groupId);
        List<Permission> GetUserPermission(int userId);
        bool IsGroupPermissionExist(Permission permission);
        bool IsUserPermissionExist(Permission permission);
    }
}
