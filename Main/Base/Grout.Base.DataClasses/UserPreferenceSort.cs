using System;

namespace Grout.Base.DataClasses
{
    [Serializable]
    public class UserPreferenceSort
    {
        public string SortAttribute { get; set; }
        public string SortValue { get; set; }
    }
}
