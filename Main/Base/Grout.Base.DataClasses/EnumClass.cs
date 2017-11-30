using System.ComponentModel;
using System.Runtime.Serialization;

namespace Grout.Base.DataClasses
{
    /// <summary>
    ///     SQL Aggregate functions
    /// </summary>
    public enum AggregateMethods
    {
        /// <summary>
        ///     Aggregation will not be applied
        /// </summary>
        None,

        /// <summary>
        ///     Returns the number of rows
        /// </summary>
        COUNT,

        /// <summary>
        ///     Returns the Maximum value in the given column
        /// </summary>
        MAX,

        /// <summary>
        ///     Returns the Minimum value in the given column
        /// </summary>
        MIN,

        /// <summary>
        ///     Returns the Average of the given column
        /// </summary>
        AVG,

        /// <summary>
        ///     Returns the SUM of the given column
        /// </summary>
        SUM,

        /// <summary>
        ///     Returns the Standard deviation of the given column
        /// </summary>
        STDEV,

        /// <summary>
        ///     Returns the variance of all the values in the given column
        /// </summary>
        VAR
    }

    /// <summary>
    ///     SQL Conditions
    /// </summary>
    public enum Conditions
    {
        /// <summary>
        ///     No Condition will be applied
        /// </summary>
        None,

        /// <summary>
        ///     Applies Equal Operator
        /// </summary>
        Equals,

        /// <summary>
        ///     Applies Not Equal Operator
        /// </summary>
        NotEquals,

        /// <summary>
        ///     Applies Lesser than Operator
        /// </summary>
        LessThan,

        /// <summary>
        ///     Applies Greater than Operator
        /// </summary>
        GreaterThan,

        /// <summary>
        ///     Applies Lesser than or Equal Operator
        /// </summary>
        LessThanOrEquals,

        /// <summary>
        ///     Applies Greater than or Equals Operator
        /// </summary>
        GreaterThanOrEquals,

        /// <summary>
        ///     Applies for NULL values
        /// </summary>
        IS,

        /// <summary>
        ///     Applies for NULL values
        /// </summary>
        IN,

        /// <summary>
        ///     Applies for NULL values
        /// </summary>
        LIKE,

        NOTIN
    }

    public enum DatabaseLogType
    {
        [Description("Restored")]
        Restored = 1,
        [Description("backed up")]
        BackedUp
    }

    public enum ItemLogType
    {
        [Description("Added")]
        Added = 1,
        [Description("Edited")]
        Edited,
        [Description("Deleted")]
        Deleted,
        [Description("Moved")]
        Moved,
        [Description("Copied")]
        Copied,
        [Description("Cloned")]
        Cloned,
        [Description("Trashed")]
        Trashed,
        [Description("Restored")]
        Restored,
        [Description("Rollbacked")]
        Rollbacked
    }

    [DataContract]
    public enum ItemType
    {
        [EnumMember]
        [Description("Category")]
        Category = 1,
        [EnumMember]
        [Description("Dashboard")]
        Dashboard,
        [EnumMember]
        [Description("Report")]
        Report,
        [EnumMember]
        [Description("Datasource")]
        Datasource,
        [EnumMember]
        [Description("Dataset")]
        Dataset,
        [EnumMember]
        [Description("File")]
        File,
        [EnumMember]
        [Description("Schedule")]
        Schedule
    }

    /// <summary>
    ///     SQL Table Join Types
    /// </summary>
    public enum JoinTypes
    {
        /// <summary>
        ///     Inner Joins
        /// </summary>
        Inner,

        /// <summary>
        ///     Full Outer Join
        /// </summary>
        FullOuter,

        /// <summary>
        ///     Left Outer Join
        /// </summary>
        Left,

        /// <summary>
        ///     Right Outer Join
        /// </summary>
        RightOuter,

        /// <summary>
        ///     Cross Join
        /// </summary>
        Cross
    }

