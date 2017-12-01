using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using Grout.Base.Data;
using Grout.Base.DataClasses;
using Grout.Base.Logger;

namespace Grout.Base
{
    public class UserManagement : IDisposable, IUserManagement
    {
        private bool _disposed;
        private readonly IRelationalDataProvider _dataProvider;
        private readonly IQueryBuilder _queryBuilder;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="provider"></param>
        public UserManagement(IQueryBuilder builder, IRelationalDataProvider provider)
        {
            _queryBuilder = builder;
            _dataProvider = provider;
        }

        public UserManagement()
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

        public Result AddUser(User user, out DateTime activationExpirationDate, out string activationCode)
        {
            try
            {
                activationExpirationDate =
                    user.CreatedDate.AddDays(GlobalAppSettings.SystemSettings.ActivationExpirationDays);
                activationCode = GenerateRandomCode(12);

                var values = new Dictionary<string, object>
                {
                    {GlobalAppSettings.DbColumns.DB_User.UserName, user.UserName},
                    {GlobalAppSettings.DbColumns.DB_User.FirstName, user.FirstName},
                    {GlobalAppSettings.DbColumns.DB_User.LastName, user.LastName},
                    {GlobalAppSettings.DbColumns.DB_User.DisplayName, user.DisplayName},
                    {GlobalAppSettings.DbColumns.DB_User.Email, user.Email},
                    {GlobalAppSettings.DbColumns.DB_User.Password, user.Password},
                    {GlobalAppSettings.DbColumns.DB_User.Contact, user.ContactNumber},
                    {GlobalAppSettings.DbColumns.DB_User.Picture, user.Avatar},
                    {GlobalAppSettings.DbColumns.DB_User.IsActive, user.IsActive},
                    {GlobalAppSettings.DbColumns.DB_User.IsActivated, user.IsActivated},
                    {GlobalAppSettings.DbColumns.DB_User.IsDeleted, user.IsDeleted},
                    {GlobalAppSettings.DbColumns.DB_User.ActivationCode, activationCode},
                    {GlobalAppSettings.DbColumns.DB_User.ResetPasswordCode, GenerateRandomCode(12)},
                    {
                        GlobalAppSettings.DbColumns.DB_User.CreatedDate,
                        user.CreatedDate.ToString(GlobalAppSettings.GetDateTimeFormat())
                    },
                    {
                        GlobalAppSettings.DbColumns.DB_User.ModifiedDate,
                        user.ModifiedDate.ToString(GlobalAppSettings.GetDateTimeFormat())
                    },
                    {GlobalAppSettings.DbColumns.DB_User.ActivationExpirationDate, activationExpirationDate.ToString(GlobalAppSettings.GetDateTimeFormat())}
                };

                var result = new Result();

                var outputColumn = new List<string> { GlobalAppSettings.DbColumns.DB_User.UserId };
                result = _dataProvider.ExecuteScalarQuery(_queryBuilder.AddToTable(
                    GlobalAppSettings.DbColumns.DB_User.DB_TableName, values, outputColumn),
                    GlobalAppSettings.ConnectionString);

                var preferenceValues = new Dictionary<string, object>
                {
                    {GlobalAppSettings.DbColumns.DB_UserPreference.UserId, result.ReturnValue},
                    {GlobalAppSettings.DbColumns.DB_UserPreference.IsActive, true},
                    {
                        GlobalAppSettings.DbColumns.DB_UserPreference.ModifiedDate,
                        DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    }
                };
                _dataProvider.ExecuteNonQuery(
                    _queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_UserPreference.DB_TableName,
                        preferenceValues));

                result.Status = true;
                return result;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception while adding user in database", e, MethodBase.GetCurrentMethod(), " UserName - " + user.UserName + " FirstName - " + user.FirstName + " LastName - " + user.LastName + " DisplayName - " + user.DisplayName + " Email - " + user.Email + " Password - " + user.Password + " Avatar - " + user.Avatar);

                activationCode = String.Empty;
                activationExpirationDate = new DateTime();

                return new Result
                {
                    Status = false,
                    Exception = e
                };
            }
        }

