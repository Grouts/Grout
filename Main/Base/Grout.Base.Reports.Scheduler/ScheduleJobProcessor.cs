using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;
using Grout.Base.Data;
using Grout.Base.DataClasses;
using Grout.Base.Encryption;
using Grout.Base.Item;
using Grout.Base.Logger;

namespace Grout.Base.Reports.Scheduler
{
    public class ScheduleJobProcessor
    {
        private static IQueryBuilder QueryBuilder { get; set; }
        private static IRelationalDataProvider DataProvider { get; set; }
        public string ExportDirectory { get; set; }
        public string ConfigFolderPath { get; set; }

        public ScheduleJobProcessor(string configPath, string exportDirectoryPath)
        {
            LogExtension.LogInfo("Initializing system settings", MethodBase.GetCurrentMethod(), " ConfigPath - " + configPath + " ExportDirectoryPath - " + exportDirectoryPath);
            GlobalAppSettings.InitializeDataBaseColumns(AppDomain.CurrentDomain.BaseDirectory +
                                                        ConfigurationManager.AppSettings["SystemConfigurationPath"] +
                                                        ServerSetup.DbSchema);
            GlobalAppSettings.InitializeSystemSettings(GlobalAppSettings.GetConfigFilepath() + ServerSetup.Configuration);

            Connection.ConnectionString = GlobalAppSettings.SystemSettings.SqlConfiguration.ConnectionString;
            if (GlobalAppSettings.DbColumns == null)
            {
                LogExtension.LogError("GlobalAppSettings.DbColumns is null", new Exception("Value can not be null"), MethodBase.GetCurrentMethod(), " ConfigPath - " + configPath + " ExportDirectoryPath - " + exportDirectoryPath);
            }
            if (GlobalAppSettings.SystemSettings == null)
            {
                LogExtension.LogError("GlobalAppSettings.SystemSettings is null", new Exception("Value can not be null"), MethodBase.GetCurrentMethod(), " ConfigPath - " + configPath + " ExportDirectoryPath - " + exportDirectoryPath);
            }
            ConfigFolderPath = configPath;
            ExportDirectory = exportDirectoryPath;

            if (GlobalAppSettings.SystemSettings.SqlConfiguration.ServerType == DataBaseType.MSSQLCE)
            {
                QueryBuilder = new SqlCeQueryBuilder();
                DataProvider =
                    new SqlCeRelationalDataAdapter(GlobalAppSettings.SystemSettings.SqlConfiguration.ConnectionString);
            }
            else
            {
                QueryBuilder = new SqlQueryBuilder();
                DataProvider =
                    new SqlRelationalDataAdapter(GlobalAppSettings.SystemSettings.SqlConfiguration.ConnectionString);
            }
        }

        #region Jobs

        public void RescheduleUnProcessedJobs(List<FailedJob> jobs)
        {
            foreach (var job in jobs)
            {
                var nextOccurenceDate = new ScheduleRecurrenceManager().GetNextScheduleDate(job.CurrentScheduleTime,
                    job.RecurrenceInfo);
                UpdateNextScheduleDate(nextOccurenceDate, job.ScheduleId);
            }
        }