    /// <summary>
    ///     SQL Logical Operators
    /// </summary>
    public enum LogicalOperators
    {
        /// <summary>
        ///     No Condition Will be applied
        /// </summary>
        None,

        /// <summary>
        ///     Applies Logical OR operation
        /// </summary>
        OR,

        /// <summary>
        ///     Applies Logical AND operation
        /// </summary>
        AND,

        /// <summary>
        ///     Applies Logical IN operation
        /// </summary>
        IN,

        /// <summary>
        ///     Applies Logical LIKE operation
        /// </summary>
        LIKE,

        /// <summary>
        ///     Applies Logical NOT operation
        /// </summary>
        NOT
    }

    public enum SystemLogType
    {
        [Description("Updated")]
        Updated = 1
    }

    public enum SystemSettingKeys
    {
        OrganizationName,
        LoginLogo,
        MainScreenLogo,
        FavIcon,
        WelcomeNoteText,
        Language,
        TimeZone,
        DateFormat,
        BaseUrl,
        ActivationExpirationDays,
        MailSettingsAddress,
        MailSettingsPassword,
        MailSettingsHost,
        MailSettingsSenderName,
        MailSettingsPort,
        MailSettingsIsSecureAuthentication        
    }

    public enum UploadImageTypes
    {
        LoginLogo = 1,
        MainScreenLogo = 2,
        Favicon = 3,
        ProfilePicture = 4
    }

    public enum UserLogType
    {
        [Description("Added")]
        Added = 1,
        [Description("Updated")]
        Updated,
        [Description("Deleted")]
        Deleted,
        [Description("Changed")]
        Changed
    }

    public enum ExportType
    {
        [Description("Excel")]
        Excel = 1,
        [Description("Html")]
        Html,
        [Description("PDF")]
        Pdf,
        [Description("Word")]
        Word
    }

    public enum ScheduleStatus
    {
        [Description("Schedule Success")]
        Success = 1,
        [Description("Schedule Fail")]
        Fail
    }

    /// <summary>Defines the type of schedule that can be used by tasks.</summary>
    public enum TaskScheduleType
    {
        /// <summary>Schedule's the task on a daily schedule.</summary>
        Daily = 1,
        /// <summary>Schedule's the task on a daily schedule.</summary>
        DailyWeekDay,
        /// <summary>Schedule's the task on a weekly schedule.</summary>
        Weekly,
        /// <summary>Schedule's the task on a monthly schedule.</summary>
        Monthly,
        /// <summary>Schedule's the task on a monthly day-of-week schedule.</summary>
        MonthlyDOW,
        /// <summary>Schedule's the task on a yearly schedule.</summary>
        Yearly,
        /// <summary>Schedule's the task on a yearly day-of-week schedule.</summary>
        YearlyDOW,
        /// <summary>Schedule's the task at a specific time of day.</summary>
        Time
    }

    public enum RecurrenceType
    {
        /// <summary>Schedule's the task on a daily schedule.</summary>
        Daily,
        /// <summary>Schedule's the task on a weekly schedule.</summary>
        Weekly,
        /// <summary>Schedule's the task on a monthly schedule.</summary>
        Monthly,
        /// <summary>Schedule's the task on a yearly schedule.</summary>
        Yearly,
    }

    ///<summary>Defines the end type of schedule</summary>

    public enum EndType
    {
        ///<summary>Specifies the schedule with an end date</summary>
        EndDate,
        ///<summary>Specifies the schedule with no end date</summary>
        NoEndDate,
        ///<summary>Specifies the schedule to end after n occurrences</summary>
        EndAfter,
    }

    /// <summary>Values for days of the week (Monday, Tuesday, etc.)</summary>


