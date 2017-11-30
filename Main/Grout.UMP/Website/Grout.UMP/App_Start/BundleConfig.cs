using System.Collections.Generic;
using System.Web.Optimization;

namespace Grout.UMP
{
	public class BundleConfig
	{
		public static void RegisterBundles(BundleCollection bundles)
		{
			var customBundleOrder = new CustomBundleOrderer();

			BundleTable.EnableOptimizations = !System.Diagnostics.Debugger.IsAttached;

			#region Layout page

            bundles.Add(new LessBundle("~/styles/layout") { Orderer = customBundleOrder }.Include(
                "~/Content/Styles/EssentialJS/ej.theme.min.css",
                "~/Content/Styles/EssentialJS/ej.widgets.core.min.css",
                "~/Content/Styles/Bootstrap/bootstrap.min.css",
                "~/Content/Styles/Bootstrap/bootstrap-reportserver.css",
                "~/Content/Styles/font-Server.css",
                "~/Content/Styles/LESS/Master.less"
        ));

			bundles.Add(new ScriptBundle("~/scripts/layout") { Orderer = customBundleOrder }.Include(
                "~/Scripts/jQuery/jquery-1.10.2.min.js",
                "~/Scripts/jQuery/jquery.easing-1.3.min.js",
                "~/Scripts/jQuery/jquery.globalize.min.js",
                "~/Scripts/Bootstrap/respond.min.js",
                "~/Scripts/AngularJS/angular.min.js",
                "~/Scripts/jQuery/jsrender.min.js",
                "~/Scripts/EssentialJS/ej.widget.all.13.2.min.js",
                "~/Scripts/Core/UMP.Core.js",
                "~/Scripts/System/CheckMailSettings.js"
        ));

			#endregion Layout page

            #region Iframe pages

            bundles.Add(new LessBundle("~/styles/iframe") { Orderer = customBundleOrder }.Include(
               "~/Content/Styles/EssentialJS/ej.theme.min.css",
               "~/Content/Styles/EssentialJS/ej.widgets.core.min.css",
               "~/Content/Styles/Bootstrap/bootstrap.min.css",
               "~/Content/Styles/Bootstrap/bootstrap-reportserver.css",
               "~/Content/Styles/font-Server.css",
               "~/Content/Styles/LESS/Master.less"
       ));

			bundles.Add(new ScriptBundle("~/scripts/iframe") { Orderer = customBundleOrder }.Include(
                "~/Scripts/jQuery/jquery-1.10.2.min.js",
                "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/jQuery/jquery.easing-1.3.min.js",
                "~/Scripts/jQuery/jquery.globalize.min.js",
                "~/Scripts/Bootstrap/respond.min.js",
                "~/Scripts/AngularJS/angular.min.js",
                "~/Scripts/EssentialJS/ej.widget.all.13.2.min.js",
                "~/Scripts/Core/UMP.Core.js"
        ));

			#endregion

            #region Login page

            bundles.Add(new LessBundle("~/styles/login") { Orderer = customBundleOrder }.Include(
                "~/Content/Styles/EssentialJS/ej.theme.min.css",
                "~/Content/Styles/EssentialJS/ej.widgets.core.min.css",
                "~/Content/Styles/Bootstrap/bootstrap.min.css",
                "~/Content/Styles/Bootstrap/bootstrap-reportserver.css",
                "~/Content/Styles/font-Server.css",
                "~/Content/Styles/LESS/login.less"
        ));

			bundles.Add(new ScriptBundle("~/scripts/login") { Orderer = customBundleOrder }.Include(
                "~/Scripts/jQuery/jquery-1.10.2.min.js",
                "~/Scripts/EssentialJS/ej.widget.all.13.2.min.js",
                 "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Core/UMP.Core.js",
                "~/Scripts/Accounts/Login.js"));

			#endregion Login page

            #region Forgot password

            bundles.Add(new LessBundle("~/styles/forgot-password") { Orderer = customBundleOrder }.Include(
                "~/Content/Styles/Bootstrap/bootstrap.min.css",
                "~/Content/Styles/LESS/ForgotPassword.less"
        ));

			bundles.Add(new ScriptBundle("~/scripts/forgot-password") { Orderer = customBundleOrder }.Include(
                "~/Scripts/jQuery/jquery-1.10.2.min.js",
                 "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Accounts/ForgotPassword.js"));

			#endregion Forgot password

            #region Account Activation

            bundles.Add(new LessBundle("~/styles/account-activate") { Orderer = customBundleOrder }.Include(
                "~/Content/Styles/Bootstrap/bootstrap.min.css",
                "~/Content/Styles/LESS/AccountActivation.less"
        ));

			bundles.Add(new ScriptBundle("~/scripts/account-activate") { Orderer = customBundleOrder }.Include(
                "~/Scripts/jQuery/jquery-1.10.2.min.js",
                "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Accounts/ForgotPassword.js"));

			#endregion Forgot password

            #region Item pages

            bundles.Add(new LessBundle("~/styles/items") { Orderer = customBundleOrder }.Include(
                "~/Content/Styles/LESS/Items.less",
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/Less/scheduler.less",
                "~/Content/Styles/LESS/Master.less",
               "~/Content/Styles/LESS/GridLayout.less"
        ));

			#region Reports page

            bundles.Add(new ScriptBundle("~/scripts/reports-page") { Orderer = customBundleOrder }.Include(
                "~/Scripts/Items/Items.js",
                "~/Scripts/scheduler/scheduler.js",
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/Report/Report.js",
                "~/Scripts/Category/Category.js"
        ));

			bundles.Add(new LessBundle("~/styles/reports-page") { Orderer = customBundleOrder }.Include(
               "~/Content/Styles/LESS/Items.less",
               "~/Content/Styles/Bootstrap/bootstrap-select.css",
               "~/Content/Styles/Less/scheduler.less",
               "~/Content/Styles/LESS/FixedLeftSectionReports.less",
               "~/Content/Styles/LESS/GridLayout.less"
       ));

			bundles.Add(new LessBundle("~/styles/add-report") { Orderer = customBundleOrder }.Include(
               "~/Content/Styles/Bootstrap/bootstrap.min.css",
               "~/Content/Styles/Bootstrap/bootstrap-reportserver.css",
               "~/Content/Styles/font-Server.css",
               "~/Content/Styles/LESS/Master.less",
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/LESS/Reports.less"
        ));

            bundles.Add(new ScriptBundle("~/scripts/add-report") { Orderer = customBundleOrder }.Include(
                "~/Scripts/jQuery/jquery-1.10.2.min.js",
                "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/Core/UMP.Core.js",
                "~/Scripts//Report/AddRdlReport.js"
        ));

			bundles.Add(new LessBundle("~/styles/select-datasource") { Orderer = customBundleOrder }.Include(
                "~/Content/Styles/EssentialJS/ej.theme.min.css",
                "~/Content/Styles/EssentialJS/ej.widgets.core.min.css",
                "~/Content/Styles/Bootstrap/bootstrap.min.css",
                "~/Content/Styles/Bootstrap/bootstrap-reportserver.css",
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/font-Server.css",
                "~/Content/Styles/LESS/Master.less",
                "~/Content/Styles/LESS/AddDataSource.less",
                "~/Content/Styles/LESS/SelectDataSource.less"
        ));

			bundles.Add(new ScriptBundle("~/scripts/select-datasource") { Orderer = customBundleOrder }.Include(
                "~/Scripts/jQuery/jquery-1.10.2.min.js",
                "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/jQuery/jquery.easing-1.3.min.js",
                "~/Scripts/jQuery/jquery.globalize.min.js",
                "~/Scripts/Bootstrap/respond.min.js",
                "~/Scripts/EssentialJS/ej.widget.all.13.2.min.js",
                "~/Scripts/Core/UMP.Core.js",
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/Report/SelectDataSource.js"
        ));

			bundles.Add(new ScriptBundle("~/scripts/upload-report") { Orderer = customBundleOrder }.Include(
               "~/Scripts/jQuery/jquery-1.10.2.min.js"
            ));

			bundles.Add(new LessBundle("~/styles/upload-report") { Orderer = customBundleOrder }.Include(
                "~/Content/Styles/Bootstrap/bootstrap.min.css",
                "~/Content/Styles/Bootstrap/bootstrap-reportserver.css",
                "~/Content/Styles/LESS/Reports.less"
        ));

			#endregion Reports page

            #region Data Sources page

            bundles.Add(new ScriptBundle("~/scripts/datasource-page") { Orderer = customBundleOrder }.Include(
                "~/Scripts/Items/Items.js",
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/DataSource/DataSource.js"
        ));

            bundles.Add(new LessBundle("~/styles/add-datasource") { Orderer = customBundleOrder }.Include(
                "~/Content/Styles/EssentialJS/ej.theme.min.css",
               "~/Content/Styles/EssentialJS/ej.widgets.core.min.css",
               "~/Content/Styles/Bootstrap/bootstrap.min.css",
               "~/Content/Styles/Bootstrap/bootstrap-reportserver.css",
               "~/Content/Styles/font-Server.css",
               "~/Content/Styles/LESS/Master.less",
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/LESS/AddDataSource.less"
        ));

			bundles.Add(new ScriptBundle("~/scripts/add-datasource") { Orderer = customBundleOrder }.Include(
                "~/Scripts/jQuery/jquery-1.10.2.min.js",
                "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/jQuery/jquery.easing-1.3.min.js",
                "~/Scripts/jQuery/jquery.globalize.min.js",
                "~/Scripts/Bootstrap/respond.min.js",
                "~/Scripts/EssentialJS/ej.widget.all.13.2.min.js",
                "~/Scripts/Core/UMP.Core.js",
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/DataSource/AddDataSource.js"
        ));

			bundles.Add(new ScriptBundle("~/scripts/edit-datasource") { Orderer = customBundleOrder }.Include(
                 "~/Scripts/jQuery/jquery-1.10.2.min.js",
                 "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/jQuery/jquery.easing-1.3.min.js",
                "~/Scripts/jQuery/jquery.globalize.min.js",
                "~/Scripts/Bootstrap/respond.min.js",
                "~/Scripts/EssentialJS/ej.widget.all.13.2.min.js",
                "~/Scripts/Core/UMP.Core.js",
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/DataSource/EditDataSource.js"
        ));

			#endregion Data Sources page

            #region Category page

            bundles.Add(new LessBundle("~/styles/category-page") { Orderer = customBundleOrder }.Include(
                "~/Content/Styles/LESS/Items.less"
        ));

			bundles.Add(new ScriptBundle("~/scripts/category-page") { Orderer = customBundleOrder }.Include(
                "~/Scripts/Items/Items.js",
                "~/Scripts/Category/Category.js"
        ));

            bundles.Add(new LessBundle("~/styles/add-category") { Orderer = customBundleOrder }.Include(
                "~/Content/Styles/Bootstrap/bootstrap.min.css",
                "~/Content/Styles/Bootstrap/bootstrap-reportserver.css",
                "~/Content/Styles/font-Server.css",
                "~/Content/Styles/LESS/Master.less",
                "~/Content/Styles/LESS/CategoryDialog.less"
        ));

			bundles.Add(new ScriptBundle("~/scripts/add-category") { Orderer = customBundleOrder }.Include(
                "~/Scripts/jQuery/jquery-1.10.2.min.js",
                "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/Category/AddCategory.js"
        ));

			bundles.Add(new LessBundle("~/styles/edit-category") { Orderer = customBundleOrder }.Include(
                "~/Content/Styles/Bootstrap/bootstrap.min.css",
                "~/Content/Styles/Bootstrap/bootstrap-reportserver.css",
                "~/Content/Styles/font-Server.css",
                "~/Content/Styles/LESS/Master.less",
                "~/Content/Styles/LESS/CategoryDialog.less"
        ));

			bundles.Add(new ScriptBundle("~/scripts/edit-category") { Orderer = customBundleOrder }.Include(
                 "~/Scripts/jQuery/jquery-1.10.2.min.js",
                 "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/Category/EditCategory.js"
        ));

			#endregion Category page

            #region Files page

            bundles.Add(new LessBundle("~/styles/file-page") { Orderer = customBundleOrder }.Include(
               "~/Content/Styles/LESS/GridLayout.less",
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/LESS/Items.less"
        ));

			bundles.Add(new ScriptBundle("~/scripts/files-page") { Orderer = customBundleOrder }.Include(
                "~/Scripts/Items/Items.js",
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/File/File.js"
         ));

            bundles.Add(new LessBundle("~/styles/add-files") { Orderer = customBundleOrder }.Include(
                "~/Content/Styles/Bootstrap/bootstrap.min.css",
                "~/Content/Styles/Bootstrap/bootstrap-reportserver.css",
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/font-Server.css",
                "~/Content/Styles/LESS/Master.less",
                "~/Content/Styles/LESS/Items.less"
         ));

			bundles.Add(new ScriptBundle("~/scripts/add-files") { Orderer = customBundleOrder }.Include(
                "~/Scripts/jQuery/jquery-1.10.2.min.js",
                "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/File/AddFile.js"
         ));

			bundles.Add(new LessBundle("~/styles/edit-files") { Orderer = customBundleOrder }.Include(
                "~/Content/Styles/Bootstrap/bootstrap.min.css",
                "~/Content/Styles/Bootstrap/bootstrap-reportserver.css",
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/font-Server.css",
                "~/Content/Styles/LESS/Master.less",
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Content/Styles/LESS/Items.less"
         ));

			bundles.Add(new ScriptBundle("~/scripts/edit-files") { Orderer = customBundleOrder }.Include(
                "~/Scripts/jQuery/jquery-1.10.2.min.js",
                "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/Core/UMP.Core.js",
                "~/Scripts/File/EditFile.js"
         ));

			#endregion Files page

            #region Item Move Copy Clone Page

            bundles.Add(new LessBundle("~/Styles/ItemAction").Include(
                "~/Content/Styles/Bootstrap/bootstrap.min.css",
                "~/Content/Styles/Bootstrap/bootstrap-reportserver.css",
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/font-Server.css",
                "~/Content/Styles/LESS/Master.less"
         ));

			bundles.Add(new ScriptBundle("~/Scripts/ItemAction").Include(
                "~/Scripts/jQuery/jquery-1.10.2.min.js",
                 "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/Core/UMP.Core.js",
                "~/Scripts/Items/ItemActions.js"
         ));

			#endregion Item Move Copy Clone Page

            #region Item Version Page

            bundles.Add(new LessBundle("~/Styles/ItemVersion").Include(
                "~/Content/Styles/EssentialJS/ej.theme.min.css",
                "~/Content/Styles/EssentialJS/ej.widgets.core.min.css",
                "~/Content/Styles/Bootstrap/bootstrap.min.css",
                "~/Content/Styles/Bootstrap/bootstrap-reportserver.css",
                "~/Content/Styles/font-Server.css",
                "~/Content/Styles/LESS/Master.less",
                "~/Content/Styles/LESS/Items.less",
                "~/Content/Styles/LESS/GridLayout.less"
         ));

			bundles.Add(new ScriptBundle("~/Scripts/ItemVersion").Include(
                "~/Scripts/jQuery/jquery-1.10.2.min.js",
                "~/Scripts/jQuery/jquery.easing-1.3.min.js",
                "~/Scripts/jQuery/jquery.globalize.min.js",
                "~/Scripts/Bootstrap/respond.min.js",
                "~/Scripts/EssentialJS/ej.widget.all.13.2.min.js",
                "~/Scripts/jQuery/jsrender.min.js",
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Core/UMP.Core.js",
                "~/Scripts/Items/ItemVersion.js"
         ));

			#endregion Item Version Page

            #endregion Item pages

            #region Permission Page

            bundles.Add(new LessBundle("~/Styles/PermissionPage").Include(
                "~/Content/Styles/LESS/Permission.less",
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/LESS/FixedLeftSectionAdminSetting.less",
               "~/Content/Styles/LESS/GridLayout.less"
         ));

			bundles.Add(new ScriptBundle("~/Scripts/PermissionPage").Include(
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/AngularJS/ej.widget.angular.min.js",
                "~/Scripts/Permission/Permission.js"
         ));

			#endregion Permission Page

            #region Add User Permission

            bundles.Add(new LessBundle("~/Styles/AddUserPermission").Include(
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/LESS/Permission.less"
         ));

			bundles.Add(new ScriptBundle("~/Scripts/AddUserPermission").Include(
                   "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/AngularJS/ej.widget.angular.min.js",
                "~/Scripts/Permission/Permission.js"
         ));

			#endregion

            #region Add Group Permission

            bundles.Add(new LessBundle("~/Styles/AddGroupPermission").Include(
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/LESS/Permission.less"
         ));

			bundles.Add(new ScriptBundle("~/Scripts/AddGroupPermission").Include(
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/AngularJS/ej.widget.angular.min.js",
                "~/Scripts/Permission/GroupPermission.js"
         ));

			#endregion

            #region Group Permission Page

            bundles.Add(new LessBundle("~/Styles/GroupPermission").Include(
                "~/Content/Styles/LESS/Permission.less",
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/LESS/FixedLeftSectionAdminSetting.less",
               "~/Content/Styles/LESS/GridLayout.less"
         ));

			bundles.Add(new ScriptBundle("~/Scripts/GroupPermission").Include(
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/AngularJS/ej.widget.angular.min.js",
                "~/Scripts/Permission/GroupPermission.js"
         ));

			#endregion Group Permission Page

            #region Schedules

            bundles.Add(new LessBundle("~/styles/schedules") { Orderer = customBundleOrder }.Include(
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/LESS/GridLayout.less",
                "~/Content/Styles/LESS/Master.less",
                "~/Content/Styles/Less/scheduler.less"));

			bundles.Add(new ScriptBundle("~/scripts/schedules") { Orderer = customBundleOrder }.Include(
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/scheduler/scheduler.js"));

			bundles.Add(new ScriptBundle("~/scripts/scheduleslisting") { Orderer = customBundleOrder }.Include(
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/scheduler/schedulelisting.js",
                "~/Scripts/scheduler/scheduler.js"));

			#endregion

            #region Edit Profile

            bundles.Add(new LessBundle("~/Styles/edit-profile").Include(
                "~/Content/Styles/Jcrop/Jcrop.min.css",
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/LESS/Master.less",
                "~/Content/Styles/LESS/EditProfile.less"
           ));

			bundles.Add(new ScriptBundle("~/Scripts/edit-profile").Include(
                "~/Scripts/Jcrop/Jcrop.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/User/EditProfile.js"
           ));

			bundles.Add(new LessBundle("~/Styles/user-profile").Include(
                "~/Content/Styles/LESS/Master.less",
                "~/Content/Styles/LESS/EditProfile.less"
           ));

			bundles.Add(new ScriptBundle("~/Scripts/user-profile").Include(
                "~/Scripts/Bootstrap/bootstrap.min.js"
           ));

			#endregion Edit Profile

            #region Active directory Users import

            bundles.Add(new LessBundle("~/Styles/user-active-directory").Include(
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/LESS/UserManagement.less",
                "~/Content/Styles/LESS/FixedLeftSectionAdminSetting.less",
               "~/Content/Styles/LESS/GridLayout.less",
               "~/Content/Styles/LESS/ActiveDirectory.less"
           ));

            bundles.Add(new ScriptBundle("~/Scripts/user-active-directory").Include(
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/User/ActiveDirectory.js"
           ));

            #endregion Active directory Users import

            #region Active directory Group import

            bundles.Add(new LessBundle("~/Styles/group-active-directory").Include(
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/LESS/UserManagement.less",
                "~/Content/Styles/LESS/FixedLeftSectionAdminSetting.less",
               "~/Content/Styles/LESS/GridLayout.less",
               "~/Content/Styles/LESS/ActiveDirectory.less"
           ));

            bundles.Add(new ScriptBundle("~/Scripts/group-active-directory").Include(
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/User/ActiveDirectory.js"
           ));

            #endregion Active directory Group import

            #region UserManagement

            bundles.Add(new LessBundle("~/Styles/user-management-module").Include(
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/LESS/UserManagement.less",
                "~/Content/Styles/LESS/FixedLeftSectionAdminSetting.less",
               "~/Content/Styles/LESS/GridLayout.less"
           ));

			bundles.Add(new ScriptBundle("~/Scripts/user-management").Include(
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/User/UserManagement.js"
           ));

            bundles.Add(new LessBundle("~/Styles/synchronize-users").Include(
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/LESS/SynchronizeUsers.less",
                "~/Content/Styles/LESS/FixedLeftSectionAdminSetting.less",
               "~/Content/Styles/LESS/GridLayout.less"
           ));

            bundles.Add(new ScriptBundle("~/Scripts/synchronize-users").Include(
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/User/SynchronizeUsers.js"
           ));

			#endregion UserManagement

            #region UserManagement Profile

            bundles.Add(new LessBundle("~/Styles/user-management-profile").Include(
                "~/Content/Styles/Jcrop/Jcrop.min.css",
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/LESS/UserManagementProfile.less",
                "~/Content/Styles/LESS/Master.less",
                "~/Content/Styles/LESS/FixedLeftSectionAdminSetting.less"
           ));

			bundles.Add(new ScriptBundle("~/Scripts/user-management-profile").Include(
                "~/Scripts/Jcrop/Jcrop.min.js",
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/User/UserManagementProfile.js"
           ));

			#endregion UserManagement Profile

            #region Groups Page

            bundles.Add(new LessBundle("~/styles/groups") { Orderer = customBundleOrder }.Include(
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/LESS/Group.less",
                "~/Content/Styles/LESS/FixedLeftSectionAdminSetting.less",
               "~/Content/Styles/LESS/GridLayout.less"
            ));

			bundles.Add(new ScriptBundle("~/scripts/groups") { Orderer = customBundleOrder }.Include(
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/AngularJS/ej.widget.angular.min.js",
                "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/Group/Group.js"));

			#endregion Groups Page

            #region Administration

            bundles.Add(new LessBundle("~/styles/administration-page") { Orderer = customBundleOrder }.Include(
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/LESS/EditSystemSettings.less",
                "~/Content/Styles/LESS/FixedLeftSectionAdminSetting.less"
            ));

			bundles.Add(new ScriptBundle("~/scripts/administration-page") { Orderer = customBundleOrder }.Include(
                 "~/Scripts/jQuery/jquery.validate.min.js",  
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/Administration/SystemSettings.js"));

			#endregion Administration

            #region System settings

            bundles.Add(new LessBundle("~/styles/system-configuration") { Orderer = customBundleOrder }.Include(
                "~/Content/Styles/Bootstrap/bootstrap.min.css",
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/Bootstrap/bootstrap-reportserver.css",
                "~/Content/Styles/EssentialJS/ej.theme.min.css",
                "~/Content/Styles/EssentialJS/ej.widgets.core.min.css",
                "~/Content/Styles/Core.css",
                "~/Content/Styles/font-Server.css",
                "~/Content/Styles/Less/SystemSettings.less"
               ));

			bundles.Add(new ScriptBundle("~/scripts/system-configuration") { Orderer = customBundleOrder }.Include(
                "~/Scripts/jQuery/jquery-1.10.2.min.js",
                "~/Scripts/jQuery/jquery.easing-1.3.min.js",
                "~/Scripts/jQuery/jquery.globalize.min.js",
                "~/Scripts/EssentialJS/ej.widget.all.13.2.min.js",
                "~/Scripts/jquery/jsrender.min.js",                
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/Core/UMP.Core.js",
                "~/Scripts/System/SystemSettings.js"
               
            ));

			#endregion System settings

            #region Add Group

            bundles.Add(new LessBundle("~/Styles/AddGroup").Include(
                "~/Content/Styles/Bootstrap/bootstrap.min.css",
                "~/Content/Styles/Bootstrap/bootstrap-reportserver.css",
                "~/Content/Styles/font-Server.css",
                "~/Content/Styles/LESS/Master.less",
                 "~/Content/Styles/LESS/Group.less"
            ));

			bundles.Add(new ScriptBundle("~/Scripts/AddGroup").Include(
               "~/Scripts/jQuery/jquery-1.10.2.min.js",
                "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/Core/UMP.Core.js",
                "~/Scripts/Group/Group.js"
            ));

			#endregion

            #region Edit Group

            bundles.Add(new LessBundle("~/Styles/EditGroup").Include(
                "~/Content/Styles/Bootstrap/bootstrap-select.css",
                "~/Content/Styles/LESS/Group.less",
                "~/Content/Styles/LESS/FixedLeftSectionAdminSetting.less",
                "~/Content/Styles/LESS/GridLayout.less"
               ));

			bundles.Add(new ScriptBundle("~/Scripts/EditGroup").Include(
                "~/Scripts/jQuery/jsrender.min.js",
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js",
                "~/Scripts/AngularJS/ej.widget.angular.min.js",
                "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/Group/EditGroup.js"
            ));

			#endregion

            #region Delete Group Confirmation

            bundles.Add(new LessBundle("~/Styles/DeleteConfirm").Include(
                "~/Content/Styles/Bootstrap/bootstrap.min.css",
                "~/Content/Styles/Bootstrap/bootstrap-reportserver.css",
                "~/Content/Styles/font-Server.css",
                "~/Content/Styles/LESS/Master.less",
                "~/Content/Styles/LESS/Group.less"
            ));

			bundles.Add(new ScriptBundle("~/Scripts/DeleteConfirm").Include(
                "~/Scripts/jQuery/jquery-1.10.2.min.js",
                "~/Scripts/jQuery/jquery.easing-1.3.min.js",
                "~/Scripts/Core/UMP.Core.js",
                "~/Scripts/jQuery/jquery.validate.min.js",
                "~/Scripts/Group/Group.js"
            ));

			#endregion

            #region Delete Edit Group Confirmation

            bundles.Add(new LessBundle("~/Styles/EditDelete").Include(
                "~/Content/Styles/Bootstrap/bootstrap.min.css",
                "~/Content/Styles/Bootstrap/bootstrap-reportserver.css",
                "~/Content/Styles/font-Server.css",
                "~/Content/Styles/LESS/Master.less",
                "~/Content/Styles/LESS/Group.less"
            ));

			bundles.Add(new ScriptBundle("~/Scripts/EditDelete").Include(
                "~/Scripts/jQuery/jquery-1.10.2.min.js",
                "~/Scripts/jQuery/jquery.easing-1.3.min.js",
                "~/Scripts/Core/UMP.Core.js",
                "~/Scripts/Group/EditGroup.js"
            ));

			bundles.Add(new ScriptBundle("~/Scripts/DeleteGroupUser").Include(
                "~/Scripts/jQuery/jquery-1.10.2.min.js",
                "~/Scripts/jQuery/jquery.easing-1.3.min.js",
                "~/Scripts/Core/UMP.Core.js",
                "~/Scripts/Group/DeleteGroupUser.js"
            ));

			#endregion
			#region Schedule Dialog
			 bundles.Add(new ScriptBundle("~/Scripts/SchedulerDialog").Include("~/Scripts/scheduler/scheduler.js",
                "~/Scripts/Bootstrap/bootstrap.min.js",
                "~/Scripts/Bootstrap/bootstrap-select.min.js"));
			 bundles.Add(new LessBundle("~/Styles/SchedulerDialog").Include("~/Content/Styles/Less/scheduler.less",
                "~/Content/Styles/Bootstrap/bootstrap-select.css"));
            #endregion
            #region bootstrap
            bundles.Add(new ScriptBundle("~/Scripts/bootstrap").Include("~/Scripts/Bootstrap/bootstrap.min.js"));
            bundles.Add(new ScriptBundle("~/Scripts/bootstrapjs").Include("~/Scripts/Bootstrap/bootstrap.min.js"));
            #endregion

        }

        public class CustomBundleOrderer : IBundleOrderer
        {
            public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
            {
                return files;
            }
        }
    }
}