public class Db_Grout
{
    public Db_EntityField Db_EntityField { get; set; }

    public Db_EntityFieldMapping Db_EntityFieldMapping { get; set; }

    public Db_DepartmentEntity Db_DepartmentEntity { get; set; }

    public Db_DepartmentPrefix_Entity Db_DepartmentPrefix_Entity { get; set; }

    public Db_User Db_User { get; set; }

    public Db_Group Db_Group { get; set; }

    public Db_UserGroup Db_UserGroup { get; set; }

    public Db_UserLogType Db_UserLogType { get; set; }

    public Db_UserLog Db_UserLog { get; set; }

    public Db_UserLogin Db_UserLogin { get; set; }

    public Db_UserPreference Db_UserPreference { get; set; }

    public Db_SystemLogType Db_SystemLogType { get; set; }

    public Db_SystemLog Db_SystemLog { get; set; }

    public Db_SystemSettings Db_SystemSettings { get; set; }

    public Db_Department Db_Department { get; set; }

    public Db_Project Db_Project { get; set; }

    public Db_Team Db_Team { get; set; }

    public Db_TeamGroup Db_TeamGroup { get; set; }

    public Db_TeamUser Db_TeamUser { get; set; }

    public Db_ProjectTeam Db_ProjectTeam { get; set; }

    public Db_Role Db_Role { get; set; }

    public Db_GroupRole Db_GroupRole { get; set; }

    public Db_UserRole Db_UserRole { get; set; }

    public Db_Entity Db_Entity { get; set; }

