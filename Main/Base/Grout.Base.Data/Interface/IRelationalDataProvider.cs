using System;
using System.Data;
using Grout.Base.DataClasses;

namespace Grout.Base.Data
{
    public interface IRelationalDataProvider
    {
        /// <summary>
        ///     For executing SQL Query that doesn't return a value
        /// </summary>
        /// <param name="query">SQL query</param>
        Result ExecuteNonQuery(string query);

        /// <summary>
        ///     For executing SQL Query that doesn't return a value
        /// </summary>
        /// <param name="query">SQL Query</param>
        /// <param name="connectionString">Connection String</param>
        Result ExecuteNonQuery(string query, string connectionString);

        /// <summary>
        ///     Executes Query that returns  the table
        /// </summary>
        /// <param name="query">SQL query</param>
        /// <returns>Query result as DataTable</returns>
        Result ExecuteReaderQuery(string query);

        /// <summary>
        ///     Executes Query that returns  the table
        /// </summary>
        /// <param name="query">SQL Query</param>
        /// <param name="connectionString">SQL Connection String</param>
        /// <returns>Query result as DataTable</returns>
        Result ExecuteReaderQuery(string query, string connectionString);

        /// <summary>
        ///     Execute Scalar SQL Query
        /// </summary>
        /// <param name="query">SQL Query</param>
        /// <returns>Result Object</returns>
        Result ExecuteScalarQuery(string query, Guid? guid = null);

        /// <summary>
        ///     Execute Scalar SQL Query
        /// </summary>
        /// <param name="query">SQL Query</param>
        /// <param name="connectionString">SQL Connection String</param>
        /// <returns>Result Object</returns>
        Result ExecuteScalarQuery(string query, string connectionString, Guid? guid = null);

        /// <summary>
        /// Execute the bulk query depends on the server type (SQL/SQLCE)
        /// </summary>
        /// <param name="query">SQL Query</param>
        Result ExecuteBulkQuery(string query);

        /// <summary>
        /// Execute the bulk query depends on the server type (SQL/SQLCE)
        /// </summary>
        /// <param name="query">SQL Query</param>
        /// <param name="connectionString">Connection String</param>
        Result ExecuteBulkQuery(string query, string connectionString);
    }
}