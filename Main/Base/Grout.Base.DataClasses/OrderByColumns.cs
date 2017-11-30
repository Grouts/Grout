namespace Grout.Base.DataClasses
{
    /// <summary>
    ///     Model class for Order by columns
    /// </summary>
    public class OrderByColumns
    {
        /// <summary>
        ///     Gets or sets the Column Name
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        ///     Gets or sets the order by direction
        /// </summary>
        public OrderByType OrderBy { get; set; }

        /// <summary>
        ///     Gets or sets the table name where the column belongs to ( Need to set only if the column belongs to a different
        ///     table in join query)
        /// </summary>
        public string TableName { get; set; }
    }
}