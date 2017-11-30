using System.Web.Mvc;
using System.Web.Routing;

namespace Grout.UMP
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });
            routes.IgnoreRoute("{*exe}", new { exe = @".*\.exe(/.*)?" });

            routes.MapRoute("Startup",
                "startup",
                new { controller = "SystemStartUpPage", action = "startup" });

            routes.MapRoute("Content",
                "content/images/{arg1}/{arg2}/{arg3}/{arg4}",
                new
                {
                    controller = "FileDownload",
                    action = "ContentRedirection",
                    arg1 = "",
                    arg2 = "",
                    arg3 = "",
                    arg4 = ""
                });

            routes.MapRoute(
               "Login",
               "login",
               new { controller = "Accounts", action = "Login" }
               );

            routes.MapRoute(
               "ForgotPassword",
               "accounts/forgot-password",
               new { controller = "Accounts", action = "ForgotPassword" }
               );

            routes.MapRoute(
               "AccountActivation",
               "accounts/activate",
               new { controller = "Accounts", action = "AccountActivation" }
               );

            routes.MapRoute(
               "ForgotPasswordConfirmation",
               "accounts/forgot-password/code-confirmation",
               new { controller = "Accounts", action = "CodeConfirmation" }
               );

            routes.MapRoute(
               "ForgotPasswordChange",
               "accounts/forgot-password/change-password",
               new { controller = "Accounts", action = "ChangePassword" }
               );

            routes.MapRoute(
                "Categories",
                "categories",
                new { controller = "Category", action = "Category" }
                );

            routes.MapRoute(
                "ItemsOfCategories",
                "categories/{id}",
                new { controller = "Category", action = "ItemsOfCategory" }
                );

            routes.MapRoute(
                "Reports",
                "reports",
                new { controller = "Reports", action = "Reports" }
                );

            routes.MapRoute(
                "Data-Sources",
                "data-sources",
                new { controller = "DataSources", action = "DataSources" }
                );

            routes.MapRoute(
                "Files",
                "files",
                new { controller = "Files", action = "Files" }
                );

            routes.MapRoute(
                "ReportRender",
                "reports/view",
                new { controller = "FileRender", action = "Index" }
                );

            routes.MapRoute(
               "ItemDownload",
               "items/download",
               new { controller = "FileRender", action = "Download" }
               );

            routes.MapRoute(
                "Administration",
                "administration",
                new { controller = "Administration", action = "Administration" }
                );

            routes.MapRoute(
                "SiteAdministration",
                "administration/site",
                new { controller = "Administration", action = "Administration" }
                );

            routes.MapRoute(
                "EmailSettingsAdministration",
                "administration/e-mail-settings",
                new { controller = "Administration", action = "Administration" }
                );

            routes.MapRoute(
                "ActiveDirectorySettingsAdministration",
                "administration/active-directory-settings",
                new { controller = "Administration", action = "Administration" }
                );

            routes.MapRoute(
                "User-Profile",
                "profile",
                new { controller = "User", action = "Profile" }
                );

            routes.MapRoute(
                "Edit-Profile",
                "profile/edit",
                new { controller = "User", action = "EditProfile" }
                );

            routes.MapRoute(
                "Change-password",
                "profile/change-password",
                new { controller = "User", action = "EditPassword" }
                );

            routes.MapRoute(
                "schedules",
                "schedules",
                new { controller = "Scheduler", action = "schedules" }
                );

            routes.MapRoute(
                 "UserManagementEditing",
                 "administration/user-management/users/edit",
                 new { controller = "UserManagement", action = "UserManagement" }
            );

            routes.MapRoute(
                "UserManagement",
                "administration/user-management",
                new { controller = "UserManagement", action = "index" }
                );

            routes.MapRoute(
                 "UserManagementListing",
                 "administration/user-management/users",
                 new { controller = "UserManagement", action = "index" }
            );
			routes.MapRoute(
                 "SyncActiveDirectoryGroup",
                 "administration/user-management/groups/active-directory/synchronize",
                 new { controller = "Group", action = "ActiveDirectoryGroup" }
            );
            routes.MapRoute(
                 "ActiveDirectoryUserImport",
                 "administration/user-management/users/import-active-directory",
                 new { controller = "UserManagement", action = "ActiveDirectoryUserImport" }
            );

            routes.MapRoute(
                 "ActiveDirectoryGroupImport",
                 "administration/user-management/groups/import-active-directory",
                 new { controller = "Group", action = "ActiveDirectoryGroupImport" }
            );

            routes.MapRoute(
                 "SynchronizeUsers",
                 "administration/user-management/users/active-directory/synchronize",
                 new { controller = "UserManagement", action = "SynchronizeUsers" }
            );

            routes.MapRoute(
                 "UserCSVupload",
                 "administration/user-management/users/import-csv",
                 new { controller = "UserManagement", action = "UploadFileFormAction" }
            );

            routes.MapRoute(
                "DownloadTemplate",
                "administration/user-management/users/download/csvtemplate",
                 new { controller = "UserManagement", action = "DownloadTemplate" }
                );

            routes.MapRoute(
                "GroupManagement",
                "administration/user-management/groups",
                new { controller = "group", action = "group" }
                );
            routes.MapRoute(
                "groupEdit",
                "administration/user-management/groups/edit",
                new { controller = "group", action = "editgroup" }
                );
            routes.MapRoute(
                "groupPermissionEdit",
                "administration/permissions/groups/edit",
                new { controller = "permission", action = "grouppermission" }
                );

            routes.MapRoute(
                "userPermissionEdit",
                "administration/permissions/users/edit",
                new { controller = "permission", action = "userpermission" }
                );

            routes.MapRoute(
                "Default",
                "",
                new { controller = "Reports", action = "Reports", id = UrlParameter.Optional });

            routes.RouteExistingFiles = true;

            //routes.Add(
            //    new LowercaseRoute("{controller}/{action}/{id}",
            //    new RouteValueDictionary(new { controller = "Reports", action = "Reports", id = UrlParameter.Optional }),
            //    new MvcRouteHandler()));
        }
    }
}