        public bool AddUserToken(int userId, string ipAddress, string token)
        {
            try
            {
                var values = new Dictionary<string, object>
                {
                    {GlobalAppSettings.DbColumns.DB_UserLogin.UserId, userId},
                    {GlobalAppSettings.DbColumns.DB_UserLogin.ClientToken, token},
                    {GlobalAppSettings.DbColumns.DB_UserLogin.IpAddress, ipAddress},
                    {GlobalAppSettings.DbColumns.DB_UserLogin.IsActive, true},
                    {
                        GlobalAppSettings.DbColumns.DB_UserLogin.LoggedInTime,
                        DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    }
                };
                var result = _dataProvider.ExecuteNonQuery(
                    _queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_UserLogin.DB_TableName, values));
                return result.Status;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception while adding entry in userlogin table", e,
                    MethodBase.GetCurrentMethod(), " UserId - " + userId + " IpAddress - " + ipAddress + " Token - " + token);
                return false;
            }
        }

        public bool AddUserLog(int logType, int currentUser, int? groupId, int userId, int oldValue = 0,
            int newValue = 0)
        {
            try
            {
                var elements = new Dictionary<string, object>
                {
                    {GlobalAppSettings.DbColumns.DB_UserLog.UserLogTypeId, logType},
                    {GlobalAppSettings.DbColumns.DB_UserLog.UpdatedUserId, currentUser},
                    {GlobalAppSettings.DbColumns.DB_UserLog.OldValue, oldValue},
                    {GlobalAppSettings.DbColumns.DB_UserLog.NewValue, newValue},
                    {GlobalAppSettings.DbColumns.DB_UserLog.TargetUserId, userId},
                    {GlobalAppSettings.DbColumns.DB_UserLog.ModifiedDate, DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())},
                    {GlobalAppSettings.DbColumns.DB_UserLog.IsActive, true}
                };
                if (groupId != null)
                    elements.Add(GlobalAppSettings.DbColumns.DB_UserLog.GroupId, groupId);
                var output = new List<string> { GlobalAppSettings.DbColumns.DB_UserLog.UserLogId };
                var result =
                    _dataProvider.ExecuteScalarQuery(
                        _queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_UserLog.DB_TableName, elements, output));
                //We have issue here, if the groupid came with 0 then the entry will be blocked due to the foreign key conflict.
                //So here we need to log the issue with lognet
                LogExtension.LogInfo("User Log has been added successfully", MethodBase.GetCurrentMethod(), " LogType - " + logType + " CurrentUser - " + currentUser + " GroupId - " + groupId + " UserId - " + userId + " OldValue - " + oldValue + " NewValue - " + newValue);
                return result.Status;
            }
            catch (Exception ex)
            {
                LogExtension.LogError("Error in adding User Log", ex, MethodBase.GetCurrentMethod(), " LogType - " + logType + " CurrentUser - " + currentUser + " GroupId - " + groupId + " UserId - " + userId + " OldValue - " + oldValue + " NewValue - " + newValue);
                return false;
            }
        }

        public bool AddSystemUserLog(int logType, int currentUser, int? userId)
        {
            try
            {
                var elements = new Dictionary<string, object>
                {
                    {GlobalAppSettings.DbColumns.DB_SystemLog.SystemLogTypeId, logType},
                    {GlobalAppSettings.DbColumns.DB_SystemLog.UpdatedUserId, currentUser},
                    {GlobalAppSettings.DbColumns.DB_SystemLog.TargetUserId, userId},
                    {
                        GlobalAppSettings.DbColumns.DB_SystemLog.ModifiedDate,
                        DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    },
                    {GlobalAppSettings.DbColumns.DB_SystemLog.IsActive, true}
                };

                var output = new List<string> { GlobalAppSettings.DbColumns.DB_SystemLog.LogId };

                var result =
                    _dataProvider.ExecuteScalarQuery(
                        _queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_SystemLog.DB_TableName, elements,
                            output));
                return result.Status;
            }
            catch (Exception ex)
            {
                LogExtension.LogError("Error in adding system Log", ex, MethodBase.GetCurrentMethod(), " LogType - " + logType + " CurrentUser - " + currentUser + " UserId - " + userId);
                return false;
            }
        }

        public List<User> SearchUsers(int? skip, int? take, string searchkey = "")
        {
            var searchString = "%" + searchkey + "%";
            var resultUsers = new List<User>();
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.UserName,
                        Condition = Conditions.LIKE,
                        Value = searchString
                    },
                    new ConditionColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.FirstName,
                        Condition = Conditions.LIKE,
                        LogicalOperator = LogicalOperators.OR,
                        Value = searchString
                    },
                    new ConditionColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.LastName,
                        Condition = Conditions.LIKE,
                        LogicalOperator = LogicalOperators.OR,
                        Value = searchString
                    },
                    new ConditionColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.Contact,
                        Condition = Conditions.LIKE,
                        LogicalOperator = LogicalOperators.OR,
                        Value = searchString
                    },
                    new ConditionColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.DisplayName,
                        Condition = Conditions.LIKE,
                        LogicalOperator = LogicalOperators.OR,
                        Value = searchString
                    },
                    new ConditionColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.Email,
                        Condition = Conditions.LIKE,
                        LogicalOperator = LogicalOperators.OR,
                        Value = searchString
                    }
                };
                var query = _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    whereColumns);
                skip = skip ?? 0;
                take = take ?? 10;
                query += " OFFSET " + skip + " ROWS FETCH NEXT " + take + " ROWS ONLY";
                var result =
                    _dataProvider.ExecuteReaderQuery(query);
                if (result.Status)
                {
                    resultUsers =
                        result.DataTable.AsEnumerable()
                            .Select(row => new User
                            {
                                UserId = row.Field<int>(GlobalAppSettings.DbColumns.DB_User.UserId),
                                UserName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.UserName),
                                FirstName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.FirstName),
                                LastName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.LastName),
                                DisplayName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.DisplayName),
                                Email = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Email),
                                Avatar = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Picture),
                                CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_User.CreatedDate),
                                           TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone())),
                                ModifiedDate = TimeZoneInfo.ConvertTimeFromUtc(row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_User.ModifiedDate),
                                            TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone())),
                            }).ToList();
                }
                return resultUsers;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in searching users", e, MethodBase.GetCurrentMethod(), " SearchString - " + searchString);
                return resultUsers;
            }
        }

        public List<string> ValidateEmailList(List<string> emailList)
        {
            var result = new Result();
            var returnList = new List<string>();
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.Email,
                        Condition = Conditions.IN,
                        Values = emailList
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.IsDeleted,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = false
                    }
                };

                result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            whereColumns));
                if (result.Status)
                {
                    returnList =
                        result.DataTable.AsEnumerable()
                            .Select(row => row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Email))
                            .ToList();
                }
                return returnList;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in validating email ids", e, MethodBase.GetCurrentMethod(), " Status - " + result.Status);
                return returnList;
            }
        }

        public List<string> ValidateUserNameList(List<string> userNameList)
        {
            var result = new Result();
            var returnList = new List<string>();
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.UserName,
                        Condition = Conditions.IN,
                        Values = userNameList
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.IsDeleted,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = false
                    }
                };

                result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            whereColumns));
                if (result.Status)
                {
                    returnList =
                        result.DataTable.AsEnumerable()
                            .Select(row => row.Field<string>(GlobalAppSettings.DbColumns.DB_User.UserName))
                            .ToList();
                }
                return returnList;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error in validating user names", e, MethodBase.GetCurrentMethod(), " Status - " + result.Status);
                return returnList;
            }
        }

        public User FindUserByEmail(string email)
        {
            var userDetails = new User();
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.Email,
                        Condition = Conditions.Equals,
                        Value = email
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.IsDeleted,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = false
                    }
                };

                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            whereColumns));
                if (result.Status)
                {
                    userDetails =
                        result.DataTable.AsEnumerable()
                            .Select(row => new User
                            {
                                UserId = row.Field<int>(GlobalAppSettings.DbColumns.DB_User.UserId),
                                UserName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.UserName),
                                FirstName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.FirstName),
                                LastName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.LastName),
                                DisplayName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.DisplayName),
                                Email = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Email),
                                Avatar = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Picture),
                                CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_User.CreatedDate),
                                           TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone())),
                                ModifiedDate = TimeZoneInfo.ConvertTimeFromUtc(row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_User.ModifiedDate),
                                            TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone())),
                            }).FirstOrDefault();
                }
                return userDetails;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting user details with email", e, MethodBase.GetCurrentMethod(), " Email - " + email);
                return userDetails;
            }
        }

        public string GetUserPassword(int userId)
        {
            var password = String.Empty;
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.UserId,
                        Condition = Conditions.Equals,
                        Value = userId
                    }
                };

                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            whereColumns));
                if (result.Status)
                {
                    password =
                        result.DataTable.AsEnumerable()
                            .Select(row => row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Password))
                            .FirstOrDefault();
                }
                return new Cryptography().Decryption(password);
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting password of user", e, MethodBase.GetCurrentMethod(), " UserId - " + userId + " Password - " + password);
                return password;
            }
        }

        public string GetUserPassword(string username)
        {
            var password = String.Empty;
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.UserName,
                        Condition = Conditions.Equals,
                        Value = username
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.IsActive,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = true
                    }
                };

                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            whereColumns));
                if (result.Status)
                {
                    password =
                        result.DataTable.AsEnumerable()
                            .Select(row => row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Password))
                            .FirstOrDefault();
                }
                return new Cryptography().Decryption(password);
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting password of user", e, MethodBase.GetCurrentMethod(), " UserName - " + username + " Password - " + password);
                return password;
            }
        }

        public User FindUserByUserName(string username)
        {
            var userDetails = new User();
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.UserName,
                        Condition = Conditions.Equals,
                        Value = username
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.IsDeleted,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = false
                    }
                };

                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            whereColumns));

                if (result.Status)
                {
                    userDetails =
                        result.DataTable.AsEnumerable()
                            .Select(row => new User
                            {
                                UserId = row.Field<int>(GlobalAppSettings.DbColumns.DB_User.UserId),
                                UserName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.UserName),
                                FirstName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.FirstName),
                                LastName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.LastName),
                                DisplayName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.DisplayName),
                                Email = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Email),
                                Avatar = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Picture),
                                CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_User.CreatedDate),
                                            TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone())),
                                ModifiedDate = TimeZoneInfo.ConvertTimeFromUtc(row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_User.ModifiedDate),
                                            TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone())),
                                IsDeleted = row.Field<bool>(GlobalAppSettings.DbColumns.DB_User.IsDeleted),
                                Status =
                                    row.Field<bool>(GlobalAppSettings.DbColumns.DB_User.IsActive)
                                        ? UserStatus.Active
                                        : UserStatus.InActive,
                                Password = (!String.IsNullOrEmpty(row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Password))) ?
                                    new Cryptography().Decryption(
                                        row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Password)) : String.Empty,
                                ContactNumber = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Contact)
                            }).FirstOrDefault();
                }
                return userDetails;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting user details with username", e, MethodBase.GetCurrentMethod(), " UserName - " + username);
                return userDetails;
            }
        }

        public User FindUserByUserId(int userId)
        {
            var userDetails = new User();
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.UserId,
                        Condition = Conditions.Equals,
                        Value = userId
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.IsDeleted,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = false
                    }
                };
                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            whereColumns));
                if (result.Status)
                {
                    userDetails =
                        result.DataTable.AsEnumerable()
                            .Select(row => new User
                            {
                                UserId = row.Field<int>(GlobalAppSettings.DbColumns.DB_User.UserId),
                                UserName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.UserName),
                                FirstName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.FirstName),
                                LastName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.LastName),
                                DisplayName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.DisplayName),
                                Email = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Email),
                                Avatar = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Picture),
                                CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_User.CreatedDate),
                                            TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone())),
                                ModifiedDate = TimeZoneInfo.ConvertTimeFromUtc(row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_User.ModifiedDate),
                                            TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone())),
                                Status =
                                    row.Field<bool>(GlobalAppSettings.DbColumns.DB_User.IsActive)
                                        ? UserStatus.Active
                                        : UserStatus.InActive,
                                ContactNumber = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Contact),
                                UserGroups = GetAllGroupsOfUser(userId),
                                Password = (!String.IsNullOrEmpty(row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Password))) ?
                                    new Cryptography().Decryption(
                                        row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Password)) : String.Empty,
                                ResetPasswordCode =
                                    row.Field<string>(GlobalAppSettings.DbColumns.DB_User.ResetPasswordCode)
                            }).FirstOrDefault();
                }
                return userDetails;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting user details with user id", e, MethodBase.GetCurrentMethod(), " UserId - " + userId);
                return userDetails;
            }
        }

        public User FindUserByActivationCode(string activationCode)
        {
            var userDetails = new User();
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.ActivationCode,
                        Condition = Conditions.Equals,
                        Value = activationCode
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.IsDeleted,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = false
                    }
                };
                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            whereColumns));
                if (result.Status)
                {
                    userDetails =
                        result.DataTable.AsEnumerable()
                            .Select(row => new User
                            {
                                UserId = row.Field<int>(GlobalAppSettings.DbColumns.DB_User.UserId),
                                UserName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.UserName),
                                FirstName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.FirstName),
                                LastName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.LastName),
                                DisplayName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.DisplayName),
                                Email = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Email),
                                Avatar = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Picture),
                                CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_User.CreatedDate),
                                           TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone())),
                                ModifiedDate = TimeZoneInfo.ConvertTimeFromUtc(row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_User.ModifiedDate),
                                            TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone())),
                                ActivationExpirationDate = TimeZoneInfo.ConvertTimeFromUtc(row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_User.ActivationExpirationDate),
                               TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone()))
                            }).FirstOrDefault();
                }
                return userDetails;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting user details with username", e, MethodBase.GetCurrentMethod(), " ActivationCode - " + activationCode);
                return userDetails;
            }
        }

        public DataResponse IsExistingUser(string username)
        {
            var dataResponse = new DataResponse();
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.UserName,
                        Condition = Conditions.Equals,
                        Value = username
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.IsDeleted,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = false
                    }
                };

                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            whereColumns));
                dataResponse.Success = result.Status;
                if (result.Status)
                {
                    dataResponse.Value = (result.DataTable.Rows.Count != 0);
                }
                return dataResponse;
            }
            catch (Exception e)
            {
                dataResponse.Success = false;
                LogExtension.LogError("Error while checking user existance", e, MethodBase.GetCurrentMethod(), " UserName - " + username);
                return dataResponse;
            }
        }

        public bool IsExistingEmail(string email)
        {
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.Email,
                        Condition = Conditions.Equals,
                        Value = email
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.IsDeleted,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = false
                    }
                };

                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            whereColumns));

                result.Status = result.DataTable.Rows.Count > 0;

                return result.Status;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while checking email existance", e, MethodBase.GetCurrentMethod(), " Email - " + email);
                return false;
            }
        }

        public string GenerateRandomCode(int length)
        {
            const string randomCharacter = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(randomCharacter, length)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());

            return result;
        }

        public bool DeleteUser(int userId)
        {
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.UserId,
                        Condition = Conditions.Equals,
                        Value = userId
                    }
                };
                var updateColumns = new List<UpdateColumn>
                {
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.IsDeleted,
                        Value = 1
                    },
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.IsActive,
                        Value = 0
                    },
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.ModifiedDate,
                        Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    }
                };

                var result = _dataProvider.ExecuteNonQuery(
                    _queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName, updateColumns,
                        whereColumns));
                return result.Status;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while deleting user", e, MethodBase.GetCurrentMethod(), " UserId - " + userId);
                return false;
            }
        }

        public bool UpdateUserProfileDetails(List<UpdateColumn> updateColumns, int userId)
        {
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.UserId,
                        Condition = Conditions.Equals,
                        Value = userId
                    }
                };
                var result = _dataProvider.ExecuteNonQuery(
                    _queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                        updateColumns, whereColumns));
                return result.Status;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while updating user profile", e, MethodBase.GetCurrentMethod(), " UserId - " + userId);
                return false;
            }
        }

        public List<User> GetUsers(List<ConditionColumn> whereConditionColumns)
        {
            var result = new Result();
            var userList = new List<User>();
            try
            {
                result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            whereConditionColumns));
                if (result.Status)
                {
                    userList =
                        result.DataTable.AsEnumerable()
                            .Select(row => new User
                            {
                                UserId = row.Field<int>(GlobalAppSettings.DbColumns.DB_User.UserId),
                                UserName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.UserName),
                                FirstName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.FirstName),
                                LastName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.LastName),
                                DisplayName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.DisplayName),
                                Email = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Email),
                                Avatar = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Picture),
                                CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_User.CreatedDate),
           TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone())),
                                ModifiedDate = TimeZoneInfo.ConvertTimeFromUtc(row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_User.ModifiedDate),
                                            TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone())),
                            }).ToList();
                }
                return userList;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting user list with whereConditionColumns", e,
                    MethodBase.GetCurrentMethod(), " Status - " + result.Status);
                return userList;
            }
        }



        public List<User> GetAllActiveEmails()
        {
            var result = new Result();
            var emailList = new List<User>();
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.IsDeleted,
                        Condition = Conditions.Equals,
                        Value = false
                    }
                };

                result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            whereColumns));

                if (result.Status)
                {
                    emailList =
                        result.DataTable.AsEnumerable()
                            .Select(row => new User
                            {
                                Email = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Email)

                            }).ToList();
                }
                return emailList;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting Email list", e,
                    MethodBase.GetCurrentMethod(), " Status - " + result.Status);
                return emailList;
            }
        }

        public List<User> GetAllUsers()
        {
            var result = new Result();
            var userList = new List<User>();
            try
            {
                var orderByColumns = new List<OrderByColumns>
                {
                    new OrderByColumns
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.FirstName,
                        OrderBy = OrderByType.Asc,
                    }
                };
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.IsActive,
                        Condition = Conditions.Equals,
                        Value = true
                    }
                };
                result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            whereColumns, orderByColumns, null, null));
                if (result.Status)
                {
                    userList =
                        result.DataTable.AsEnumerable()
                            .Select(row => new User
                            {
                                UserId = row.Field<int>(GlobalAppSettings.DbColumns.DB_User.UserId),
                                UserName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.UserName),
                                FirstName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.FirstName),
                                LastName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.LastName),
                                DisplayName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.DisplayName),
                                Email = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Email),
                                Avatar = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Picture)
                            }).ToList();
                }
                return userList;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting user list", e,
                    MethodBase.GetCurrentMethod(), " Status - " + result.Status);
                return userList;
            }
        }

        public List<User> GetAllActiveInactiveUsers()
        {
            var result = new Result();
            var userList = new List<User>();
            try
            {


                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.IsDeleted,
                        Condition = Conditions.Equals,
                        Value = false
                    }
                };
                result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            whereColumns));
                if (result.Status)
                {
                    userList =
                        result.DataTable.AsEnumerable()
                            .Select(row => new User
                            {
                                UserId = row.Field<int>(GlobalAppSettings.DbColumns.DB_User.UserId),
                                UserName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.UserName),
                                FirstName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.FirstName),
                                LastName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.LastName),
                                DisplayName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.DisplayName),
                                Email = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Email),
                                Avatar = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Picture)
                            }).ToList();
                }
                return userList;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting user list", e,
                    MethodBase.GetCurrentMethod(), " Status - " + result.Status);
                return userList;
            }
        }

        public bool IsValidEmail(string emailAddress)
        {
            try
            {
                var isValidEmail = new MailAddress(emailAddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetUserPreferentSort(int userId)
        {
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_UserPreference.UserId,
                        Condition = Conditions.Equals,
                        Value = userId
                    }
                };
                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            whereColumns));
                return result.Status
                    ? result.DataTable.AsEnumerable()
                        .Select(r => r.Field<string>(GlobalAppSettings.DbColumns.DB_UserPreference.ItemSort))
                        .FirstOrDefault()
                    : string.Empty;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting user prefer sorting", e,
                    MethodBase.GetCurrentMethod(), " UserId - " + userId);
                return string.Empty;
            }
        }

        public string GetUserPreferenceFilters(int userId)
        {
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_UserPreference.UserId,
                        Condition = Conditions.Equals,
                        Value = userId
                    }
                };
                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            whereColumns));
                return result.Status
                    ? result.DataTable.AsEnumerable()
                        .Select(r => r.Field<string>(GlobalAppSettings.DbColumns.DB_UserPreference.ItemFilters))
                        .FirstOrDefault()
                    : string.Empty;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting user prefer filters", e,
                    MethodBase.GetCurrentMethod(), " UserId - " + userId);
                return string.Empty;
            }
        }

        public bool UpdateUserPreferenceDetails(List<UpdateColumn> updateColumns, int userId)
        {
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_UserPreference.UserId,
                        Condition = Conditions.Equals,
                        Value = userId
                    }
                };
                var result = _dataProvider.ExecuteNonQuery(
                    _queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_UserPreference.DB_TableName,
                        updateColumns, whereColumns));
                return result.Status;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while updating user preference details", e, MethodBase.GetCurrentMethod(), " UserId - " + userId);
                return false;
            }
        }

        public string GetUserPreferTimeZone(int userId)
        {
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_UserPreference.UserId,
                        Condition = Conditions.Equals,
                        Value = userId
                    }
                };
                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(
                            GlobalAppSettings.DbColumns.DB_UserPreference.DB_TableName,
                            whereColumns));
                return result.Status
                    ? result.DataTable.AsEnumerable()
                        .Select(r => r.Field<string>(GlobalAppSettings.DbColumns.DB_UserPreference.TimeZone))
                        .FirstOrDefault()
                    : string.Empty;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting user prefer filters", e,
                    MethodBase.GetCurrentMethod(), " UserId - " + userId);
                return string.Empty;
            }
        }

        public bool ActivateUser(int userId, string password)
        {
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.UserId,
                        Condition = Conditions.Equals,
                        Value = userId
                    }
                };
                var updateColumns = new List<UpdateColumn>
                {
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.Password,
                        Value = password
                    },
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.IsActive,
                        Value = true
                    },
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.ActivationCode,
                        Value = "default"
                    },
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.ModifiedDate,
                        Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    },
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.IsActivated,
                        Value = true
                    }
                };
                var result = _dataProvider.ExecuteNonQuery(
                    _queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName, updateColumns,
                        whereColumns));
                return result.Status;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while activating user", e,
                    MethodBase.GetCurrentMethod(), " UserId - " + userId + " Password - " + password);
                return false;
            }
        }

        public int GetRecordSizeFromUserPreference(int userId)
        {
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_UserPreference.UserId,
                        Condition = Conditions.Equals,
                        Value = userId
                    }
                };
                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectAllRecordsFromTable(
                            GlobalAppSettings.DbColumns.DB_UserPreference.DB_TableName,
                            whereColumns));
                if (result.Status)
                {
                    var records =
                        result.DataTable.AsEnumerable()
                            .Select(s => s.Field<int?>(GlobalAppSettings.DbColumns.DB_UserPreference.RecordSize))
                            .FirstOrDefault();
                    return records != null ? Convert.ToInt32(records) : 0;
                }
                return 0;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting record count of user", e,
                    MethodBase.GetCurrentMethod(), " UserId - " + userId);
                return 0;
            }
        }

        public int GetUserId(string userName)
        {
            const int userId = 0;
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.UserName,
                        Condition = Conditions.Equals,
                        Value = userName
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.IsDeleted,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = false
                    }
                };

                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectTopRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            new List<SelectedColumn>
                            {
                                new SelectedColumn {ColumnName = GlobalAppSettings.DbColumns.DB_User.UserId}
                            }, 1,
                            whereColumns, null,
                            null, null));
                if (result.Status)
                {
                    var firstRecord = result.DataTable.AsEnumerable().FirstOrDefault();
                    if (firstRecord == null) return userId;
                    return firstRecord
                        .Field<int>(GlobalAppSettings.DbColumns.DB_User.UserId);
                }
                return userId;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting user id from user name", e,
                    MethodBase.GetCurrentMethod(), " UserName - " + userName + " UserId - " + userId);
                return userId;
            }
        }

        public string GetUserName(int userId)
        {
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.UserId,
                        Condition = Conditions.Equals,
                        Value = userId
                    }
                };

                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectTopRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            new List<SelectedColumn>
                            {
                                new SelectedColumn {ColumnName = GlobalAppSettings.DbColumns.DB_User.UserName}
                            }, 1,
                            whereColumns, null,
                            null, null));

                if (result.Status)
                {
                    var firstRecord = result.DataTable.AsEnumerable().FirstOrDefault();

                    return firstRecord == null
                        ? String.Empty
                        : firstRecord.Field<string>(GlobalAppSettings.DbColumns.DB_User.UserName);
                }
                return String.Empty;


            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting username from userid", e,
                    MethodBase.GetCurrentMethod(), " UserId - " + userId);
                return String.Empty;
            }
        }

        public DataResponse IsActiveUser(int userId)
        {
            var dataResponse = new DataResponse();
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.UserId,
                        Condition = Conditions.Equals,
                        Value = userId
                    }
                };

                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectTopRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            new List<SelectedColumn>
                            {
                                new SelectedColumn {ColumnName = GlobalAppSettings.DbColumns.DB_User.IsActive}
                            }, 1,
                            whereColumns, null,
                            null, null));
                dataResponse.Success = result.Status;
                if (result.Status)
                {
                    dataResponse.Value = result.DataTable.AsEnumerable()
                        .Select(s => s.Field<bool>(GlobalAppSettings.DbColumns.DB_User.IsActive))
                        .FirstOrDefault();
                }
                return dataResponse;
            }
            catch (Exception e)
            {
                dataResponse.Success = false;
                LogExtension.LogError("Error while validating active user", e,
                    MethodBase.GetCurrentMethod(), " UserId - " + userId);
                return dataResponse;
            }
        }

        public DataResponse IsDeletedUser(int userId)
        {
            var dataResponse = new DataResponse();
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.UserId,
                        Condition = Conditions.Equals,
                        Value = userId
                    }
                };

                var result =
                    _dataProvider.ExecuteReaderQuery(
                        _queryBuilder.SelectTopRecordsFromTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            new List<SelectedColumn>
                            {
                                new SelectedColumn {ColumnName = GlobalAppSettings.DbColumns.DB_User.IsDeleted}
                            }, 1,
                            whereColumns, null,
                            null, null));
                dataResponse.Success = result.Status;
                if (result.Status)
                {
                    dataResponse.Value = result.DataTable.AsEnumerable()
                        .Select(s => s.Field<bool>(GlobalAppSettings.DbColumns.DB_User.IsDeleted))
                        .FirstOrDefault();
                }
                return dataResponse;
            }
            catch (Exception e)
            {
                dataResponse.Success = false;
                LogExtension.LogError("Error while validating deleted user", e,
                    MethodBase.GetCurrentMethod(), " UserId - " + userId);
                return dataResponse;
            }
        }

        public bool UpdateLoginTime(int userId, DateTime loginTime)
        {
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.UserId,
                        Condition = Conditions.Equals,
                        Value = userId,
                    }
                };

                var updatedcolumns = new List<UpdateColumn>
                {
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_User.LastLogin,
                        Value = loginTime
                    }
                };

                var result = _dataProvider.ExecuteReaderQuery(
                    _queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName, updatedcolumns,
                        whereColumns));
                return result.Status;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while updating last login time", e,
                    MethodBase.GetCurrentMethod(), " UserId - " + userId + " LoginTime - " + loginTime);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereConditionColumns"></param>
        /// <returns></returns>
        public Result UserLogView(List<ConditionColumn> whereConditionColumns)
        {
            var query = "SELECT GroupTable." + GlobalAppSettings.DbColumns.DB_Group.Name + " as GroupName, User1." +
                        GlobalAppSettings.DbColumns.DB_User.FirstName + ", User1." +
                        GlobalAppSettings.DbColumns.DB_User.Picture + ", UserLog." +
                        GlobalAppSettings.DbColumns.DB_UserLog.UserLogId + ", UserLog." +
                        GlobalAppSettings.DbColumns.DB_UserLog.UserLogTypeId + ", UserLog." +
                        GlobalAppSettings.DbColumns.DB_UserLog.UpdatedUserId + ", User1." +
                        GlobalAppSettings.DbColumns.DB_User.UserId + " AS CurrentUserId, UserLog." +
                        GlobalAppSettings.DbColumns.DB_UserLog.GroupId + ", UserLog." +
                        GlobalAppSettings.DbColumns.DB_UserLog.OldValue + ", UserLog." +
                        GlobalAppSettings.DbColumns.DB_UserLog.NewValue + ", UserLog." +
                        GlobalAppSettings.DbColumns.DB_UserLog.TargetUserId + ", UserLog." +
                        GlobalAppSettings.DbColumns.DB_UserLog.ModifiedDate + ",User1." +
                        GlobalAppSettings.DbColumns.DB_User.LastName + ",User1." +
                        GlobalAppSettings.DbColumns.DB_User.DisplayName + ", User2." +
                        GlobalAppSettings.DbColumns.DB_User.FirstName + " AS TargetUserFullName, UserLogType." +
                        GlobalAppSettings.DbColumns.DB_UserLogType.Name + " FROM [" +
                        GlobalAppSettings.DbColumns.DB_User.DB_TableName + "] AS User1 INNER JOIN " +
                        GlobalAppSettings.DbColumns.DB_UserLog.DB_TableName + " AS UserLog ON User1." +
                        GlobalAppSettings.DbColumns.DB_User.UserId + " = UserLog." +
                        GlobalAppSettings.DbColumns.DB_UserLog.UpdatedUserId + " INNER JOIN [" +
                        GlobalAppSettings.DbColumns.DB_User.DB_TableName + "] AS User2 ON UserLog." +
                        GlobalAppSettings.DbColumns.DB_UserLog.TargetUserId + " = User2." +
                        GlobalAppSettings.DbColumns.DB_User.UserId + " INNER JOIN " +
                        GlobalAppSettings.DbColumns.DB_UserLogType.DB_TableName + " AS UserLogType ON UserLog." +
                        GlobalAppSettings.DbColumns.DB_UserLog.UserLogTypeId + " = UserLogType." +
                        GlobalAppSettings.DbColumns.DB_UserLogType.UserLogTypeId + " LEFT OUTER JOIN [" +
                        GlobalAppSettings.DbColumns.DB_Group.DB_TableName + "] AS GroupTable ON UserLog." +
                        GlobalAppSettings.DbColumns.DB_UserLog.GroupId + " = GroupTable." +
                        GlobalAppSettings.DbColumns.DB_Group.GroupId;
            query = _queryBuilder.ApplyWhereClause(query, whereConditionColumns);
            return _dataProvider.ExecuteReaderQuery(query);
        }

        public Result SystemLogView(List<ConditionColumn> whereConditionColumns)
        {
            var viewquery = "SELECT User1." + GlobalAppSettings.DbColumns.DB_User.FirstName + ",User1." +
                            GlobalAppSettings.DbColumns.DB_User.UserId + " AS CurrentUserId, User1." +
                            GlobalAppSettings.DbColumns.DB_User.Picture + ", SystemLog." +
                            GlobalAppSettings.DbColumns.DB_SystemLog.LogId + ", SystemLog." +
                            GlobalAppSettings.DbColumns.DB_SystemLog.SystemLogTypeId + ", SystemLog." +
                            GlobalAppSettings.DbColumns.DB_SystemLog.UpdatedUserId + ", SystemLog." +
                            GlobalAppSettings.DbColumns.DB_SystemLog.TargetUserId + ", SystemLog." +
                            GlobalAppSettings.DbColumns.DB_SystemLog.ModifiedDate + ",User1." +
                            GlobalAppSettings.DbColumns.DB_User.LastName + ",User1." +
                            GlobalAppSettings.DbColumns.DB_User.DisplayName + ", User2." +
                            GlobalAppSettings.DbColumns.DB_User.FirstName + " AS TargetUserFullName, SystemLogType." +
                            GlobalAppSettings.DbColumns.DB_SystemLogType.Name + " FROM [" +
                            GlobalAppSettings.DbColumns.DB_User.DB_TableName + "] AS User1 INNER JOIN " +
                            GlobalAppSettings.DbColumns.DB_SystemLog.DB_TableName + " AS SystemLog ON User1." +
                            GlobalAppSettings.DbColumns.DB_User.UserId + " = SystemLog." +
                            GlobalAppSettings.DbColumns.DB_SystemLog.UpdatedUserId + " INNER JOIN  [" +
                            GlobalAppSettings.DbColumns.DB_User.DB_TableName + "] AS User2 ON SystemLog." +
                            GlobalAppSettings.DbColumns.DB_SystemLog.TargetUserId + " = User2." +
                            GlobalAppSettings.DbColumns.DB_User.UserId + " INNER JOIN " +
                            GlobalAppSettings.DbColumns.DB_SystemLogType.DB_TableName +
                            " AS SystemLogType ON SystemLogType." +
                            GlobalAppSettings.DbColumns.DB_SystemLogType.SystemLogTypeId + " = SystemLog." +
                            GlobalAppSettings.DbColumns.DB_SystemLog.SystemLogTypeId + "";
            var query = _queryBuilder.ApplyWhereClause(viewquery, whereConditionColumns);
            return _dataProvider.ExecuteReaderQuery(query);
        }


        public List<UserGroup> GetAllGroupsOfUser(int userId)
        {
            var groupList = new List<UserGroup>();
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName,
                        ColumnName = GlobalAppSettings.DbColumns.DB_UserGroup.UserId,
                        Condition = Conditions.Equals,
                        Value = userId
                    },
                    new ConditionColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName,
                        ColumnName = GlobalAppSettings.DbColumns.DB_UserGroup.IsActive,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = true
                    },
                    new ConditionColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_Group.DB_TableName,
                        ColumnName = GlobalAppSettings.DbColumns.DB_Group.IsActive,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = true
                    }
                };
                var selected = new List<SelectedColumn>
                {
                    new SelectedColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_Group.DB_TableName,
                        ColumnName = "*"
                    },
                    new SelectedColumn
                    {
                        TableName = GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName,
                        ColumnName = GlobalAppSettings.DbColumns.DB_UserGroup.UserId
                    }
                };
                var joinSpecification = new List<JoinSpecification>
                {
                    new JoinSpecification
                    {
                        Table = GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName,
                        Column =
                            new List<JoinColumn>
                            {
                                new JoinColumn
                                {
                                    TableName = GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName,
                                    JoinedColumn = GlobalAppSettings.DbColumns.DB_UserGroup.GroupId,
                                    Operation = Conditions.Equals,
                                    ParentTableColumn = GlobalAppSettings.DbColumns.DB_Group.GroupId,
                                    ParentTable = GlobalAppSettings.DbColumns.DB_Group.DB_TableName
                                }
                            },
                        JoinType = JoinTypes.Inner
                    },
                    new JoinSpecification
                    {
                        Table = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                        Column =
                            new List<JoinColumn>
                            {
                                new JoinColumn
                                {
                                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                                    JoinedColumn = GlobalAppSettings.DbColumns.DB_User.UserId,
                                    Operation = Conditions.Equals,
                                    ParentTableColumn = GlobalAppSettings.DbColumns.DB_UserGroup.UserId,
                                    ParentTable = GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName
                                }
                            },
                        JoinType = JoinTypes.Left
                    }
                };
                var result = _dataProvider.ExecuteReaderQuery(_queryBuilder.ApplyWhereClause(
                    _queryBuilder.ApplyMultipleJoins(GlobalAppSettings.DbColumns.DB_Group.DB_TableName, selected,
                        joinSpecification), whereColumns));
                if (result.Status)
                {
                    groupList =
                        result.DataTable.AsEnumerable()
                            .Select(row => new UserGroup
                            {
                                UserId = row.Field<int>(GlobalAppSettings.DbColumns.DB_UserGroup.UserId),
                                Id = row.Field<int>(GlobalAppSettings.DbColumns.DB_Group.GroupId),
                                Name = row.Field<string>(GlobalAppSettings.DbColumns.DB_Group.Name),
                                Color = row.Field<string>(GlobalAppSettings.DbColumns.DB_Group.Color)
                            }).Distinct().ToList();
                }
                return groupList;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while getting group list of user", e, MethodBase.GetCurrentMethod(), " UserId - " + userId);
                return groupList;
            }
        }

        public bool UpdateUserSortPreference(string sortXml, int userId)
        {
            var updatedcolumns = new List<UpdateColumn>
            {
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPreference.ItemSort,
                    Value = sortXml
                }
            };
            var condition = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserPreference.UserId,
                    Condition = Conditions.Equals,
                    Value = userId
                }
            };
            try
            {
                var result = _dataProvider.ExecuteNonQuery(
                    _queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_UserPreference.DB_TableName,
                        updatedcolumns, condition));
                return result.Status;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Error while updating user preference sort order", e,
                    MethodBase.GetCurrentMethod(), " ItemSort - " + sortXml + " UserId - " + userId);
                return false;
            }
        }

        /// <summary>
        ///     Set reset password for specific user email
        /// </summary>
        /// <param name="userId">user id which is used to find from the user table ,if user id is found and then update the user table</param>
        public DataResponse SetResetPasswordForUser(int userId)
        {
            var dataResponse = new DataResponse();
            var resetPasswordCode = GenerateRandomCode(16);
            dataResponse.Value = resetPasswordCode;
            var updateColumns = new List<UpdateColumn>
            {
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.ResetPasswordCode,
                    Value = resetPasswordCode
                }
            };
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.UserId,
                    Condition = Conditions.Equals,
                    Value = userId
                }
            };
            try
            {
                var result = _dataProvider.ExecuteNonQuery(
                    _queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_User.DB_TableName, updateColumns,
                        whereColumns));
                dataResponse.Success = result.Status;
                return dataResponse;
            }
            catch (Exception e)
            {
                dataResponse.Success = false;
                return dataResponse;
            }
        }

        public DataTable GetUserDetail(List<SortCollection> sorted, int? skip, int? take = 10, string searchkey = "", List<FilterCollection> filterCollection = null)
        {
            var sortDescription = " Order by " + GlobalAppSettings.DbColumns.DB_User.DisplayName + " asc";

            if (sorted != null && sorted.Any())
            {
                if (sorted.FirstOrDefault().Name.ToLower() == "statusdescription")
                {
                    sorted.FirstOrDefault().Name = "IsActive";
                    sortDescription = " Order by " + sorted.FirstOrDefault().Name + " " + (
                        sorted.FirstOrDefault().Direction.ToLower() == "ascending"
                            ? "desc"
                            : "asc");
                }
                else
                {
                    if (sorted.FirstOrDefault().Name.ToLower() == "name")
                    {
                        sorted.FirstOrDefault().Name = "DisplayName";
                    }
                    sortDescription = " Order by " + sorted.FirstOrDefault().Name + " " + (
                        sorted.FirstOrDefault().Direction.ToLower() == "ascending"
                            ? "asc"
                            : "desc");
                }
            }

            var query = "SELECT "+
                        "a." +
                        GlobalAppSettings.DbColumns.DB_User.UserName + " AS UserName,a." +
                        GlobalAppSettings.DbColumns.DB_User.FirstName + " AS FirstName,a." +
                        GlobalAppSettings.DbColumns.DB_User.LastName + " AS LastName,a." +
                        GlobalAppSettings.DbColumns.DB_User.Email + " AS Email,a." +
                        GlobalAppSettings.DbColumns.DB_User.IsActive + " AS IsActive,a." +
                        GlobalAppSettings.DbColumns.DB_User.UserId + " AS Id,a."
                        + GlobalAppSettings.DbColumns.DB_User.DisplayName +
                        " as DisplayName FROM [" +
                        GlobalAppSettings.DbColumns.DB_User.DB_TableName + "] AS a where a." +
                        GlobalAppSettings.DbColumns.DB_User.IsDeleted + "=0";

            query = "SELECT * from (" + query +
                         ") UserTable ";

            query += " WHERE (" + GlobalAppSettings.DbColumns.DB_User.DisplayName +
                    " like '%" + searchkey + "%' OR " + GlobalAppSettings.DbColumns.DB_User.Email +
                    " like '%" + searchkey + "%' OR " + GlobalAppSettings.DbColumns.DB_User.FirstName +
                    " like '%" + searchkey + "%' OR " + GlobalAppSettings.DbColumns.DB_User.LastName +
                    " like '%" + searchkey + "%') ";

            if (filterCollection != null && filterCollection.Any())
            {
                foreach (var filter in filterCollection)
                {
                    query += " AND " + filter.PropertyName;
                    switch (filter.FilterType)
                    {
                        case "startswith":
                            query += " like '" + filter.FilterKey + "%'";
                            break;
                        case "endswith":
                            query += " like '%" + filter.FilterKey + "'";
                            break;
                        case "contains":
                            query += " like '%" + filter.FilterKey + "%'";
                            break;
                        case "equal":
                            query += " = '" + filter.FilterKey + "'";
                            break;
                        case "notequal":
                            query += " != '" + filter.FilterKey + "'";
                            break;
                        default:
                            query += " like '%" + filter.FilterKey + "%'";
                            break;
                    }

                }
            }
            query += sortDescription;
            return _dataProvider.ExecuteReaderQuery(query).DataTable;
        }

    }
}