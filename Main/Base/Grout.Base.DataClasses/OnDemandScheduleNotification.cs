using System.Collections.Generic;

namespace Grout.Base.DataClasses
{
    public class OnDemandScheduleNotification
    {
        public List<int> UsersList { get; set; }

        public string Message { get; set; }

        public Status Status { get; set; }

    }
}
