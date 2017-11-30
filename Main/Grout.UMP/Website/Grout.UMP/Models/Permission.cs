using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Script.Serialization;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Item;

namespace Grout.UMP.Models
{
    public class PermissionModel
    {
        private readonly ItemManagement _itemManagement = new ItemManagement();
        private readonly JavaScriptSerializer _javascriptSerializer = new JavaScriptSerializer();
        private readonly PermissionSet _permissionBase = new PermissionSet();

        public string GetItemScope(PermissionEntity permissionEntity)
        {
            var result = new List<ItemDetail>();
            if (permissionEntity == PermissionEntity.SpecificCategory ||                
                permissionEntity == PermissionEntity.ReportsInCategory)
            {
                result = _itemManagement.GetAllItems(ItemType.Category);
            }
            else if (permissionEntity == PermissionEntity.SpecificDataSource)
            {
                result = _itemManagement.GetAllItems(ItemType.Datasource);
            }
            else if (permissionEntity == PermissionEntity.SpecificFiles)
            {
                result = _itemManagement.GetAllItems(ItemType.File);
            }
            else if (permissionEntity == PermissionEntity.SpecificReports)
            {
                result = _itemManagement.GetAllItems(ItemType.Report);
            }
            else if (permissionEntity == PermissionEntity.SpecificSchedule)
            {
                result = _itemManagement.GetAllItems(ItemType.Schedule);
            }

            return _javascriptSerializer.Serialize(result);
        }

        public string GetPermissionEntity(int accessMode = (int) PermissionAccess.Read)
        {
            var permissionEntity = _permissionBase.GetPermissionEntity(accessMode).AsEnumerable()
                .Select(row => new
                {
                    Id = row.Field<int>(GlobalAppSettings.DbColumns.DB_PermissionEntity.Id),
                    Name = row.Field<string>(GlobalAppSettings.DbColumns.DB_PermissionEntity.Name),
                    Type = row.Field<int>(GlobalAppSettings.DbColumns.DB_PermissionEntity.EntityType)
                }).OrderBy(s => s.Name).ToList();
			permissionEntity.Remove(permissionEntity.Find(f => f.Name.ToString() == GlobalAppSettings.GetDescription(PermissionEntity.AllDashboards)));
            permissionEntity.Remove(permissionEntity.Find(f => f.Name.ToString() == GlobalAppSettings.GetDescription(PermissionEntity.SpecificDashboard)));
            permissionEntity.Remove(permissionEntity.Find(f => f.Name.ToString() == GlobalAppSettings.GetDescription(PermissionEntity.DashboardsInCategory)));
            return _javascriptSerializer.Serialize(permissionEntity);
        }
    }
}