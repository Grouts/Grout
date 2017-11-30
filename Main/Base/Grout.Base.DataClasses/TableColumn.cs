namespace Grout.Base.DataClasses
{
    /// <summary>
    ///     Model class for new column of a table
    /// </summary>
    public class TableColumn
    {
        /// <summary>
        ///     Gets or sets the column name
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        ///     Gets or sets the colum data type
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        ///     Gets or sets the supported data length
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        ///     Gets or sets whether the column should acccept NULL values
        /// </summary>
        public bool NotNull { get; set; }

        /// <summary>
        ///     Gets or sets if the column is primary key
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        ///     Gets or sets if thye column is unique
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        ///     Gets or sets whether the column is identity
        /// </summary>
        public bool isIdentity { get; set; }

        /// <summary>
        ///     Gets or sets te IdentitySpecification class object
        /// </summary>
        public IdentitySpecification Identity { get; set; }

        /// <summary>
        ///     Gets or sets whether the column has foreign key relation
        /// </summary>
        public bool HasForeignKeyRelation { get; set; }

        /// <summary>
        ///     Gets or sets the ForeignKeyRelation class object
        /// </summary>
        public ForeignKeyRelation ForeignKey { get; set; }
    }
}