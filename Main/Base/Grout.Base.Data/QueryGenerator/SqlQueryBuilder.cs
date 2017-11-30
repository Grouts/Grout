using Grout.Base.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Grout.Base.Data
{
    public class SqlQueryBuilder : IQueryBuilder
    {
        #region Private Variables

        Regex regexName = new Regex("^[a-zA-Z_]*$");

        #endregion

        #region Private Methods

        /// <summary>
        ///     Returns mathematical operator for the given condition
        /// </summary>
        /// <param name="condition">Conditions Enum</param>
        /// <returns>Mathematical operator as string</returns>
        private static string GetConditionOperator(Conditions condition)
        {
            switch (condition)
            {
                case Conditions.Equals:
                    return "=";

                case Conditions.GreaterThan:
                    return ">";

                case Conditions.GreaterThanOrEquals:
                    return ">=";

                case Conditions.LessThan:
                    return "<";

                case Conditions.LessThanOrEquals:
                    return "<=";

                case Conditions.NotEquals:
                    return "!=";

                case Conditions.IS:
                    return " IS ";

                case Conditions.IN:
                    return " IN ";

                case Conditions.LIKE:
                    return " LIKE ";

                case Conditions.NOTIN:
                    return " NOT IN ";

                default:
                    return String.Empty;
            }
        }

        /// <summary>
        ///     Checks if the value is a number type
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>True if the object is a number type</returns>
        private static bool IsNumber(object value)
        {
            if (value == null)
                return false;
            return (
                value is double ||
                value is int ||
                value is short ||
                value is long ||
                value is decimal
                );
        }

        #endregion Private Methods

        public string AddToTable(string tableName, Dictionary<string, object> values)
        {
            if (String.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("Table Name is null");
            }
            else
            {
                tableName = tableName.Trim();

                if (!regexName.IsMatch(tableName))
                {
                    throw new ArgumentException("Table name should not contain special characters");
                }
            }
            if (tableName.Contains(" "))
                throw new ArgumentException("Table Name has whitespace");

            var queryString = new StringBuilder();
            queryString.Append("INSERT INTO ");
            queryString.Append("[" + tableName.Replace("'", "") + "] (");
            var columnValues = new StringBuilder();
            var counter = 0;

            if (values == null || values.Count <= 0)
            {
                throw new ArgumentNullException("The Values should not be null");
            }
            foreach (var value in values)
            {
                if (String.IsNullOrWhiteSpace(value.Key))
                {
                    throw new ArgumentNullException("The key field should not be null");
                }
                else
                {
                    if (value.Key.Trim().Contains(" "))
                        throw new ArgumentException("Column Name has whitespace");

                    if (!regexName.IsMatch(value.Key))
                    {
                        throw new ArgumentException("Column name should not contain special characters");
                    }
                }

                queryString.Append("[" + value.Key.Trim() + "]");
                columnValues.Append(value.Value == DBNull.Value
                    ? "Null" : ((IsNumber(value.Value))
                    ? value.Value : "'" + value.Value + "'"));

                if (counter != values.Keys.Count - 1)
                {
                    queryString.Append(",");
                    columnValues.Append(",");
                }
                counter++;
            }
            queryString.Append(") VALUES (");
            queryString.Append(columnValues);
            queryString.Append(")");
            return queryString.ToString();
        }

        public string AddToTableWithGUID(string tableName, Dictionary<string, object> values, string guidColumn, Guid? guid = null)
        {
            if (String.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("Table Name is null");
            }
            else
            {
                tableName = tableName.Trim();

                if (!regexName.IsMatch(tableName))
                {
                    throw new ArgumentException("Table name should not contain special characters");
                }
            }
            if (tableName.Contains(" "))
                throw new ArgumentException("Table Name has whitespace");

            var queryString = new StringBuilder();
            queryString.Append("INSERT INTO ");
            queryString.Append("[" + tableName.Replace("'", "") + "] (");
            var columnValues = new StringBuilder();
            var counter = 0;
            if (values == null || values.Count <= 0)
            {
                throw new ArgumentNullException("The Values should not be null");
            }
            foreach (var value in values)
            {
                if (String.IsNullOrWhiteSpace(value.Key))
                {
                    throw new ArgumentNullException("The key field should not be null");
                }
                else
                {
                    if (value.Key.Trim().Contains(" "))
                        throw new ArgumentException("Column Name has whitespace");

                    if (!regexName.IsMatch(value.Key))
                    {
                        throw new ArgumentException("Column name should not contain special characters");
                    }
                }

                queryString.Append("[" + value.Key + "]");

                columnValues.Append(value.Value == DBNull.Value ? "Null"
                        : ((IsNumber(value.Value)) ? value.Value : "'" + value.Value + "'"));

                if (counter != values.Keys.Count - 1)
                {
                    queryString.Append(",");
                    columnValues.Append(",");
                }
                counter++;
            }
            if (String.IsNullOrWhiteSpace(guidColumn))
            {
                throw new ArgumentNullException("Guid Column Name is null");
            }
            else
            {
                guidColumn = guidColumn.Trim();

                if (!regexName.IsMatch(guidColumn))
                {
                    throw new ArgumentException("Guid Column name should not contain special characters");
                }
            }
            if (guidColumn.Contains(" "))
                throw new ArgumentException("Guid Column Name has whitespace");

            queryString.Append(",[" + guidColumn + "]) OUTPUT Inserted." + guidColumn + " VALUES (" + columnValues + ",NEWID())");
            return queryString.ToString();
        }

        public string AddToTable(string tableName, Dictionary<string, object> values, List<string> outputColumns)
        {
            if (String.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("Table Name is null");
            }
            else
            {
                tableName = tableName.Trim();

                if (!regexName.IsMatch(tableName))
                {
                    throw new ArgumentException("Table name should not contain special characters");
                }
            }
            if (tableName.Contains(" "))
                throw new ArgumentException("Table Name has whitespace");

            var queryString = new StringBuilder();
            queryString.Append("INSERT INTO ");
            queryString.Append("[" + tableName.Replace("'", "") + "] (");
            var columnValues = new StringBuilder();
            int counter = 0;

            if (values == null || values.Count <= 0)
            {
                throw new ArgumentNullException("The Values should not be null");
            }
            foreach (var value in values)
            {
                if (String.IsNullOrWhiteSpace(value.Key))
                {
                    throw new ArgumentNullException("The key field should not be null");
                }
                else
                {
                    if (value.Key.Trim().Contains(" "))
                        throw new ArgumentException("Column Name has whitespace");

                    if (!regexName.IsMatch(value.Key))
                    {
                        throw new ArgumentException("Column name should not contain special characters");
                    }
                }
                queryString.Append("[" + value.Key + "]");
                columnValues.Append(value.Value == DBNull.Value ? "Null"
                        : ((IsNumber(value.Value)) ? value.Value : "'" + value.Value + "'"));
                if (counter != values.Keys.Count - 1)
                {
                    queryString.Append(",");
                    columnValues.Append(",");
                }
                counter++;
            }
            queryString.Append(")");
            if (outputColumns != null)
            {
                queryString.Append(" OUTPUT ");
                for (int i = 0; i < outputColumns.Count; i++)
                {
                    if (String.IsNullOrWhiteSpace(outputColumns[i]))
                    {
                        throw new ArgumentNullException("Column Name is null");
                    }
                    else
                    {
                        outputColumns[i] = outputColumns[i].Trim();

                        if (!regexName.IsMatch(outputColumns[i]))
                        {
                            throw new ArgumentException("Column name should not contain special characters");
                        }
                    }
                    if (outputColumns[i].Contains(" "))
                        throw new ArgumentException("Column Name has whitespace");

                    queryString.Append(" Inserted." + outputColumns[i]);
                    if (i != outputColumns.Count - 1)
                        queryString.Append(",");
                }
            }
            queryString.Append(" VALUES (");
            queryString.Append(columnValues);
            queryString.Append(")");
            return queryString.ToString();
        }

        public string UpdateRowInTable(string tableName, List<UpdateColumn> updateColumns,
            List<ConditionColumn> whereConditionColumns)
        {
            var queryString = new StringBuilder();
            queryString.Append("UPDATE ");
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                tableName = tableName.Trim();

                if (!regexName.IsMatch(tableName))
                {
                    throw new ArgumentException("Table name should not contain special characters");
                }

                if (tableName.Contains(" "))
                    throw new ArgumentException("Table Name has whitespace");

                queryString.Append("[" + tableName + "]");
                queryString.Append(" SET ");
                for (var i = 0; i < updateColumns.Count; i++)
                {
                    if (String.IsNullOrWhiteSpace(updateColumns[i].ColumnName))
                    {
                        throw new ArgumentNullException("The column name should not be null");
                    }
                    else
                    {
                        updateColumns[i].ColumnName = updateColumns[i].ColumnName.Trim();
                        if (updateColumns[i].ColumnName.Contains(" "))
                            throw new ArgumentException("Column Name has whitespace");

                        if (!regexName.IsMatch(updateColumns[i].ColumnName))
                        {
                            throw new ArgumentException("Column name should not contain special characters");
                        }
                    }

                    queryString.Append("[" + updateColumns[i].ColumnName + "]=");
                    queryString.Append(updateColumns[i].Value == DBNull.Value ? "Null"
                        : ((IsNumber(updateColumns[i].Value)) ? updateColumns[i].Value : "'" + updateColumns[i].Value + "'"));
                    if (i != updateColumns.Count - 1)
                    {
                        queryString.Append(",");
                    }
                }
            }
            else
            {
                throw new ArgumentNullException("The table name should not be null");
            }
            return ApplyWhereClause(queryString.ToString(), whereConditionColumns);
        }


        #region Create and Delete Tables

        public string CreateTable(string tableName, List<TableColumn> columns)
        {
            var queryString = new StringBuilder();
            queryString.Append("CREATE TABLE ");
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                tableName = tableName.Trim();

                if (!regexName.IsMatch(tableName))
                {
                    throw new ArgumentException("Table name should not contain special characters");
                }

                if (tableName.Contains(" "))
                    throw new ArgumentException("Table Name has whitespace");

                queryString.Append("[" + tableName + "](");
                for (var i = 0; i < columns.Count; i++)
                {
                    if (String.IsNullOrWhiteSpace(columns[i].ColumnName))
                    {
                        throw new ArgumentNullException("The column name should not be null");
                    }
                    else
                    {
                        columns[i].ColumnName = columns[i].ColumnName.Trim();
                        if (columns[i].ColumnName.Contains(" "))
                            throw new ArgumentException("Column Name has whitespace");

                        if (!regexName.IsMatch(columns[i].ColumnName))
                        {
                            throw new ArgumentException("Column name should not contain special characters");
                        }
                    }
                    queryString.Append("[" + columns[i].ColumnName + "] ");
                    queryString.Append((columns[i].Length != 0)
                        ? columns[i].DataType + "(" + columns[i].Length + ")"
                        : columns[i].DataType);
                    if (columns[i].IsUnique && !columns[i].IsPrimaryKey) queryString.Append(" UNIQUE ");
                    if (columns[i].isIdentity)
                    {
                        if (columns[i].Identity != null)
                        {
                            queryString.Append(" IDENTITY(" + columns[i].Identity.IdentitySeed + "," +
                                               columns[i].Identity.IdentityIncrement + ") ");
                        }
                        else
                        {
                            throw new ArgumentNullException("Identity is null");
                        }
                    }
                    if (columns[i].NotNull) queryString.Append(" NOT NULL ");
                    if (i != columns.Count - 1)
                    {
                        queryString.Append(",");
                    }
                }
                var primaryKeyColumns = columns.Where(i => i.IsPrimaryKey).ToList();
                var foreignKeyColumns = columns.Where(i => i.HasForeignKeyRelation).ToList();
                if (primaryKeyColumns != null)
                {
                    queryString.Append(" CONSTRAINT [" + tableName + "_PK] PRIMARY KEY CLUSTERED ( ");
                    for (var i = 0; i < primaryKeyColumns.Count; i++)
                    {
                        queryString.Append("[" + primaryKeyColumns[i].ColumnName + "] ASC");
                        if (i != primaryKeyColumns.Count - 1) queryString.Append(",");
                    }
                    queryString.Append(")");
                }
                if (foreignKeyColumns != null)
                {
                    foreach (var column in foreignKeyColumns)
                    {
                        if (String.IsNullOrWhiteSpace(column.ColumnName))
                        {
                            throw new ArgumentNullException("The column name should not be null");
                        }
                        else
                        {
                            column.ColumnName = column.ColumnName.Trim();
                            if (column.ColumnName.Contains(" "))
                                throw new ArgumentException("Column Name has whitespace");

                            if (!regexName.IsMatch(column.ColumnName))
                            {
                                throw new ArgumentException("Column name should not contain special characters");
                            }
                        }

                        if (String.IsNullOrWhiteSpace(column.ForeignKey.ColumnName))
                        {
                            throw new ArgumentNullException("The column name should not be null");
                        }
                        else
                        {
                            column.ForeignKey.ColumnName = column.ForeignKey.ColumnName.Trim();
                            if (column.ForeignKey.ColumnName.Contains(" "))
                                throw new ArgumentException("Column Name has whitespace");

                            if (!regexName.IsMatch(column.ForeignKey.ColumnName))
                            {
                                throw new ArgumentException("Column name should not contain special characters");
                            }
                        }

                        if (String.IsNullOrWhiteSpace(column.ForeignKey.TableName))
                        {
                            throw new ArgumentNullException("The table name should not be null");
                        }
                        else
                        {
                            column.ForeignKey.TableName = column.ForeignKey.TableName.Trim();
                            if (column.ForeignKey.TableName.Contains(" "))
                                throw new ArgumentException("Table Name has whitespace");

                            if (!regexName.IsMatch(column.ForeignKey.TableName))
                            {
                                throw new ArgumentException("Table name should not contain special characters");
                            }
                        }
                        queryString.Append(" CONSTRAINT [" + column.ColumnName + "_FK] FOREIGN KEY ");
                        queryString.Append("([" + column.ColumnName + "]) ");
                        queryString.Append("REFERENCES [" + column.ForeignKey.TableName + "] ([" +
                                           column.ForeignKey.ColumnName + "]) ");
                    }
                }
                queryString.Append(")");
            }
            else
            {
                throw new ArgumentNullException("The table name should not be null");
            }
            return queryString.ToString();
        }

        public string DeleteTableFromDB(string tableName)
        {
            if (String.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("Table Name is null");
            }
            else
            {
                tableName = tableName.Trim();

                if (!regexName.IsMatch(tableName))
                {
                    throw new ArgumentException("Table name should not contain special characters");
                }
            }
            if (tableName.Contains(" "))
                throw new ArgumentException("Table Name has whitespace");

            return "DROP TABLE [" + tableName.Replace("'", "") + "]";
        }

        #endregion

        #region Create and Delete Database

        public string CreateDatabase(string dbName)
        {
            if (String.IsNullOrWhiteSpace(dbName))
            {
                throw new ArgumentNullException("DataBase name is null");
            }

            return "CREATE DATABASE [" + dbName.Replace("'", "") + "]";
        }

        public string DeleteDatabase(string dbName)
        {
            if (String.IsNullOrWhiteSpace(dbName))
            {
                throw new ArgumentNullException("DataBase name is null");
            }

            return "DROP DATABASE [" + dbName.Replace("'", "") + "]";
        }

        #endregion

        #region Deleting Rows from Table

        public string DeleteRowFromTable(string tableName, List<ConditionColumn> row)
        {
            if (String.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("Table Name is null");
            }
            else
            {
                tableName = tableName.Trim();
                if (!regexName.IsMatch(tableName))
                {
                    throw new ArgumentException("Table name should not contain special characters");
                }
            }
            if (tableName.Contains(" "))
                throw new ArgumentException("Table Name has whitespace");

            var queryString = new StringBuilder();
            queryString.Append("DELETE FROM ");
            queryString.Append("[" + tableName.Replace("'", "") + "]");
            return ApplyWhereClause(queryString.ToString(), row);
        }

        public string DeleteAllRowsFromTable(string tableName)
        {
            if (String.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("Table Name is null");
            }
            else
            {
                tableName = tableName.Trim();
                if (!regexName.IsMatch(tableName))
                {
                    throw new ArgumentException("Table name should not contain special characters");
                }
            }
            if (tableName.Contains(" "))
                throw new ArgumentException("Table Name has whitespace");

            return "DELETE FROM [" + tableName + "]";
        }

        #endregion

        public string ApplyGroupBy(string query, List<GroupByColumn> columns)
        {
            var queryString = new StringBuilder();
            if (!string.IsNullOrEmpty(query))
            {
                queryString.Append(query);
                if (columns != null && columns.Count > 0)
                {
                    queryString.Append("  GROUP BY  ");
                    for (var i = 0; i < columns.Count; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(columns[i].TableName))
                        {
                            columns[i].TableName = columns[i].TableName.Trim();

                            if (!regexName.IsMatch(columns[i].TableName))
                            {
                                throw new ArgumentException("Table name should not contain special characters");
                            }

                            if (columns[i].TableName.Contains(" "))
                                throw new ArgumentException("Table Name has whitespace");
                        }

                        if (String.IsNullOrWhiteSpace(columns[i].ColumnName))
                        {
                            throw new ArgumentNullException("The column name should not be null");
                        }
                        else
                        {
                            columns[i].ColumnName = columns[i].ColumnName.Trim();
                            if (columns[i].ColumnName.Contains(" "))
                                throw new ArgumentException("Column Name has whitespace");

                            if (!regexName.IsMatch(columns[i].ColumnName))
                            {
                                throw new ArgumentException("Column name should not contain special characters");
                            }
                        }

                        queryString.Append(((!string.IsNullOrWhiteSpace(columns[i].TableName))
                            ? "[" + columns[i].TableName + "]."
                            : " ") + "[" + columns[i].ColumnName + "]");
                        if (i != columns.Count - 1)
                        {
                            queryString.Append(",");
                        }
                    }
                }
            }
            return queryString.ToString();
        }

        public string ApplyOrderBy(string query, List<OrderByColumns> columns)
        {
            var queryString = new StringBuilder();
            if (!string.IsNullOrEmpty(query))
            {
                queryString.Append(query);
                if (columns != null && columns.Count > 0)
                {
                    queryString.Append(" ORDER BY ");
                    for (var i = 0; i < columns.Count; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(columns[i].TableName))
                        {
                            columns[i].TableName = columns[i].TableName.Trim();

                            if (!regexName.IsMatch(columns[i].TableName))
                            {
                                throw new ArgumentException("Table name should not contain special characters");
                            }

                            if (columns[i].TableName.Contains(" "))
                                throw new ArgumentException("Table Name has whitespace");
                        }

                        if (String.IsNullOrWhiteSpace(columns[i].ColumnName))
                        {
                            throw new ArgumentNullException("The column name should not be null");
                        }
                        else
                        {
                            columns[i].ColumnName = columns[i].ColumnName.Trim();
                            if (columns[i].ColumnName.Contains(" "))
                                throw new ArgumentException("Column Name has whitespace");

                            if (!regexName.IsMatch(columns[i].ColumnName))
                            {
                                throw new ArgumentException("Column name should not contain special characters");
                            }
                        }

                        queryString.Append(((!string.IsNullOrWhiteSpace(columns[i].TableName))
                           ? "[" + columns[i].TableName + "]."
                           : " ") + "[" + columns[i].ColumnName + "]");

                        if (columns[i].OrderBy != OrderByType.None)
                            queryString.Append(" " + columns[i].OrderBy);
                        if (i != columns.Count - 1)
                        {
                            queryString.Append(",");
                        }
                    }
                }
            }
            return queryString.ToString();
        }

        public string ApplyWhereClause(string query, List<ConditionColumn> whereColumns)
        {
            var queryString = new StringBuilder();
            if (!string.IsNullOrEmpty(query))
            {
                queryString.Append(query);
                if (whereColumns != null && whereColumns.Count > 0)
                {
                    queryString.Append("  WHERE  ");
                    for (var i = 0; i < whereColumns.Count; i++)
                    {
                        if (whereColumns[i].Condition == Conditions.IN || whereColumns[i].Condition == Conditions.NOTIN)
                        {
                            if (whereColumns[i].LogicalOperator != LogicalOperators.None && i != 0)
                            {
                                queryString.Append(" " + whereColumns[i].LogicalOperator);
                            }
                            queryString.Append(((!string.IsNullOrWhiteSpace(whereColumns[i].TableName))
                                ? " [" + whereColumns[i].TableName + "]."
                                : " ") + "[" + whereColumns[i].ColumnName + "]");
                            queryString.Append(" " + GetConditionOperator(whereColumns[i].Condition));
                            queryString.Append("(");
                            if (whereColumns[i].Values != null)
                            {
                                for (var j = 0; j < whereColumns[i].Values.Count; j++)
                                {
                                    if (j != 0)
                                    {
                                        queryString.Append(",");
                                    }
                                    queryString.Append("'" + whereColumns[i].Values[j] + "'");
                                }
                            }

                            queryString.Append(")");
                        }
                        else
                        {
                            if (whereColumns[i].LogicalOperator != LogicalOperators.None && i != 0)
                            {
                                queryString.Append(" " + whereColumns[i].LogicalOperator);
                            }
                            if (!String.IsNullOrWhiteSpace(whereColumns[i].TableName))
                            {
                                whereColumns[i].TableName = whereColumns[i].TableName.Trim();
                                if (!regexName.IsMatch(whereColumns[i].TableName))
                                {
                                    throw new ArgumentException("Table name should not contain special characters");
                                }
                                if (whereColumns[i].TableName.Contains(" "))
                                    throw new ArgumentException("Table Name has whitespace");
                            }
                            
                            queryString.Append(((!string.IsNullOrWhiteSpace(whereColumns[i].TableName))
                                ? " [" + whereColumns[i].TableName + "]."
                                : " ") + "[" + whereColumns[i].ColumnName + "]");
                            queryString.Append(GetConditionOperator(whereColumns[i].Condition));
                            if (whereColumns[i].Condition == Conditions.LIKE)
                                queryString.Append(whereColumns[i].Value == DBNull.Value
                               ? "Null" : ((IsNumber(whereColumns[i].Value))
                               ? whereColumns[i].Value : (whereColumns[i].Value == null) ? "%%" : "'%" + whereColumns[i].Value + "%'"));
                            else
                            queryString.Append(whereColumns[i].Value == DBNull.Value 
                                ? "Null" : ((IsNumber(whereColumns[i].Value)) 
                                ? whereColumns[i].Value : (whereColumns[i].Value == null) ? "NULL" : "'" + whereColumns[i].Value + "'"));
                        }
                    }
                }
            }
            return queryString.ToString();
        }

        public string ApplyHavingClause(string query, List<ConditionColumn> havingClauseColumns)
        {
            var queryString = new StringBuilder();
            if (!string.IsNullOrEmpty(query))
            {
                queryString.Append(query);
                if (havingClauseColumns != null && havingClauseColumns.Count > 0)
                {
                    queryString.Append(" HAVING ");
                    for (var i = 0; i < havingClauseColumns.Count; i++)
                    {
                        if (havingClauseColumns[i].LogicalOperator != LogicalOperators.None && i != 0)
                        {
                            queryString.Append(" " + havingClauseColumns[i].LogicalOperator);
                        }
                        if (havingClauseColumns[i].Aggregation != AggregateMethods.None)
                        {
                            queryString.Append(" " + havingClauseColumns[i].Aggregation);
                            queryString.Append("(" +
                                               ((!string.IsNullOrWhiteSpace(havingClauseColumns[i].TableName))
                                                   ? " [" + havingClauseColumns[i].TableName + "]."
                                                   : "") + "[" + havingClauseColumns[i].ColumnName + "])");
                        }
                        else
                            queryString.Append(((!string.IsNullOrWhiteSpace(havingClauseColumns[i].TableName))
                                ? " [" + havingClauseColumns[i].TableName + "]."
                                : " ") + "[" + havingClauseColumns[i].ColumnName + "]");
                        queryString.Append(GetConditionOperator(havingClauseColumns[i].Condition));

                        queryString.Append(havingClauseColumns[i].Value == DBNull.Value
                            ? "Null" : ((IsNumber(havingClauseColumns[i].Value))
                            ? havingClauseColumns[i].Value : "'" + havingClauseColumns[i].Value + "'"));
                    }
                }
            }
            return queryString.ToString();
        }

        #region Apply Joins Methods

        public string ApplyJoins(string query, JoinSpecification joinSpecification)
        {
            var queryString = new StringBuilder();
            if (joinSpecification.Table == null && joinSpecification.Column == null) return query;
            if (!String.IsNullOrWhiteSpace(query))
            {
                queryString.Append(query);
                queryString.Append(" " + joinSpecification.JoinType + " JOIN ");

                if (String.IsNullOrWhiteSpace(joinSpecification.Table))
                {
                    throw new ArgumentNullException("Table Name is null");
                }
                else
                {
                    joinSpecification.Table = joinSpecification.Table.Trim();

                    if (!regexName.IsMatch(joinSpecification.Table))
                    {
                        throw new ArgumentException("Table name should not contain special characters");
                    }
                }
                if (joinSpecification.Table.Contains(" "))
                    throw new ArgumentException("Table Name has whitespace");


                queryString.Append("[" + joinSpecification.Table + "]");
                if (!String.IsNullOrWhiteSpace(joinSpecification.JoinTableAliasName))
                    queryString.Append(" AS " + joinSpecification.JoinTableAliasName);

                queryString.Append(" ON ");
                queryString.Append("(");
                for (var j = 0; j < joinSpecification.Column.Count; j++)
                {
                   
                    var parentTable =
                        query.Substring((query.LastIndexOf("FROM") != -1)
                            ? query.LastIndexOf("FROM") + 4
                            : query.LastIndexOf("from") + 4);

                    if (joinSpecification.Column[j].LogicalOperator != LogicalOperators.None && j != 0)
                    {
                        queryString.Append(" " + joinSpecification.Column[j].LogicalOperator);
                    }

                    if (String.IsNullOrWhiteSpace(joinSpecification.Column[j].ParentTableColumn))
                    {
                        throw new ArgumentNullException("Table Name is null");
                    }
                    else
                    {
                        joinSpecification.Column[j].ParentTableColumn =
                            joinSpecification.Column[j].ParentTableColumn.Trim();

                        if (!regexName.IsMatch(joinSpecification.Column[j].ParentTableColumn))
                        {
                            throw new ArgumentException("Table name should not contain special characters");
                        }
                    }
                    if (joinSpecification.Column[j].ParentTableColumn.Contains(" "))
                        throw new ArgumentException("Table Name has whitespace");


                    queryString.Append(parentTable + ".[" + joinSpecification.Column[j].ParentTableColumn + "]" +
                                       GetConditionOperator(joinSpecification.Column[j].Operation));


                    if (!String.IsNullOrWhiteSpace(joinSpecification.JoinTableAliasName))
                    {
                        joinSpecification.JoinTableAliasName = joinSpecification.JoinTableAliasName.Trim();

                        if (!regexName.IsMatch(joinSpecification.JoinTableAliasName))
                        {
                            throw new ArgumentException("Alias name should not contain special characters");
                        }
                        if (joinSpecification.JoinTableAliasName.Contains(" "))
                            throw new ArgumentException("Alias Name has whitespace");
                    }

                    queryString.Append((joinSpecification.Column[j].ConditionValue!=null)
                        ? joinSpecification.Column[j].ConditionValue
                        : (!String.IsNullOrWhiteSpace(joinSpecification.JoinTableAliasName))
                            ? joinSpecification.JoinTableAliasName
                            : "[" + joinSpecification.Column[j].TableName + "]");

                    
                    queryString.Append((joinSpecification.Column[j].ConditionValue==null) ? ".[" + joinSpecification.Column[j].JoinedColumn + "]" : "");
                    
                }
                queryString.Append(")");
            }
            return queryString.ToString();
        }

        public string ApplyJoins(string tableName, List<SelectedColumn> columnsToSelect, JoinSpecification joinSpecification)
        {
            var query = SelectRecordsFromTable(tableName, columnsToSelect);
            return ApplyJoins(query, joinSpecification);
        }

        public string ApplyJoins(string tableName, List<SelectedColumn> columnsToSelect, JoinSpecification joinSpecification,
            List<ConditionColumn> whereClauseColumns)
        {
            var query = SelectRecordsFromTable(tableName, columnsToSelect);
            return ApplyWhereClause(ApplyJoins(query, joinSpecification), whereClauseColumns);
        }

        public string ApplyJoins(string tableName, List<SelectedColumn> columnsToSelect,
            List<ConditionColumn> whereClauseColumns, List<GroupByColumn> groupByColumns,
            List<OrderByColumns> orderByColumns, List<ConditionColumn> havingClauseColumns, JoinSpecification joinSpecification)
        {
            var query = SelectRecordsFromTable(tableName, columnsToSelect);
            query = ApplyJoins(query, joinSpecification);
            if (whereClauseColumns != null)
            {
                query = ApplyWhereClause(query, whereClauseColumns);
            }
            if (groupByColumns != null)
            {
                query = ApplyGroupBy(query, groupByColumns);
            }
            if (havingClauseColumns != null)
            {
                query = ApplyHavingClause(query, havingClauseColumns);
            }
            if (orderByColumns != null)
            {
                query = ApplyOrderBy(query, orderByColumns);
            }
            return query;
        }

        public string ApplyMultipleJoins(string query, List<JoinSpecification> joinSpecification)
        {
            var queryString = new StringBuilder();
            queryString.Append(query);
            for (var t = 0; t < joinSpecification.Count(); t++)
            {
                if (joinSpecification[t].Table == null && joinSpecification[t].Column == null) return query;
                if (!String.IsNullOrWhiteSpace(query))
                {
                    queryString.Append(" " + joinSpecification[t].JoinType + " JOIN ");

                    if (String.IsNullOrWhiteSpace(joinSpecification[t].Table))
                    {
                        throw new ArgumentNullException("Table Name is null");
                    }
                    else
                    {
                        joinSpecification[t].Table = joinSpecification[t].Table.Trim();

                        if (!regexName.IsMatch(joinSpecification[t].Table))
                        {
                            throw new ArgumentException("Table name should not contain special characters");
                        }
                    }
                    if (joinSpecification[t].Table.Contains(" "))
                        throw new ArgumentException("Table Name has whitespace");


                    queryString.Append("[" + joinSpecification[t].Table + "]");
                    if (!String.IsNullOrWhiteSpace(joinSpecification[t].JoinTableAliasName))
                        queryString.Append(" AS " + joinSpecification[t].JoinTableAliasName);
                    queryString.Append(" ON ");
                    queryString.Append("(");
                    for (var j = 0; j < joinSpecification[t].Column.Count; j++)
                    {

                        if (joinSpecification[t].Column[j].LogicalOperator != LogicalOperators.None && j != 0)
                        {
                            queryString.Append(" " + joinSpecification[t].Column[j].LogicalOperator);
                        }
                        
                        if (String.IsNullOrWhiteSpace(joinSpecification[t].Column[j].ParentTable))
                        {
                            throw new ArgumentNullException("Table Name is null");
                        }
                        else
                        {
                            joinSpecification[t].Column[j].ParentTable = joinSpecification[t].Column[j].ParentTable.Trim();

                            if (!regexName.IsMatch(joinSpecification[t].Column[j].ParentTable))
                            {
                                throw new ArgumentException("Table name should not contain special characters");
                            }
                        }
                        if (joinSpecification[t].Column[j].ParentTable.Contains(" "))
                            throw new ArgumentException("Table Name has whitespace");

                        if (String.IsNullOrWhiteSpace(joinSpecification[t].Column[j].ParentTableColumn))
                        {
                            throw new ArgumentNullException("Table Name is null");
                        }
                        else
                        {
                            joinSpecification[t].Column[j].ParentTableColumn =
                                joinSpecification[t].Column[j].ParentTableColumn.Trim();

                            if (!regexName.IsMatch(joinSpecification[t].Column[j].ParentTableColumn))
                            {
                                throw new ArgumentException("Table name should not contain special characters");
                            }
                        }
                        if (joinSpecification[t].Column[j].ParentTableColumn.Contains(" "))
                            throw new ArgumentException("Table Name has whitespace");


                        queryString.Append(" [" + joinSpecification[t].Column[j].ParentTable + "] " + ".[" +
                                           joinSpecification[t].Column[j].ParentTableColumn + "]" +
                                           GetConditionOperator(joinSpecification[t].Column[j].Operation));

                        
                        


                        if (!String.IsNullOrWhiteSpace(joinSpecification[t].JoinTableAliasName))
                        {
                            joinSpecification[t].JoinTableAliasName = joinSpecification[t].JoinTableAliasName.Trim();

                            if (!regexName.IsMatch(joinSpecification[t].JoinTableAliasName))
                            {
                                throw new ArgumentException("Alias name should not contain special characters");
                            }
                            if (joinSpecification[t].JoinTableAliasName.Contains(" "))
                                throw new ArgumentException("Alias Name has whitespace");
                        }
                        
                        queryString.Append((joinSpecification[t].Column[j].ConditionValue != null)
                            ? (IsNumber(joinSpecification[t].Column[j].ConditionValue)) ? joinSpecification[t].Column[j].ConditionValue : "'"+joinSpecification[t].Column[j].ConditionValue+"'"
                            : (!String.IsNullOrWhiteSpace(joinSpecification[t].JoinTableAliasName))
                                ? joinSpecification[t].JoinTableAliasName
                                : "[" + joinSpecification[t].Column[j].TableName + "]");
                        
                        queryString.Append((joinSpecification[t].Column[j].ConditionValue == null) ? ".[" + joinSpecification[t].Column[j].JoinedColumn + "]" : "");
                        
                        
                    }
                    queryString.Append(")");
                }
            }
            return queryString.ToString();
        }

        public string ApplyMultipleJoins(string tableName, List<SelectedColumn> columnsToSelect, List<JoinSpecification> joinSpecification)
        {
            var query = SelectRecordsFromTable(tableName, columnsToSelect);
            return ApplyMultipleJoins(query, joinSpecification);
        }

        #endregion

        #region Selection Methods

        public string SelectAllRecordsFromTable(string tableName)
        {
            if (String.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("Table Name is null");
            }
            else
            {
                tableName = tableName.Trim();

                if (!regexName.IsMatch(tableName))
                {
                    throw new ArgumentException("Table name should not contain special characters");
                }
            }
            if (tableName.Contains(" "))
                throw new ArgumentException("Table Name has whitespace");

            return "SELECT * FROM [" + tableName.Replace("'", "") + "]";
        }

        public string SelectRecordsFromTable(string tableName, List<SelectedColumn> columns)
        {
            var queryString = new StringBuilder();
            if (String.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("Table Name is null");
            }
            else
            {
                tableName = tableName.Trim();
                if (!regexName.IsMatch(tableName))
                {
                    throw new ArgumentException("Table name should not contain special characters");
                }
            }
            if (tableName.Contains(" "))
                throw new ArgumentException("Table Name has whitespace");

            queryString.Append("SELECT ");
            for (var i = 0; i < columns.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(columns[i].TableName))
                {
                    columns[i].TableName = columns[i].TableName.Trim();
                    if (!regexName.IsMatch(columns[i].TableName))
                    {
                        throw new ArgumentException("Table name should not contain special characters");
                    }
                    if (columns[i].TableName.Contains(" "))
                        throw new ArgumentException("Table Name has whitespace");
                }

                if (columns[i].IsDistinct) queryString.Append(" DISTINCT ");
                if (columns[i].Aggregation != AggregateMethods.None)
                {
                    queryString.Append(columns[i].Aggregation + "(" +
                                       ((!string.IsNullOrWhiteSpace(columns[i].TableName))
                                           ? "[" + columns[i].TableName + "]."
                                           : "") + columns[i].ColumnName + ")");
                }
                else
                    queryString.Append(
                        ((!string.IsNullOrWhiteSpace(columns[i].JoinAliasName))
                            ? columns[i].JoinAliasName + "."
                            : (!string.IsNullOrWhiteSpace(columns[i].TableName))
                                ? "[" + columns[i].TableName + "]."
                                : "") + columns[i].ColumnName);
                if (!string.IsNullOrWhiteSpace(columns[i].AliasName))
                    queryString.Append(" AS [" + columns[i].AliasName + "]");
                if (i != columns.Count - 1)
                {
                    queryString.Append(",");
                }
            }
            queryString.Append(" FROM ");
            queryString.Append("[" + tableName + "]");
            return queryString.ToString();
        }

        public string SelectAllRecordsFromTable(string tableName, AggregateMethods aggregationMethod)
        {
            if (String.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("Table Name is null");
            }
            else
            {
                tableName = tableName.Trim();

                if (!regexName.IsMatch(tableName))
                {
                    throw new ArgumentException("Table name should not contain special characters");
                }
            }
            if (tableName.Contains(" "))
                throw new ArgumentException("Table Name has whitespace");

            return "SELECT " + ((aggregationMethod != AggregateMethods.None) ? aggregationMethod + "(*)" : "*") +
                   " FROM [" + tableName + "]";
        }

        public string SelectTopRecordsFromTable(string tableName, List<SelectedColumn> columns, int numberOfRecords)
        {
            var queryString = new StringBuilder();

            if (String.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException("Table Name is null");
            }
            else
            {
                tableName = tableName.Trim();
                if (!regexName.IsMatch(tableName))
                {
                    throw new ArgumentException("Table name should not contain special characters");
                }
            }
            if (tableName.Contains(" "))
                throw new ArgumentException("Table Name has whitespace");

            queryString.Append("SELECT TOP ");
            queryString.Append(numberOfRecords);
            queryString.Append(" ");
            for (var i = 0; i < columns.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(columns[i].TableName))
                {
                    columns[i].TableName = columns[i].TableName.Trim();
                    if (!regexName.IsMatch(columns[i].TableName))
                    {
                        throw new ArgumentException("Table name should not contain special characters");
                    }
                    if (columns[i].TableName.Contains(" "))
                        throw new ArgumentException("Table Name has whitespace");
                }

                if (columns[i].IsDistinct) queryString.Append(" DISTINCT ");
                if (columns[i].Aggregation != AggregateMethods.None)
                {
                    queryString.Append(columns[i].Aggregation + "(" +
                                       ((!string.IsNullOrWhiteSpace(columns[i].TableName))
                                           ? "[" + columns[i].TableName + "]."
                                           : "") + columns[i].ColumnName + ")");
                }
                else
                    queryString.Append(
                        ((!string.IsNullOrWhiteSpace(columns[i].JoinAliasName))
                            ? columns[i].JoinAliasName + "."
                            : (!string.IsNullOrWhiteSpace(columns[i].TableName))
                                ? "[" + columns[i].TableName + "]."
                                : "") + columns[i].ColumnName);
                if (!string.IsNullOrWhiteSpace(columns[i].AliasName))
                    queryString.Append(" AS [" + columns[i].AliasName + "]");
                if (i != columns.Count - 1)
                {
                    queryString.Append(",");
                }
            }
            queryString.Append(" FROM ");
            queryString.Append("[" + tableName + "]");
            return queryString.ToString();
        }

        public string SelectAllRecordsFromTable(string tableName, List<ConditionColumn> whereConditionColumns)
        {
            return ApplyWhereClause(SelectAllRecordsFromTable(tableName), whereConditionColumns);
        }

        public string SelectRecordsFromTable(string tableName, List<SelectedColumn> columns,
            List<ConditionColumn> whereConditionColumns)
        {
            return ApplyWhereClause(SelectRecordsFromTable(tableName, columns), whereConditionColumns);
        }

        public string SelectRecordsFromTable(string tableName, List<SelectedColumn> columnsToSelect,
            List<ConditionColumn> whereConditionColumns, List<OrderByColumns> orderByColumns,
            List<GroupByColumn> groupByColumns, List<ConditionColumn> havingConditionColumns)
        {
            var query = SelectRecordsFromTable(tableName, columnsToSelect);
            if (whereConditionColumns != null && whereConditionColumns.Count > 0)
            {
                query = ApplyWhereClause(query, whereConditionColumns);
            }
            if (groupByColumns != null && groupByColumns.Count > 0)
            {
                query = ApplyGroupBy(query, groupByColumns);
            }
            if (orderByColumns != null && orderByColumns.Count > 0)
            {
                query = ApplyOrderBy(query, orderByColumns);
            }
            if (havingConditionColumns != null && havingConditionColumns.Count > 0)
            {
                query = ApplyHavingClause(query, havingConditionColumns);
            }
            return query;
        }

        public string SelectAllRecordsFromTable(string tableName, List<ConditionColumn> whereConditionColumns,
            List<OrderByColumns> orderByColumns, List<GroupByColumn> groupByColumns,
            List<ConditionColumn> havingConditionColumns)
        {
            var query = SelectAllRecordsFromTable(tableName);
            if (whereConditionColumns != null && whereConditionColumns.Count > 0)
            {
                query = ApplyWhereClause(query, whereConditionColumns);
            }
            if (groupByColumns != null && groupByColumns.Count > 0)
            {
                query = ApplyGroupBy(query, groupByColumns);
            }
            if (orderByColumns != null && orderByColumns.Count > 0)
            {
                query = ApplyOrderBy(query, orderByColumns);
            }
            if (havingConditionColumns != null && havingConditionColumns.Count > 0)
            {
                query = ApplyHavingClause(query, havingConditionColumns);
            }
            return query;
        }

        public string SelectTopRecordsFromTable(string tableName, List<SelectedColumn> columnsToSelect,
            int numberOfRecords, List<ConditionColumn> whereConditionColumns, List<OrderByColumns> orderByColumns,
            List<GroupByColumn> groupByColumns, List<ConditionColumn> havingConditionColumns)
        {
            var query = SelectTopRecordsFromTable(tableName, columnsToSelect, numberOfRecords);
            if (whereConditionColumns != null && whereConditionColumns.Count > 0)
            {
                query = ApplyWhereClause(query, whereConditionColumns);
            }
            if (groupByColumns != null && groupByColumns.Count > 0)
            {
                query = ApplyGroupBy(query, groupByColumns);
            }
            if (orderByColumns != null && orderByColumns.Count > 0)
            {
                query = ApplyOrderBy(query, orderByColumns);
            }
            if (havingConditionColumns != null && havingConditionColumns.Count > 0)
            {
                query = ApplyHavingClause(query, havingConditionColumns);
            }
            return query;
        }

        #endregion

    }
}