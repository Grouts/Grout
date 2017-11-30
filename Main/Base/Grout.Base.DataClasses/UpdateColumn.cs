namespace Grout.Base.DataClasses
{
    /// <summary>
    ///     Model class for column to update
    /// </summary>
    public class UpdateColumn
    {
        /// <summary>
        ///     Gets or sets the column name
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        ///     Gets or sets the row cell value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        ///     Gets or sets the name of the table the column belongs to (Optional)
        /// </summary>
        public string TableName { get; set; }
    }
}