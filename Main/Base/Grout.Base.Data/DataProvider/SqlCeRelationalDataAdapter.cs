using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Reflection;
using Grout.Base.DataClasses;
using Grout.Base.Logger;

namespace Grout.Base.Data
{
    public class SqlCeRelationalDataAdapter : IRelationalDataProvider
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
        public SqlCeRelationalDataAdapter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Result ExecuteNonQuery(string query)
        {
            var result = new Result();
            if (!string.IsNullOrWhiteSpace(_connectionString))
            {
                using (var connection = new SqlCeConnection(_connectionString))
                {
                    using (var command = new SqlCeCommand(query, connection))
                    {
                        try
                        {
                            command.Connection.Open();
                            result.ReturnValue = command.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            result.Status = false;
                            result.Exception = e;
                            LogExtension.LogError("Exception on ExecuteNonQuery", e, MethodBase.GetCurrentMethod(), " Query - " + query + " ConnectionString - " + connection);
                        }
                        finally
                        {
                            command.Connection.Close();
                            command.Dispose();
                        }
                    }
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
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var connection = new SqlCeConnection(connectionString);

                var command = new SqlCeCommand(query, connection);
                try
                {
                    command.Connection.Open();
                    result.ReturnValue = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    result.Status = false;
                    result.Exception = e;
                    LogExtension.LogError("Exception on ExecuteNonQuery", e, MethodBase.GetCurrentMethod(), " Query - " + query + " ConnectionString - " + connection);
                }
                finally
                {
                    command.Connection.Close();
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
                var connection = new SqlCeConnection(_connectionString);

                var adapter = new SqlCeDataAdapter(query, connection);
                try
                {
                    adapter.Fill(result.DataTable);
                }
                catch (Exception e)
                {
                    result.Status = false;
                    result.Exception = e;
                    LogExtension.LogError("Exception on ExecuteReaderQuery", e, MethodBase.GetCurrentMethod(), " Query - " + query + " ConnectionString - " + connection);
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
                var connection = new SqlCeConnection(connectionString);

                var adapter = new SqlCeDataAdapter(query, connection);
                try
                {
                    adapter.Fill(result.DataTable);
                }
                catch (Exception e)
                {
                    result.Status = false;
                    result.Exception = e;
                    LogExtension.LogError("Exception on ExecuteReaderQuery", e, MethodBase.GetCurrentMethod(), " Query - " + query + " ConnectionString - " + connection);
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
                var connection = new SqlCeConnection(_connectionString);

                var command = new SqlCeCommand(query, connection);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    command.CommandText = "SELECT @@IDENTITY";
                    result.ReturnValue = (String.IsNullOrWhiteSpace(guid.ToString())) ? command.ExecuteScalar() : guid;
                }
                catch (Exception e)
                {
                    result.Status = false;
                    result.Exception = e;
                    LogExtension.LogError("Exception on ExecuteScalarQuery", e, MethodBase.GetCurrentMethod(), " Query - " + query + " ConnectionString - " + connection + " Guid - " + guid);
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
                var connection = new SqlCeConnection(connectionString);

                var command = new SqlCeCommand(query, connection);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    command.CommandText = "SELECT @@IDENTITY";
                    result.ReturnValue = (String.IsNullOrWhiteSpace(guid.ToString())) ? command.ExecuteScalar() : guid;
                }
                catch (Exception e)
                {
                    result.Status = false;
                    result.Exception = e;
                    LogExtension.LogError("Exception on ExecuteScalarQuery", e, MethodBase.GetCurrentMethod(), " Query - " + query + " ConnectionString - " + connection + " Guid - " + guid);
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
                using (var connection = new SqlCeConnection(_connectionString))
                {
                    string[] splitter = new string[] { ";" };
                    var querySplit = query.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
                    for (var queryInc = 0; queryInc < querySplit.Length; queryInc++)
                    {
                        using (var command = new SqlCeCommand(querySplit[queryInc], connection))
                        {
                            try
                            {
                                command.Connection.Open();
                                result.ReturnValue = command.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                result.Status = false;
                                result.Exception = e;
                                LogExtension.LogError("Exception on ExecuteBulkQuery", e, MethodBase.GetCurrentMethod(), " Query - " + query + " ConnectionString - " + connection);
                            }
                            finally
                            {
                                command.Connection.Close();
                                command.Dispose();
                            }
                        }
                    }
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
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                using (var connection = new SqlCeConnection(connectionString))
                {
                    string[] splitter = new string[] { ";" };
                    var querySplit = query.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
                    for (var a = 0; a < querySplit.Length; a++)
                    {
                        using (var command = new SqlCeCommand(querySplit[a], connection))
                        {
                            try
                            {
                                command.Connection.Open();
                                result.ReturnValue = command.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                result.Status = false;
                                result.Exception = e;
                                LogExtension.LogError("Exception on ExecuteBulkQuery", e, MethodBase.GetCurrentMethod(), " Query - " + query + " ConnectionString - " + connection);
                            }
                            finally
                            {
                                command.Connection.Close();
                                command.Dispose();
                            }
                        }
                    }
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