    public Db_EntityFieldType Db_EntityFieldType { get; set; }
    public Db_Grout()
    {
        this.Db_EntityField = new Db_EntityField { EntityFieldId = "EntityFieldId", EntityFieldTypeId = "EntityFieldTypeId", Name = "Name", Description = "Description", CreatedBy = "CreatedBy", ModifiedBy = "ModifiedBy", CreatedDate = "CreatedDate", ModifiedDate = "ModifiedDate", IsActive = "IsActive", }; this.Db_EntityFieldMapping = new Db_EntityFieldMapping { EntityFieldMappingId = "EntityFieldMappingId", EntityId = "EntityId", EntityFieldId = "EntityFieldId", CreatedBy = "CreatedBy", ModifiedBy = "ModifiedBy", CreatedDate = "CreatedDate", ModifiedDate = "ModifiedDate", IsActive = "IsActive", }; this.Db_DepartmentEntity = new Db_DepartmentEntity { Id = "Id", EntityId = "EntityId", DepartmentId = "DepartmentId", CreatedBy = "CreatedBy", ModifiedBy = "ModifiedBy", CreatedDate = "CreatedDate", ModifiedDate = "ModifiedDate", IsActive = "IsActive", }; this.Db_DepartmentPrefix_Entity = new Db_DepartmentPrefix_Entity { DepartmentPrefix_ItemTypeId = "DepartmentPrefix_ItemTypeId", FieldsOfEntity = "FieldsOfEntity", ProjectId = "ProjectId", }; this.Db_User = new Db_User { UserId = "UserId", UserName = "UserName", FirstName = "FirstName", LastName = "LastName", DisplayName = "DisplayName", Email = "Email", Password = "Password", Contact = "Contact", Picture = "Picture", CreatedDate = "CreatedDate", ModifiedDate = "ModifiedDate", LastLogin = "LastLogin", ActivationExpirationDate = "ActivationExpirationDate", ActivationCode = "ActivationCode", ResetPasswordCode = "ResetPasswordCode", LastResetAttempt = "LastResetAttempt", IsActivated = "IsActivated", IsActive = "IsActive", IsDeleted = "IsDeleted", }; this.Db_Group = new Db_Group { GroupId = "GroupId", Name = "Name", Description = "Description", Color = "Color", CreatedBy = "CreatedBy", ModifiedBy = "ModifiedBy", CreatedDate = "CreatedDate", ModifiedDate = "ModifiedDate", IsActive = "IsActive", }; this.Db_UserGroup = new Db_UserGroup { UserGroupId = "UserGroupId", GroupId = "GroupId", UserId = "UserId", CreatedDate = "CreatedDate", ModifiedDate = "ModifiedDate", IsActive = "IsActive", }; this.Db_UserLogType = new Db_UserLogType { UserLogTypeId = "UserLogTypeId", Name = "Name", IsActive = "IsActive", }; this.Db_UserLog = new Db_UserLog { UserLogId = "UserLogId", UserLogTypeId = "UserLogTypeId", GroupId = "GroupId", OldValue = "OldValue", NewValue = "NewValue", UpdatedUserId = "UpdatedUserId", TargetUserId = "TargetUserId", ModifiedDate = "ModifiedDate", IsActive = "IsActive", }; this.Db_UserLogin = new Db_UserLogin { UserLoginId = "UserLoginId", UserId = "UserId", ClientToken = "ClientToken", IpAddress = "IpAddress", LoggedInTime = "LoggedInTime", IsActive = "IsActive", }; this.Db_UserPreference = new Db_UserPreference { UserPreferenceId = "UserPreferenceId", UserId = "UserId", Language = "Language", TimeZone = "TimeZone", RecordSize = "RecordSize", ItemSort = "ItemSort", ItemFilters = "ItemFilters", Notifications = "Notifications", ModifiedDate = "ModifiedDate", IsActive = "IsActive", }; this.Db_SystemLogType = new Db_SystemLogType { SystemLogTypeId = "SystemLogTypeId", Name = "Name", IsActive = "IsActive", }; this.Db_SystemLog = new Db_SystemLog { LogId = "LogId", SystemLogTypeId = "SystemLogTypeId", UpdatedUserId = "UpdatedUserId", TargetUserId = "TargetUserId", ModifiedDate = "ModifiedDate", IsActive = "IsActive", }; this.Db_SystemSettings = new Db_SystemSettings { SystemSettingsId = "SystemSettingsId", Key = "Key", Value = "Value", CreatedBy = "CreatedBy", ModifiedBy = "ModifiedBy", ModifiedDate = "ModifiedDate", IsActive = "IsActive", }; this.Db_Department = new Db_Department { DepartmentId = "DepartmentId", Name = "Name", Description = "Description", CreatedBy = "CreatedBy", ModifiedBy = "ModifiedBy", CreatedDate = "CreatedDate", ModifiedDate = "ModifiedDate", IsActive = "IsActive", }; this.Db_Project = new Db_Project { ProjectId = "ProjectId", DepartmentId = "DepartmentId", Name = "Name", Description = "Description", Logo = "Logo", CreatedBy = "CreatedBy", ModifiedBy = "ModifiedBy", CreatedDate = "CreatedDate", ModifiedDate = "ModifiedDate", StartDate = "StartDate", EndDate = "EndDate", IsActive = "IsActive", }; this.Db_Team = new Db_Team { TeamId = "TeamId", Name = "Name", Description = "Description", Color = "Color", Logo = "Logo", CreatedDate = "CreatedDate", ModifiedDate = "ModifiedDate", IsActive = "IsActive", }; this.Db_TeamGroup = new Db_TeamGroup { TeamGroupId = "TeamGroupId", GroupId = "GroupId", TeamId = "TeamId", CreatedDate = "CreatedDate", ModifiedDate = "ModifiedDate", IsActive = "IsActive", }; this.Db_TeamUser = new Db_TeamUser { TeamUserId = "TeamUserId", UserId = "UserId", TeamId = "TeamId", CreatedDate = "CreatedDate", ModifiedDate = "ModifiedDate", IsActive = "IsActive", }; this.Db_ProjectTeam = new Db_ProjectTeam { ProjectTeamId = "ProjectTeamId", ProjectId = "ProjectId", TeamId = "TeamId", CreatedDate = "CreatedDate", ModifiedDate = "ModifiedDate", IsActive = "IsActive", }; this.Db_Role = new Db_Role { RoleId = "RoleId", Name = "Name", Description = "Description", CreatedDate = "CreatedDate", ModifiedDate = "ModifiedDate", IsActive = "IsActive", }; this.Db_GroupRole = new Db_GroupRole { GroupRoleId = "GroupRoleId", GroupId = "GroupId", RoleId = "RoleId", TeamId = "TeamId", CreatedDate = "CreatedDate", ModifiedDate = "ModifiedDate", IsActive = "IsActive", }; this.Db_UserRole = new Db_UserRole { UserRoleId = "UserRoleId", UserId = "UserId", RoleId = "RoleId", TeamId = "TeamId", CreatedDate = "CreatedDate", ModifiedDate = "ModifiedDate", IsActive = "IsActive", }; this.Db_Entity = new Db_Entity { EntityId = "EntityId", Name = "Name", Description = "Description", Logo = "Logo", CreatedBy = "CreatedBy", ModifiedBy = "ModifiedBy", CreatedDate = "CreatedDate", ModifiedDate = "ModifiedDate", IsActive = "IsActive", }; this.Db_EntityFieldType = new Db_EntityFieldType { EntityFieldTypeId = "EntityFieldTypeId", Name = "Name", Description = "Description", CreatedBy = "CreatedBy", ModifiedBy = "ModifiedBy", CreatedDate = "CreatedDate", ModifiedDate = "ModifiedDate", IsActive = "IsActive", };
    }
}

