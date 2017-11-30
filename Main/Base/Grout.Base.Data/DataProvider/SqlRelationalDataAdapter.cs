using System;
using System.Data.SqlClient;
using System.Reflection;
using System.Data;
using Grout.Base.DataClasses;
using Grout.Base.Logger;
using System.Linq;

namespace Grout.Base.Data
{
    public class SqlRelationalDataAdapter : IRelationalDataProvider
    {
        #region Private Properties

        private string _connectionString;

        #endregion Private Properties

        #region Public Properties

        public string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        #endregion Public Properties

        /// <summary>
        /// Creates an instance of RelationalDataAdapter Class with connection string
        /// </summary>
        /// <param name="connectionString">Connection string of the DB to be connected</param>
        public SqlRelationalDataAdapter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Result ExecuteNonQuery(string query)
        {
            var result = new Result();
            if (!string.IsNullOrWhiteSpace(_connectionString))
            {
                var connection = new SqlConnection(_connectionString);

                var command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    result.ReturnValue = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    result.Status = false;
                    result.Exception = e;

                    try
                    {
                        var userOptions = ExecuteReaderQuery("dbcc useroptions");
                        var dbFormat = userOptions.DataTable.AsEnumerable()
                            .Select(a => new
                            {
                                Key = a.Field<string>("Set Option"),
                                Value = a.Field<string>("Value")
                            }).ToDictionary(a => a.Key, a => a.Value);
                        var dbOptions = string.Empty;
                        foreach (var data in dbFormat)
                        {
                            dbOptions = dbOptions + data.Key + ":" + data.Value + ";";
                        }
                        LogExtension.LogError("Exception on ExecuteNonQuery", e, MethodBase.GetCurrentMethod(),
                            "Query - " + query + " ConnectionString - " + connection + "\nSql server user options:" +
                            dbOptions);
                    }
                    catch (Exception ex)
                    {
                        LogExtension.LogError("Exception on ExecuteNonQuery - Error in getting user options", ex,
                            MethodBase.GetCurrentMethod());
                        return result;
                    }
                }
                finally
                {
                    connection.Close();
                    command.Dispose();
                }
            }
            else
            {
                var exception = new Exception("Invalid Connection string");
                result.Status = false;
                result.Exception = exception;
                throw exception;
            }
            result.Status = true;
            return result;
        }

