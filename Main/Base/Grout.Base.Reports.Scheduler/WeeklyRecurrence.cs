using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Grout.Base.Data;
using Grout.Base.DataClasses;


namespace Grout.Base.Reports.Scheduler
{
    public class WeeklyRecurrence
    {

        private readonly Connection connection = new Connection();
        private readonly IRelationalDataProvider dataProvider;
        private readonly IQueryBuilder queryBuilder;

        public DateTime utc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0);

        public WeeklyRecurrence()
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

        public WeeklyRecurrence(IQueryBuilder builder, IRelationalDataProvider provider)
        {
            queryBuilder = builder;
            dataProvider = provider;
        }

        public DateTime? GetNextWeeklyScheduleDate(Schedule schedule, DateTime scheduledDate)
        {
            DateTime processDate = scheduledDate;
            DateTime? nextSchedule = processDate;
            
            if (schedule.WeeklySchedule != null)
            {
                DateTime nextTentativeDate = processDate.AddDays(-1);
                nextTentativeDate = GetNextDay(nextTentativeDate, schedule);
                
                while (nextTentativeDate <= utc)
                {
                    nextTentativeDate = GetNextDay(nextTentativeDate, schedule);
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

        DateTime GetNextDay(DateTime input, Schedule schedule)
        {
            DateTime? returnDate = null;
            do
            {
                input = input.DayOfWeek == DayOfWeek.Saturday ? input.AddDays(((schedule.WeeklySchedule.WeeksInterval - 1) * 7) + 1) : input.AddDays(1);
                switch (input.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        if (schedule.WeeklySchedule.DaysOfWeek.Sunday)
                            returnDate = input;
                        break;
                    case DayOfWeek.Monday:
                        if (schedule.WeeklySchedule.DaysOfWeek.Monday)
                            returnDate = input;
                        break;
                    case DayOfWeek.Tuesday:
                        if (schedule.WeeklySchedule.DaysOfWeek.Tuesday)
                            returnDate = input;
                        break;
                    case DayOfWeek.Wednesday:
                        if (schedule.WeeklySchedule.DaysOfWeek.Wednesday)
                            returnDate = input;
                        break;
                    case DayOfWeek.Thursday:
                        if (schedule.WeeklySchedule.DaysOfWeek.Thursday)
                            returnDate = input;
                        break;
                    case DayOfWeek.Friday:
                        if (schedule.WeeklySchedule.DaysOfWeek.Friday)
                            returnDate = input;
                        break;
                    case DayOfWeek.Saturday:
                        if (schedule.WeeklySchedule.DaysOfWeek.Saturday)
                            returnDate = input;
                        break;
                }
            } while (!returnDate.HasValue);
            return returnDate.Value;
        }
    }
}
