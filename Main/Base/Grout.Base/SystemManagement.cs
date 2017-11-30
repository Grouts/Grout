using Grout.Base.Data;
using Grout.Base.DataClasses;
using Grout.Base.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Grout.Base
{
    public class SystemManagement : IDisposable
    {
        private bool _disposed;
        private readonly IRelationalDataProvider _dataProvider;
        private readonly IQueryBuilder _queryBuilder;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="provider"></param>
        public SystemManagement(IQueryBuilder builder, IRelationalDataProvider provider)
        {
            _queryBuilder = builder;
            _dataProvider = provider;
        }
        public SystemManagement()
        {
            if (GlobalAppSettings.DbSupport == DataBaseType.MSSQLCE)
            {
                _dataProvider = new SqlCeRelationalDataAdapter(Connection.ConnectionString);
                _queryBuilder = new SqlCeQueryBuilder();
            }
            else
            {
                _dataProvider = new SqlRelationalDataAdapter(Connection.ConnectionString);
                _queryBuilder = new SqlQueryBuilder();
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        public bool UpdateSystemSetting(string value, string whereConditionColumnname)
        {
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_SystemSettings.Key,
                        Condition = Conditions.LIKE,
                        Value = whereConditionColumnname
                    }
                };
                var updateColumns = new List<UpdateColumn>
                {
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_SystemSettings.Value,
                        Value = value
                    }
                };
                var result = _dataProvider.ExecuteNonQuery(
                    _queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_SystemSettings.DB_TableName,
                        updateColumns,
                        whereColumns));
                return result.Status;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while updating system settings", e,
                    MethodBase.GetCurrentMethod(), "WhereConditionColumnname - " + whereConditionColumnname + " Value - " + value);
                return false;
            }
        }
    }
}
