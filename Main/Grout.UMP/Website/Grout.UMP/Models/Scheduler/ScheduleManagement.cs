using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Data;
using Grout.Base.Reports.Scheduler;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Grout.UMP.Models
{
    public class ScheduleManagement
    {
        private readonly IRelationalDataProvider dataProvider;
        private readonly IQueryBuilder queryBuilder;
        private readonly UserManagement userManagement = new UserManagement();
        private readonly ScheduleRecurrenceManager recurrenceManager = new ScheduleRecurrenceManager();

        public ScheduleManagement(IQueryBuilder builder, IRelationalDataProvider provider)
        {
            queryBuilder = builder;
            dataProvider = provider;
        }

        public Result AddScheduleItem(ScheduleItem item)
        {
            var result = new Result();
            var id = String.Empty;

            var itemValues = new Dictionary<string, object>
            {
                {GlobalAppSettings.DbColumns.DB_Item.Name, item.Name},                 
                {GlobalAppSettings.DbColumns.DB_Item.ItemTypeId, Convert.ToInt32(ItemType.Schedule)},
                {
                    GlobalAppSettings.DbColumns.DB_Item.CreatedById,
                    HttpContext.Current.User.Identity.Name
                },
                {GlobalAppSettings.DbColumns.DB_Item.CreatedDate, DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())},
                {
                    GlobalAppSettings.DbColumns.DB_Item.ModifiedById,
                    HttpContext.Current.User.Identity.Name
                },
                {GlobalAppSettings.DbColumns.DB_Item.ModifiedDate, DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())},
                {GlobalAppSettings.DbColumns.DB_Item.IsActive, true}
            };


            try
            {
                var guid = Guid.NewGuid();
                id =
                    dataProvider.ExecuteScalarQuery(
                        queryBuilder.AddToTableWithGUID(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, itemValues,
                            GlobalAppSettings.DbColumns.DB_Item.Id, guid), guid).ReturnValue.ToString();
            }
            catch (SqlException ex)
            {
                result.Exception = ex;
                result.Status = false;
                return result;
            }

            result.ReturnValue = item.ScheduleId = item.Recurrence.ScheduleId = new Guid(id);
            result.Status = true;

            var values = new Dictionary<string, object>
            {
                {GlobalAppSettings.DbColumns.DB_ScheduleDetail.ItemId, item.ItemId},
                {GlobalAppSettings.DbColumns.DB_ScheduleDetail.Name, item.Name},
                {GlobalAppSettings.DbColumns.DB_ScheduleDetail.ScheduleId, item.ScheduleId},
                {GlobalAppSettings.DbColumns.DB_ScheduleDetail.RecurrenceTypeId, Convert.ToInt32(item.RecurrenceTypeId)},
                {GlobalAppSettings.DbColumns.DB_ScheduleDetail.ExportTypeId, Convert.ToInt32(item.ExportTypeId)},
                {
                    GlobalAppSettings.DbColumns.DB_ScheduleDetail.RecurrenceInfo,
                    ScheduleHelper.XMLSerializer(item.Recurrence)
                },
                {GlobalAppSettings.DbColumns.DB_ScheduleDetail.EndAfter, Convert.ToInt32(item.EndAfter)},
                {GlobalAppSettings.DbColumns.DB_ScheduleDetail.CreatedDate, DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())},
                {
                    GlobalAppSettings.DbColumns.DB_ScheduleDetail.CreatedById,
                    HttpContext.Current.User.Identity.Name
                },
                {GlobalAppSettings.DbColumns.DB_ScheduleDetail.ModifiedDate, DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())},
                {
                    GlobalAppSettings.DbColumns.DB_ScheduleDetail.ModifiedById,
                    HttpContext.Current.User.Identity.Name
                },
                {GlobalAppSettings.DbColumns.DB_ScheduleDetail.IsEnabled, item.IsEnabled},
                {GlobalAppSettings.DbColumns.DB_ScheduleDetail.IsActive, true}
            };

            var nextscheduleDate = recurrenceManager.GetNextScheduleDate(item.Recurrence.StartBoundary,
                ScheduleHelper.XMLSerializer(item.Recurrence));

            if (nextscheduleDate != null)
            {
                values.Add(GlobalAppSettings.DbColumns.DB_ScheduleDetail.NextSchedule, nextscheduleDate.Value.ToString(GlobalAppSettings.GetDateTimeFormat()));
            }
            else
            {
                values.Add(GlobalAppSettings.DbColumns.DB_ScheduleDetail.NextSchedule, DBNull.Value);
            }

            values.Add(GlobalAppSettings.DbColumns.DB_ScheduleDetail.StartDate, item.Recurrence.StartBoundary.ToString(GlobalAppSettings.GetDateTimeFormat()));

            if (item.EndDate != null)
            {
                values.Add(GlobalAppSettings.DbColumns.DB_ScheduleDetail.EndDate, item.Recurrence.EndBoundary.Value.ToString(GlobalAppSettings.GetDateTimeFormat()));
            }

            try
            {
                dataProvider.ExecuteNonQuery(
                    queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName, values));
            }
            catch (SqlException e)
            {
                result.Exception = e;
                result.Status = false;
                return result;
            }

            return result;
        }

        public Result SubscribeUsers(Guid scheduleId, dynamic users)
        {
            var result = new Result();
            foreach (var user in users)
            {
                var values = new Dictionary<string, object>
                {
                    {GlobalAppSettings.DbColumns.DB_SubscribedUser.ScheduleId, scheduleId},
                    {
                        GlobalAppSettings.DbColumns.DB_SubscribedUser.RecipientUserId,
                        Convert.ToInt32(userManagement.GetUserId(user.Value))
                    },
                    {
                        GlobalAppSettings.DbColumns.DB_SubscribedUser.SubscribedById,
                        HttpContext.Current.User.Identity.Name
                    },
                    {GlobalAppSettings.DbColumns.DB_SubscribedUser.SubscribedDate, DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())},
                    {GlobalAppSettings.DbColumns.DB_SubscribedUser.ModifiedDate, DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())},
                    {GlobalAppSettings.DbColumns.DB_SubscribedUser.IsActive, true}
                };

                try
                {
                    dataProvider.ExecuteNonQuery(
                        queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName, values));
                }
                catch (SqlException e)
                {
                    result.Exception = e;
                    result.Status = false;
                    return result;
                }
            }

            return result;
        }

        public Result SubscribeGroups(Guid scheduleId, dynamic groups)
        {
            var result = new Result();

            foreach (var group in groups)
            {
                var values = new Dictionary<string, object>
                {
                    {GlobalAppSettings.DbColumns.DB_SubscribedGroup.ScheduleId, scheduleId},
                    {GlobalAppSettings.DbColumns.DB_SubscribedGroup.RecipientGroupId, Convert.ToInt32(@group)},
                    {
                        GlobalAppSettings.DbColumns.DB_SubscribedGroup.SubscribedById,
                        HttpContext.Current.User.Identity.Name
                    },
                    {GlobalAppSettings.DbColumns.DB_SubscribedGroup.ModifiedDate, DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())},
                    {GlobalAppSettings.DbColumns.DB_SubscribedGroup.SubscribedDate, DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())},
                    {GlobalAppSettings.DbColumns.DB_SubscribedGroup.IsActive, true}
                };

                try
                {
                    dataProvider.ExecuteNonQuery(
                        queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_SubscribedGroup.DB_TableName, values));
                }
                catch (SqlException e)
                {
                    result.Exception = e;
                    result.Status = false;
                    return result;
                }
            }

            return result;
        }

        public dynamic GetExportTypes()
        {
            DataTable dt = new DataTable();
            try
            {
                var selectedColumns = new List<SelectedColumn>
                {
                    new SelectedColumn {ColumnName = GlobalAppSettings.DbColumns.DB_ExportType.Id},
                    new SelectedColumn {ColumnName = GlobalAppSettings.DbColumns.DB_ExportType.Name}
                };
                dt =
                    dataProvider.ExecuteReaderQuery(
                        queryBuilder.SelectRecordsFromTable(GlobalAppSettings.DbColumns.DB_ExportType.DB_TableName,
                            selectedColumns))
                        .DataTable;
            }
            catch (SqlException e)
            {

            }

            return dt.AsEnumerable().ToList();
        }

        public T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public dynamic GetParseSchedule(dynamic scheduleItem)
        {
            ScheduleItem item = new ScheduleItem();
            Schedule schedule = new Schedule();

            item.Name = scheduleItem["ScheduleName"].Value;

            if (scheduleItem["ItemId"].Value != "null")
            {
                schedule.ItemId = item.ItemId = new Guid(scheduleItem["ItemId"].Value);
            }

            string exportType = scheduleItem["ExportType"].Value;
            switch (exportType)
            {
                case "PDF":
                    item.ExportTypeId = Convert.ToInt32(ExportType.Pdf);
                    break;
                case "Word":
                    item.ExportTypeId = Convert.ToInt32(ExportType.Word);
                    break;
                case "Excel":
                    item.ExportTypeId = Convert.ToInt32(ExportType.Excel);
                    break;
                case "HTML":
                    item.ExportTypeId = Convert.ToInt32(ExportType.Html);
                    break;

            }
            item.IsEnabled = schedule.Enabled = Convert.ToBoolean(scheduleItem["IsEnabled"]) ? true : false;
            string recurrenceType = (scheduleItem["RecurrenceType"].Value).ToString();
            switch (recurrenceType)
            {
                case "Daily":

                    schedule.DailySchedule = new DailySchedule()
                    {
                        DaysInterval = Convert.ToInt32(scheduleItem["RecurrenceInterval"])
                    };
                    schedule.Type = TaskScheduleType.Daily;
                    item.RecurrenceTypeId = GetRecurrenceTypeId("Daily");
                    break;
                case "DailyWeekDay":
                    schedule.DailyWeekDaySchedule = new DailyWeekDaySchedule() { WeekDay = true };
                    schedule.Type = TaskScheduleType.DailyWeekDay;
                    item.RecurrenceTypeId = GetRecurrenceTypeId("DailyWeekDay");
                    break;

                case "Weekly":
                    schedule.WeeklySchedule = new WeeklySchedule()
                    {
                        DaysOfWeek = new DaysOfWeek()
                        {
                            Monday = Convert.ToBoolean(scheduleItem["Monday"].Value),
                            Tuesday = Convert.ToBoolean(scheduleItem["Tuesday"].Value),
                            Wednesday = Convert.ToBoolean(scheduleItem["Wednesday"].Value),
                            Thursday = Convert.ToBoolean(scheduleItem["Thursday"].Value),
                            Friday = Convert.ToBoolean(scheduleItem["Friday"].Value),
                            Saturday = Convert.ToBoolean(scheduleItem["Saturday"].Value),
                            Sunday = Convert.ToBoolean(scheduleItem["Sunday"].Value),
                        },
                        WeeksInterval = Convert.ToInt32(scheduleItem["RecurrenceInterval"].Value)
                    };
                    item.RecurrenceTypeId = GetRecurrenceTypeId("Weekly");
                    schedule.Type = TaskScheduleType.Weekly;
                    break;

                case "Monthly":
                    schedule.MonthlySchedule = new MonthlySchedule()
                    {
                        DayOfMonth = Convert.ToInt32(scheduleItem["DaysOfMonth"].Value),
                        Months = Convert.ToInt32(scheduleItem["RecurrenceInterval"].Value)
                    };
                    schedule.Type = TaskScheduleType.Monthly;
                    item.RecurrenceTypeId = GetRecurrenceTypeId("Monthly");
                    break;
                case "MonthlyDOW":
                    DayOfTheWeek monthlyDayOfWeek = ParseEnum<DayOfTheWeek>(scheduleItem["DayOfWeek"].Value);
                    WeekOfMonth monthlyWeekOfMonth = ParseEnum<WeekOfMonth>(scheduleItem["WeekOfMonth"].Value);
                    schedule.MonthlyDowSchedule = new MonthlyDowSchedule()
                    {

                        DayOfWeek = monthlyDayOfWeek,
                        WeekOfMonth = monthlyWeekOfMonth,
                        Months = Convert.ToInt32(scheduleItem["RecurrenceInterval"].Value),
                    };
                    schedule.Type = TaskScheduleType.MonthlyDOW;
                    item.RecurrenceTypeId = GetRecurrenceTypeId("MonthlyDOW");
                    break;

                case "Yearly":
                    MonthOfTheYear monthOfYear = ParseEnum<MonthOfTheYear>(scheduleItem["MonthOfYear"].Value);
                    schedule.YearlySchedule = new YearlySchedule()
                    {
                        DayOfMonth = Convert.ToInt32(scheduleItem["DaysOfMonth"].Value),
                        YearsInterval = Convert.ToInt32(scheduleItem["RecurrenceInterval"].Value),
                        Month = monthOfYear
                    };
                    schedule.Type = TaskScheduleType.Yearly;
                    item.RecurrenceTypeId = GetRecurrenceTypeId("Yearly");
                    break;
                case "YearlyDOW":
                    DayOfTheWeek yearlyDayOfWeek = ParseEnum<DayOfTheWeek>(scheduleItem["DayOfWeek"].Value);
                    WeekOfMonth yearlyWeekOfMonth = ParseEnum<WeekOfMonth>(scheduleItem["WeekOfMonth"].Value);
                    MonthOfTheYear yearlyMonthOfYear = ParseEnum<MonthOfTheYear>(scheduleItem["MonthOfYear"].Value);

                    schedule.YearlyDowSchedule = new YearlyDowSchedule()
                    {
                        DayOfWeek = yearlyDayOfWeek,
                        YearsInterval = Convert.ToInt32(scheduleItem["RecurrenceInterval"].Value),
                        WeekOfMonth = yearlyWeekOfMonth,
                        MonthOfYear = yearlyMonthOfYear
                    };
                    schedule.Type = TaskScheduleType.YearlyDOW;
                    item.RecurrenceTypeId = GetRecurrenceTypeId("YearlyDOW");
                    break;
            }

            #region Start and End Boundary

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone());
            item.StartDate =
                schedule.StartBoundary =
                    TimeZoneInfo.ConvertTimeToUtc(
                        Convert.ToDateTime(scheduleItem["StartDate"].Value.ToString(), CultureInfo.InvariantCulture),
                        timeZone);

            string scheduleEndType = scheduleItem["ScheduleEndType"].Value.ToString();

            switch (scheduleEndType)
            {
                case "EndBy":
                    item.EndDate =
                        schedule.EndBoundary =
                            TimeZoneInfo.ConvertTimeToUtc(
                                Convert.ToDateTime(scheduleItem["EndDate"].Value.ToString(),
                                    CultureInfo.InvariantCulture),
                                timeZone);
                    schedule.EndType = EndType.EndDate;
                    break;
                case "NoEnd":
                    item.EndDate = schedule.EndBoundary = DateTime.MaxValue;
                    schedule.EndType = EndType.NoEndDate;
                    break;
                case "EndAfter":
                    item.EndAfter = schedule.Occurrences = Convert.ToInt32(scheduleItem["RecurrenceFactor"].Value);
                    item.EndDate = null;
                    schedule.EndType = EndType.EndAfter;
                    break;
            }

            #endregion

            item.Recurrence = schedule;

            return item;
        }

        public int GetRecurrenceTypeId(string recurrenceType)
        {
            DataTable dt = new DataTable();
            try
            {
                var selectedColumns = new List<SelectedColumn>
                {
                    new SelectedColumn {ColumnName = GlobalAppSettings.DbColumns.DB_RecurrenceType.Id}
                };

                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_RecurrenceType.Name,
                        Condition = Conditions.Equals,
                        Value = recurrenceType
                    }
                };

                dt =
                    dataProvider.ExecuteReaderQuery(
                        queryBuilder.SelectRecordsFromTable(GlobalAppSettings.DbColumns.DB_RecurrenceType.DB_TableName,
                            selectedColumns, whereColumns)).DataTable;
            }
            catch (SqlException e)
            {

            }
            return Convert.ToInt32(dt.Rows[0].ItemArray[0]);
        }

        public dynamic GetRecurrenceType(int recurrenceTypeId)
        {
            DataTable dt = new DataTable();

            var selectedColumns = new List<SelectedColumn>
            {
                new SelectedColumn {ColumnName = GlobalAppSettings.DbColumns.DB_RecurrenceType.Name}
            };

            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_RecurrenceType.Id,
                    Condition = Conditions.Equals,
                    Value = recurrenceTypeId
                }
            };

            try
            {
                dt =
                    dataProvider.ExecuteReaderQuery(
                        queryBuilder.SelectRecordsFromTable(GlobalAppSettings.DbColumns.DB_RecurrenceType.DB_TableName,
                            selectedColumns, whereColumns)).DataTable;
            }
            catch (SqlException e)
            {

            }
            return dt.Rows[0].ItemArray[0].ToString();
        }

        /// <summary>
        /// Get the list of Schedules of a user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>List of schedules</returns>
        public List<ScheduleViewModel> GetSchedules(int userId)
        {
            var utcNow = DateTime.UtcNow;
            var scheduleList =
                new ScheduleJobProcessor(GlobalAppSettings.GetConfigFilepath() + ServerSetup.Configuration, GlobalAppSettings.GetSchedulerExportPath()).GetUserSchedule(userId)
                    .AsEnumerable()
                    .Select(schedule => new ScheduleViewModel
                    {
                        Id = schedule.Field<Guid>("ScheduleId"),
                        ItemId = schedule.Field<Guid>("ItemId"),
                        Name = schedule.Field<string>("ScheduleName"),
                        ItemName = schedule.Field<string>("ItemName"),
                        IsEnabled = schedule.Field<bool>("IsEnabled"),
                        LastRun =
                            String.IsNullOrEmpty(Convert.ToString(schedule.Field<dynamic>("LastRun")))
                                ? null
                                : schedule.Field<dynamic>("LastRun"),
                        NextSchedule =
                            String.IsNullOrEmpty(Convert.ToString(schedule.Field<dynamic>("NextSchedule")))
                                ? null
                                : schedule.Field<dynamic>("NextSchedule"),
                        Status =
                            schedule.Field<bool>("IsEnabled") == false
                                ? "Inactive"
                                : (schedule.Field<dynamic>("NextSchedule") == null ||
                                   schedule.Field<dynamic>("NextSchedule") > utcNow)
                                    ? "Active"
                                    : "Completed",
                        CanWrite = schedule.Field<bool>("CanWrite"),
                        CanDelete = schedule.Field<bool>("CanDelete")
                    }).ToList();

            var schedules =
                scheduleList.GroupBy(r => r.Id)
                    .Select(g => g.OrderByDescending(r => r.LastRun).FirstOrDefault())
                    .ToList();

            foreach (var schedule in schedules)
            {
                //Assign value for LastRun
                if (schedule.LastRun == null)
                {
                    schedule.LastRunString = (schedule.NextSchedule == null || schedule.NextSchedule < utcNow)
                        ? "Never run"
                        : (schedule.NextSchedule != null && schedule.NextSchedule > utcNow)
                            ? "Yet to run"
                            : "Never run";
                }
                else
                {
                    schedule.LastRun = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(schedule.LastRun),
                        TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone()));
                    schedule.LastRunString =
                        schedule.LastRun.ToString(GlobalAppSettings.SystemSettings.DateFormat + " hh:mm tt");
                }


                //Assign value for NextSchedule
                if (schedule.NextSchedule == null)
                {
                    schedule.Status = (schedule.LastRun != null) ? "Completed" : schedule.Status;
                    schedule.NextScheduleString = "Never run";
                }
                else
                {
                    schedule.NextSchedule = TimeZoneInfo.ConvertTimeFromUtc(Convert.ToDateTime(schedule.NextSchedule),
                        TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone()));
                    schedule.NextScheduleString =
                        schedule.NextSchedule.ToString(GlobalAppSettings.SystemSettings.DateFormat + " hh:mm tt");
                }
            }

            return schedules;
        }

        #region Edit Schedule Binding

        public ScheduleItem GetScheduledItem(Guid id)
        {
            var scheduleItem = new ScheduleItem();

            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ScheduleId,
                    Condition = Conditions.Equals,
                    Value = id
                }
            };
            try
            {
                var dataRow =
                    dataProvider.ExecuteReaderQuery(
                        queryBuilder.SelectAllRecordsFromTable(
                            GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName, whereColumns)).DataTable.Rows[0];

                scheduleItem.ItemId = new Guid(dataRow[GlobalAppSettings.DbColumns.DB_ScheduleDetail.ItemId].ToString());
                scheduleItem.Name = dataRow[GlobalAppSettings.DbColumns.DB_ScheduleDetail.Name].ToString();
                scheduleItem.ScheduleId = new Guid(dataRow[GlobalAppSettings.DbColumns.DB_ScheduleDetail.ScheduleId].ToString());
                scheduleItem.ExportTypeId =
                    Convert.ToInt32(dataRow[GlobalAppSettings.DbColumns.DB_ScheduleDetail.ExportTypeId]);
                scheduleItem.RecurrenceTypeId =
                    Convert.ToInt32(dataRow[GlobalAppSettings.DbColumns.DB_ScheduleDetail.RecurrenceTypeId]);
                scheduleItem.RecurrenceInfo =
                    dataRow[GlobalAppSettings.DbColumns.DB_ScheduleDetail.RecurrenceInfo].ToString();

                scheduleItem.StartDate =
                    TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.Parse(dataRow[GlobalAppSettings.DbColumns.DB_ScheduleDetail.StartDate].ToString()),
                        TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone()));

                scheduleItem.StartDateString = scheduleItem.StartDate.ToString("MM/dd/yyyy hh:mm tt");

                var endAfter = Convert.ToInt32(dataRow[GlobalAppSettings.DbColumns.DB_ScheduleDetail.EndAfter]);
                if (endAfter != 0)
                {
                    scheduleItem.EndType = EndType.EndAfter.ToString();
                }
                else
                {
                    var endDate = dataRow[GlobalAppSettings.DbColumns.DB_ScheduleDetail.EndDate].ToString();
                    if (!string.IsNullOrEmpty(endDate))
                    {
                        var maxValue = DateTime.MaxValue;
                        var currentDate = Convert.ToDateTime(endDate);
                        if (currentDate.Date == maxValue.Date)
                        {
                            scheduleItem.EndType = EndType.NoEndDate.ToString();
                        }
                        else
                        {
                            scheduleItem.EndType = EndType.EndDate.ToString();
                        }
                        var convertedEndDate = TimeZoneInfo.ConvertTimeFromUtc(currentDate,
                            TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone()));
                        scheduleItem.EndDate = convertedEndDate;
                        scheduleItem.EndDateString = convertedEndDate.ToString("MM/dd/yyyy hh:mm tt");
                    }
                }


                scheduleItem.EndAfter = Convert.ToInt32(dataRow[GlobalAppSettings.DbColumns.DB_ScheduleDetail.EndAfter]);
                var nextschedule = dataRow[GlobalAppSettings.DbColumns.DB_ScheduleDetail.NextSchedule].ToString();
                if (!string.IsNullOrEmpty(nextschedule))
                {
                    scheduleItem.NextSchedule =
                        TimeZoneInfo.ConvertTimeFromUtc(
                            DateTime.Parse(
                                dataRow[GlobalAppSettings.DbColumns.DB_ScheduleDetail.NextSchedule].ToString()),
                            TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone()));
                }
                scheduleItem.ModifiedById =
                    Convert.ToInt32(dataRow[GlobalAppSettings.DbColumns.DB_ScheduleDetail.ModifiedById]);
                scheduleItem.ModifiedDate =
                    DateTime.Parse(dataRow[GlobalAppSettings.DbColumns.DB_ScheduleDetail.ModifiedDate].ToString());
                scheduleItem.CreatedById =
                    Convert.ToInt32(dataRow[GlobalAppSettings.DbColumns.DB_ScheduleDetail.CreatedById]);
                scheduleItem.IsEnabled =
                    Convert.ToBoolean(dataRow[GlobalAppSettings.DbColumns.DB_ScheduleDetail.IsEnabled]);
                scheduleItem.IsActive =
                    Convert.ToBoolean(dataRow[GlobalAppSettings.DbColumns.DB_ScheduleDetail.IsActive]);
            }
            catch (SqlException e)
            {

            }
            return scheduleItem;
        }

        public List<string> GetSubscribedUserList(Guid scheduleId)
        {
            List<string> dbUserList = new List<string>();

            var selectColumns = new List<SelectedColumn>
            {
                new SelectedColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedUser.RecipientUserId,
                    TableName = GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName
                }
            };

            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedUser.ScheduleId,
                    Condition = Conditions.Equals,
                    Value = scheduleId,
                    TableName = GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName,
                    LogicalOperator = LogicalOperators.AND
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedUser.IsActive,
                    Value = true,
                    Condition = Conditions.Equals,
                    TableName = GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName,
                    LogicalOperator = LogicalOperators.AND
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.IsActive,
                    Condition = Conditions.Equals,
                    Value = true,
                    LogicalOperator = LogicalOperators.AND,
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName
                }
            };


            var joinSpecification = new List<JoinSpecification>
            {
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
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_SubscribedUser.RecipientUserId,
                                ParentTable = GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName
                            }
                        },

                    JoinType = JoinTypes.Inner
                }
            };

            try
            {
                var dataTabl = dataProvider.ExecuteReaderQuery(
                      queryBuilder.ApplyWhereClause(
                          queryBuilder.ApplyMultipleJoins(GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName,
                              selectColumns, joinSpecification), whereColumns)).DataTable.AsEnumerable().ToList();

                foreach (DataRow dr in dataTabl)
                {
                    dbUserList.Add(userManagement.GetUserName(Convert.ToInt32(dr.ItemArray[0])));
                }
            }

            catch (SqlException e)
            {

            }
            return dbUserList;
        }

        public List<int> GetSubscribedGroupList(Guid scheduleId)
        {
            List<int> dbGroupList = new List<int>();

            DataTable dt = new DataTable();

            var selectColumns = new List<SelectedColumn>
            {
                new SelectedColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.RecipientGroupId
                }
            };

            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.ScheduleId,
                    Condition = Conditions.Equals,
                    Value = scheduleId,
                    TableName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.DB_TableName
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.IsActive,
                    Condition = Conditions.Equals,
                    Value = true,
                    LogicalOperator = LogicalOperators.AND,
                    TableName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.DB_TableName
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Group.IsActive,
                    Condition = Conditions.Equals,
                    Value = true,
                    LogicalOperator = LogicalOperators.AND,
                    TableName = GlobalAppSettings.DbColumns.DB_Group.DB_TableName
                }
            };

            var joinSpecification = new List<JoinSpecification>
            {
                new JoinSpecification
                {
                    Table = GlobalAppSettings.DbColumns.DB_Group.DB_TableName,
                    Column =
                        new List<JoinColumn>
                        {
                            new JoinColumn
                            {
                                TableName = GlobalAppSettings.DbColumns.DB_Group.DB_TableName,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_Group.GroupId,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_SubscribedGroup.RecipientGroupId,
                                ParentTable = GlobalAppSettings.DbColumns.DB_SubscribedGroup.DB_TableName
                            }
                        },

                    JoinType = JoinTypes.Inner
                }
            };

            try
            {
                dt = dataProvider.ExecuteReaderQuery(queryBuilder.ApplyWhereClause(
                    queryBuilder.ApplyMultipleJoins(GlobalAppSettings.DbColumns.DB_SubscribedGroup.DB_TableName,
                        selectColumns, joinSpecification), whereColumns)).DataTable;

                foreach (DataRow dr in dt.Rows)
                {
                    dbGroupList.Add(Convert.ToInt32(dr.ItemArray[0]));
                }
            }
            catch (SqlException e)
            {

            }
            return dbGroupList;
        }

        #endregion

        #region Update Schedule

        public void UpdateScheduleItem(ScheduleItem scheduleItem)
        {
            var whereItemColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Id,
                    Condition = Conditions.Equals,
                    Value = scheduleItem.ScheduleId,
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName
                }
            };

            var updateItemColumns = new List<UpdateColumn>
            {
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name,
                    Value = scheduleItem.Name
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ModifiedById,
                    Value = HttpContext.Current.User.Identity.Name
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ModifiedDate,
                    Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                }
            };

            try
            {
                dataProvider.ExecuteNonQuery(
                    queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, updateItemColumns,
                        whereItemColumns));
            }
            catch (SqlException e)
            {

            }

            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ScheduleId,
                    Condition = Conditions.Equals,
                    Value = scheduleItem.ScheduleId,
                    TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName
                }
            };
            var updateColumns = new List<UpdateColumn>
            {
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.Name,
                    Value = scheduleItem.Name
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ExportTypeId,
                    Value = scheduleItem.ExportTypeId
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.RecurrenceTypeId,
                    Value = scheduleItem.RecurrenceTypeId
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.StartDate,
                    Value = scheduleItem.StartDate.ToString(GlobalAppSettings.GetDateTimeFormat())
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.RecurrenceInfo,
                    Value = ScheduleHelper.XMLSerializer(scheduleItem.Recurrence)
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ModifiedById,
                    Value = HttpContext.Current.User.Identity.Name
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ModifiedDate,
                    Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.IsActive,
                    Value = true
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.IsEnabled,
                    Value = scheduleItem.IsEnabled
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.EndAfter,
                    Value = scheduleItem.EndAfter
                }
            };


            if (scheduleItem.EndDate != null)
            {
                updateColumns.Add(new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.EndDate,
                    Value = scheduleItem.EndDate.Value.ToString(GlobalAppSettings.GetDateTimeFormat())
                });
            }
            else
            {
                updateColumns.Add(new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.EndDate,
                    Value = DBNull.Value
                });
            }

            var nextOccurrence = recurrenceManager.GetNextScheduleDate(scheduleItem.Recurrence.StartBoundary,
                ScheduleHelper.XMLSerializer(scheduleItem.Recurrence));

            if (nextOccurrence != null)
            {
                updateColumns.Add(new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.NextSchedule,
                    Value = nextOccurrence.Value.ToString(GlobalAppSettings.GetDateTimeFormat())
                });
            }
            else
            {
                updateColumns.Add(new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.NextSchedule,
                    Value = DBNull.Value
                });
            }

            try
            {
                dataProvider.ExecuteNonQuery(
                    queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName,
                        updateColumns, whereColumns));
            }
            catch (SqlException e)
            {

            }
        }

        public Result UpdateSubscribedUser(Guid scheduleId, dynamic usersList)
        {
            var result = new Result();

            var userList = new List<int>();

            for (int i = 0; i < usersList.Count; i++)
            {
                userList.Add(userManagement.GetUserId(usersList[i].Value));
            }

            var dbUserNameList = GetSubscribedUserList(scheduleId);
            var dbUserList = new List<int>();
            foreach (string dbuser in dbUserNameList)
            {
                dbUserList.Add(userManagement.GetUserId(dbuser));
            }

            IEnumerable<int> resultList = dbUserList.Except(userList);

            if (resultList.Count() > 0)
            {
                DeactivateUser(scheduleId, resultList);
            }

            foreach (var user in userList)
            {

                DataTable dt = new DataTable();
                var whereColumns = new List<ConditionColumn>();
                whereColumns.Add(new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedUser.ScheduleId,
                    Condition = Conditions.Equals,
                    Value = scheduleId,
                    TableName = GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName
                });
                whereColumns.Add(new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedUser.RecipientUserId,
                    Condition = Conditions.Equals,
                    Value = user,
                    LogicalOperator = LogicalOperators.AND,
                    TableName = GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName
                });

                try
                {
                    dt =
                        dataProvider.ExecuteReaderQuery(
                            queryBuilder.SelectAllRecordsFromTable(
                                GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName, whereColumns)).DataTable;
                }
                catch (SqlException e)
                {

                }

                if (dt.Rows.Count > 0)
                {
                    ActivateUser(scheduleId, user);
                }
                else
                {
                    var values = new Dictionary<string, object>();

                    values.Add(GlobalAppSettings.DbColumns.DB_SubscribedUser.ScheduleId, scheduleId);
                    values.Add(GlobalAppSettings.DbColumns.DB_SubscribedUser.SubscribedById,
                        HttpContext.Current.User.Identity.Name);
                    values.Add(GlobalAppSettings.DbColumns.DB_SubscribedUser.RecipientUserId, user);
                    values.Add(GlobalAppSettings.DbColumns.DB_SubscribedUser.IsActive, true);
                    values.Add(GlobalAppSettings.DbColumns.DB_SubscribedUser.SubscribedDate,
                        DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat()));
                    values.Add(GlobalAppSettings.DbColumns.DB_SubscribedUser.ModifiedDate,
                        DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat()));

                    try
                    {
                        dataProvider.ExecuteNonQuery(
                            queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName, values));
                    }
                    catch (SqlException e)
                    {
                        result.Exception = e;
                        result.Status = false;
                        return result;
                    }
                }
            }

            return result;
        }

        public Result UpdateSubscribedGroup(Guid scheduleId, dynamic groupsList)
        {
            var result = new Result();

            List<int> groupList = new List<int>();

            for (int i = 0; i < groupsList.Count; i++)
            {
                groupList.Add(Convert.ToInt32(groupsList[i].Value));
            }

            List<int> dbGroupList = GetSubscribedGroupList(scheduleId);

            IEnumerable<int> resultList = dbGroupList.Except(groupList);

            if (resultList.Count() > 0)
            {
                DeactivateGroup(scheduleId, resultList);
            }

            foreach (var group in groupList)
            {

                DataTable dt = new DataTable();
                var whereColumns = new List<ConditionColumn>();
                whereColumns.Add(new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.ScheduleId,
                    Condition = Conditions.Equals,
                    Value = scheduleId
                });
                whereColumns.Add(new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.RecipientGroupId,
                    Condition = Conditions.Equals,
                    Value = group,
                    LogicalOperator = LogicalOperators.AND
                });

                try
                {
                    dt =
                        dataProvider.ExecuteReaderQuery(
                            queryBuilder.SelectAllRecordsFromTable(
                                GlobalAppSettings.DbColumns.DB_SubscribedGroup.DB_TableName, whereColumns)).DataTable;
                }
                catch (SqlException e)
                {

                }

                if (dt.Rows.Count > 0)
                {
                    ActivateGroup(scheduleId, Convert.ToInt32(group));
                }
                else
                {
                    var values = new Dictionary<string, object>();

                    values.Add(GlobalAppSettings.DbColumns.DB_SubscribedGroup.ScheduleId, scheduleId);
                    values.Add(GlobalAppSettings.DbColumns.DB_SubscribedGroup.RecipientGroupId, Convert.ToInt32(group));
                    values.Add(GlobalAppSettings.DbColumns.DB_SubscribedGroup.IsActive, true);
                    values.Add(GlobalAppSettings.DbColumns.DB_SubscribedGroup.SubscribedById,
                        HttpContext.Current.User.Identity.Name);
                    values.Add(GlobalAppSettings.DbColumns.DB_SubscribedGroup.SubscribedDate, DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat()));
                    values.Add(GlobalAppSettings.DbColumns.DB_SubscribedGroup.ModifiedDate, DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat()));

                    try
                    {
                        dataProvider.ExecuteNonQuery(
                            queryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_SubscribedGroup.DB_TableName, values));
                    }
                    catch (SqlException e)
                    {
                        result.Exception = e;
                        result.Status = false;
                        return result;
                    }
                }
            }

            return result;
        }

        #endregion

        #region User & Group Activation and Deactivation

        /// <summary>
        /// Activates the user
        /// </summary>
        /// <param name="scheduleId"></param>
        /// <param name="username"></param>
        public void ActivateUser(Guid scheduleId, int userId)
        {
            var whereColumns = new List<ConditionColumn>();
            whereColumns.Add(new ConditionColumn
            {
                ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedUser.ScheduleId,
                Condition = Conditions.Equals,
                Value = scheduleId,
                TableName = GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName
            });

            whereColumns.Add(new ConditionColumn
            {
                ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedUser.RecipientUserId,
                Condition = Conditions.Equals,
                Value = userId,
                LogicalOperator = LogicalOperators.AND,
                TableName = GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName
            });

            whereColumns.Add(new ConditionColumn
            {
                ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedUser.IsActive,
                Condition = Conditions.Equals,
                Value = 0,
                LogicalOperator = LogicalOperators.AND,
                TableName = GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName
            });


            var updateColumns = new List<UpdateColumn>();
            updateColumns.Add(new UpdateColumn
            {
                ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedUser.IsActive,
                Value = 1
            });
            updateColumns.Add(new UpdateColumn
            {
                ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedUser.ModifiedDate,
                Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
            });

            try
            {
                dataProvider.ExecuteNonQuery(
                    queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName,
                        updateColumns, whereColumns));
            }
            catch (SqlException e)
            {

            }
        }

        /// <summary>
        /// Deactivate the user
        /// </summary>
        /// <param name="scheduleId"></param>
        /// <param name="resultList"></param>
        public void DeactivateUser(Guid scheduleId, IEnumerable<int> resultUserList)
        {
            foreach (int userId in resultUserList)
            {
                var whereColumns = new List<ConditionColumn>();
                whereColumns.Add(new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedUser.ScheduleId,
                    Condition = Conditions.Equals,
                    Value = scheduleId,
                    TableName = GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName
                });

                whereColumns.Add(new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedUser.RecipientUserId,
                    Condition = Conditions.Equals,
                    Value = userId,
                    LogicalOperator = LogicalOperators.AND,
                    TableName = GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName
                });

                whereColumns.Add(new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedUser.IsActive,
                    Condition = Conditions.Equals,
                    Value = 1,
                    LogicalOperator = LogicalOperators.AND,
                    TableName = GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName
                });

                var updateColumns = new List<UpdateColumn>();
                updateColumns.Add(new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedUser.IsActive,
                    Value = 0
                });
                updateColumns.Add(new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedUser.ModifiedDate,
                    Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                });

                try
                {
                    dataProvider.ExecuteNonQuery(
                        queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName,
                            updateColumns, whereColumns));
                }
                catch (SqlException e)
                {

                }
            }
        }

        /// <summary>
        /// Activate the group
        /// </summary>
        /// <param name="scheduleId"></param>
        /// <param name="groupId"></param>
        public void ActivateGroup(Guid scheduleId, int groupId)
        {
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.ScheduleId,
                    Condition = Conditions.Equals,
                    Value = scheduleId,
                    TableName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.DB_TableName
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.RecipientGroupId,
                    Condition = Conditions.Equals,
                    Value = groupId,
                    LogicalOperator = LogicalOperators.AND,
                    TableName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.DB_TableName
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.IsActive,
                    Condition = Conditions.Equals,
                    Value = 0,
                    LogicalOperator = LogicalOperators.AND,
                    TableName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.DB_TableName
                }
            };



            var updateColumns = new List<UpdateColumn>
            {
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.IsActive,
                    Value = 1
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.ModifiedDate,
                    Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                }
            };

            try
            {
                dataProvider.ExecuteNonQuery(
                    queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_SubscribedGroup.DB_TableName,
                        updateColumns, whereColumns));
            }
            catch (SqlException e)
            {

            }
        }

        /// <summary>
        /// Deactivate the group
        /// </summary>
        /// <param name="scheduleId"></param>        
        /// <param name="resultGroupList"></param>
        public void DeactivateGroup(Guid scheduleId, IEnumerable<int> resultGroupList)
        {
            foreach (int groupId in resultGroupList)
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.ScheduleId,
                        Condition = Conditions.Equals,
                        Value = scheduleId,
                        TableName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.DB_TableName
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.RecipientGroupId,
                        Condition = Conditions.Equals,
                        Value = groupId,
                        LogicalOperator = LogicalOperators.AND,
                        TableName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.DB_TableName
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.IsActive,
                        Condition = Conditions.Equals,
                        Value = 1,
                        LogicalOperator = LogicalOperators.AND,
                        TableName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.DB_TableName
                    }
                };



                var updateColumns = new List<UpdateColumn>
                {
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.IsActive,
                        Value = 0
                    },
                    new UpdateColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedGroup.ModifiedDate,
                        Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())
                    }
                };

                try
                {
                    dataProvider.ExecuteNonQuery(
                        queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_SubscribedGroup.DB_TableName,
                            updateColumns, whereColumns));
                }
                catch (SqlException e)
                {

                }
            }
        }

        #endregion

        #region Enable_Disable_Remove Schedule

        /// <summary>
        /// Enabling a schedule
        /// </summary>
        /// <param name="scheduleId"></param>
        public void EnableSchedule(Guid scheduleId)
        {
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ScheduleId,
                    Condition = Conditions.Equals,
                    Value = scheduleId,
                    TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName
                }
            };

            var updateColumns = new List<UpdateColumn>
            {
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.IsEnabled,
                    Value = true,
                    TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ModifiedDate,
                    Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat()),
                    TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ModifiedById,
                    Value = Convert.ToInt32(HttpContext.Current.User.Identity.Name),
                    TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName
                }
            };

            try
            {
                dataProvider.ExecuteNonQuery(
                    queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName,
                        updateColumns, whereColumns));
            }
            catch (SqlException e)
            {

            }
        }

        /// <summary>
        /// Disabling a schedule
        /// </summary>
        /// <param name="scheduleId"></param>
        public void DisableSchedule(Guid scheduleId)
        {
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ScheduleId,
                    Condition = Conditions.Equals,
                    Value = scheduleId,
                    TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName
                }
            };

            var updateColumns = new List<UpdateColumn>
            {
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.IsEnabled,
                    Value = 0,
                    TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ModifiedDate,
                    Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat()),
                    TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ModifiedById,
                    Value = Convert.ToInt32(HttpContext.Current.User.Identity.Name),
                    TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName
                }
            };

            try
            {
                dataProvider.ExecuteNonQuery(
                    queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName,
                        updateColumns, whereColumns));
            }
            catch (SqlException e)
            {

            }
        }

        /// <summary>
        /// Removes a schedule
        /// </summary>
        /// <param name="scheduleId"></param>
        public void RemoveSchedule(Guid scheduleId)
        {
            var itemWhereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Id,
                    Condition = Conditions.Equals,
                    Value = scheduleId,
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName
                }
            };
            var itemColumns = new List<UpdateColumn>
            {
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.IsActive,
                    Value = false,
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ModifiedDate,
                    Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat()),
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.ModifiedById,
                    Value = Convert.ToInt32(HttpContext.Current.User.Identity.Name),
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName
                }
            };

            try
            {
                dataProvider.ExecuteNonQuery(
                    queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, itemColumns,
                        itemWhereColumns));
            }
            catch (SqlException e)
            {

            }

            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ScheduleId,
                    Condition = Conditions.Equals,
                    Value = scheduleId,
                    TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName
                }
            };

            var scheduleDetailColumns = new List<UpdateColumn>
            {
                new UpdateColumn 
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.IsActive,
                    Value = false,
                    TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ModifiedDate,
                    Value = DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat()),
                    TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName
                },
                new UpdateColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ModifiedById,
                    Value = Convert.ToInt32(HttpContext.Current.User.Identity.Name),
                    TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName
                }
            };

            try
            {
                dataProvider.ExecuteNonQuery(
                    queryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName,
                        scheduleDetailColumns, whereColumns));
            }
            catch (SqlException e)
            {

            }
        }

        #endregion
    }
}