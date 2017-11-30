using System;

namespace Grout.Base.DataClasses
{
    public class GlobalSearchResult
    {
        public Guid ItemId { get; set; }

        public ItemType ItemType { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int CreatedById { get; set; }

        public string CreatedByDisplayName { get; set; }

        public bool CanRead { get; set; }

        public bool CanWrite { get; set; }

        public bool CanDelete { get; set; }

        public bool IsCategory { get; set; }

        public int GroupId { get; set; }

        public Guid CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string CategoryDescription { get; set; }

        public int ItemCount { get; set; }

        public int UserId { get; set; }

        public int ModifiedById { get; set; }

        public string ModifiedByFullName { get; set; }

        public string CreatedDate { get; set; }

        public string ModifiedDate { get; set; }

        public bool IsCreatedByActive { get; set; }

        public bool IsModifiedByActive { get; set; }

        public int TotalRecordCount { get; set; }
    }
}
