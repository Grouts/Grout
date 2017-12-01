public class DB_Grout
    {
    public EntityField EntityField { get; set; }

    public EntityFieldMapping EntityFieldMapping { get; set; }

    public DepartmentEntity DepartmentEntity { get; set; }

    public DepartmentPrefix_Entity DepartmentPrefix_Entity { get; set; }

    public User User { get; set; }

    public Group Group { get; set; }

    public UserGroup UserGroup { get; set; }

    public UserLogType UserLogType { get; set; }

    public UserLog UserLog { get; set; }

    public UserLogin UserLogin { get; set; }

    public UserPreference UserPreference { get; set; }

    public SystemLogType SystemLogType { get; set; }

    public SystemLog SystemLog { get; set; }

    public SystemSettings SystemSettings { get; set; }

    public Department Department { get; set; }

    public Project Project { get; set; }

    public Team Team { get; set; }

    public TeamGroup TeamGroup { get; set; }

    public TeamUser TeamUser { get; set; }

    public ProjectTeam ProjectTeam { get; set; }

    public Role Role { get; set; }

    public GroupRole GroupRole { get; set; }

    public UserRole UserRole { get; set; }

    public Entity Entity { get; set; }

    public EntityFieldType EntityFieldType { get; set; }

}public class EntityField
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

}public class EntityFieldMapping
    {
    public string EntityFieldMappingId { get; set; }

    public string EntityId { get; set; }

    public string EntityFieldId { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DepartmentEntity
    {
    public string Id { get; set; }

    public string EntityId { get; set; }

    public string DepartmentId { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class DepartmentPrefix_Entity
    {
    public string DepartmentPrefix_ItemTypeId { get; set; }

    public string FieldsOfEntity { get; set; }

    public string ProjectId { get; set; }

}public class User
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

}public class Group
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

}public class UserGroup
    {
    public string UserGroupId { get; set; }

    public string GroupId { get; set; }

    public string UserId { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class UserLogType
    {
    public string UserLogTypeId { get; set; }

    public string Name { get; set; }

    public string IsActive { get; set; }

}public class UserLog
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

}public class UserLogin
    {
    public string UserLoginId { get; set; }

    public string UserId { get; set; }

    public string ClientToken { get; set; }

    public string IpAddress { get; set; }

    public string LoggedInTime { get; set; }

    public string IsActive { get; set; }

}public class UserPreference
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

}public class SystemLogType
    {
    public string SystemLogTypeId { get; set; }

    public string Name { get; set; }

    public string IsActive { get; set; }

}public class SystemLog
    {
    public string LogId { get; set; }

    public string SystemLogTypeId { get; set; }

    public string UpdatedUserId { get; set; }

    public string TargetUserId { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class SystemSettings
    {
    public string SystemSettingsId { get; set; }

    public string Key { get; set; }

    public string Value { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Department
    {
    public string DepartmentId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Project
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

}public class Team
    {
    public string TeamId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Color { get; set; }

    public string Logo { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class TeamGroup
    {
    public string TeamGroupId { get; set; }

    public string GroupId { get; set; }

    public string TeamId { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class TeamUser
    {
    public string TeamUserId { get; set; }

    public string UserId { get; set; }

    public string TeamId { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class ProjectTeam
    {
    public string ProjectTeamId { get; set; }

    public string ProjectId { get; set; }

    public string TeamId { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Role
    {
    public string RoleId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class GroupRole
    {
    public string GroupRoleId { get; set; }

    public string GroupId { get; set; }

    public string RoleId { get; set; }

    public string TeamId { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class UserRole
    {
    public string UserRoleId { get; set; }

    public string UserId { get; set; }

    public string RoleId { get; set; }

    public string TeamId { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}public class Entity
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

}public class EntityFieldType
    {
    public string EntityFieldTypeId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public string CreatedDate { get; set; }

    public string ModifiedDate { get; set; }

    public string IsActive { get; set; }

}DB_Grout dbGrout = new DB_Grout
    {EntityField= new EntityField { EntityFieldId="EntityFieldId",EntityFieldTypeId="EntityFieldTypeId",Name="Name",Description="Description",CreatedBy="CreatedBy",ModifiedBy="ModifiedBy",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",},EntityFieldMapping= new EntityFieldMapping { EntityFieldMappingId="EntityFieldMappingId",EntityId="EntityId",EntityFieldId="EntityFieldId",CreatedBy="CreatedBy",ModifiedBy="ModifiedBy",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",},DepartmentEntity= new DepartmentEntity { Id="Id",EntityId="EntityId",DepartmentId="DepartmentId",CreatedBy="CreatedBy",ModifiedBy="ModifiedBy",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",},DepartmentPrefix_Entity= new DepartmentPrefix_Entity { DepartmentPrefix_ItemTypeId="DepartmentPrefix_ItemTypeId",FieldsOfEntity="FieldsOfEntity",ProjectId="ProjectId",},User= new User { UserId="UserId",UserName="UserName",FirstName="FirstName",LastName="LastName",DisplayName="DisplayName",Email="Email",Password="Password",Contact="Contact",Picture="Picture",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",LastLogin="LastLogin",ActivationExpirationDate="ActivationExpirationDate",ActivationCode="ActivationCode",ResetPasswordCode="ResetPasswordCode",LastResetAttempt="LastResetAttempt",IsActivated="IsActivated",IsActive="IsActive",IsDeleted="IsDeleted",},Group= new Group { GroupId="GroupId",Name="Name",Description="Description",Color="Color",CreatedBy="CreatedBy",ModifiedBy="ModifiedBy",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",},UserGroup= new UserGroup { UserGroupId="UserGroupId",GroupId="GroupId",UserId="UserId",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",},UserLogType= new UserLogType { UserLogTypeId="UserLogTypeId",Name="Name",IsActive="IsActive",},UserLog= new UserLog { UserLogId="UserLogId",UserLogTypeId="UserLogTypeId",GroupId="GroupId",OldValue="OldValue",NewValue="NewValue",UpdatedUserId="UpdatedUserId",TargetUserId="TargetUserId",ModifiedDate="ModifiedDate",IsActive="IsActive",},UserLogin= new UserLogin { UserLoginId="UserLoginId",UserId="UserId",ClientToken="ClientToken",IpAddress="IpAddress",LoggedInTime="LoggedInTime",IsActive="IsActive",},UserPreference= new UserPreference { UserPreferenceId="UserPreferenceId",UserId="UserId",Language="Language",TimeZone="TimeZone",RecordSize="RecordSize",ItemSort="ItemSort",ItemFilters="ItemFilters",Notifications="Notifications",ModifiedDate="ModifiedDate",IsActive="IsActive",},SystemLogType= new SystemLogType { SystemLogTypeId="SystemLogTypeId",Name="Name",IsActive="IsActive",},SystemLog= new SystemLog { LogId="LogId",SystemLogTypeId="SystemLogTypeId",UpdatedUserId="UpdatedUserId",TargetUserId="TargetUserId",ModifiedDate="ModifiedDate",IsActive="IsActive",},SystemSettings= new SystemSettings { SystemSettingsId="SystemSettingsId",Key="Key",Value="Value",CreatedBy="CreatedBy",ModifiedBy="ModifiedBy",ModifiedDate="ModifiedDate",IsActive="IsActive",},Department= new Department { DepartmentId="DepartmentId",Name="Name",Description="Description",CreatedBy="CreatedBy",ModifiedBy="ModifiedBy",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",},Project= new Project { ProjectId="ProjectId",DepartmentId="DepartmentId",Name="Name",Description="Description",Logo="Logo",CreatedBy="CreatedBy",ModifiedBy="ModifiedBy",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",StartDate="StartDate",EndDate="EndDate",IsActive="IsActive",},Team= new Team { TeamId="TeamId",Name="Name",Description="Description",Color="Color",Logo="Logo",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",},TeamGroup= new TeamGroup { TeamGroupId="TeamGroupId",GroupId="GroupId",TeamId="TeamId",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",},TeamUser= new TeamUser { TeamUserId="TeamUserId",UserId="UserId",TeamId="TeamId",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",},ProjectTeam= new ProjectTeam { ProjectTeamId="ProjectTeamId",ProjectId="ProjectId",TeamId="TeamId",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",},Role= new Role { RoleId="RoleId",Name="Name",Description="Description",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",},GroupRole= new GroupRole { GroupRoleId="GroupRoleId",GroupId="GroupId",RoleId="RoleId",TeamId="TeamId",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",},UserRole= new UserRole { UserRoleId="UserRoleId",UserId="UserId",RoleId="RoleId",TeamId="TeamId",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",},Entity= new Entity { EntityId="EntityId",Name="Name",Description="Description",Logo="Logo",CreatedBy="CreatedBy",ModifiedBy="ModifiedBy",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",},EntityFieldType= new EntityFieldType { EntityFieldTypeId="EntityFieldTypeId",Name="Name",Description="Description",CreatedBy="CreatedBy",ModifiedBy="ModifiedBy",CreatedDate="CreatedDate",ModifiedDate="ModifiedDate",IsActive="IsActive",},};