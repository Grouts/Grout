using System;
namespace Grout.Base.DataClasses
{
    public class ItemVersion
    {
        public Guid ItemId { get; set; }

        public string ItemName { get; set; }

        public int VersionNumber { get; set; }

        public int RollBackVersionNumber { get; set; }

        public bool IsCurrentVersion { get; set; }

        public int CreatedById { get; set; }

        public bool IsActive { get; set; }

        public string Comment { get; set; }

        public string CreatedByName { get; set; }

        public bool CanWrite { get; set; }

        public ItemType ItemTypeId { get; set; }

        public string CreatedDateString { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
