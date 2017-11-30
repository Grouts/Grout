using System;
using System.Reflection;
using Grout.Base;
using System.Web.Mvc;
using Grout.Base.DataClasses;
using Grout.Base.Utilities;
using Grout.Base.Logger;


namespace Grout.UMP.ActionFilters
{
    public class AppicationVersionValidationActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                var settings = new SystemSettingsSerializer().Deserialize(GlobalAppSettings.GetConfigFilepath() + ServerSetup.Configuration);
                if (filterContext.HttpContext.Request.Url != null)
                {
                   string[] segments = filterContext.HttpContext.Request.Url.Segments;
                   if (settings == null || (segments.Length == 2 && (segments[1].ToLower() == "startup" || segments[1].ToLower() == "login")) || (segments.Length == 3 && ((segments[1].ToLower() == "error/" && segments[2].ToLower() == "httperror500") || (segments[1].ToLower() == "user/" && segments[2].ToLower() == "avatar"))))
                    {
                        base.OnActionExecuting(filterContext);
                    }
                    else if (GlobalAppSettings.IsLatestVersion)
                    {
                        new DatabaseSchemaUpdater();
                        GlobalAppSettings.IsLatestVersion = DatabaseSchemaUpdater.IsLatestVersion();
                        if (GlobalAppSettings.IsLatestVersion)
                        {
                          
                            LogExtension.LogError("Application Error 500 - Error in updating database schema", null, MethodBase.GetCurrentMethod());

                            filterContext.Result = new ViewResult
                            {
                                ViewName = "../Error/HttpError500"
                            };
                        }
                        base.OnActionExecuting(filterContext);
                    }
                    else
                    {
                        base.OnActionExecuting(filterContext);
                    }
                }
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception occured in AppicationVersionValidationActionFilter :", e, MethodBase.GetCurrentMethod());
            }
        }
    }
}