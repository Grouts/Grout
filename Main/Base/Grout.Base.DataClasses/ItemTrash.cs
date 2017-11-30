using System;

namespace Grout.Base.DataClasses
{
    public class ItemTrash
    {
        public int Id { get; set; }

        public Guid ItemId { get; set; }

        public int TrashedById { get; set; }

        public string TrashedDate { get; set; }

        public bool IsActive { get; set; }
    }
}