    public enum DayOfTheWeek
    {
        /// <summary>Sunday</summary>
        Sunday = 0,
        /// <summary>Monday</summary>
        Monday,
        /// <summary>Tuesday</summary>
        Tuesday,
        /// <summary>Wednesday</summary>
        Wednesday,
        /// <summary>Thursday</summary>
        Thursday,
        /// <summary>Friday</summary>
        Friday,
        /// <summary>Saturday</summary>
        Saturday,
        ///<summary> Days</summary>
        day,
        /// <summary>Week days</summary>
        weekday,
        ///<summary> Week End days</summary>
        weekendday
    }

    /// <summary>Values for months of the year (January, February, etc.)</summary>


    public enum MonthOfTheYear
    {
        /// <summary>January</summary>
        January = 1,
        /// <summary>February</summary>
        February,
        /// <summary>March</summary>
        March,
        /// <summary>April</summary>
        April,
        /// <summary>May</summary>
        May,
        /// <summary>June</summary>
        June,
        /// <summary>July</summary>
        July,
        /// <summary>August</summary>
        August,
        /// <summary>September</summary>
        September,
        /// <summary>October</summary>
        October,
        /// <summary>November</summary>
        November,
        /// <summary>December</summary>
        December
    }

    /// <summary>Values for week of month (first, second, ..., last)</summary>

    public enum WeekOfMonth
    {
        /// <summary>First week of the month</summary>
        first = 1,
        /// <summary>Second week of the month</summary>
        second,
        /// <summary>Third week of the month</summary>
        third,
        /// <summary>Fourth week of the month</summary>
        fourth,
        /// <summary>Last week of the month</summary>
        last
    }

    public enum Status
    {
        [Description("Success")]
        Success,
        [Description("Failure")]
        Failure
    }

    public enum ExceptionEnum
    {
        [Description("Group category exist")]
        GroupCategoryExist,
        [Description("No Exception")]
        NoException,
        [Description("Sql exception")]
        SqlException,
        [Description("UserExists")]
        UserExists,
        [Description("UserNotFound")]
        UserNotFound,
        [Description("GroupNotFound")]
        GroupNotFound
    }


    public enum DataBaseType
    {
        [Description("MSSQL")]
        MSSQL,
        [Description("MySQL")]
        MySQL,
        [Description("MSSQLCE")]
        MSSQLCE
    }

    public enum AuthenticationType
    {
        [Description("Windows")]
        Windows,
        [Description("SQL Server")]
        SqlServer
    }

    public enum UserStatus
    {
        [Description("Active")]
        Active,
        [Description("InActive")]
        InActive
    }

    public enum PermissionAccess
    {
        [Description("Read")]
        Read = 2,
        [Description("Read, Write")]
        ReadWrite=6,
        [Description("Read, Write, Delete")]
        ReadWriteDelete = 14,
        [Description("Create")]
        Create=1
    }

    public enum PermissionEntity
    {
        [Description("All Reports")]
        AllReports = 1,
        [Description("Reports in Category")]
        ReportsInCategory,
        [Description("Specific Report")]
        SpecificReports,
        [Description("All Categories")]
        AllCategories,
        [Description("Specific Category")]
        SpecificCategory,
        [Description("All Data Sources")]
        AllDataSources,
        [Description("Specific Data Source")]
        SpecificDataSource,
        [Description("All Files")]
        AllFiles,
        [Description("Specific Files")]
        SpecificFiles,
        [Description("All Schedules")]
        AllSchedules,
        [Description("Specific Schedule")]
        SpecificSchedule,
        [Description("All Dashboards")]
        AllDashboards,
        [Description("Dashboards in Category")]
        DashboardsInCategory,
        [Description("Specific Dashboard")]
        SpecificDashboard
    }
    public enum EntityType
    {
        [Description("Specific Type Entity")]
        SpecificType = 0,
        [Description ("All Type Entity")]
        AllType=1,
        [Description("In Type Entity")]
        InType = 2
    }

    public enum LoginResponse
    {
        ValidUser,
        InvalidUserName,
        InvalidPassword,
        ThrottledUser,
        DeactivatedUser,
        DeletedUser
    }

}