public class Db_EntityField
    {
    public string EntityFieldId { get; set; }

    public string EntityFieldTypeId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Db_EntityFieldMapping
    {
    public string EntityFieldMappingId { get; set; }

    public string EntityId { get; set; }

    public string EntityFieldId { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Db_DepartmentEntity
    {
    public string Id { get; set; }

    public string EntityId { get; set; }

    public string DepartmentId { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Db_DepartmentPrefix_Entity
    {
    public string DepartmentPrefix_ItemTypeId { get; set; }

    public string FieldsOfEntity { get; set; }

    public string ProjectId { get; set; }

}public class Db_User
    {
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

}public class Db_Group
    {
    public string GroupId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Color { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Db_UserGroup
    {
    public string UserGroupId { get; set; }

    public string GroupId { get; set; }

    public string UserId { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Db_UserLogType
    {
    public string UserLogTypeId { get; set; }

    public string Name { get; set; }

    public string IsActive { get; set; }

}public class Db_UserLog
    {
    public string UserLogId { get; set; }

    public string UserLogTypeId { get; set; }

    public string GroupId { get; set; }

    public string OldValue { get; set; }

    public string NewValue { get; set; }

    public string UpdatedUserId { get; set; }

    public string TargetUserId { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Db_UserLogin
    {
    public string UserLoginId { get; set; }

    public string UserId { get; set; }

    public string ClientToken { get; set; }

    public string IpAddress { get; set; }

    public string LoggedInTime { get; set; }

    public string IsActive { get; set; }

}public class Db_UserPreference
    {
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

}public class Db_SystemLogType
    {
    public string SystemLogTypeId { get; set; }

    public string Name { get; set; }

    public string IsActive { get; set; }

}public class Db_SystemLog
    {
    public string LogId { get; set; }

    public string SystemLogTypeId { get; set; }

    public string UpdatedUserId { get; set; }

    public string TargetUserId { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Db_SystemSettings
    {
    public string SystemSettingsId { get; set; }

    public string Key { get; set; }

    public string Value { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Db_Department
    {
    public string DepartmentId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Db_Project
    {
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

}public class Db_Team
    {
    public string TeamId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Color { get; set; }

    public string Logo { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Db_TeamGroup
    {
    public string TeamGroupId { get; set; }

    public string GroupId { get; set; }

    public string TeamId { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Db_TeamUser
    {
    public string TeamUserId { get; set; }

    public string UserId { get; set; }

    public string TeamId { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Db_ProjectTeam
    {
    public string ProjectTeamId { get; set; }

    public string ProjectId { get; set; }

    public string TeamId { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Db_Role
    {
    public string RoleId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Db_GroupRole
    {
    public string GroupRoleId { get; set; }

    public string GroupId { get; set; }

    public string RoleId { get; set; }

    public string TeamId { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Db_UserRole
    {
    public string UserRoleId { get; set; }

    public string UserId { get; set; }

    public string RoleId { get; set; }

    public string TeamId { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Db_Entity
    {
    public string EntityId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Logo { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Db_EntityFieldType
    {
    public string EntityFieldTypeId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}
