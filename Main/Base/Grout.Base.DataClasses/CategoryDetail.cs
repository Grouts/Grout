using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grout.Base.DataClasses
{
    public class CategoryDetail : ItemPermissions
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public string TrimmedName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(Name) == false)
                {
                    return Name.Length > 18 ? Name.Substring(0, 18) + "..." : Name;
                }
                return String.Empty;
            }
            set
            {

            }
        }

        public string Description { get; set; }

        public int ReportsCount { get; set; }

        public ItemType ItemType
        {
            get
            {
                return ItemType.Category;
            }
        }
    }
}
