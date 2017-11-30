using System;
using System.Collections.Generic;
using Grout.Base.DataClasses;

namespace Grout.Base.Reports.Scheduler
{
    public class ScheduleRecipientComparer : IEqualityComparer<ScheduleRecipientDetail>
    {
        public bool Equals(ScheduleRecipientDetail x, ScheduleRecipientDetail y)
        {
            return x.UserId == y.UserId;
        }

        public int GetHashCode(ScheduleRecipientDetail obj)
        {
            return (int)obj.UserId;
        }
    }
}
