using System.Collections.Generic;

namespace Grout.Base.DataClasses
{
    /// <summary>
    ///     Model class for
    /// </summary>
    public class ConditionColumn
    {
        /// <summary>
        ///     Gets or sets Column Name
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        ///     Gets or sets the Value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        ///     Gets or sets the Value
        /// </summary>
        public List<string> Values { get; set; }

        /// <summary>
        ///     Gets or sets the Aggregation Method
        /// </summary>
        public AggregateMethods Aggregation { get; set; }

        /// <summary>
        ///     Gets or sets the Operator condition
        /// </summary>
        public Conditions Condition { get; set; }

        /// <summary>
        ///     Gets or sets the Logical Operator
        /// </summary>
        public LogicalOperators LogicalOperator { get; set; }

        /// <summary>
        ///     Gets or sets the name of the table the column belongs to ( Need to set only if the column belongs to a different
        ///     table in join query)
        /// </summary>
        public string TableName { get; set; }
    }
}