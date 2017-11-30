namespace Grout.Base.DataClasses
{
    public class ForeignKeyRelation
    {
        /// <summary>
        ///     Gets or sets the table name from which the foreign key is refered
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        ///     Gets or sets the column name to be refered as foreign key in the current table
        /// </summary>
        public string ColumnName { get; set; }
    }
}