        public Result ExecuteNonQuery(string query, string connectionString)
        {
            var result = new Result();
            result.DataTable = new DataTable();
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var connection = new SqlConnection(connectionString);

                var command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    result.ReturnValue = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    result.Status = false;
                    result.Exception = e;
                    try
                    {
                        var userOptions = ExecuteReaderQuery("dbcc useroptions");
                        var dbFormat = userOptions.DataTable.AsEnumerable()
                            .Select(a => new
                            {
                                Key = a.Field<string>("Set Option"),
                                Value = a.Field<string>("Value")
                            }).ToDictionary(a => a.Key, a => a.Value);
                        var dbOptions = string.Empty;
                        foreach (var data in dbFormat)
                        {
                            dbOptions = dbOptions + data.Key + ":" + data.Value + ";";
                        }
                        LogExtension.LogError("Exception on ExecuteNonQuery", e, MethodBase.GetCurrentMethod(),
                            "Query - " + query + " ConnectionString - " + connection + "\nSql server user options:" +
                            dbOptions);
                    }
                    catch (Exception ex)
                    {
                        LogExtension.LogError("Exception on ExecuteNonQuery - Error in getting user options", ex,
                            MethodBase.GetCurrentMethod());
                        return result;
                    }
                }
                finally
                {
                    connection.Close();
                    command.Dispose();
                }
            }
            else
            {
                var exception = new Exception("Invalid Connection string");
                result.Status = false;
                result.Exception = exception;
                throw exception;
            }
            result.Status = true;
            return result;
        }

        public Result ExecuteReaderQuery(string query)
        {
            var result = new Result();
            if (!string.IsNullOrWhiteSpace(_connectionString))
            {
                var connection = new SqlConnection(_connectionString);

                var adapter = new SqlDataAdapter(query, connection);
                try
                {
                    adapter.Fill(result.DataTable);
                }
                catch (Exception e)
                {
                    result.Status = false;
                    result.Exception = e;
                    try
                    {
                        var userOptions = ExecuteReaderQuery("dbcc useroptions");
                        var dbFormat = userOptions.DataTable.AsEnumerable()
                            .Select(a => new
                            {
                                Key = a.Field<string>("Set Option"),
                                Value = a.Field<string>("Value")
                            }).ToDictionary(a => a.Key, a => a.Value);
                        var dbOptions = string.Empty;
                        foreach (var data in dbFormat)
                        {
                            dbOptions = dbOptions + data.Key + ":" + data.Value + ";";
                        }
                        LogExtension.LogError("Exception on ExecuteReaderQuery", e, MethodBase.GetCurrentMethod(),
                            "Query - " + query + " ConnectionString - " + connection + "\nSql server user options:" +
                            dbOptions);
                    }
                    catch (Exception ex)
                    {
                        LogExtension.LogError("Exception on ExecuteReaderQuery - Error in getting user options", ex,
                            MethodBase.GetCurrentMethod());
                        return result;
                    }
                }
                finally
                {
                    adapter.Dispose();
                    connection.Close();
                }
            }
            else
            {
                var exception = new Exception("Invalid Connection string");
                result.Status = false;
                result.Exception = exception;
                throw exception;
            }
            result.Status = true;
            return result;
        }

        public Result ExecuteReaderQuery(string query, string connectionString)
        {
            var result = new Result();
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var connection = new SqlConnection(connectionString);

                var adapter = new SqlDataAdapter(query, connection);
                try
                {
                    adapter.Fill(result.DataTable);
                }
                catch (Exception e)
                {
                    result.Status = false;
                    result.Exception = e;
                    try
                    {
                        var userOptions = ExecuteReaderQuery("dbcc useroptions");
                        var dbFormat = userOptions.DataTable.AsEnumerable()
                            .Select(a => new
                            {
                                Key = a.Field<string>("Set Option"),
                                Value = a.Field<string>("Value")
                            }).ToDictionary(a => a.Key, a => a.Value);
                        var dbOptions = string.Empty;
                        foreach (var data in dbFormat)
                        {
                            dbOptions = dbOptions + data.Key + ":" + data.Value + ";";
                        }
                        LogExtension.LogError("Exception on ExecuteReaderQuery", e, MethodBase.GetCurrentMethod(),
                            "Query - " + query + " ConnectionString - " + connection + "\nSql server user options:" +
                            dbOptions);
                    }
                    catch (Exception ex)
                    {
                        LogExtension.LogError("Exception on ExecuteReaderQuery - Error in getting user options", ex,
                            MethodBase.GetCurrentMethod());
                        return result;
                    }
                }
                finally
                {
                    adapter.Dispose();
                    connection.Close();
                }
            }
            else
            {
                var exception = new Exception("Invalid Connection string");
                result.Status = false;
                result.Exception = exception;
                throw exception;
            }
            result.Status = true;
            return result;
        }

        public Result ExecuteScalarQuery(string query, Guid? guid = null)
        {
            var result = new Result();
            if (!string.IsNullOrWhiteSpace(_connectionString))
            {
                var connection = new SqlConnection(_connectionString);

                var command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    result.ReturnValue = command.ExecuteScalar();
                }
                catch (Exception e)
                {
                    result.Status = false;
                    result.Exception = e;
                    try
                    {
                        var userOptions = ExecuteReaderQuery("dbcc useroptions");
                        var dbFormat = userOptions.DataTable.AsEnumerable()
                            .Select(a => new
                            {
                                Key = a.Field<string>("Set Option"),
                                Value = a.Field<string>("Value")
                            }).ToDictionary(a => a.Key, a => a.Value);
                        var dbOptions = string.Empty;
                        foreach (var data in dbFormat)
                        {
                            dbOptions = dbOptions + data.Key + ":" + data.Value + ";";
                        }
                        LogExtension.LogError("Exception on ExecuteScalarQuery", e, MethodBase.GetCurrentMethod(),
                            "Query - " + query + " ConnectionString - " + connection + "\nSql server user options:" +
                            dbOptions);
                    }
                    catch (Exception ex)
                    {
                        LogExtension.LogError("Exception on ExecuteScalarQuery - Error in getting user options", ex,
                            MethodBase.GetCurrentMethod());
                        return result;
                    }

                }
                finally
                {
                    connection.Close();
                    command.Dispose();
                }
            }
            else
            {
                var exception = new Exception("Invalid Connection string");
                result.Status = false;
                result.Exception = exception;
                throw exception;
            }
            result.Status = true;
            return result;
        }

        public Result ExecuteScalarQuery(string query, string connectionString, Guid? guid = null)
        {
            var result = new Result();
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var connection = new SqlConnection(connectionString);

                var command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    result.ReturnValue = command.ExecuteScalar();
                }
                catch (Exception e)
                {
                    result.Status = false;
                    result.Exception = e;
                    try
                    {
                        var userOptions = ExecuteReaderQuery("dbcc useroptions");
                        var dbFormat = userOptions.DataTable.AsEnumerable()
                            .Select(a => new
                            {
                                Key = a.Field<string>("Set Option"),
                                Value = a.Field<string>("Value")
                            }).ToDictionary(a => a.Key, a => a.Value);
                        var dbOptions = string.Empty;
                        foreach (var data in dbFormat)
                        {
                            dbOptions = dbOptions + data.Key + ":" + data.Value + ";";
                        }
                        LogExtension.LogError("Exception on ExecuteScalarQuery", e, MethodBase.GetCurrentMethod(),
                            "Query - " + query + " ConnectionString - " + connection + "\nSql server user options:" +
                            dbOptions);
                    }
                    catch (Exception ex)
                    {
                        LogExtension.LogError("Exception on ExecuteScalarQuery - Error in getting user options", ex,
                            MethodBase.GetCurrentMethod());
                        return result;
                    }
                }
                finally
                {
                    connection.Close();
                    command.Dispose();
                }
            }
            else
            {
                var exception = new Exception("Invalid Connection string");
                result.Status = false;
                result.Exception = exception;
                throw exception;
            }
            result.Status = true;
            return result;
        }

        public Result ExecuteBulkQuery(string query)
        {
            var result = new Result();

            if (!string.IsNullOrWhiteSpace(_connectionString))
            {
                var connection = new SqlConnection(_connectionString);

                var command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    result.ReturnValue = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    result.Status = false;
                    result.Exception = e;
                    try
                    {
                        var userOptions = ExecuteReaderQuery("dbcc useroptions");
                        var dbFormat = userOptions.DataTable.AsEnumerable()
                            .Select(a => new
                            {
                                Key = a.Field<string>("Set Option"),
                                Value = a.Field<string>("Value")
                            }).ToDictionary(a => a.Key, a => a.Value);
                        var dbOptions = string.Empty;
                        foreach (var data in dbFormat)
                        {
                            dbOptions = dbOptions + data.Key + ":" + data.Value + ";";
                        }
                        LogExtension.LogError("Exception on ExecuteBulkQuery", e, MethodBase.GetCurrentMethod(),
                            "Query - " + query + " ConnectionString - " + connection + "\nSql server user options:" +
                            dbOptions);
                    }
                    catch (Exception ex)
                    {
                        LogExtension.LogError("Exception on ExecuteBulkQuery - Error in getting user options", ex,
                            MethodBase.GetCurrentMethod());
                        return result;
                    }

                }
                finally
                {
                    connection.Close();
                    command.Dispose();
                }
            }
            else
            {
                var exception = new Exception("Invalid Connection string");
                result.Status = false;
                result.Exception = exception;
                throw exception;
            }
            result.Status = true;
            return result;
        }

        public Result ExecuteBulkQuery(string query, string connectionString)
        {
            var result = new Result();

            var returnVal = 0;
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var connection = new SqlConnection(connectionString);

                var command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    result.ReturnValue = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    try
                    {
                        var userOptions = ExecuteReaderQuery("dbcc useroptions");
                        var dbFormat = userOptions.DataTable.AsEnumerable()
                            .Select(a => new
                            {
                                Key = a.Field<string>("Set Option"),
                                Value = a.Field<string>("Value")
                            }).ToDictionary(a => a.Key, a => a.Value);
                        var dbOptions = string.Empty;
                        foreach (var data in dbFormat)
                        {
                            dbOptions = dbOptions + data.Key + ":" + data.Value + ";";
                        }
                        LogExtension.LogError("Exception on ExecuteBulkQuery", e, MethodBase.GetCurrentMethod(),
                            "Query - " + query + " ConnectionString - " + connection + "\nSql server user options:" +
                            dbOptions);
                    }
                    catch (Exception ex)
                    {
                        LogExtension.LogError("Exception on ExecuteBulkQuery - Error in getting user options", ex,
                            MethodBase.GetCurrentMethod());
                        return result;
                    }
                }
                finally
                {
                    connection.Close();
                    command.Dispose();
                }
            }
            else
            {
                var exception = new Exception("Invalid Connection string");
                result.Status = false;
                result.Exception = exception;
                throw exception;
            }
            result.Status = true;
            return result;
        }
    }
}