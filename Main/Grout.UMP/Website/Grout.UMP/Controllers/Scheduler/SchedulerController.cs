using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Grout.Base;
using Grout.UMP.Models;
using Newtonsoft.Json;
using Grout.Base.DataClasses;

namespace Grout.UMP.Controllers
{
    [Authorize]
    public class SchedulerController : Controller
    {
        readonly ScheduleManagement scheduleManagement = new ScheduleManagement(GlobalAppSettings.QueryBuilder, GlobalAppSettings.DataProvider);
        private readonly PermissionSet _permissionBase = new PermissionSet();
        private readonly UserManagement _userManagement = new UserManagement();

        [HttpPost]
        public JsonResult AddSchedule(string scheduleList)
        {
            var result = true;

            var scheduleDetails = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(scheduleList);
            var scheduleItem = scheduleDetails["ScheduleItem"];
            ScheduleItem item = scheduleManagement.GetParseSchedule(scheduleItem);
            var scheduleAddResult = scheduleManagement.AddScheduleItem(item);

            var subscribedUserList = scheduleDetails["UserList"];
            if (subscribedUserList != null)
            {
                scheduleManagement.SubscribeUsers(item.ScheduleId, subscribedUserList);
            }

            var subscribedGroupList = scheduleDetails["GroupList"];
            if (subscribedGroupList != null)
            {
                scheduleManagement.SubscribeGroups(item.ScheduleId, subscribedGroupList);
            }
            var permissionValue = new Permission
            {
                IsUserPermission = true,
                ItemId = new Guid(scheduleAddResult.ReturnValue.ToString()),
                PermissionAccess = PermissionAccess.ReadWriteDelete,
                PermissionEntity = PermissionEntity.SpecificSchedule,
                TargetId = Convert.ToInt32(HttpContext.User.Identity.Name)
            };
            _permissionBase.AddPermissionToUser(permissionValue);
            return Json(new { Result = result });
        }

        public JsonResult GetScheduleInfo(string scheduleId)
        {
            var scheduleItem = scheduleManagement.GetScheduledItem(new Guid(scheduleId));
            var scheduleUser = scheduleManagement.GetSubscribedUserList(new Guid(scheduleId));
            var scheduleGroup = scheduleManagement.GetSubscribedGroupList(new Guid(scheduleId));

            var schedule = ScheduleHelper.XMLDeserializer(scheduleItem.RecurrenceInfo);

            schedule.ScheduleId = scheduleItem.ScheduleId;
            schedule.ItemId = scheduleItem.ItemId;
            schedule.StartBoundary = schedule.StartBoundary;
            schedule.EndBoundary = schedule.EndBoundary;
            schedule.Occurrences = schedule.Occurrences;
            schedule.Enabled = schedule.Enabled;
            schedule.Type = schedule.Type;

            scheduleItem.RecurrenceType = scheduleManagement.GetRecurrenceType(scheduleItem.RecurrenceTypeId);
            scheduleItem.ExportType = ((ExportType)scheduleItem.ExportTypeId).ToString();

            switch (schedule.Type)
            {
                case TaskScheduleType.Daily:

                    var daily = schedule.DailySchedule;
                    schedule.DailySchedule = new DailySchedule { DaysInterval = daily.DaysInterval };
                    break;
                case TaskScheduleType.DailyWeekDay:

                    var dailyWeekDay = schedule.DailyWeekDaySchedule;
                    schedule.DailyWeekDaySchedule = new DailyWeekDaySchedule { WeekDay = dailyWeekDay.WeekDay };

                    break;
                case TaskScheduleType.Weekly:

                    var weeklySchedule = schedule.WeeklySchedule;
                    schedule.WeeklySchedule = new WeeklySchedule { DaysOfWeek = weeklySchedule.DaysOfWeek, WeeksInterval = weeklySchedule.WeeksInterval };
                    break;
                case TaskScheduleType.Monthly:

                    var monthlySchedule = schedule.MonthlySchedule;
                    schedule.MonthlySchedule = new MonthlySchedule { DayOfMonth = monthlySchedule.DayOfMonth, Months = monthlySchedule.Months };
                    break;
                case TaskScheduleType.MonthlyDOW:

                    var monthlyDowSchedule = schedule.MonthlyDowSchedule;
                    schedule.MonthlyDowSchedule = new MonthlyDowSchedule
                    {
                        WeekOfMonth = monthlyDowSchedule.WeekOfMonth,
                        DayOfWeek = monthlyDowSchedule.DayOfWeek,
                        Months = monthlyDowSchedule.Months,
                        DaysOfTheWeek = monthlyDowSchedule.DayOfWeek.ToString(),
                        WeeksOfTheMonth = monthlyDowSchedule.WeekOfMonth.ToString()
                    };

                    break;
                case TaskScheduleType.Yearly:

                    var yearlySchedule = schedule.YearlySchedule;
                    schedule.YearlySchedule = new YearlySchedule { DayOfMonth = yearlySchedule.DayOfMonth, MonthsOfTheYear = yearlySchedule.Month.ToString(), Month = yearlySchedule.Month, YearsInterval = yearlySchedule.YearsInterval };

                    break;
                case TaskScheduleType.YearlyDOW:

                    var yearlyDowSchedule = schedule.YearlyDowSchedule;
                    schedule.YearlyDowSchedule = new YearlyDowSchedule { DayOfWeek = yearlyDowSchedule.DayOfWeek, DaysOfTheWeek = yearlyDowSchedule.DayOfWeek.ToString(), WeeksOfTheMonth = yearlyDowSchedule.WeekOfMonth.ToString(), WeekOfMonth = yearlyDowSchedule.WeekOfMonth, MonthsOfTheYear = yearlyDowSchedule.MonthOfYear.ToString(), MonthOfYear = yearlyDowSchedule.MonthOfYear, YearsInterval = yearlyDowSchedule.YearsInterval };
                    break;
            }

            scheduleItem.Recurrence = schedule;

            return Json(new { ScheduleItem = scheduleItem, SubscribedUser = scheduleUser, SubscribedGroup = scheduleGroup });
        }