        public void ProcessSchedule(Guid scheduleId)
        {
            LogExtension.LogInfo("Schedule job started", MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId);
            var scheduleContent = GetScheduleContent(scheduleId);
            scheduleContent.ExportFormat = (scheduleContent.ExportTypeId == ExportType.Pdf)
                ? "pdf"
                : (scheduleContent.ExportTypeId == ExportType.Excel)
                    ? "xls"
                    : (scheduleContent.ExportTypeId == ExportType.Word) ? "doc" : "html";
            var writeReport = ScheduleReportWriter.GenerateReport(ExportDirectory, scheduleContent, scheduleContent.UserName, scheduleContent.Password);

            var nextOccurenceDate =
                new ScheduleRecurrenceManager().GetNextScheduleDate(scheduleContent.CurrentScheduleTime,
                    scheduleContent.RecurrenceInfo);

            UpdateNextScheduleDate(nextOccurenceDate, scheduleContent.ScheduleId);

            if (writeReport)
            {
                LogExtension.LogInfo("Sending mail for receipient users started", MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId + " ScheduleExportFormat -" + scheduleContent.ExportFormat + " ExportTypeId - " + scheduleContent.ExportTypeId + " NextOccurenceDate - " + nextOccurenceDate);
                var groupList = GetGroupRecipientsOfSchedule(scheduleContent.ScheduleId);
                groupList.AddRange(GetUserRecipientsOfSchedule(scheduleContent.ScheduleId));
                var recipients = groupList.Distinct(new ScheduleRecipientComparer()).ToList();
                var subject = "Grout : Your scheduled report";
                var tasks = new List<Task>();

                UpdateScheduleJobStatus(scheduleContent.ScheduleId, ScheduleStatus.Success, false);

                UpdateMailSettings();
                try
                {
                    foreach (var recipient in recipients)
                    {
                        var body =
                               "<html xmlns='http://www.w3.org/1999/xhtml' xmlns='http://www.w3.org/1999/xhtml' xmlns:v='urn:schemas-microsoft-com:vml' xmlns:o='urn:schemas-microsoft-com:office:office'><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8'><meta name='viewport' content='width=device-width'>	<!--[if gte mso 9]><xml> <o:OfficeDocumentSettings>  <o:AllowPNG/>  <o:PixelsPerInch>96</o:PixelsPerInch> </o:OfficeDocumentSettings></xml><![endif]--></head><body style='min-width: 100%;-webkit-text-size-adjust: 100%;-ms-text-size-adjust: 100%;margin: 0;padding: 0;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;text-align: left;line-height: 19px;font-size: 14px;background-color: rgb(244,244,244);padding-top: 10px;width: 100% !important;'><center style='width: 100%;min-width: 580px;'><table style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='email-container' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='email-header' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;background-color: #5c6bc0;padding-right: 10px;padding-left: 10px;padding-top: 10px;padding-bottom: 18px;border-top-right-radius: 5px;border-top-left-radius: 5px;border-collapse: collapse !important;'><table class='body container' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;height: 100%;width: 580px;margin: 0;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;line-height: 19px;font-size: 14px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='sync-inc wrapper' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 2px 10px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><h6 style='color: white;font-family: &quot;Segoe UI&quot;,sans-serif !important;font-weight: 500;padding: 0;margin: 0;text-align: left;line-height: 1.3;word-break: normal;font-size: 20px;font-style: normal;'><a href='"
                              + GlobalAppSettings.SystemSettings.BaseUrl + "/accounts/login' style='color: white;text-decoration: none;'>"
                              + GlobalAppSettings.SystemSettings.OrganizationName + "</a></h6></td><td class='user-login wrapper last' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 11px 10px 0px;vertical-align: top;text-align: left;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse;'><p style='margin: 0;margin-bottom: 10px;color: white;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: 400;text-align: right;line-height: 1.3;font-size: 11px; font-style: normal;'><a href='"
                              + GlobalAppSettings.SystemSettings.BaseUrl + "/accounts/login' style='color: white;text-decoration: none;'>LOG IN</a></p></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='wrapper last' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 50px 0px 10px 273px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><img src='"
                              + GlobalAppSettings.SystemSettings.BaseUrl + "/Content/Images/Application/"
                              + GlobalAppSettings.SystemSettings.MainScreenLogo + "' style='outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;width: auto;max-width: 100%;float: none;clear: both;margin: 0 auto;'></td></tr><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='welcome wrapper last' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 18px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><h6 style='color: white;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: lighter;padding: 0;margin: 0;text-align: center;line-height: 1.3;word-break: normal;font-size: 20px;'>Scheduled Report</h6></td></tr></tbody></table></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='message-body' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;background-color: white;padding-right: 10px;padding-left: 10px;border-bottom-right-radius: 5px;border-bottom-left-radius: 5px;border-collapse: collapse !important;'><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='activation-msg' style='padding-top: 20px;padding-left: 4px; padding-bottom: 12px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto; vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;'><tbody><tr><td style='padding: 0px 0px 10px;'><p style='margin-bottom: 4px;margin: 0;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;'>Hello "
                              + recipient.FullName + ",</p></td></tr></tbody></table><p style='margin-bottom: 2px;margin: 0;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;text-align: left;line-height: 19px;font-size: 12px;'>Please find attached, the report that you had requested.</p></td></tr></tbody></table><table style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td style='padding-left: 4px;padding-bottom: 0px; padding-top: 0px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><table class='block-grid five-up' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;width: 100%;max-width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='activate-btn center' style='padding: 0px 0px 10px;'><p style='margin-bottom: 2px; margin: 0;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;text-align: left;line-height: 19px;font-size: 12px;'>Schedule—"
                              + scheduleContent.ScheduleName + " has exported the report "
                              + scheduleContent.ReportName + ".</p></td></tr></tbody></table></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='signature' style='padding-left: 4px;padding-bottom: 0px; padding-top: 0px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><p style='margin-bottom: 6px;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;margin-top: 0px;'>Regards,</p><p style='margin: 0;margin-bottom: 10px;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;'>"
                              + GlobalAppSettings.SystemSettings.OrganizationName + "</p></td></tr></table></td></tr></tbody></table></td></tr></tbody></table></center></body></html>";
                        var mailMessage = new MailMessage();
                        mailMessage.To.Add(new MailAddress(recipient.Email));
                        mailMessage.From = new MailAddress(GlobalAppSettings.SystemSettings.MailSettingsAddress,
                            GlobalAppSettings.SystemSettings.MailSettingsSenderName);
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true;
                        mailMessage.Attachments.Add(
                            new Attachment(ExportDirectory + scheduleContent.ScheduleId + "\\" +
                                           scheduleContent.ReportName +
                                           "." + scheduleContent.ExportFormat));
                        tasks.Add(Task.Factory.StartNew(() =>
                        {
                            var isEmailSentSuccessfully = true;
                            try
                            {
                                new MailSender().SendEmail(mailMessage);
                            }
                            catch (Exception ex)
                            {
                                LogExtension.LogError("Exception while sending mail", ex, MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId + " ScheduleUserName - " + scheduleContent.UserName + " SchedulePassword - " + scheduleContent.Password + " ScheduleName - " + scheduleContent.ScheduleName + " ScheduleReportName - " + scheduleContent.ReportName + " ScheduleExportFormat -" + scheduleContent.ExportFormat + " ExportTypeId - " + scheduleContent.ExportTypeId + " FromAddress - " + GlobalAppSettings.SystemSettings.MailSettingsAddress + " ToAddress - " + recipient.Email + " NextOccurenceDate - " + nextOccurenceDate);
                                isEmailSentSuccessfully = false;
                            }

                            #region Scheduler User and Group Log

                            var userLogs =
                                groupList.Where(p => p.UserId == recipient.UserId && p.GroupId == null).ToList();
                            if (userLogs.Count > 0)
                            {
                                UpdateScheduleStatusInScheduleUserLog(scheduleContent.ScheduleId, recipient.UserId,
                                    isEmailSentSuccessfully
                                        ? ScheduleStatus.Success
                                        : ScheduleStatus.Fail, false);
                            }
                            var groupLogs =
                                groupList.Where(p => p.GroupId != null && p.UserId == recipient.UserId).ToList();
                            if (groupLogs.Count > 0)
                            {
                                foreach (var groupLog in groupLogs)
                                {
                                    UpdateScheduleStatusInScheduleGroupLog(scheduleContent.ScheduleId, recipient.UserId,
                                        Convert.ToInt32(groupLog.GroupId), isEmailSentSuccessfully
                                            ? ScheduleStatus.Success
                                            : ScheduleStatus.Fail, false);
                                }
                            }

                            #endregion
                        }));
                    }
                    Task.WaitAll(tasks.ToArray());
                }
                catch (Exception e)
                {
                    LogExtension.LogError("Exception is thrown while sending mail to recepients", e, MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId + " ScheduleUserName - " + scheduleContent.UserName + " SchedulePassword - " + scheduleContent.Password + " ScheduleName - " + scheduleContent.ScheduleName + " ScheduleReportName - " + scheduleContent.ReportName + " ScheduleExportFormat -" + scheduleContent.ExportFormat + " ExportTypeId - " + scheduleContent.ExportTypeId + " NextOccurenceDate - " + nextOccurenceDate);
                }
                LogExtension.LogInfo("Sending mail for receipient users successfull", MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId + " ScheduleUserName - " + scheduleContent.UserName + " SchedulePassword - " + scheduleContent.Password + " ScheduleName - " + scheduleContent.ScheduleName + " ScheduleReportName - " + scheduleContent.ReportName + " ScheduleExportFormat -" + scheduleContent.ExportFormat + " ExportTypeId - " + scheduleContent.ExportTypeId + " NextOccurenceDate - " + nextOccurenceDate);
            }
            else
            {
                UpdateScheduleJobStatus(scheduleContent.ScheduleId, ScheduleStatus.Fail, false);
            }
            LogExtension.LogInfo("Schedule job is successful", MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId + " ScheduleUserName - " + scheduleContent.UserName + " SchedulePassword - " + scheduleContent.Password + " ScheduleName - " + scheduleContent.ScheduleName + " ScheduleReportName - " + scheduleContent.ReportName + " ScheduleExportFormat -" + scheduleContent.ExportFormat + " ExportTypeId - " + scheduleContent.ExportTypeId + " NextOccurenceDate - " + nextOccurenceDate);
        }

