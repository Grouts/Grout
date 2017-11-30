using Grout.Base.DataClasses;
using System;
using System.Collections.Generic;

namespace Grout.Base.Data
{
    /// <summary>
    ///     Interface for query builder
    /// </summary>
    public interface IQueryBuilder
    {
        /// <summary>
        ///     Adds a row in to the table
        /// </summary>
        /// <param name="tableName">Table</param>
        /// <param name="values"></param>
        /// <returns>Query string</returns>
        string AddToTable(string tableName, Dictionary<string, object> values);

        /// <summary>
        ///     Adds a row in to the table
        /// </summary>
        /// <param name="tableName">Table</param>
        /// <param name="values"></param>
        /// <param name="guidColumn"></param>
        /// <returns>Query string</returns>
        string AddToTableWithGUID(string tableName, Dictionary<string, object> values, string guidColumn, Guid? guid = null);

        /// <summary>
        ///     Adds a row to the table and returns newly created row
        /// </summary>
        /// <param name="tableName">Table</param>
        /// <param name="values"></param>
        /// <param name="outputColumns">List of column to return</param>
        /// <returns>Query String</returns>
        string AddToTable(string tableName, Dictionary<string, object> values, List<string> outputColumns);

        /// <summary>
        ///     Updates the row in table
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="updateColumns">List of columns to update</param>
        /// <param name="whereConditionColumns">List of conditions to apply in where clause</param>
        /// <returns>Query string</returns>
        string UpdateRowInTable(string tableName, List<UpdateColumn> updateColumns,
            List<ConditionColumn> whereConditionColumns);

        /// <summary>
        ///     Deletes rows from table
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="row">List of rows to delete</param>
        /// <returns>Query string</returns>
        string DeleteRowFromTable(string tableName, List<ConditionColumn> row);

        /// <summary>
        ///     Deletes the table
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>Query string</returns>
        string DeleteTableFromDB(string tableName);

        /// <summary>
        ///     Create table
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columns">Columns of the table</param>
        /// <returns>Query string</returns>
        string CreateTable(string tableName, List<TableColumn> columns);

        /// <summary>
        ///     Creates a database
        /// </summary>
        /// <param name="dbName">Database name</param>
        /// <returns>Query string</returns>
        string CreateDatabase(string dbName);

        /// <summary>
        ///     Delete the database
        /// </summary>
        /// <param name="dbName">Database name</param>
        /// <returns>Query string</returns>
        string DeleteDatabase(string dbName);

        /// <summary>
        ///     Delets all rows from the table
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <returns>Query string</returns>
        string DeleteAllRowsFromTable(string tableName);

        /// <summary>
        ///     Selects all the records from the table
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Query string</returns>
        string SelectAllRecordsFromTable(string tableName);

        /// <summary>
        ///     Selects all columns from table based on where clause condition
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="whereConditionColumns">Columns to apply where clause</param>
        /// <returns>Query string</returns>
        string SelectAllRecordsFromTable(string tableName, List<ConditionColumn> whereConditionColumns);

        /// <summary>
        ///     Select all data from table based on the aggrigation method
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="aggregationMethod">Aggregation method to apply</param>
        /// <returns>Query string</returns>
        string SelectAllRecordsFromTable(string tableName, AggregateMethods aggregationMethod);

        /// <summary>
        ///     Selects all records from table
        /// </summary>
        /// <param name="tableName">Name of tht table</param>
        /// <param name="whereConditionColumns">Columns to apply where clause</param>
        /// <param name="orderByColumns">Columns to order the table</param>
        /// <param name="groupByColumns">Columns to group the table</param>
        /// <param name="havingConditionColumns">Column to apply having clause</param>
        /// <returns>Returns the query</returns>
        string SelectAllRecordsFromTable(string tableName, List<ConditionColumn> whereConditionColumns,
            List<OrderByColumns> orderByColumns, List<GroupByColumn> groupByColumns,
            List<ConditionColumn> havingConditionColumns);

        /// <summary>
        ///     Selects the list of columns from the table
        /// </summary>
        /// <param name="tableName">Name if the table</param>
        /// <param name="columns">List of columns to select</param>
        /// <returns>Query string</returns>
        string SelectRecordsFromTable(string tableName, List<SelectedColumn> columns);

        /// <summary>
        ///     Select specified columns based on where condition
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="columns">Columns to select</param>
        /// <param name="values"></param>
        /// <returns> Query string</returns>
        string SelectRecordsFromTable(string tableName, List<SelectedColumn> columns, List<ConditionColumn> values);

