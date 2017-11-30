using Grout.UMP.Handlers;
using System.Web.Http;
using System.Web.Routing;

namespace Grout.UMP
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            RouteTable.Routes.MapHttpRoute(
                name: "ApiLogin",
                routeTemplate: "api/accounts/login",
                defaults: new
                {
                    controller = "accountsapi",
                    action = "login"
                }).RouteHandler = new ApiRouteHandler();

            RouteTable.Routes.MapHttpRoute(
                 name: "ApiChangePassword",
                 routeTemplate: "api/accounts/changepassword",
                 defaults: new
                 {
                     controller = "accountsapi",
                     action = "changepassword"
                 }).RouteHandler = new ApiRouteHandler();

            RouteTable.Routes.MapHttpRoute(
                name: "GetdatasourceById",
                  routeTemplate: "api/reportserverapi/get-data-source",
                  defaults: new
                  {
                      controller = "reportserverapi",
                      action = "getdatasource"
                  }).RouteHandler = new ApiRouteHandler();

            RouteTable.Routes.MapHttpRoute(
                name: "LoginAPI",
                  routeTemplate: "api/reportserverapi/login",
                  defaults: new
                  {
                      controller = "AccountsApi",
                      action = "login"
                  }).RouteHandler = new ApiRouteHandler();

            RouteTable.Routes.MapHttpRoute(
                name: "IsUMPServer",
                  routeTemplate: "api/reportserverapi/isvalid",
                  defaults: new
                  {
                      controller = "AccountsApi",
                      action = "isvalid"
                  }).RouteHandler = new ApiRouteHandler();

            RouteTable.Routes.MapHttpRoute(
               name: "downloaddatasourceByName",
                 routeTemplate: "api/reportserverapi/download-data-source",
                 defaults: new
                 {
                     controller = "reportserverapi",
                     action = "downloaddatasource"
                 }).RouteHandler = new ApiRouteHandler();

            RouteTable.Routes.MapHttpRoute(
                name: "DownloadReport",
                routeTemplate: "api/reportserverapi/download-report",
                defaults: new
                {
                    controller = "reportserverapi",
                    action = "DownloadItem",
                    id = RouteParameter.Optional
                }).RouteHandler = new ApiRouteHandler();

            RouteTable.Routes.MapHttpRoute(
                name: "DownloadFile",
                routeTemplate: "api/reportserverapi/get-file",
                defaults: new
                {
                    controller = "reportserverapi",
                    action = "GetFile",
                    id = RouteParameter.Optional
                }).RouteHandler = new ApiRouteHandler();

            RouteTable.Routes.MapHttpRoute(
                name: "AddReport",
                routeTemplate: "api/reportserverapi/add-report",
                defaults: new
                {
                    controller = "reportserverapi",
                    action = "AddReport",
                    id = RouteParameter.Optional
                }).RouteHandler = new ApiRouteHandler();

            RouteTable.Routes.MapHttpRoute(
                name: "AddDataSource",
                routeTemplate: "api/reportserverapi/add-data-source",
                defaults: new
                {
                    controller = "reportserverapi",
                    action = "AddDataSource",
                    id = RouteParameter.Optional
                }).RouteHandler = new ApiRouteHandler();

            RouteTable.Routes.MapHttpRoute(
                name: "UpdateReport",
                routeTemplate: "api/reportserverapi/edit-report",
                defaults: new
                {
                    controller = "reportserverapi",
                    action = "UpdateReport",
                    id = RouteParameter.Optional
                }).RouteHandler = new ApiRouteHandler();

            RouteTable.Routes.MapHttpRoute(
                name: "UpdateDataSource",
                routeTemplate: "api/reportserverapi/edit-data-source",
                defaults: new
                {
                    controller = "reportserverapi",
                    action = "UpdateDataSource",
                    id = RouteParameter.Optional
                }).RouteHandler = new ApiRouteHandler();

            RouteTable.Routes.MapHttpRoute(
                name: "GetReportById",
                routeTemplate: "api/reportcontrolapi/getreportbyid",
                defaults: new
                {
                    controller = "reportcontrolapi",
                    action = "GetReportById"
                }).RouteHandler = new ApiRouteHandler();

            RouteTable.Routes.MapHttpRoute(
                name: "PostReportAction",
                routeTemplate: "api/reportcontrolapi/postreportaction",
                defaults: new
                    {
                        controller = "reportcontrolapi",
                        action = "PostReportAction"
                    }).RouteHandler = new ApiRouteHandler();

            RouteTable.Routes.MapHttpRoute(
                name: "GetUserItems",
                routeTemplate: "api/reportserverapi/get-items",
                defaults: new
                {
                    controller = "reportserverapi",
                    action = "GetItems"
                }).RouteHandler = new ApiRouteHandler();

            RouteTable.Routes.MapHttpRoute("GetResource",
                "api/reportcontrolapi/getresource",
                new { controller = "reportcontrolapi", action = "getresource" }).RouteHandler = new ApiRouteHandler();
        }
    }
}