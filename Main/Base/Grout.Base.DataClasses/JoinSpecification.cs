using System.Collections.Generic;

namespace Grout.Base.DataClasses
{
    /// <summary>
    ///     Model class for Join specification
    /// </summary>
    public class JoinSpecification
    {
        /// <summary>
        ///     Gets or sets the Join Type
        /// </summary>
        public JoinTypes JoinType { get; set; }

        /// <summary>
        ///     Gets or sets the joined tables collection
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        ///     Gets or sets the Mapping columns for join
        /// </summary>
        public List<JoinColumn> Column { get; set; }

        public string JoinTableAliasName { get; set; }
    }
}