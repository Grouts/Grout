namespace Grout.Base.DataClasses
{
    /// <summary>
    ///     Model class for column in select statement
    /// </summary>
    public class SelectedColumn
    {
        /// <summary>
        ///     Gets or sets the table name from which the column need to be selected( Need to set only if the column belongs to a
        ///     different table in join query)
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        ///     Gets or sets the column name
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        ///     Gets or sets the column Alias name
        /// </summary>
        public string AliasName { get; set; }

        /// <summary>
        ///     Gets or sets the aggrigation method to apply on the column
        /// </summary>
        public AggregateMethods Aggregation { get; set; }

        /// <summary>
        ///     Gets or sets whether the returned column should be distinct
        /// </summary>
        public bool IsDistinct { get; set; }

        public string JoinAliasName { get; set; }
    }
}