using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Grout.Base.Data;
using Grout.Base.DataClasses;

namespace Grout.Base.Reports.Scheduler
{
    public class MonthlyRecurrence
    {
        DateTime processDate;
        DateTime? nextSchedule;
        DateTime nextTentativeDate;
        DateTime utc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, 0);

        private readonly Connection connection = new Connection();
        private readonly IRelationalDataProvider dataProvider;
        private readonly IQueryBuilder queryBuilder;

        public MonthlyRecurrence()
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

        public MonthlyRecurrence(IQueryBuilder builder, IRelationalDataProvider provider)
        {
            queryBuilder = builder;
            dataProvider = provider;
        }

        public DateTime? GetNextMonthlyScheduleDate(Schedule schedule, DateTime scheduledDate)
        {
            processDate = scheduledDate;
            nextSchedule = processDate;
            var hours = processDate.Hour;
            var min = processDate.Minute;

            if (schedule.MonthlySchedule != null)
            {
                int everyXMonths = schedule.MonthlySchedule.Months;
                int xDate = schedule.MonthlySchedule.DayOfMonth;

                int daysInMonth = DateTime.DaysInMonth(processDate.Year, processDate.Month);

                if (processDate == schedule.StartBoundary)
                {

                    if ((processDate <= utc && xDate <= daysInMonth) || (processDate.Day <= xDate && xDate <= daysInMonth) ||
                        (processDate.Day > xDate && xDate <= daysInMonth))
                    {
                        nextTentativeDate = new DateTime(processDate.Year, processDate.Month, xDate, hours, min, 0);
                    }
                    else
                    {
                        nextTentativeDate = new DateTime(processDate.Year, processDate.Month, daysInMonth,hours,min,0);
                    }
                    while (nextTentativeDate <= utc)
                    {
                        nextTentativeDate = GetNextDate(nextTentativeDate, xDate, everyXMonths);
                        nextTentativeDate = new DateTime(nextTentativeDate.Year, nextTentativeDate.Month, nextTentativeDate.Day, hours, min, 0);
                    }
                }
                else
                {
                    do
                    {
                        nextTentativeDate = GetNextDate(processDate, xDate, everyXMonths);
                        nextTentativeDate = new DateTime(nextTentativeDate.Year, nextTentativeDate.Month, nextTentativeDate.Day, hours, min, 0);
                    } while (nextTentativeDate <= utc);
                }

                #region
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
                #endregion
            }
            return nextSchedule;
        }

        DateTime GetNextDate(DateTime input, int xDate, int everyXMonths)
        {
            DateTime nextDate;
            input = input.AddMonths(everyXMonths);
            int daysInMonth = DateTime.DaysInMonth(input.Year, input.Month);
            if (xDate <= daysInMonth)
                nextDate = new DateTime(input.Year, input.Month, xDate);
            else
            {
                nextDate = new DateTime(input.Year, input.Month, daysInMonth);
            }
            return nextDate;
        }

