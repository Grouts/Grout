using System;
using System.Data;

namespace Grout.Base.DataClasses
{
    public class Result
    {
        public bool Status { get; set; }
        public Exception Exception { get; set; }
        public DataTable DataTable { get; set; }
        public object ReturnValue { get; set; }

        public Result()
        {
            DataTable = new DataTable();
        }
    }
}