        /// <summary>
        ///     Selects given columns from the table
        /// </summary>
        /// <param name="tableName">Name of tht table</param>
        /// <param name="columnsToSelect">List of column to select from the table</param>
        /// <param name="whereConditionColumns">Columns to apply where clause</param>
        /// <param name="orderByColumns">Columns to order the table</param>
        /// <param name="groupByColumns">Columns to group the table</param>
        /// <param name="havingConditionColumns">Column to apply having clause</param>
        /// <returns>Returns the query</returns>
        string SelectRecordsFromTable(string tableName, List<SelectedColumn> columnsToSelect,
            List<ConditionColumn> whereConditionColumns, List<OrderByColumns> orderByColumns,
            List<GroupByColumn> groupByColumns, List<ConditionColumn> havingConditionColumns);

        /// <summary>
        ///     Selects top records from the table
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columns">List of columns to select from the table</param>
        /// <param name="numberOfRecords">Number of top records to fetch</param>
        /// <returns>Query string</returns>
        string SelectTopRecordsFromTable(string tableName, List<SelectedColumn> columns, int numberOfRecords);

        /// <summary>
        ///     Selects Top columns from the table
        /// </summary>
        /// <param name="tableName">Name of tht table</param>
        /// <param name="columnsToSelect">List of column to select from the table</param>
        /// <param name="numberOfRecords">Number of top rows to select</param>
        /// <param name="whereConditionColumns">Columns to apply where clause</param>
        /// <param name="orderByColumns">Columns to order the table</param>
        /// <param name="groupByColumns">Columns to group the table</param>
        /// <param name="havingConditionColumns">Column to apply having clause</param>
        /// <returns>Returns the query</returns>
        string SelectTopRecordsFromTable(string tableName, List<SelectedColumn> columnsToSelect, int numberOfRecords,
            List<ConditionColumn> whereConditionColumns, List<OrderByColumns> orderByColumns,
            List<GroupByColumn> groupByColumns, List<ConditionColumn> havingConditionColumns);

        /// <summary>
        ///     Appends group by clause to the query string
        /// </summary>
        /// <param name="query">Query string to append group by clause</param>
        /// <param name="groupByColumns"></param>
        /// <returns>Query string</returns>
        string ApplyGroupBy(string query, List<GroupByColumn> groupByColumns);

        /// <summary>
        ///     Applies Order by clause to the query string
        /// </summary>
        /// <param name="query">Query to append order by clause</param>
        /// <param name="orderByColumns"></param>
        /// <returns>Query string</returns>
        string ApplyOrderBy(string query, List<OrderByColumns> orderByColumns);

        /// <summary>
        ///     Appends where clause to the query string
        /// </summary>
        /// <param name="query">Query string to append where clause</param>
        /// <param name="whereConditionColumns"></param>
        /// <returns>Query string</returns>
        string ApplyWhereClause(string query, List<ConditionColumn> whereConditionColumns);

        /// <summary>
        ///     Appends having clause to the query string
        /// </summary>
        /// <param name="query">Query string to append having clause</param>
        /// <param name="havingConditionColumns"></param>
        /// <returns>Query string</returns>
        string ApplyHavingClause(string query, List<ConditionColumn> havingConditionColumns);

        /// <summary>
        ///     Joins two or more tables
        /// </summary>
        /// <param name="query">SQL Query</param>
        /// <param name="joinSpecification">Join specification object</param>
        /// <returns>Query string</returns>
        string ApplyJoins(string query, JoinSpecification joinSpecification);

        /// <summary>
        ///     Joins Mulitple tables
        /// </summary>
        /// <param name="query">SQL Query</param>
        /// <param name="joinSpecification">Join specification object</param>
        /// <returns>Query string</returns>
        string ApplyMultipleJoins(string query, List<JoinSpecification> joinSpecification);

        /// <summary>
        ///     Joins Multiple tables
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="columnsToSelect">List of columns to select</param>
        /// <param name="joinSpecification">Join specification object</param>
        /// <returns>Query string</returns>
        string ApplyMultipleJoins(string tableName, List<SelectedColumn> columnsToSelect, List<JoinSpecification> joinSpecification);

        /// <summary>
        ///     Joins two or more tables
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="columnsToSelect">List of columns to select</param>
        /// <param name="joinSpecification">Join specification object</param>
        /// <returns>Query string</returns>
        string ApplyJoins(string tableName, List<SelectedColumn> columnsToSelect, JoinSpecification joinSpecification);

        /// <summary>
        ///     Joins Two or more tables
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnsToSelect">List of columns to select</param>
        /// <param name="whereClauseColumns">List to columns to apply where condition</param>
        /// <param name="groupByColumns">List to columns to group the table</param>
        /// <param name="orderByColumns">List to columns to order the table</param>
        /// <param name="havingClauseColumns">List to columns to apply having condition</param>
        /// <param name="joinSpecification">Join specification object</param>
        /// <returns>Query string</returns>
        string ApplyJoins(string tableName, List<SelectedColumn> columnsToSelect,
            List<ConditionColumn> whereClauseColumns, List<GroupByColumn> groupByColumns,
            List<OrderByColumns> orderByColumns, List<ConditionColumn> havingClauseColumns, JoinSpecification joinSpecification);
    }
}