        public DateTime? GetNextMonthlyDOWScheduleDate(Schedule schedule, DateTime scheduledDate)
        {
            processDate = scheduledDate;
            nextSchedule = processDate;
            var hours = processDate.Hour;
            var min = processDate.Minute;

            if (schedule.MonthlyDowSchedule != null)
            {
                var weekOfMonth = schedule.MonthlyDowSchedule.WeekOfMonth;
                var daysOfWeek = schedule.MonthlyDowSchedule.DayOfWeek;
                var everyXMonth = schedule.MonthlyDowSchedule.Months;

                switch (daysOfWeek)
                {
                    case DayOfTheWeek.Sunday:
                    case DayOfTheWeek.Monday:
                    case DayOfTheWeek.Tuesday:
                    case DayOfTheWeek.Wednesday:
                    case DayOfTheWeek.Thursday:
                    case DayOfTheWeek.Friday:
                    case DayOfTheWeek.Saturday:
                        nextTentativeDate = GetNthSpecificDay(processDate.Year, processDate.Month, hours, min, weekOfMonth, (DayOfWeek)daysOfWeek);
                        while (nextTentativeDate <= utc || nextTentativeDate < schedule.StartBoundary)
                        {
                            processDate = processDate.AddMonths(everyXMonth);
                            nextTentativeDate = GetNthSpecificDay(processDate.Year, processDate.Month,hours, min , weekOfMonth, (DayOfWeek)daysOfWeek);
                        }
                        break;

                    case DayOfTheWeek.day:
                        nextTentativeDate = GetNthDay(processDate.Year, processDate.Month, hours, min, weekOfMonth);
                        while (nextTentativeDate <= utc || nextTentativeDate < schedule.StartBoundary)
                        {
                            processDate = processDate.AddMonths(everyXMonth);
                            nextTentativeDate = GetNthDay(processDate.Year, processDate.Month, hours, min, weekOfMonth);
                        }
                        break;

                    case DayOfTheWeek.weekday:
                        nextTentativeDate = GetNthWeekDay(processDate.Year, processDate.Month, hours, min, weekOfMonth);
                        while (nextTentativeDate <= utc || nextTentativeDate < schedule.StartBoundary)
                        {
                            processDate = processDate.AddMonths(everyXMonth);
                            nextTentativeDate = GetNthWeekDay(processDate.Year, processDate.Month, hours, min, weekOfMonth);
                        }
                        break;

                    case DayOfTheWeek.weekendday:
                        nextTentativeDate = GetNthWeekendDay(processDate.Year, processDate.Month, hours, min, weekOfMonth);
                        while (nextTentativeDate <= utc || nextTentativeDate < schedule.StartBoundary)
                        {
                            processDate = processDate.AddMonths(everyXMonth);
                            nextTentativeDate = GetNthWeekendDay(processDate.Year, processDate.Month, hours, min, weekOfMonth);
                        }
                        break;
                }

                nextTentativeDate = new DateTime(nextTentativeDate.Year, nextTentativeDate.Month, nextTentativeDate.Day, hours, min, 0);

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


        public DateTime GetNthDay(int year, int month,int hours ,int mins, WeekOfMonth weekOfMonth)
        {
            DateTime specifiedDay;
            switch (weekOfMonth)
            {
                case WeekOfMonth.first:
                    specifiedDay = new DateTime(year, month, 1, hours, mins, 0);
                    break;
                case WeekOfMonth.second:
                    specifiedDay = new DateTime(year, month, 2, hours, mins, 0);
                    break;
                case WeekOfMonth.third:
                    specifiedDay = new DateTime(year, month, 3, hours, mins, 0);
                    break;
                case WeekOfMonth.fourth:
                    specifiedDay = new DateTime(year, month, 4, hours, mins, 0);
                    break;
                case WeekOfMonth.last:
                default:
                    var lastday = DateTime.DaysInMonth(year, month);
                    specifiedDay = new DateTime(year, month, lastday, hours, mins, 0);
                    break;
            }
            return specifiedDay;
        }

        public DateTime GetNthWeekDay(int year, int month,int hours, int min, WeekOfMonth weekOfMonth)
        {

            DateTime date = new DateTime(year, month, 1, hours, min, 0);

            while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }
            switch (weekOfMonth)
            {
                case WeekOfMonth.first:
                    break;

                case WeekOfMonth.second:
                    date = date.AddDays(1);
                    while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        date = date.AddDays(1);
                    }
                    break;

                case WeekOfMonth.third:
                    date = date.AddDays(2);

                    while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        date = date.AddDays(2);
                    }
                    break;

                case WeekOfMonth.fourth:
                    switch(date.DayOfWeek)
                    {
                        case DayOfWeek.Monday:
                        case DayOfWeek.Tuesday:
                            date = date.AddDays(3);
                            break;
                        case DayOfWeek.Wednesday:
                            date = date.AddDays(5);
                            break;
                        case DayOfWeek.Thursday:
                        case DayOfWeek.Friday:
                            date = date.AddDays(5);
                            break;
                    }
                    break;

                case WeekOfMonth.last:
                default:
                    var lastday = DateTime.DaysInMonth(year, month);
                    date = new DateTime(year, month, lastday, hours, min, 0);
                    while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        date = date.AddDays(-1);
                    }
                    break;
            }
            return date;
        }

        public DateTime GetNthWeekendDay(int year, int month,int hours,int min, WeekOfMonth weekOfMonth)
        {
            DateTime date = new DateTime(year, month, 1, hours, min, 0);
            while (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }


            switch (weekOfMonth)
            {
                case WeekOfMonth.first:
                    //returns the default value
                    break;

                case WeekOfMonth.second:
                    if (date.DayOfWeek == DayOfWeek.Saturday)
                    {
                        date = date.AddDays(1);
                    }
                    else if (date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        date = date.AddDays(6);
                    }
                    break;

                case WeekOfMonth.third:
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        date = date.AddDays(7);
                    }

                    break;

                case WeekOfMonth.fourth:
                    if (date.DayOfWeek == DayOfWeek.Saturday)
                    {
                        date = date.AddDays(8);
                    }
                    else if (date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        date = date.AddDays(13);
                    }
                    break;

                case WeekOfMonth.last:
                default:
                    var lastday = DateTime.DaysInMonth(year, month);
                    date = new DateTime(year, month, lastday, hours, min, 0);
                    while (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    {
                        date = date.AddDays(-1);
                    }
                    break;
            }
            return date;
        }

        private DateTime GetNthSpecificDay(int year, int month,int hours,int min, WeekOfMonth weekOfMonth, DayOfWeek dayOfWeek)
        {
            DateTime date = new DateTime(year, month, 1, hours, min, 0);

            while (date.DayOfWeek != dayOfWeek)
            {
               date = date.AddDays(1);
            }

            switch (weekOfMonth)
            {
                case WeekOfMonth.first:
                    //returns the default value
                    break;

                case WeekOfMonth.second:
                    date = date.AddDays(7);
                    break;

                case WeekOfMonth.third:
                    date = date.AddDays(14);
                    break;

                case WeekOfMonth.fourth:
                    date = date.AddDays(21);
                    break;

                case WeekOfMonth.last:
                default:
                    var lastday = DateTime.DaysInMonth(year, month);
                    date = new DateTime(year, month, lastday, hours, min, 0);
                    while (date.DayOfWeek != dayOfWeek)
                    {
                        date = date.AddDays(-1);
                    }
                    break;
            }

            return date;
        }

    }
}
