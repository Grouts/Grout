public class DB_Grout
    {
    public DB_EntityField DB_EntityField { get; set; }

    public DB_EntityFieldMapping DB_EntityFieldMapping { get; set; }

    public DB_DepartmentEntity DB_DepartmentEntity { get; set; }

    public DB_DepartmentPrefix_Entity DB_DepartmentPrefix_Entity { get; set; }

    public DB_ApplicationVersion DB_ApplicationVersion { get; set; }

    public DB_User DB_User { get; set; }

    public DB_Group DB_Group { get; set; }

    public DB_UserGroup DB_UserGroup { get; set; }

    public DB_UserLogType DB_UserLogType { get; set; }

    public DB_UserLog DB_UserLog { get; set; }

    public DB_UserLogin DB_UserLogin { get; set; }

    public DB_UserPreference DB_UserPreference { get; set; }

    public DB_SystemLogType DB_SystemLogType { get; set; }

    public DB_SystemLog DB_SystemLog { get; set; }

    public DB_SystemSettings DB_SystemSettings { get; set; }

    public DB_Department DB_Department { get; set; }

    public DB_Project DB_Project { get; set; }

    public DB_Team DB_Team { get; set; }

    public DB_TeamGroup DB_TeamGroup { get; set; }

    public DB_TeamUser DB_TeamUser { get; set; }

    public DB_ProjectTeam DB_ProjectTeam { get; set; }

    public DB_Role DB_Role { get; set; }

    public DB_GroupRole DB_GroupRole { get; set; }

    public DB_UserRole DB_UserRole { get; set; }

    public DB_Entity DB_Entity { get; set; }

    public DB_EntityFieldType DB_EntityFieldType { get; set; }
public DB_Grout()
    {this.DB_EntityField= new DB_EntityField { DB_TableName = "EntityField", EntityFieldId="EntityFieldId",EntityFieldTypeId="EntityFieldTypeId",Name="Name",Description="Description",CreatedBy="CreatedBy",ModifiedBy="ModifiedBy",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",};this.DB_EntityFieldMapping= new DB_EntityFieldMapping { DB_TableName = "EntityFieldMapping", EntityFieldMappingId="EntityFieldMappingId",EntityId="EntityId",EntityFieldId="EntityFieldId",CreatedBy="CreatedBy",ModifiedBy="ModifiedBy",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",};this.DB_DepartmentEntity= new DB_DepartmentEntity { DB_TableName = "DepartmentEntity", Id="Id",EntityId="EntityId",DepartmentId="DepartmentId",CreatedBy="CreatedBy",ModifiedBy="ModifiedBy",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",};this.DB_DepartmentPrefix_Entity= new DB_DepartmentPrefix_Entity { DB_TableName = "DepartmentPrefix_Entity", DepartmentPrefix_ItemTypeId="DepartmentPrefix_ItemTypeId",FieldsOfEntity="FieldsOfEntity",ProjectId="ProjectId",};this.DB_ApplicationVersion= new DB_ApplicationVersion { DB_TableName = "ApplicationVersion", VersionNumber="VersionNumber",};this.DB_User= new DB_User { DB_TableName = "User", UserId="UserId",UserName="UserName",FirstName="FirstName",LastName="LastName",DisplayName="DisplayName",Email="Email",Password="Password",Contact="Contact",Picture="Picture",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",LastLogin="LastLogin",ActivationExpirationDate="ActivationExpirationDate",ActivationCode="ActivationCode",ResetPasswordCode="ResetPasswordCode",LastResetAttempt="LastResetAttempt",IsActivated="IsActivated",IsActive="IsActive",IsDeleted="IsDeleted",};this.DB_Group= new DB_Group { DB_TableName = "Group", GroupId="GroupId",Name="Name",Description="Description",Color="Color",CreatedBy="CreatedBy",ModifiedBy="ModifiedBy",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",};this.DB_UserGroup= new DB_UserGroup { DB_TableName = "UserGroup", UserGroupId="UserGroupId",GroupId="GroupId",UserId="UserId",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",};this.DB_UserLogType= new DB_UserLogType { DB_TableName = "UserLogType", UserLogTypeId="UserLogTypeId",Name="Name",IsActive="IsActive",};this.DB_UserLog= new DB_UserLog { DB_TableName = "UserLog", UserLogId="UserLogId",UserLogTypeId="UserLogTypeId",GroupId="GroupId",OldValue="OldValue",NewValue="NewValue",UpdatedUserId="UpdatedUserId",TargetUserId="TargetUserId",ModifiedDate="ModifiedDate",IsActive="IsActive",};this.DB_UserLogin= new DB_UserLogin { DB_TableName = "UserLogin", UserLoginId="UserLoginId",UserId="UserId",ClientToken="ClientToken",IpAddress="IpAddress",LoggedInTime="LoggedInTime",IsActive="IsActive",};this.DB_UserPreference= new DB_UserPreference { DB_TableName = "UserPreference", UserPreferenceId="UserPreferenceId",UserId="UserId",Language="Language",TimeZone="TimeZone",RecordSize="RecordSize",ItemSort="ItemSort",ItemFilters="ItemFilters",Notifications="Notifications",ModifiedDate="ModifiedDate",IsActive="IsActive",};this.DB_SystemLogType= new DB_SystemLogType { DB_TableName = "SystemLogType", SystemLogTypeId="SystemLogTypeId",Name="Name",IsActive="IsActive",};this.DB_SystemLog= new DB_SystemLog { DB_TableName = "SystemLog", LogId="LogId",SystemLogTypeId="SystemLogTypeId",UpdatedUserId="UpdatedUserId",TargetUserId="TargetUserId",ModifiedDate="ModifiedDate",IsActive="IsActive",};this.DB_SystemSettings= new DB_SystemSettings { DB_TableName = "SystemSettings", SystemSettingsId="SystemSettingsId",Key="Key",Value="Value",CreatedBy="CreatedBy",ModifiedBy="ModifiedBy",ModifiedDate="ModifiedDate",IsActive="IsActive",};this.DB_Department= new DB_Department { DB_TableName = "Department", DepartmentId="DepartmentId",Name="Name",Description="Description",CreatedBy="CreatedBy",ModifiedBy="ModifiedBy",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",};this.DB_Project= new DB_Project { DB_TableName = "Project", ProjectId="ProjectId",DepartmentId="DepartmentId",Name="Name",Description="Description",Logo="Logo",CreatedBy="CreatedBy",ModifiedBy="ModifiedBy",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",StartDate="StartDate",EndDate="EndDate",IsActive="IsActive",};this.DB_Team= new DB_Team { DB_TableName = "Team", TeamId="TeamId",Name="Name",Description="Description",Color="Color",Logo="Logo",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",};this.DB_TeamGroup= new DB_TeamGroup { DB_TableName = "TeamGroup", TeamGroupId="TeamGroupId",GroupId="GroupId",TeamId="TeamId",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",};this.DB_TeamUser= new DB_TeamUser { DB_TableName = "TeamUser", TeamUserId="TeamUserId",UserId="UserId",TeamId="TeamId",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",};this.DB_ProjectTeam= new DB_ProjectTeam { DB_TableName = "ProjectTeam", ProjectTeamId="ProjectTeamId",ProjectId="ProjectId",TeamId="TeamId",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",};this.DB_Role= new DB_Role { DB_TableName = "Role", RoleId="RoleId",Name="Name",Description="Description",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",};this.DB_GroupRole= new DB_GroupRole { DB_TableName = "GroupRole", GroupRoleId="GroupRoleId",GroupId="GroupId",RoleId="RoleId",TeamId="TeamId",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",};this.DB_UserRole= new DB_UserRole { DB_TableName = "UserRole", UserRoleId="UserRoleId",UserId="UserId",RoleId="RoleId",TeamId="TeamId",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",};this.DB_Entity= new DB_Entity { DB_TableName = "Entity", EntityId="EntityId",Name="Name",Description="Description",Logo="Logo",CreatedBy="CreatedBy",ModifiedBy="ModifiedBy",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",};this.DB_EntityFieldType= new DB_EntityFieldType { DB_TableName = "EntityFieldType", EntityFieldTypeId="EntityFieldTypeId",Name="Name",Description="Description",CreatedBy="CreatedBy",ModifiedBy="ModifiedBy",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",};}}public class DB_EntityField
    { public string DB_TableName { get; set; }
    public string EntityFieldId { get; set; }

    public string EntityFieldTypeId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DB_EntityFieldMapping
    { public string DB_TableName { get; set; }
    public string EntityFieldMappingId { get; set; }

    public string EntityId { get; set; }

    public string EntityFieldId { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DB_DepartmentEntity
    { public string DB_TableName { get; set; }
    public string Id { get; set; }

    public string EntityId { get; set; }

    public string DepartmentId { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DB_DepartmentPrefix_Entity
    { public string DB_TableName { get; set; }
    public string DepartmentPrefix_ItemTypeId { get; set; }

    public string FieldsOfEntity { get; set; }

    public string ProjectId { get; set; }

}public class DB_ApplicationVersion
    { public string DB_TableName { get; set; }
    public string VersionNumber { get; set; }

}public class DB_User
    { public string DB_TableName { get; set; }
    public string UserId { get; set; }

    public string UserName { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string DisplayName { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public string Contact { get; set; }

    public string Picture { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string LastLogin { get; set; }

    public string ActivationExpirationDate { get; set; }

    public string ActivationCode { get; set; }

    public string ResetPasswordCode { get; set; }

    public string LastResetAttempt { get; set; }

    public string IsActivated { get; set; }

    public string IsActive { get; set; }

    public string IsDeleted { get; set; }

}public class DB_Group
    { public string DB_TableName { get; set; }
    public string GroupId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Color { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DB_UserGroup
    { public string DB_TableName { get; set; }
    public string UserGroupId { get; set; }

    public string GroupId { get; set; }

    public string UserId { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DB_UserLogType
    { public string DB_TableName { get; set; }
    public string UserLogTypeId { get; set; }

    public string Name { get; set; }

    public string IsActive { get; set; }

}public class DB_UserLog
    { public string DB_TableName { get; set; }
    public string UserLogId { get; set; }

    public string UserLogTypeId { get; set; }

    public string GroupId { get; set; }

    public string OldValue { get; set; }

    public string NewValue { get; set; }

    public string UpdatedUserId { get; set; }

    public string TargetUserId { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DB_UserLogin
    { public string DB_TableName { get; set; }
    public string UserLoginId { get; set; }

    public string UserId { get; set; }

    public string ClientToken { get; set; }

    public string IpAddress { get; set; }

    public string LoggedInTime { get; set; }

    public string IsActive { get; set; }

}public class DB_UserPreference
    { public string DB_TableName { get; set; }
    public string UserPreferenceId { get; set; }

    public string UserId { get; set; }

    public string Language { get; set; }

    public string TimeZone { get; set; }

    public string RecordSize { get; set; }

    public string ItemSort { get; set; }

    public string ItemFilters { get; set; }

    public string Notifications { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DB_SystemLogType
    { public string DB_TableName { get; set; }
    public string SystemLogTypeId { get; set; }

    public string Name { get; set; }

    public string IsActive { get; set; }

}public class DB_SystemLog
    { public string DB_TableName { get; set; }
    public string LogId { get; set; }

    public string SystemLogTypeId { get; set; }

    public string UpdatedUserId { get; set; }

    public string TargetUserId { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DB_SystemSettings
    { public string DB_TableName { get; set; }
    public string SystemSettingsId { get; set; }

    public string Key { get; set; }

    public string Value { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DB_Department
    { public string DB_TableName { get; set; }
    public string DepartmentId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DB_Project
    { public string DB_TableName { get; set; }
    public string ProjectId { get; set; }

    public string DepartmentId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Logo { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string StartDate { get; set; }

    public string EndDate { get; set; }

    public string IsActive { get; set; }

}public class DB_Team
    { public string DB_TableName { get; set; }
    public string TeamId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Color { get; set; }

    public string Logo { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DB_TeamGroup
    { public string DB_TableName { get; set; }
    public string TeamGroupId { get; set; }

    public string GroupId { get; set; }

    public string TeamId { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DB_TeamUser
    { public string DB_TableName { get; set; }
    public string TeamUserId { get; set; }

    public string UserId { get; set; }

    public string TeamId { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DB_ProjectTeam
    { public string DB_TableName { get; set; }
    public string ProjectTeamId { get; set; }

    public string ProjectId { get; set; }

    public string TeamId { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DB_Role
    { public string DB_TableName { get; set; }
    public string RoleId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DB_GroupRole
    { public string DB_TableName { get; set; }
    public string GroupRoleId { get; set; }

    public string GroupId { get; set; }

    public string RoleId { get; set; }

    public string TeamId { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DB_UserRole
    { public string DB_TableName { get; set; }
    public string UserRoleId { get; set; }

    public string UserId { get; set; }

    public string RoleId { get; set; }

    public string TeamId { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DB_Entity
    { public string DB_TableName { get; set; }
    public string EntityId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Logo { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DB_EntityFieldType
    { public string DB_TableName { get; set; }
    public string EntityFieldTypeId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}
