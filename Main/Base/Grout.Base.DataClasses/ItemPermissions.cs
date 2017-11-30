using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grout.Base.DataClasses
{
    public class ItemPermissions
    {
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
        public bool CanSchedule { get; set; }
        public bool CanOpen { get; set; }
        public bool CanMove { get; set; }
        public bool CanCopy { get; set; }
        public bool CanClone { get; set; }
    }
}
