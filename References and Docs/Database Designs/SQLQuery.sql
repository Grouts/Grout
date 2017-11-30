CREATE TABLE [Grout_User](
	[UserId] [int] IDENTITY(1,1) primary key NOT NULL,
	[UserName] [nvarchar](100) NOT NULL,
	[FirstName] [nvarchar](255) NOT NULL,
	[LastName] [nvarchar](255) NULL,
	[DisplayName] [nvarchar](512) NULL,
	[Email] [nvarchar](255) NOT NULL,
	[Password] [nvarchar](255) NOT NULL,
	[Contact] [nvarchar](20) NULL,
	[Picture] [nvarchar](100) NOT NULL,	
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NULL,
	[LastLogin] [datetime] NULL,
	[ActivationExpirationDate] [datetime] NULL,
	[ActivationCode] [nvarchar](255) NOT NULL,
	[ResetPasswordCode] [nvarchar](255) NULL,
	[LastResetAttempt] [datetime] NULL,
	[IsActivated] [bit] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDeleted] [bit] NOT NULL)
;

CREATE TABLE [Grout_Group](
	[GroupId] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](1026) NULL,
	[Color] [nvarchar](255) NOT NULL DEFAULT 'White',
	[CreatedBy] [INT] NOT NULL,
	[ModifiedBy] [INT] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;

ALTER TABLE [Grout_Group]  ADD FOREIGN KEY([CreatedBy]) REFERENCES [Grout_User] ([UserId])
;
ALTER TABLE [Grout_Group]  ADD FOREIGN KEY([ModifiedBy]) REFERENCES [Grout_User] ([UserId])
;

