using Grout.Base.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grout.Base.Data
{
    public class SqlCeQueryBuilder : IQueryBuilder
    {
        public string AddToTable(string tableName, Dictionary<string, object> values)
        {
            var queryString = new StringBuilder();
            queryString.Append("INSERT INTO ");
            queryString.Append("[" + tableName.Replace("'", "") + "] (");
            var columnValues = new StringBuilder();
            var counter = 0;
            foreach (var value in values)
            {
                queryString.Append("[" + value.Key.Trim() + "]");

                columnValues.Append(QueryHelper.GetData(value.Value));

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

        public string AddToTableWithGUID(string tableName, Dictionary<string, object> values, string guidColumn,
            Guid? guid = null)
        {
            var queryString = new StringBuilder();
            queryString.Append("INSERT INTO ");
            queryString.Append("[" + tableName.Replace("'", "") + "] (");
            var columnValues = new StringBuilder();
            var counter = 0;
            foreach (var value in values)
            {
                queryString.Append("[" + value.Key + "]");
                columnValues.Append(QueryHelper.GetData(value.Value));
                if (counter != values.Keys.Count - 1)
                {
                    queryString.Append(",");
                    columnValues.Append(",");
                }
                counter++;
            }
            var guidValue = String.Empty;
            if (guid == null)
            {
                guidValue = "NEWID()";
            }
            else
            {
                guidValue = guid.ToString();
            }

            queryString.Append(",[" + guidColumn + "]) VALUES (" + columnValues + ",'" + guidValue + "')");
            return queryString.ToString();
        }

        public string AddToTable(string tableName, Dictionary<string, object> values, List<string> outputColumns)
        {
            var queryString = new StringBuilder();
            queryString.Append("INSERT INTO ");
            queryString.Append("[" + tableName.Replace("'", "") + "] (");
            var columnValues = new StringBuilder();
            var counter = 0;
            foreach (var value in values)
            {
                queryString.Append("[" + value.Key + "]");
                columnValues.Append(QueryHelper.GetData(value.Value));
                if (counter != values.Keys.Count - 1)
                {
                    queryString.Append(",");
                    columnValues.Append(",");
                }
                counter++;
            }
            queryString.Append(")");
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
                queryString.Append("[" + tableName + "]");
                queryString.Append(" SET ");
                for (var i = 0; i < updateColumns.Count; i++)
                {
                    queryString.Append("[" + updateColumns[i].ColumnName + "]=");
                    queryString.Append(QueryHelper.GetData(updateColumns[i].Value));
                    if (i != updateColumns.Count - 1)
                    {
                        queryString.Append(",");
                    }
                }
            }
            return ApplyWhereClause(queryString.ToString(), whereConditionColumns);
        }

        public string DeleteRowFromTable(string tableName, List<ConditionColumn> row)
        {
            var queryString = new StringBuilder();
            queryString.Append("DELETE FROM ");
            queryString.Append("[" + tableName.Replace("'", "") + "]");
            return ApplyWhereClause(queryString.ToString(), row);
        }

        public string DeleteTableFromDB(string tableName)
        {
            return "DROP TABLE [" + tableName.Replace("'", "") + "]";
        }

        public string CreateTable(string tableName, List<TableColumn> columns)
        {
            var queryString = new StringBuilder();
            queryString.Append("CREATE TABLE ");
            queryString.Append("[" + tableName + "](");
            for (var i = 0; i < columns.Count; i++)
            {
                queryString.Append("[" + columns[i].ColumnName + "] ");
                queryString.Append((columns[i].Length != 0)
                    ? columns[i].DataType + "(" + columns[i].Length + ")"
                    : columns[i].DataType);
                if (columns[i].IsUnique && !columns[i].IsPrimaryKey) queryString.Append(" UNIQUE ");
                if (columns[i].isIdentity)
                {
                    queryString.Append(" IDENTITY(" + columns[i].Identity.IdentitySeed + "," +
                                       columns[i].Identity.IdentityIncrement + ") ");
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
                    queryString.Append(" CONSTRAINT [" + column.ColumnName + "_FK] FOREIGN KEY ");
                    queryString.Append("([" + column.ColumnName + "]) ");
                    queryString.Append("REFERENCES [" + column.ForeignKey.TableName + "] ([" +
                                       column.ForeignKey.ColumnName + "]) ");
                }
            }
            queryString.Append(")");
            return queryString.ToString();
        }

        public string CreateDatabase(string dbName)
        {
            return "CREATE DATABASE [" + dbName.Replace("'", "") + "]";
        }

        public string DeleteDatabase(string dbName)
        {
            return "DROP DATABASE [" + dbName.Replace("'", "") + "]";
        }

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
            if (!String.IsNullOrEmpty(query))
            {
                queryString.Append(query);
                if (columns != null && columns.Count > 0)
                {
                    queryString.Append(" ORDER BY ");
                    for (var i = 0; i < columns.Count; i++)
                    {
                        queryString.Append("[" + columns[i].ColumnName + "]");
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
            if (!String.IsNullOrEmpty(query))
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
                            queryString.Append(" " + QueryHelper.GetConditionOperator(whereColumns[i].Condition));
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
                            queryString.Append(((!string.IsNullOrWhiteSpace(whereColumns[i].TableName))
                                ? " [" + whereColumns[i].TableName + "]."
                                : " ") + "[" + whereColumns[i].ColumnName + "]");
                            queryString.Append(QueryHelper.GetConditionOperator(whereColumns[i].Condition));
                            if (whereColumns[i].Condition == Conditions.LIKE)
                                queryString.Append(whereColumns[i].Value == DBNull.Value
                               ? "Null" : ((QueryHelper.IsNumber(whereColumns[i].Value))
                               ? whereColumns[i].Value : (whereColumns[i].Value == null) ? "%%" : "'%" + whereColumns[i].Value + "%'"));
                            else
                                queryString.Append(whereColumns[i].Value == DBNull.Value
                                    ? "Null" : ((QueryHelper.IsNumber(whereColumns[i].Value))
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
                        queryString.Append(QueryHelper.GetConditionOperator(havingClauseColumns[i].Condition));
                        queryString.Append(QueryHelper.GetData(havingClauseColumns[i].Value));
                    }
                }
            }
            return queryString.ToString();
        }

        public string DeleteAllRowsFromTable(string tableName)
        {
            return "DELETE FROM [" + tableName + "]";
        }

        public string ApplyJoins(string query, JoinSpecification joinSpecification)
        {
            var queryString = new StringBuilder();
            if (joinSpecification.Table == null && joinSpecification.Column == null) return query;
            if (!String.IsNullOrWhiteSpace(query))
            {
                queryString.Append(query);
                queryString.Append(" " + joinSpecification.JoinType + " JOIN ");

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
                        queryString.Append(" " + joinSpecification.Column[j].LogicalOperator+" ");
                    }

                    queryString.Append(parentTable + ".[" + joinSpecification.Column[j].ParentTableColumn + "]" +
                                       QueryHelper.GetConditionOperator(joinSpecification.Column[j].Operation));
                    queryString.Append((joinSpecification.Column[j].ConditionValue != null)
                        ? joinSpecification.Column[j].ConditionValue
                        : (!String.IsNullOrWhiteSpace(joinSpecification.JoinTableAliasName))
                            ? joinSpecification.JoinTableAliasName
                            : "[" + joinSpecification.Column[j].TableName + "]");

                    queryString.Append((joinSpecification.Column[j].ConditionValue == null) ? ".[" + joinSpecification.Column[j].JoinedColumn + "]" : "");
                    
                    
                }
                queryString.Append(")");
            }
            return queryString.ToString();
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

                        queryString.Append("[" + joinSpecification[t].Column[j].ParentTable + "] " + ".[" +
                                           joinSpecification[t].Column[j].ParentTableColumn + "]" +
                                           QueryHelper.GetConditionOperator(joinSpecification[t].Column[j].Operation));
                        queryString.Append((joinSpecification[t].Column[j].ConditionValue != null)
                            ? (QueryHelper.IsNumber(joinSpecification[t].Column[j].ConditionValue)) ? joinSpecification[t].Column[j].ConditionValue : "'" + joinSpecification[t].Column[j].ConditionValue + "'"
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

        public string ApplyJoins(string tableName, List<SelectedColumn> columnsToSelect, JoinSpecification joinSpecification)
        {
            var query = SelectRecordsFromTable(tableName, columnsToSelect);
            return ApplyJoins(query, joinSpecification);
        }

        public string ApplyMultipleJoins(string tableName, List<SelectedColumn> columnsToSelect, List<JoinSpecification> joinSpecification)
        {
            var query = SelectRecordsFromTable(tableName, columnsToSelect);
            return ApplyMultipleJoins(query, joinSpecification);
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

        #region Selection Methods

        public string SelectAllRecordsFromTable(string tableName)
        {
            return "SELECT * FROM [" + tableName.Replace("'", "") + "]";
        }

        public string SelectRecordsFromTable(string tableName, List<SelectedColumn> columns)
        {
            var queryString = new StringBuilder();
            queryString.Append("SELECT ");
            for (var i = 0; i < columns.Count; i++)
            {
                if (columns[i].IsDistinct) queryString.Append(" DISTINCT ");
                if (columns[i].Aggregation != AggregateMethods.None)
                {
                    queryString.Append(columns[i].Aggregation + "([" +
                                       ((!string.IsNullOrWhiteSpace(columns[i].TableName))
                                           ? "[" + columns[i].TableName + "]."
                                           : "") + columns[i].ColumnName + "])");
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
            return "SELECT " + ((aggregationMethod != AggregateMethods.None) ? aggregationMethod + "(*)" : "*") +
                   " FROM [" + tableName + "]";
        }

        public string SelectTopRecordsFromTable(string tableName, List<SelectedColumn> columns, int numberOfRecords)
        {
            var queryString = new StringBuilder();
            queryString.Append("SELECT TOP ");
            queryString.Append(numberOfRecords);
            queryString.Append(" ");
            for (var i = 0; i < columns.Count; i++)
            {
                if (columns[i].IsDistinct) queryString.Append(" DISTINCT ");
                if (columns[i].Aggregation != AggregateMethods.None)
                {
                    queryString.Append(columns[i].Aggregation + "([" + columns[i].ColumnName + "])");
                }
                else
                    queryString.Append(((!string.IsNullOrWhiteSpace(columns[i].JoinAliasName))
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

        #endregion Selection Methods

        public string ApplyJoins(string tableName, List<SelectedColumn> columnsToSelect, JoinSpecification joinSpecification,
            List<ConditionColumn> whereClauseColumns)
        {
            var query = SelectRecordsFromTable(tableName, columnsToSelect);
            return ApplyWhereClause(ApplyJoins(query, joinSpecification), whereClauseColumns);
        }
    }
}