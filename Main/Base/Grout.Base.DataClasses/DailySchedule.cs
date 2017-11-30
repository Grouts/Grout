using System;
namespace Grout.Base.DataClasses
{
    [Serializable]
    public class DailySchedule : IDailyScheduler
    {
        public int DaysInterval { get; set; }
    }
}
