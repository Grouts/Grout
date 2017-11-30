using System;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.UMP.ActionFilters;
using Grout.UMP.Models;
using System.DirectoryServices;
using Grout.Base.Logger;
using System.Reflection;

namespace Grout.UMP.Controllers
{
    [Authorize]
    [AdminActionFilter]
    public class AdministrationController : Controller
    {
        private readonly ApiHandler _apiHandler = new ApiHandler();
        private readonly UserManagement _userDetails = new UserManagement();

        public ActionResult Administration()
        {
            var systemSettings = SystemSettingsModel.GetSystemSettings();
            ViewBag.SystemSettings = systemSettings;
            ViewBag.listTimeZone = TimeZoneInfo.GetSystemTimeZones().ToList();
            ViewBag.SystemTimeZone = TimeZoneInfo.FindSystemTimeZoneById(systemSettings.TimeZone);

            return View();
        }
        [HttpPost]
        public void UpdateSystemSettings(string systemSettingsData)
        {
            var systemSettings = JsonConvert.DeserializeObject<SystemSettings>(systemSettingsData);
            SystemSettingsModel.UpdateSystemSettings(systemSettings);
            GlobalAppSettings.InitializeSystemSettings(GlobalAppSettings.GetConfigFilepath() + ServerSetup.Configuration);
        }

        [HttpPost]
        public JsonResult CheckMailSettingsExist()
        {
            var isAdmin = GlobalAppSettings.IsAdmin(Convert.ToInt32(HttpContext.User.Identity.Name));
            return Json(new { result = SystemSettingsModel.MailSettingsExist(), isAdmin });
        }

    }
}