        public void UpdateMailSettings()
        {
            LogExtension.LogInfo("Updating mail settings started", MethodBase.GetCurrentMethod(), " ConfigurationFolderPath - " + ConfigFolderPath);
            try
            {
                GlobalAppSettings.InitializeSystemSettings(ConfigFolderPath + "config.xml");
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception is thrown while updating mail settings", e, MethodBase.GetCurrentMethod(), " ConfigurationFolderPath - " + ConfigFolderPath + "config.xml");
                return;
            }
            LogExtension.LogInfo("Updating mail settings successful", MethodBase.GetCurrentMethod(), " ConfigurationFolderPath - " + ConfigFolderPath);
        }

        public void UpdateTimeOutStatus(Guid scheduleId)
        {
            var scheduleContent = GetScheduleContent(scheduleId);
            var nextOccurenceDate =
                new ScheduleRecurrenceManager().GetNextScheduleDate(scheduleContent.CurrentScheduleTime,
                    scheduleContent.RecurrenceInfo);
            UpdateNextScheduleDate(nextOccurenceDate, scheduleContent.ScheduleId);
            UpdateScheduleJobStatus(scheduleContent.ScheduleId, ScheduleStatus.Fail, false);
        }

        #endregion

        #region Database Access

        public List<ScheduleRecipientDetail> GetGroupRecipientsOfSchedule(Guid scheduleId)
        {
            LogExtension.LogInfo("Reading group recepients of schedule", MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId);
            var groupList = new List<ScheduleRecipientDetail>();
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
            var joinSpecification = new JoinSpecification
            {
                Table = GlobalAppSettings.DbColumns.DB_Group.DB_TableName,
                Column =
                    new List<JoinColumn>
                    {
                        new JoinColumn
                        {
                            TableName = GlobalAppSettings.DbColumns.DB_Group.DB_TableName,
                            JoinedColumn = GlobalAppSettings.DbColumns.DB_Group.Id,
                            Operation = Conditions.Equals,
                            ParentTableColumn = GlobalAppSettings.DbColumns.DB_SubscribedGroup.RecipientGroupId,
                            ParentTable = GlobalAppSettings.DbColumns.DB_SubscribedGroup.DB_TableName

                        }
                    },
                JoinType = JoinTypes.Inner
            };
            try
            {
                var dataTable =
                    DataProvider.ExecuteReaderQuery(
                        QueryBuilder.ApplyWhereClause(
                            QueryBuilder.ApplyJoins(GlobalAppSettings.DbColumns.DB_SubscribedGroup.DB_TableName,
                                selectColumns, joinSpecification), whereColumns)).DataTable;

                foreach (DataRow dataRow in dataTable.Rows)
                {
                    groupList.AddRange(GetUsersOfGroup(Convert.ToInt32(dataRow.ItemArray[0])));
                }
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception while reading group recepients of schedule", e, MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId);
                return groupList;
            }
            LogExtension.LogInfo("Reading group recepients of schedule is successful", MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId);
            return groupList;
        }

        public List<ScheduleRecipientDetail> GetUserRecipientsOfSchedule(Guid scheduleId)
        {
            LogExtension.LogInfo("Reading user recepients of schedule started", MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId);
            var userList = new List<ScheduleRecipientDetail>();
            var selectColumns = new List<SelectedColumn>
            {
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedUser.RecipientUserId
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.Email
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.FirstName
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.LastName
                }
            };
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedUser.ScheduleId,
                    Condition = Conditions.Equals,
                    Value = scheduleId
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_SubscribedUser.IsActive,
                    Condition = Conditions.Equals,
                    Value = true,
                    LogicalOperator = LogicalOperators.AND,
                    TableName = GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName
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
            var joinSpecification = new JoinSpecification
            {
                Table = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                Column =
                    new List<JoinColumn>
                    {
                        new JoinColumn
                        {
                            TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                            JoinedColumn = GlobalAppSettings.DbColumns.DB_User.Id,
                            Operation = Conditions.Equals,
                            ParentTableColumn = GlobalAppSettings.DbColumns.DB_SubscribedUser.RecipientUserId,
                            ParentTable = GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName

                        }
                    },
                JoinType = JoinTypes.Inner
            };

            try
            {
                var dataTable =
                    DataProvider.ExecuteReaderQuery(
                        QueryBuilder.ApplyWhereClause(
                            QueryBuilder.ApplyJoins(GlobalAppSettings.DbColumns.DB_SubscribedUser.DB_TableName,
                                selectColumns, joinSpecification), whereColumns)).DataTable;
                userList = dataTable.AsEnumerable().Select(r =>
                    new ScheduleRecipientDetail
                    {
                        UserId = r.Field<int>(GlobalAppSettings.DbColumns.DB_SubscribedUser.RecipientUserId),
                        Email = r.Field<string>(GlobalAppSettings.DbColumns.DB_User.Email),
                        FullName = r.Field<string>(GlobalAppSettings.DbColumns.DB_User.FirstName) + " " + r.Field<string>(GlobalAppSettings.DbColumns.DB_User.LastName)
                    }).ToList();
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception while reading user recepients", e, MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId);
                return userList;
            }
            LogExtension.LogInfo("Reading user recepients of schedule successful", MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId);
            return userList;
        }

        public List<ScheduleRecipientDetail> GetUsersOfGroup(int groupId)
        {
            var groupTable = GetAllUserByGroupId(groupId);
            var groupUsers = groupTable.AsEnumerable().Select(r =>
                new ScheduleRecipientDetail
                {
                    UserId = r.Field<int>(GlobalAppSettings.DbColumns.DB_UserGroup.UserId),
                    GroupId = groupId,
                    Email = r.Field<string>(GlobalAppSettings.DbColumns.DB_User.Email),
                    FullName = r.Field<string>(GlobalAppSettings.DbColumns.DB_User.DisplayName)
                }).ToList();
            return groupUsers;
        }

