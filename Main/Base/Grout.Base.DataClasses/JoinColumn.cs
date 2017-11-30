namespace Grout.Base.DataClasses
{
    /// <summary>
    ///     Model class for Joined column
    /// </summary>
    public class JoinColumn
    {
        private Conditions _operation = Conditions.Equals;

        /// <summary>
        ///     Table Name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        ///     Joined table name from child table
        /// </summary>
        public string JoinedColumn { get; set; }

        /// <summary>
        ///     Parent table column name
        /// </summary>
        public string ParentTable { get; set; }

        /// <summary>
        ///     Parent table column name
        /// </summary>
        public string ParentTableColumn { get; set; }

        /// <summary>
        /// Logical operators
        /// </summary>
        public LogicalOperators LogicalOperator { get; set; }

        /// <summary>
        /// CondtionValue
        /// </summary>
        public object ConditionValue { get; set; }

        /// <summary>
        ///     Gets or sets the operator for the columns
        /// </summary>
        public Conditions Operation
        {
            get { return _operation; }
            set { _operation = value; }
        }
    }
}