CREATE TABLE [Grout_UserGroup](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[GroupId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;

ALTER TABLE [Grout_UserGroup]  ADD FOREIGN KEY([GroupId]) REFERENCES [Grout_Group] ([GroupId])
;
ALTER TABLE [Grout_UserGroup]  ADD FOREIGN KEY([UserId]) REFERENCES [Grout_User] ([UserId])
;

CREATE TABLE [Grout_UserLogType](
	[UserLogTypeId] [int] IDENTITY(1,1) primary key NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[IsActive] [bit] NOT NULL)
;

CREATE TABLE [Grout_UserLog](
	[Id] [int] IDENTITY(1,1) primary key NOT NULL,
	[UserLogTypeId] [int] NOT NULL,	
	[GroupId] [int] NULL,
	[OldValue] [int] NULL,
	[NewValue] [int] NULL,
	[UpdatedUserId] [int] NOT NULL,
	[TargetUserId] [int] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;

ALTER TABLE [Grout_UserLog]  ADD  FOREIGN KEY([UserLogTypeId]) REFERENCES [Grout_UserLogType] ([UserLogTypeId])
;
ALTER TABLE [Grout_UserLog]  ADD  FOREIGN KEY([GroupId]) REFERENCES [Grout_Group] ([GroupId])
;
ALTER TABLE [Grout_UserLog]  ADD  FOREIGN KEY([TargetUserId]) REFERENCES [Grout_User] ([UserId])
;
ALTER TABLE [Grout_UserLog]  ADD  FOREIGN KEY([UpdatedUserId]) REFERENCES [Grout_User] ([UserId])
;


CREATE TABLE [Grout_UserLogin](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[UserId] [int] NOT NULL,
	[ClientToken] [nvarchar](4000) NOT NULL,
	[IpAddress] [nvarchar](50) NOT NULL,
	[LoggedInTime] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;


ALTER TABLE [Grout_UserLogin]  ADD FOREIGN KEY([UserId]) REFERENCES [Grout_User] ([UserId])
;

CREATE TABLE [Grout_UserPreference](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[UserId] [int] NOT NULL,
	[Language] [nvarchar](4000) NULL,
	[TimeZone] [nvarchar](100) NULL,
	[RecordSize] [int] NULL,
	[ItemSort] [nvarchar](4000) NULL,
	[ItemFilters] [nvarchar](4000) NULL,
	[Notifications] [nvarchar](4000) NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;

ALTER TABLE [Grout_UserPreference] ADD FOREIGN KEY([UserId]) REFERENCES [Grout_User] ([UserId])
;

CREATE TABLE [Grout_SystemLogType](
	[SystemLogTypeId] [int] IDENTITY(1,1) primary key NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[IsActive] [bit] NOT NULL)
;

CREATE TABLE [Grout_SystemLog](
	[LogId] [int] IDENTITY(1,1) primary key NOT NULL,
	[SystemLogTypeId] [int] NOT NULL,
	[UpdatedUserId] [int] NOT NULL,
	[TargetUserId] [int] NOT NULL,		
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] bit NOT NULL)
;

ALTER TABLE [Grout_SystemLog]  ADD FOREIGN KEY([SystemLogTypeId]) REFERENCES [Grout_SystemLogType] ([SystemLogTypeId])
;
ALTER TABLE [Grout_SystemLog]  ADD FOREIGN KEY([UpdatedUserId]) REFERENCES [Grout_User] ([UserId])
;
ALTER TABLE [Grout_SystemLog]  ADD FOREIGN KEY([TargetUserId]) REFERENCES [Grout_User] ([UserId])
;

CREATE TABLE [Grout_SystemSettings](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[Key] [nvarchar](255) NOT NULL,
	[Value] [nvarchar](4000) NULL,
	[CreatedBy] [INT] NOT NULL,
	[ModifiedBy] [INT] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;

ALTER TABLE [Grout_Entity]  ADD FOREIGN KEY([CreatedBy]) REFERENCES [Grout_User] ([UserId])
;
ALTER TABLE [Grout_Entity]  ADD FOREIGN KEY([ModifiedBy]) REFERENCES [Grout_User] ([UserId])
;


CREATE TABLE [Grout_ProjectType](
	[ProjectTypeId] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](1026) NULL,
	[CreatedBy] [INT] NOT NULL,
	[ModifiedBy] [INT] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;

CREATE TABLE [Grout_Project](
	[ProjectId] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[ProjectTypeId] [int] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](1026) NULL,
	[Logo] [nvarchar](255) NOT NULL DEFAULT 'White',
	[CreatedBy] [INT] NOT NULL,
	[ModifiedBy] [INT] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[IsActive] [bit] NOT NULL)
;

ALTER TABLE [Grout_Project]  ADD FOREIGN KEY([ProjectTypeId]) REFERENCES [Grout_ProjectType] ([ProjectTypeId])
;


CREATE TABLE [Grout_Team](
	[TeamId] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](1026) NULL,
	[Color] [nvarchar](255) NOT NULL DEFAULT 'White',
	[Logo] [nvarchar](255) NOT NULL DEFAULT 'White',
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;

CREATE TABLE [Grout_TeamGroup](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[GroupId] [int] NOT NULL,
	[TeamId] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;

ALTER TABLE [Grout_TeamGroup]  ADD FOREIGN KEY([GroupId]) REFERENCES [Grout_Group] ([GroupId])
;
ALTER TABLE [Grout_TeamGroup]  ADD FOREIGN KEY([TeamId]) REFERENCES [Grout_Team] ([TeamId])
;

CREATE TABLE [Grout_TeamUser](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[UserId] [int] NOT NULL,
	[TeamId] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;

ALTER TABLE [Grout_TeamUser]  ADD FOREIGN KEY([UserId]) REFERENCES [Grout_User] ([UserId])
;
ALTER TABLE [Grout_TeamUser]  ADD FOREIGN KEY([TeamId]) REFERENCES [Grout_Team] ([TeamId])
;

CREATE TABLE [Grout_ProjectTeam](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[ProjectId] [int] NOT NULL,
	[TeamId] [int] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;

ALTER TABLE [Grout_ProjectTeam]  ADD FOREIGN KEY([TeamId]) REFERENCES [Grout_Team] ([TeamId])
;
ALTER TABLE [Grout_ProjectTeam]  ADD FOREIGN KEY([ProjectId]) REFERENCES [Grout_Project] ([ProjectId])
;

CREATE TABLE [Grout_Role](
	[RoleId] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](1026) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;

CREATE TABLE [Grout_GroupRole](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[GroupId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
	[TeamId] [int] NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;

ALTER TABLE [Grout_GroupRole]  ADD FOREIGN KEY([GroupId]) REFERENCES [Grout_Group] ([GroupId])
;
ALTER TABLE [Grout_GroupRole]  ADD FOREIGN KEY([RoleId]) REFERENCES [Grout_Role] ([RoleId])
;
ALTER TABLE [Grout_GroupRole]  ADD FOREIGN KEY([TeamId]) REFERENCES [Grout_Team] ([TeamId])
;

CREATE TABLE [Grout_UserRole](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[UserId] [int] NOT NULL,
	[RoleId] [int] NOT NULL,
	[TeamId] [int] NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;

ALTER TABLE [Grout_UserRole]  ADD FOREIGN KEY([UserId]) REFERENCES [Grout_User] ([UserId])
;
ALTER TABLE [Grout_UserRole]  ADD FOREIGN KEY([RoleId]) REFERENCES [Grout_Role] ([RoleId])
;
ALTER TABLE [Grout_UserRole]  ADD FOREIGN KEY([TeamId]) REFERENCES [Grout_Team] ([TeamId])
;

--story, task , defect
CREATE TABLE [Grout_Entity](
	[EntityId] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](1026) NULL,
	[Logo] [nvarchar](255) NOT NULL DEFAULT 'White',
	[CreatedBy] [INT] NOT NULL,
	[ModifiedBy] [INT] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;

ALTER TABLE [Grout_Entity]  ADD FOREIGN KEY([CreatedBy]) REFERENCES [Grout_User] ([UserId])
;
ALTER TABLE [Grout_Entity]  ADD FOREIGN KEY([ModifiedBy]) REFERENCES [Grout_User] ([UserId])
;

-- user, role, team, project, group, date
CREATE TABLE [Grout_EntityFieldType](
	[EntityFieldTypeId] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](1026) NULL,
	[CreatedBy] [INT] NOT NULL,
	[ModifiedBy] [INT] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;

ALTER TABLE [Grout_EntityFieldType]  ADD FOREIGN KEY([CreatedBy]) REFERENCES [Grout_User] ([UserId])
;
ALTER TABLE [Grout_EntityFieldType]  ADD FOREIGN KEY([ModifiedBy]) REFERENCES [Grout_User] ([UserId])
;

-- name, description, storypoints, due date
CREATE TABLE [Grout_EntityField](
	[EntityFieldId] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[EntityFieldTypeId] [int] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](1026) NULL,
	[CreatedBy] [INT] NOT NULL,
	[ModifiedBy] [INT] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;

ALTER TABLE [Grout_EntityField]  ADD FOREIGN KEY([EntityFieldTypeId]) REFERENCES [Grout_EntityFieldType] ([EntityFieldTypeId])
;
ALTER TABLE [Grout_EntityField]  ADD FOREIGN KEY([CreatedBy]) REFERENCES [Grout_User] ([UserId])
;
ALTER TABLE [Grout_EntityField]  ADD FOREIGN KEY([ModifiedBy]) REFERENCES [Grout_User] ([UserId])
;

--mapping
CREATE TABLE [Grout_EntityFieldMapping](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[EntityId] (INT) NOT NULL,
	[EntityFieldId] (INT) NOT NULL,
	[CreatedBy] [INT] NOT NULL,
	[ModifiedBy] [INT] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;

ALTER TABLE [Grout_EntityFieldMapping]  ADD FOREIGN KEY([EntityId]) REFERENCES [Grout_Entity] ([EntityId])
;

ALTER TABLE [Grout_EntityFieldMapping]  ADD FOREIGN KEY([EntityFieldId]) REFERENCES [Grout_EntityField] ([EntityFieldId])
;

ALTER TABLE [Grout_EntityFieldMapping]  ADD FOREIGN KEY([CreatedBy]) REFERENCES [Grout_User] ([UserId])
;
ALTER TABLE [Grout_EntityFieldMapping]  ADD FOREIGN KEY([ModifiedBy]) REFERENCES [Grout_User] ([UserId])
;



--mapping
CREATE TABLE [Grout_ProjectEntity](
	[Id] [int] IDENTITY(1,1) PRIMARY KEY NOT NULL,
	[EntityId] (INT) NOT NULL,
	[ProjectTypeId] (INT) NOT NULL,
	[CreatedBy] [INT] NOT NULL,
	[ModifiedBy] [INT] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NOT NULL,
	[IsActive] [bit] NOT NULL)
;

ALTER TABLE [Grout_ProjectEntity]  ADD FOREIGN KEY([EntityId]) REFERENCES [Grout_Entity] ([EntityId])
;

ALTER TABLE [Grout_ProjectEntity]  ADD FOREIGN KEY([ProjectTypeId]) REFERENCES [Grout_ProjectType] ([ProjectTypeId])
;

ALTER TABLE [Grout_ProjectEntity]  ADD FOREIGN KEY([CreatedBy]) REFERENCES [Grout_User] ([UserId])
;
ALTER TABLE [Grout_ProjectEntity]  ADD FOREIGN KEY([ModifiedBy]) REFERENCES [Grout_User] ([UserId])
;

INSERT into [Grout_SystemLogType] (Name,IsActive) VALUES (N'Updated',1)
;

INSERT into [Grout_UserLogType] (Name,IsActive) VALUES ( N'Added',1)
;
INSERT into [Grout_UserLogType] (Name,IsActive) VALUES ( N'Updated',1)
;
INSERT into [Grout_UserLogType] (Name,IsActive) VALUES ( N'Deleted',1)
;
INSERT into [Grout_UserLogType] (Name,IsActive) VALUES ( N'Changed',1)
;

INSERT into [Grout_Group] (Name,Description,Color, CreatedDate,ModifiedDate,IsActive) VALUES (N'System Administrator','Has administrative rights for the dashboard server','#ff0000',GETDATE(),GETDATE(), 1)
;