        public ScheduleContent GetScheduleContent(Guid scheduleId)
        {
            LogExtension.LogInfo("Fetching schedule content", MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId);
            var scheduleContent = new ScheduleContent();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ScheduleId,
                    Condition = Conditions.Equals,
                    Value = scheduleId
                }
            };

            var selected = new List<SelectedColumn>
            {
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName,
                    ColumnName = "*"
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.UserName
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.Password
                }
            };
            var joinSpecification =
                new JoinSpecification
                {
                    Table = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    Column =
                        new List<JoinColumn>
                        {
                            new JoinColumn
                            {
                                TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                                JoinedColumn = GlobalAppSettings.DbColumns.DB_User.Id,
                                Operation = Conditions.Equals,
                                ParentTableColumn = GlobalAppSettings.DbColumns.DB_ScheduleDetail.CreatedById,
                            }
                        },
                    JoinType = JoinTypes.Inner
                };
            try
            {
                var cryptoGraphy = new Cryptography();
                var dataTable =
                    DataProvider.ExecuteReaderQuery(
                        QueryBuilder.ApplyWhereClause(
                            QueryBuilder.ApplyJoins(GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName, selected,
                                joinSpecification), whereColumns)).DataTable;
                scheduleContent = dataTable.AsEnumerable().Select(row => new ScheduleContent
                {
                    ItemId = row.Field<Guid>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.ItemId),
                    ScheduleId = row.Field<Guid>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.ScheduleId),
                    ScheduleName = row.Field<string>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.Name),
                    ReportName = GetItemName(row.Field<Guid>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.ItemId)),
                    ExportTypeId =
                        (ExportType)
                            Enum.GetValues(typeof(ExportType))
                                .GetValue(
                                    Convert.ToInt32(
                                        row.Field<int>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.ExportTypeId)) - 1),
                    CurrentScheduleTime =
                        row.Field<DateTime>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.NextSchedule),
                    RecurrenceInfo = row.Field<string>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.RecurrenceInfo),
                    UserName = row.Field<string>(GlobalAppSettings.DbColumns.DB_User.UserName),
                    Password = cryptoGraphy.Decryption(row.Field<string>(GlobalAppSettings.DbColumns.DB_User.Password))
                }).First();

                var currentVersionDetail = new ItemManagement().FindItemCurrentVersion(scheduleContent.ItemId);

                scheduleContent.FileLocation = GlobalAppSettings.GetItemsPath() + scheduleContent.ItemId + "\\" +
                                               currentVersionDetail.AsEnumerable()
                                                   .Select(
                                                       a =>
                                                           a.Field<int>(
                                                               GlobalAppSettings.DbColumns.DB_ItemVersion.VersionNumber))
                                                   .FirstOrDefault() + "\\" +
                                               currentVersionDetail.AsEnumerable()
                                                   .Select(
                                                       a =>
                                                           a.Field<string>(
                                                               GlobalAppSettings.DbColumns.DB_ItemVersion.ItemName))
                                                   .FirstOrDefault();
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception while fetching schedule content", e, MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId + " ScheduleUserName - " + scheduleContent.UserName + " SchedulePassword - " + scheduleContent.Password + " ScheduleName - " + scheduleContent.ScheduleName + " ScheduleReportName - " + scheduleContent.ReportName + " ScheduleExportFormat -" + scheduleContent.ExportFormat + " ExportTypeId - " + scheduleContent.ExportTypeId + " ItemId - " + scheduleContent.ItemId + " CurrentScheduleTime - " + scheduleContent.CurrentScheduleTime + " RecurrenceInfo - " + scheduleContent.RecurrenceInfo);
                return scheduleContent;
            }
            LogExtension.LogInfo("Fetching schedule contnet scuccessful", MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId + " ScheduleUserName - " + scheduleContent.UserName + " SchedulePassword - " + scheduleContent.Password + " ScheduleName - " + scheduleContent.ScheduleName + " ScheduleReportName - " + scheduleContent.ReportName + " ScheduleExportFormat -" + scheduleContent.ExportFormat + " ExportTypeId - " + scheduleContent.ExportTypeId + " ItemId - " + scheduleContent.ItemId + " CurrentScheduleTime - " + scheduleContent.CurrentScheduleTime + " RecurrenceInfo - " + scheduleContent.RecurrenceInfo);
            return scheduleContent;
        }

        public DataTable GetAllUserByGroupId(int groupId)
        {
            var dataTable = new DataTable();
            var selected = new List<SelectedColumn>
            {
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName,
                    ColumnName = "*"
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.Email
                },
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.DisplayName
                }
            };
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_UserGroup.GroupId,
                    Condition = Conditions.Equals,
                    Value = groupId,
                    TableName = GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_User.IsActive,
                    Condition = Conditions.Equals,
                    Value = true,
                    TableName = GlobalAppSettings.DbColumns.DB_User.DB_TableName,
                    LogicalOperator = LogicalOperators.AND
                }
            };

            var joinSpecification = new JoinSpecification
            {
                Table = GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName,
                Column =
                    new List<JoinColumn>
                    {
                        new JoinColumn
                        {
                            TableName = GlobalAppSettings.DbColumns.DB_UserGroup.DB_TableName,
                            JoinedColumn = GlobalAppSettings.DbColumns.DB_UserGroup.UserId,
                            Operation = Conditions.Equals,
                            ParentTableColumn = GlobalAppSettings.DbColumns.DB_User.Id,
                            ParentTable = GlobalAppSettings.DbColumns.DB_User.DB_TableName

                        }
                    },
                JoinType = JoinTypes.Inner
            };

            try
            {
                dataTable =
                    DataProvider.ExecuteReaderQuery(
                        QueryBuilder.ApplyWhereClause(
                            QueryBuilder.ApplyJoins(GlobalAppSettings.DbColumns.DB_User.DB_TableName, selected,
                                joinSpecification), whereColumns)).DataTable;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception while reading users of group", e, MethodBase.GetCurrentMethod(), " GroupId - " + groupId);
                return dataTable;
            }

            return dataTable;
        }

        public string GetItemName(Guid itemId)
        {
            var itemName = String.Empty;
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Id,
                    Condition = Conditions.Equals,
                    Value = itemId
                }
            };

            var selected = new List<SelectedColumn>
            {
                new SelectedColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name
                }
            };
            try
            {
                var dataTable =
                    DataProvider.ExecuteReaderQuery(
                        QueryBuilder.SelectRecordsFromTable(GlobalAppSettings.DbColumns.DB_Item.DB_TableName, selected,
                            whereColumns)).DataTable;
                itemName =
                    dataTable.AsEnumerable()
                        .Select(r => r.Field<string>(GlobalAppSettings.DbColumns.DB_Item.Name))
                        .First();
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception while reading item name", e, MethodBase.GetCurrentMethod(), " ItemId - " + itemId +  " ItemName - " + itemName);
                return itemName;
            }
            return itemName;
        }

        public void UpdateNextScheduleDate(DateTime? nextOccurence, Guid scheduleId)
        {
            LogExtension.LogInfo("Updating next schedule started", MethodBase.GetCurrentMethod(),
                " NextOccurence - " + nextOccurence.Value.ToString(GlobalAppSettings.GetDateTimeFormat()) +
                " ScheduleId - " + scheduleId);

            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ScheduleId,
                    Condition = Conditions.Equals,
                    Value = scheduleId,
                }
            };

            var updateColumns = new List<UpdateColumn>();
            if (nextOccurence != null)
            {
                updateColumns.Add(new UpdateColumn
                {
                    TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName,
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.NextSchedule,
                    Value = nextOccurence.Value.ToString(GlobalAppSettings.GetDateTimeFormat())
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
                DataProvider.ExecuteNonQuery(
                    QueryBuilder.UpdateRowInTable(GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName,
                        updateColumns,
                        whereColumns));
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception while updating next schedule", e, MethodBase.GetCurrentMethod(),
                    " NextOccurence - " + nextOccurence.Value.ToString(GlobalAppSettings.GetDateTimeFormat()) +
                    " ScheduleId - " + scheduleId);

                return;
            }
            LogExtension.LogInfo("Updating next schedule successful", MethodBase.GetCurrentMethod(),
                " NextOccurence - " + nextOccurence.Value.ToString(GlobalAppSettings.GetDateTimeFormat()) +
                " ScheduleId - " + scheduleId);
        }

        public void UpdateScheduleJobStatus(Guid scheduleId, ScheduleStatus status, bool isOnDemand)
        {

            try
            {
                var values = new Dictionary<string, object>
                {
                    {GlobalAppSettings.DbColumns.DB_ScheduleLog.ScheduleId, scheduleId},
                    {GlobalAppSettings.DbColumns.DB_ScheduleLog.ScheduleStatusId, Convert.ToInt32(status)},
                    {GlobalAppSettings.DbColumns.DB_ScheduleLog.ExecutedDate, DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())},
                    {GlobalAppSettings.DbColumns.DB_ScheduleLog.ModifiedDate, DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat())},
                    {GlobalAppSettings.DbColumns.DB_ScheduleLog.IsActive, true},
                    {GlobalAppSettings.DbColumns.DB_ScheduleLog.IsOnDemand, isOnDemand}
                };
                DataProvider.ExecuteNonQuery(
                    QueryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_ScheduleLog.DB_TableName, values));
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception while updating schedule job status", e, MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId + " Status - " + Convert.ToInt32(status) + " IsOnDemand - " + isOnDemand);
                return;
            }
        }

        public void UpdateScheduleStatusInScheduleUserLog(Guid scheduleId, int userId, ScheduleStatus status,
            bool isOnDemand)
        {
            try
            {
                var values = new Dictionary<string, object>();
                values.Add(GlobalAppSettings.DbColumns.DB_ScheduleLogUser.ScheduleId, scheduleId);
                values.Add(GlobalAppSettings.DbColumns.DB_ScheduleLogUser.DeliveredUserId, userId);
                values.Add(GlobalAppSettings.DbColumns.DB_ScheduleLogUser.DeliveredDate, DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat()));
                values.Add(GlobalAppSettings.DbColumns.DB_ScheduleLogUser.IsActive, true);
                values.Add(GlobalAppSettings.DbColumns.DB_ScheduleLogUser.IsOnDemand, isOnDemand);
                values.Add(GlobalAppSettings.DbColumns.DB_ScheduleLogUser.ScheduleStatusId, Convert.ToInt32(status));
                DataProvider.ExecuteNonQuery(
                    QueryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_ScheduleLogUser.DB_TableName, values));
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception while adding schedule user log", e, MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId + " UserId - " + userId + " Status - " + Convert.ToInt32(status));
            }
        }

        public void UpdateScheduleStatusInScheduleGroupLog(Guid scheduleId, int userId, int groupId,
            ScheduleStatus status, bool isOnDemand)
        {
            try
            {
                var values = new Dictionary<string, object>();
                values.Add(GlobalAppSettings.DbColumns.DB_ScheduleLogGroup.ScheduleId, scheduleId);
                values.Add(GlobalAppSettings.DbColumns.DB_ScheduleLogGroup.GroupId, groupId);
                values.Add(GlobalAppSettings.DbColumns.DB_ScheduleLogGroup.DeliveredUserId, userId);
                values.Add(GlobalAppSettings.DbColumns.DB_ScheduleLogGroup.DeliveredDate, DateTime.UtcNow.ToString(GlobalAppSettings.GetDateTimeFormat()));
                values.Add(GlobalAppSettings.DbColumns.DB_ScheduleLogGroup.IsActive, true);
                values.Add(GlobalAppSettings.DbColumns.DB_ScheduleLogGroup.IsOnDemand, isOnDemand);
                values.Add(GlobalAppSettings.DbColumns.DB_ScheduleLogGroup.ScheduleStatusId, Convert.ToInt32(status));
                DataProvider.ExecuteNonQuery(
                    QueryBuilder.AddToTable(GlobalAppSettings.DbColumns.DB_ScheduleLogGroup.DB_TableName, values));
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception while adding schedule user log", e, MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId + " UserId - " + userId + " GroupId - " + groupId + " Status - " + Convert.ToInt32(status) + " IsOnDemand - " + isOnDemand);
            }
        }

        public List<ScheduleJob> GetScheduleJobs(DateTime startTime, DateTime endTime)
        {
            LogExtension.LogInfo("Fetching schedules between " + startTime.ToString() + " and " + endTime.ToString(), MethodBase.GetCurrentMethod());
            var scheduleList = new List<ScheduleJob>();
            try
            {
                var whereColumns = new List<ConditionColumn>
                {
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.IsActive,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = true
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.IsEnabled,
                        Condition = Conditions.Equals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = true
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.NextSchedule,
                        Condition = Conditions.GreaterThan,
                        LogicalOperator = LogicalOperators.AND,
                        Value = startTime.ToString(GlobalAppSettings.GetDateTimeFormat())
                    },
                    new ConditionColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.NextSchedule,
                        Condition = Conditions.LessThanOrEquals,
                        LogicalOperator = LogicalOperators.AND,
                        Value = endTime.ToString(GlobalAppSettings.GetDateTimeFormat())
                    }
                };
                var selectColumns = new List<SelectedColumn>
                {
                    new SelectedColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ScheduleId
                    },
                    new SelectedColumn
                    {
                        ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.NextSchedule
                    }
                };

                var dataTable =
                    DataProvider.ExecuteReaderQuery(
                        QueryBuilder.SelectRecordsFromTable(
                            GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName, selectColumns, whereColumns))
                        .DataTable;
                scheduleList =
                    dataTable.AsEnumerable()
                        .Select(
                            r =>
                                new ScheduleJob
                                {
                                    ScheduleId = r.Field<Guid>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.ScheduleId),
                                    CurrentScheduleTime =
                                        r.Field<DateTime>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.NextSchedule)
                                }).ToList();

                LogExtension.LogInfo(
                    "Retrieved schedules between " + startTime.ToString() + " and " +
                    endTime.ToString(), MethodBase.GetCurrentMethod());

                return scheduleList;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception is thrown while fetching schedules between " + startTime.ToString() + " and " +
                    endTime.ToString(), e, MethodBase.GetCurrentMethod());
                return scheduleList;
            }
        }

        public List<FailedJob> GetFailedJobs(DateTime startTime, DateTime endTime)
        {
            var jobList = new List<FailedJob>();
            var whereColumns = new List<ConditionColumn>
            {
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.IsActive,
                    Condition = Conditions.Equals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = true
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.NextSchedule,
                    Condition = Conditions.GreaterThan,
                    LogicalOperator = LogicalOperators.AND,
                    Value = startTime
                },
                new ConditionColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.NextSchedule,
                    Condition = Conditions.LessThanOrEquals,
                    LogicalOperator = LogicalOperators.AND,
                    Value = endTime
                }
            };
            var selectColumns = new List<SelectedColumn>
            {
                new SelectedColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ItemId
                },
                new SelectedColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.NextSchedule
                },
                new SelectedColumn
                {
                    ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.RecurrenceInfo
                }
            };
            try
            {
                var dataTable =
                    DataProvider.ExecuteReaderQuery(
                        QueryBuilder.SelectRecordsFromTable(
                            GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName, selectColumns, whereColumns))
                        .DataTable;

                jobList = dataTable.AsEnumerable().Select(r =>
                    new FailedJob
                    {
                        ScheduleId = r.Field<Guid>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.ItemId),
                        CurrentScheduleTime =
                            r.Field<DateTime>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.NextSchedule),
                        RecurrenceInfo = r.Field<string>(GlobalAppSettings.DbColumns.DB_ScheduleDetail.RecurrenceInfo)
                    }).ToList();

                return jobList;
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception while getting failed schedules between " + startTime.ToString() + " and " +
                    endTime.ToString(), e, MethodBase.GetCurrentMethod());
                return jobList;
            }
        }

        public DataTable GetUserSchedule(int userId)
        {
            var schedules = new DataTable();

            try
            {
                var scheduleItems = new ItemManagement().GetItems(userId, ItemType.Schedule);

                if (scheduleItems != null && scheduleItems.result.Any())
                {
                    var whereColumns = new List<ConditionColumn>
                    {
                        new ConditionColumn
                        {
                            TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName,
                            ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.IsActive,
                            Condition = Conditions.Equals,
                            LogicalOperator = LogicalOperators.AND,
                            Value = true
                        },
                        new ConditionColumn
                        {
                            TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName,
                            ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ScheduleId,
                            Condition = Conditions.IN,
                            LogicalOperator = LogicalOperators.AND,
                            Values = scheduleItems.result.Select(s => s.Id.ToString()).ToList()
                        }
                    };

                    var selectColumns = new List<SelectedColumn>
                    {
                        new SelectedColumn
                        {
                            ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ScheduleId,
                            TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName,
                            AliasName = "ScheduleId"
                        },
                        new SelectedColumn
                        {
                            ColumnName = GlobalAppSettings.DbColumns.DB_Item.Id,
                            TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                            AliasName = "ItemId"
                        },
                        new SelectedColumn
                        {
                            ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.Name,
                            TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName,
                            AliasName = "ScheduleName"
                        },
                        new SelectedColumn
                        {
                            ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.IsEnabled,
                            TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName,
                            AliasName = "IsEnabled"
                        },
                        new SelectedColumn
                        {
                            ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.NextSchedule,
                            TableName = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName,
                            AliasName = "NextSchedule"
                        },
                        new SelectedColumn
                        {
                            ColumnName = GlobalAppSettings.DbColumns.DB_Item.Name,
                            TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                            AliasName = "ItemName"
                        },
                        new SelectedColumn
                        {
                            ColumnName = GlobalAppSettings.DbColumns.DB_ScheduleLog.ExecutedDate,
                            TableName = GlobalAppSettings.DbColumns.DB_ScheduleLog.DB_TableName,
                            AliasName = "LastRun"
                        }
                    };

                    var joinSpecification = new List<JoinSpecification>
                    {
                        new JoinSpecification
                        {
                            Table = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                            Column =
                                new List<JoinColumn>
                                {
                                    new JoinColumn
                                    {
                                        TableName = GlobalAppSettings.DbColumns.DB_Item.DB_TableName,
                                        JoinedColumn = GlobalAppSettings.DbColumns.DB_Item.Id,
                                        Operation = Conditions.Equals,
                                        ParentTableColumn = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ItemId,
                                        ParentTable = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName

                                    }
                                },
                            JoinType = JoinTypes.Inner
                        },
                        new JoinSpecification
                        {
                            Table = GlobalAppSettings.DbColumns.DB_ScheduleLog.DB_TableName,
                            Column =
                                new List<JoinColumn>
                                {
                                    new JoinColumn
                                    {
                                        TableName = GlobalAppSettings.DbColumns.DB_ScheduleLog.DB_TableName,
                                        JoinedColumn = GlobalAppSettings.DbColumns.DB_ScheduleLog.ScheduleId,
                                        Operation = Conditions.Equals,
                                        ParentTableColumn = GlobalAppSettings.DbColumns.DB_ScheduleDetail.ScheduleId,
                                        ParentTable = GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName

                                    }
                                },
                            JoinType = JoinTypes.Left
                        }
                    };


                    var query =
                        QueryBuilder.ApplyWhereClause(
                            QueryBuilder.ApplyMultipleJoins(GlobalAppSettings.DbColumns.DB_ScheduleDetail.DB_TableName,
                                selectColumns,
                                joinSpecification), whereColumns);

                    schedules = DataProvider.ExecuteReaderQuery(query).DataTable;

                    if (schedules.Rows.Count > 0)
                    {
                        schedules.Columns.Add(new DataColumn("CanWrite", typeof(bool)));
                        schedules.Columns.Add(new DataColumn("CanDelete", typeof(bool)));

                        foreach (DataRow schedule in schedules.Rows)
                        {
                            var scheduleItem =
                                scheduleItems.result.FirstOrDefault(
                                    f => f.Id == schedule.Field<Guid>("ScheduleId"));

                            if (scheduleItem != null)
                            {
                                schedule["CanWrite"] = scheduleItem.CanWrite;
                                schedule["CanDelete"] = scheduleItem.CanDelete;
                            }
                        }
                    }
                }

                return schedules;
            }
            catch (Exception ex)
            {
                LogExtension.LogError("Exception while reading user's schedules", ex, MethodBase.GetCurrentMethod(), " UserId - " + userId);
                return schedules;
            }
        }

        #endregion

        #region OnDemand

        public void OnDemandSchedule(Guid scheduleId)
        {
            LogExtension.LogInfo("On demand schedule job started", MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId);

            var scheduleContent = GetScheduleContent(scheduleId);

            scheduleContent.ExportFormat = (scheduleContent.ExportTypeId == ExportType.Pdf)
                ? "pdf"
                : (scheduleContent.ExportTypeId == ExportType.Excel)
                    ? "xls"
                    : (scheduleContent.ExportTypeId == ExportType.Word) ? "doc" : "html";

            if (!Directory.Exists(GlobalAppSettings.GetItemsPath() + "TempItems\\"))
            {
                Directory.CreateDirectory(GlobalAppSettings.GetItemsPath() + "TempItems\\");
            }

            var writeReport =
                ScheduleReportWriter.GenerateReport(GlobalAppSettings.GetItemsPath() + "TempItems\\",
                    scheduleContent, scheduleContent.UserName,scheduleContent.Password);

            if (writeReport)
            {
                LogExtension.LogInfo("Sending mail for receipient users started", MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId + " ScheduleUserName - " + scheduleContent.UserName + " SchedulePassword - " + scheduleContent.Password + " ScheduleName - " + scheduleContent.ScheduleName + " ScheduleReportName - " + scheduleContent.ReportName + " ScheduleExportFormat -" + scheduleContent.ExportFormat + " ExportTypeId - " + scheduleContent.ExportTypeId);
                var groupList = GetGroupRecipientsOfSchedule(scheduleContent.ScheduleId);
                groupList.AddRange(GetUserRecipientsOfSchedule(scheduleContent.ScheduleId));
                var recipients = groupList.Distinct(new ScheduleRecipientComparer()).ToList();
                var subject = "Grout : Your scheduled report";
                var tasks = new List<Task>();
                UpdateScheduleJobStatus(scheduleContent.ScheduleId, ScheduleStatus.Success, true);
                try
                {
                    foreach (var recipient in recipients)
                    {
                        var body =
                           "<html xmlns='http://www.w3.org/1999/xhtml' xmlns='http://www.w3.org/1999/xhtml' xmlns:v='urn:schemas-microsoft-com:vml' xmlns:o='urn:schemas-microsoft-com:office:office'><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8'><meta name='viewport' content='width=device-width'>	<!--[if gte mso 9]><xml> <o:OfficeDocumentSettings>  <o:AllowPNG/>  <o:PixelsPerInch>96</o:PixelsPerInch> </o:OfficeDocumentSettings></xml><![endif]--></head><body style='min-width: 100%;-webkit-text-size-adjust: 100%;-ms-text-size-adjust: 100%;margin: 0;padding: 0;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;text-align: left;line-height: 19px;font-size: 14px;background-color: rgb(244,244,244);padding-top: 10px;width: 100% !important;'><center style='width: 100%;min-width: 580px;'><table style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='email-container' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='email-header' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;background-color: #5c6bc0;padding-right: 10px;padding-left: 10px;padding-top: 10px;padding-bottom: 18px;border-top-right-radius: 5px;border-top-left-radius: 5px;border-collapse: collapse !important;'><table class='body container' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;height: 100%;width: 580px;margin: 0;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;line-height: 19px;font-size: 14px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='sync-inc wrapper' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 2px 10px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><h6 style='color: white;font-family: &quot;Segoe UI&quot;,sans-serif !important;font-weight: 500;padding: 0;margin: 0;text-align: left;line-height: 1.3;word-break: normal;font-size: 20px;font-style: normal;'><a href='"
                          + GlobalAppSettings.SystemSettings.BaseUrl + "/accounts/login' style='color: white;text-decoration: none;'>"
                          + GlobalAppSettings.SystemSettings.OrganizationName + "</a></h6></td><td class='user-login wrapper last' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 11px 10px 0px;vertical-align: top;text-align: left;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse;'><p style='margin: 0;margin-bottom: 10px;color: white;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: 400;text-align: right;line-height: 1.3;font-size: 11px; font-style: normal;'><a href='"
                          + GlobalAppSettings.SystemSettings.BaseUrl + "/accounts/login' style='color: white;text-decoration: none;'>LOG IN</a></p></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='wrapper last' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 50px 0px 10px 273px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><img src='"
                          + GlobalAppSettings.SystemSettings.BaseUrl + "/Content/Images/Application/"
                          + GlobalAppSettings.SystemSettings.MainScreenLogo + "' style='outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;width: auto;max-width: 100%;float: none;clear: both;margin: 0 auto;'></td></tr><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='welcome wrapper last' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 18px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;position: relative;border-collapse: collapse !important;'><h6 style='color: white;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: lighter;padding: 0;margin: 0;text-align: center;line-height: 1.3;word-break: normal;font-size: 20px;'>Scheduled Report</h6></td></tr></tbody></table></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='message-body' style='word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;padding: 0px 0px 10px;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;background-color: white;padding-right: 10px;padding-left: 10px;border-bottom-right-radius: 5px;border-bottom-left-radius: 5px;border-collapse: collapse !important;'><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='activation-msg' style='padding-top: 20px;padding-left: 4px; padding-bottom: 12px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto; vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;'><tbody><tr><td style='padding: 0px 0px 10px;'><p style='margin-bottom: 4px;margin: 0;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;'>Hello "
                          + recipient.FullName + ",</p></td></tr></tbody></table><p style='margin-bottom: 2px;margin: 0;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;text-align: left;line-height: 19px;font-size: 12px;'> Please find attached, the report that you had requested.</p></td></tr></tbody></table><table style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td style='padding-left: 4px;padding-bottom: 0px; padding-top: 0px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><table class='block-grid five-up' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;width: 100%;max-width: 580px;'><tbody><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='activate-btn center' style='padding: 0px 0px 10px;'><p style='margin-bottom: 2px; margin: 0;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;text-align: left;line-height: 19px;font-size: 12px;'> Schedule—"
                          + scheduleContent.ScheduleName + " has exported the report "
                          + scheduleContent.ReportName + ".</p></td></tr></tbody></table></td></tr></tbody></table><table class='twelve columns' style='border-spacing: 0;border-collapse: collapse;padding: 0;vertical-align: top;text-align: left;margin: 0 auto;width: 580px;'><tr style='padding: 0;vertical-align: top;text-align: left;'><td class='signature' style='padding-left: 4px;padding-bottom: 0px; padding-top: 0px; word-break: break-word;-webkit-hyphens: auto;-moz-hyphens: auto;hyphens: auto;vertical-align: top;text-align: left;color: #222222;font-family: &quot;Helvetica&quot;, &quot;Segoe UI&quot;, sans-serif;font-weight: normal;margin: 0;line-height: 19px;font-size: 14px;border-collapse: collapse !important;'><p style='margin-bottom: 6px;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;margin-top: 0px;'>Regards,</p><p style='margin: 0;margin-bottom: 10px;color: #353535;font-family: &quot;Segoe UI&quot;,sans-serif;font-weight: normal;padding: 0;text-align: left;line-height: 19px;font-size: 12px;'>"
                          + GlobalAppSettings.SystemSettings.OrganizationName + "</p></td></tr></table></td></tr></tbody></table></td></tr></tbody></table></center></body></html>";

                        var mailMessage = new MailMessage();
                        mailMessage.To.Add(new MailAddress(recipient.Email));
                        mailMessage.From = new MailAddress(GlobalAppSettings.SystemSettings.MailSettingsAddress,
                            GlobalAppSettings.SystemSettings.MailSettingsSenderName);
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true;
                        mailMessage.Attachments.Add(
                            new Attachment(GlobalAppSettings.GetItemsPath() + "TempItems\\" +
                                           scheduleContent.ScheduleId + "\\" + scheduleContent.ReportName +
                                           "." + scheduleContent.ExportFormat));

                        tasks.Add(Task.Factory.StartNew(() =>
                        {
                            var isEmailSentSuccessfully = true;
                            try
                            {
                                new MailSender().SendEmail(mailMessage);
                            }
                            catch (Exception ex)
                            {
                                LogExtension.LogError("Exception while sending mail to user", ex, MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId + " ScheduleUserName - " + scheduleContent.UserName + " SchedulePassword - " + scheduleContent.Password + " ScheduleName - " + scheduleContent.ScheduleName + " ScheduleReportName - " + scheduleContent.ReportName + " ScheduleExportFormat -" + scheduleContent.ExportFormat + " ExportTypeId - " + scheduleContent.ExportTypeId + " FromAddress - " + GlobalAppSettings.SystemSettings.MailSettingsAddress + " ToAddress - " + recipient.Email);
                                isEmailSentSuccessfully = false;
                            }

                            #region Scheduler User and Group Log

                            var userLogs =
                                groupList.Where(p => p.UserId == recipient.UserId && p.GroupId == null).ToList();
                            if (userLogs.Count > 0)
                            {
                                UpdateScheduleStatusInScheduleUserLog(scheduleContent.ScheduleId, recipient.UserId,
                                    isEmailSentSuccessfully
                                        ? ScheduleStatus.Success
                                        : ScheduleStatus.Fail, true);
                            }
                            var groupLogs =
                                groupList.Where(p => p.GroupId != null && p.UserId == recipient.UserId).ToList();
                            if (groupLogs.Count > 0)
                            {
                                foreach (var groupLog in groupLogs)
                                {
                                    UpdateScheduleStatusInScheduleGroupLog(scheduleContent.ScheduleId, recipient.UserId,
                                        Convert.ToInt32(groupLog.GroupId), isEmailSentSuccessfully
                                            ? ScheduleStatus.Success
                                            : ScheduleStatus.Fail, true);
                                }
                            }

                            #endregion

                        }));
                    }
                    Task.WaitAll(tasks.ToArray());

                }
                catch (Exception e)
                {
                    LogExtension.LogError("Exception is thrown while sending mail to recepients", e, MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId + " ScheduleUserName - " + scheduleContent.UserName + " SchedulePassword - " + scheduleContent.Password + " ScheduleName - " + scheduleContent.ScheduleName + " ScheduleReportName - " + scheduleContent.ReportName + " ScheduleExportFormat -" + scheduleContent.ExportFormat + " ExportTypeId - " + scheduleContent.ExportTypeId);
                }
                LogExtension.LogInfo("Sending mail for receipient users successfull", MethodBase.GetCurrentMethod(), " ScheduleId - " + scheduleId + " ScheduleUserName - " + scheduleContent.UserName + " SchedulePassword - " + scheduleContent.Password + " ScheduleName - " + scheduleContent.ScheduleName + " ScheduleReportName - " + scheduleContent.ReportName + " ScheduleExportFormat -" + scheduleContent.ExportFormat + " ExportTypeId - " + scheduleContent.ExportTypeId);
            }
            else
            {
                UpdateScheduleJobStatus(scheduleContent.ScheduleId, ScheduleStatus.Fail, true);
            }
        }

        #endregion
    }
}