        public JsonResult UpdateSchedule(string scheduleList)
        {
            var scheduleDetails = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(scheduleList);
            var scheduleItem = scheduleDetails["ScheduleItem"];
            var scheduleId = new Guid(scheduleItem["ScheduleId"].Value);
            ScheduleItem item = scheduleManagement.GetParseSchedule(scheduleItem);
            item.ScheduleId = scheduleId;
            item.Recurrence.ScheduleId = scheduleId;
            scheduleManagement.UpdateScheduleItem(item);

            var subscribedUsers = scheduleDetails["UserList"];
            scheduleManagement.UpdateSubscribedUser(scheduleId, subscribedUsers);

            var subscribedGroups = scheduleDetails["GroupList"];
            scheduleManagement.UpdateSubscribedGroup(scheduleId, subscribedGroups);
            return null;
        }

        #region Enable_Disable_Remove Schedule

        public JsonResult EnableSchedule(string scheduleId)
        {
            scheduleManagement.EnableSchedule(new Guid(scheduleId));
            return null;
        }

        public JsonResult DisableSchedule(string scheduleId)
        {
            scheduleManagement.DisableSchedule(new Guid(scheduleId));
            return null;
        }

        public JsonResult RemoveSchedule(string scheduleId)
        {
            scheduleManagement.RemoveSchedule(new Guid(scheduleId));
            return Json(new { Success = true });
        }

        #endregion

        public void OnDemandSchedule(string scheduleId)
        {
            var processor = new ScheduleJobProcessor(GlobalAppSettings.GetConfigFilepath() + ServerSetup.Configuration, GlobalAppSettings.GetSchedulerExportPath());
            processor.OnDemandSchedule(new Guid(scheduleId));
        }

        public PartialViewResult _DeleteSchedule()
        {
            return PartialView();
        }

        public ActionResult GetSchedulerDialog()
        {
            return PartialView("../Shared/_SchedulerDialog");
        }

        /// <summary>
        /// Get the enum values.
        /// </summary>
        /// <returns>Json Result of the operation</returns>
        public JsonResult GetRecurrenceType()
        {
            string[] recurrenceType = Enum.GetNames(typeof(RecurrenceType));
            string[] weekOfMonth = Enum.GetNames(typeof(WeekOfMonth));
            string[] daysOfWeek = Enum.GetNames(typeof(DayOfTheWeek));
            string[] monthOfYear = Enum.GetNames(typeof(MonthOfTheYear));

            var zone = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(GlobalAppSettings.GetSessionTimeZone())).ToString("MM/dd/yyyy hh:mm tt");
            return Json(new { RecurrenceType = recurrenceType, Weeks = weekOfMonth, Days = daysOfWeek, Months = monthOfYear, TimeZoneDateTime = zone });
        }

        public ActionResult Schedules()
        {
            return View(scheduleManagement.GetSchedules(Convert.ToInt32(HttpContext.User.Identity.Name)));
        }

        [HttpPost]
        public ActionResult GetSchedules()
        {
            return Json(scheduleManagement.GetSchedules(Convert.ToInt32(HttpContext.User.Identity.Name)));
        }

        public JsonResult CheckScheduleNameExist(string scheduleId, string scheduleName)
        {
            bool result = false;
            var schedule =
                scheduleManagement.GetSchedules(Convert.ToInt32(HttpContext.User.Identity.Name))
                    .FirstOrDefault(f => f.Name == scheduleName);

            if (schedule != null)
            {
                result = scheduleId == null ? true : schedule.Id == new Guid(scheduleId) ? false : true;
            }
            return Json(new { Result = result });
        }
    }
}
