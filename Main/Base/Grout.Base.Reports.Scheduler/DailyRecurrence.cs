using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Grout.Base.Data;
using Grout.Base.DataClasses;

namespace Grout.Base.Reports.Scheduler
{
    public class DailyRecurrence
    {
        DateTime processDate;
        DateTime nextTentativeDate;
        DateTime? nextSchedule;
        DateTime utc = new DateTime(DateTime.UtcNow.Year,DateTime.UtcNow.Month,DateTime.UtcNow.Day,DateTime.UtcNow.Hour,DateTime.UtcNow.Minute,0);

        private readonly Connection connection = new Connection();
        private readonly IRelationalDataProvider dataProvider;
        private readonly IQueryBuilder queryBuilder;

        public DailyRecurrence()
        {
            if (GlobalAppSettings.DbSupport == DataBaseType.MSSQLCE)
            {
                dataProvider = new SqlCeRelationalDataAdapter(Connection.ConnectionString);
                queryBuilder = new SqlCeQueryBuilder();
            }
            else
            {
                dataProvider = new SqlRelationalDataAdapter(Connection.ConnectionString);
                queryBuilder = new SqlQueryBuilder();
            }            
        }

        public DailyRecurrence(IQueryBuilder builder, IRelationalDataProvider provider)
        {
            queryBuilder = builder;
            dataProvider = provider;
        }


        /// <summary>
        /// Get the next Daily schedule value.
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="scheduledDate"></param>
        /// <returns>DateTime</returns>
        public DateTime? GetNextDailyScheduleDate(Schedule schedule, DateTime scheduledDate)
        {
            processDate = scheduledDate;
            nextSchedule = processDate;

            if (schedule.DailySchedule != null)
            {
                nextTentativeDate = processDate;
                while (nextTentativeDate <= utc || nextTentativeDate < schedule.StartBoundary)
                {
                    nextTentativeDate = nextTentativeDate.AddDays(schedule.DailySchedule.DaysInterval);
                }

                switch (schedule.EndType)
                {
                    case EndType.EndDate:
                        if (nextTentativeDate <= schedule.EndBoundary)
                        {
                            nextSchedule = nextTentativeDate;
                        }
                        else
                        {
                            nextSchedule = null;
                        }
                        break;

                    case EndType.NoEndDate:
                        nextSchedule = nextTentativeDate;
                        break;

                    case EndType.EndAfter:
                        DataTable dataTable = new DataTable();
                        var whereColumns = new List<ConditionColumn>();
                        whereColumns.Add(new ConditionColumn
                        {
                            ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleLog.ScheduleId,
                            Condition = Conditions.Equals,
                            Value = schedule.ScheduleId
                        });
                        whereColumns.Add(new ConditionColumn
                        {
                            ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleLog.ExecutedDate,
                            Condition = Conditions.GreaterThanOrEquals,
                            Value = schedule.StartBoundary,
                            LogicalOperator = LogicalOperators.AND
                        });
                        whereColumns.Add(new ConditionColumn
                        {
                            ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleLog.IsOnDemand,
                            Condition = Conditions.GreaterThanOrEquals,
                            Value = false,
                            LogicalOperator = LogicalOperators.AND
                        });
                        try
                        {
                            dataTable =
                                dataProvider.ExecuteReaderQuery(
                                    queryBuilder.SelectAllRecordsFromTable(
                                        GlobalAppSettings.DbColumns.DB_ScheduleLog.DB_TableName, whereColumns))
                                    .DataTable;
                            var recurrenceCount = dataTable.Rows.Count;
                            if (recurrenceCount > 0)
                            {
                                if (recurrenceCount < schedule.Occurrences)
                                {
                                    nextSchedule = nextTentativeDate;
                                }
                                else
                                {
                                    nextSchedule = null;
                                }
                            }
                            else
                            {
                                nextSchedule = nextTentativeDate;
                            }
                        }
                        catch (SqlException e)
                        {

                        }
                        break;
                }
            }
            return nextSchedule;
        }

        /// <summary>
        /// Get the next Daily Weekday schedule value.
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="scheduledDate"></param>
        /// <returns>DateTime</returns>
        public DateTime? GetNextDailyWeekDayScheduleDate(Schedule schedule, DateTime scheduledDate)
        {
            processDate = scheduledDate;
            nextSchedule = processDate;

            if (schedule.DailyWeekDaySchedule != null)
            {
                if (processDate == schedule.StartBoundary)
                {
                    if (processDate <= utc ||(processDate.DayOfWeek == DayOfWeek.Saturday || processDate.DayOfWeek == DayOfWeek.Sunday ))
                    {
                        nextTentativeDate = GetNextWeekday(processDate);
                    }
                    else
                    {
                        nextTentativeDate = processDate;
                    }
                }
                else
                {
                    nextTentativeDate = GetNextWeekday(processDate);
                }

                while (nextTentativeDate <= utc || nextTentativeDate < schedule.StartBoundary)
                {
                    nextTentativeDate = GetNextWeekday(nextTentativeDate);
                }

                switch (schedule.EndType)
                {
                    case EndType.EndDate:
                        if (nextTentativeDate <= schedule.EndBoundary)
                        {
                            nextSchedule = nextTentativeDate;
                        }
                        else
                        {
                            nextSchedule = null;
                        }
                        break;
                    case EndType.NoEndDate:
                        nextSchedule = nextTentativeDate;
                        break;
                    case EndType.EndAfter:
                        DataTable dataTable = new DataTable();
                        var whereColumns = new List<ConditionColumn>();
                        whereColumns.Add(new ConditionColumn
                        {
                            ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleLog.ScheduleId,
                            Condition = Conditions.Equals,
                            Value = schedule.ScheduleId
                        });
                        whereColumns.Add(new ConditionColumn
                        {
                            ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleLog.ExecutedDate,
                            Condition = Conditions.GreaterThanOrEquals,
                            Value = schedule.StartBoundary,
                            LogicalOperator = LogicalOperators.AND
                        });
                        whereColumns.Add(new ConditionColumn
                        {
                            ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleLog.IsOnDemand,
                            Condition = Conditions.GreaterThanOrEquals,
                            Value = false,
                            LogicalOperator = LogicalOperators.AND
                        });
                        try
                        {
                            dataTable =
                                dataProvider.ExecuteReaderQuery(
                                    queryBuilder.SelectAllRecordsFromTable(
                                        GlobalAppSettings.DbColumns.DB_ScheduleLog.DB_TableName, whereColumns))
                                    .DataTable;
                            var recurrenceCount = dataTable.Rows.Count;
                            if (recurrenceCount > 0)
                            {
                                if (recurrenceCount < schedule.Occurrences)
                                {
                                    nextSchedule = nextTentativeDate;
                                }
                                else
                                {
                                    nextSchedule = null;
                                }
                            }
                            else
                            {
                                nextSchedule = nextTentativeDate;
                            }
                        }
                        catch (SqlException e)
                        {

                        }
                        break;
                }
            }
            return nextSchedule;
        }

        /// <summary>
        /// Get the next Weekday value. This will increment the input date until it finds the next non-Saturday and non-Sunday dates.
        /// </summary>
        /// <param name="input"></param>
        /// <returns>DateTime</returns>
        public DateTime GetNextWeekday(DateTime input)
        {
            do
            {
                input = input.AddDays(1);
            } while (input.DayOfWeek == DayOfWeek.Saturday || input.DayOfWeek == DayOfWeek.Sunday);

            return input;
        }
    }
}
