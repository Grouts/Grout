using Grout.Base;
using Grout.Base.DataClasses;
using Grout.UMP.ActionFilters;
using Grout.UMP.Controllers;
using Grout.UMP.Models;
using System;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.UI;
using System.Web.WebPages;
using Grout.Base.Logger;
using Grout.Base.Utilities;

namespace Grout.UMP
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            DisplayModeProvider.Instance.Modes.Insert(0, new DefaultDisplayMode("Mobile")
            {
                ContextCondition = (context => DeviceDetection.IsMobile)
            });

            GlobalFilters.Filters.Add(new NoCacheActionFilter
            {
                Duration = 1,
                VaryByParam = "none",
                Location = OutputCacheLocation.Client,
                NoStore = true
            });
            //GlobalFilters.Filters.Add(new AppicationVersionValidationActionFilter());
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            GlobalAppSettings.InitializeSystemSettings(GlobalAppSettings.GetConfigFilepath() + ServerSetup.Configuration);

            GlobalAppSettings.IsLatestVersion = DatabaseSchemaUpdater.IsLatestVersion();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (DeviceDetection.IsMobile)
            {
                var segments = HttpContext.Current.Request.Url;
                var isAjaxRequest = new HttpRequestWrapper(Context.Request).IsAjaxRequest();

                if ((!isAjaxRequest) && (HttpContext.Current.Request.RequestType.ToLower() != "post")
                    && (!segments.AbsolutePath.ToLower().Contains("/content/")
                    && !segments.AbsolutePath.ToLower().Contains("/scripts/")
                    && !segments.AbsolutePath.ToLower().Contains("/autodiscover/")
                    && !segments.AbsolutePath.ToLower().Contains("/bundles/")
                    && !HttpContext.Current.Response.IsRequestBeingRedirected
                    && !segments.AbsolutePath.ToLower().Contains("/error/")
                    && !segments.AbsolutePath.ToLower().Contains("/signalr/")
                    && !segments.AbsolutePath.ToLower().Contains("/userprofile/avatar")
                    && !segments.AbsolutePath.ToLower().Contains("/home/signout")
                    /*&& !segments.AbsolutePath.Equals("/")*/) &&   //Issue while sign out from home page and hence commented this. Need to check the purpose of this condition and can be added later.
                   (HttpContext.Current.Request.CurrentExecutionFilePathExtension != ".axd"
                    && HttpContext.Current.Request.CurrentExecutionFilePathExtension != ".ico"
                    && HttpContext.Current.Request.CurrentExecutionFilePathExtension != ".js"
                    && HttpContext.Current.Request.CurrentExecutionFilePathExtension != ".png"
                    && HttpContext.Current.Request.CurrentExecutionFilePathExtension != ".jpg"
                    && HttpContext.Current.Request.CurrentExecutionFilePathExtension != ".txt"
                    && HttpContext.Current.Request.CurrentExecutionFilePathExtension != ".css"
                    && HttpContext.Current.Request.CurrentExecutionFilePathExtension != ".aspx"
                    && HttpContext.Current.Request.CurrentExecutionFilePathExtension != ".woff"
                    && HttpContext.Current.Request.CurrentExecutionFilePathExtension != ".gif"))
                {
                    var cookie = new HttpCookie("mobile_cookie", Request.Url.AbsolutePath);

                    HttpContext.Current.Response.Cookies.Add(cookie);
                }
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            try
            {
                var exception = Server.GetLastError();

                LogExtension.LogError("Application Error - Server's Last Error", exception,
                    MethodBase.GetCurrentMethod());

                var httpException = exception as HttpException;

                var httpStatusCode = (exception is HttpException) ? httpException.GetHttpCode() : 0;

                var routeData = new RouteData();

                if (httpStatusCode == 404)
                {
                    routeData.Values.Add("controller", "Error");
                    routeData.Values.Add("action", "HttpError404");
                    LogExtension.LogError("Application Error 404", httpException, MethodBase.GetCurrentMethod());
                }
                else
                {
                    routeData.Values.Add("controller", "Error");
                    routeData.Values.Add("action", "HttpError500");
                    LogExtension.LogError("Application Error 500", httpException, MethodBase.GetCurrentMethod());
                }

                Server.ClearError();
                Response.Clear();
                IController errorController = new ErrorController();
                errorController.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
            }
            catch (Exception ex)
            {
                LogExtension.LogError("Error in Application Error ", ex, MethodBase.GetCurrentMethod());
            }
        }
    }
}