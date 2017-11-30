namespace Grout.Base
{
    public class EnumClass
    {
        public enum ItemLogType
        {
            Added = 1,
            Shared = 2,
            Edited = 3,
            Deleted = 4,
            Moved = 5,
            Copied = 6,
            Cloned = 7,
            Favoured = 8,
            Trashed = 9,
            Restored = 10,
            Rollbacked = 11,
            UnFavoured = 12
        }

        public enum SystemSettingKeys
        {
            OrganizationName,
            LoginLogo,
            MainScreenLogo,
            FavIcon,
            WelcomeNoteText,
            Language,
            TimeZone,
            DateFormat,
            Sharing,
            CreateFolders,
            ActivationExpirationDays,
            ReportCount,
            BaseUrl,
            ApiUrl,
            MailSettingsAddress,
            MailSettingsPassword,
            MailSettingsHost,
            MailSettingsSenderName,
            MailSettingsPort,
            MailSettingsIsSecureAuthentication
        }

        public enum ReportFolderLogType
        {
            Added = 1,
        }

        public enum SystemLogType
        {
            Updated = 1,
        }

        public enum UserLogType
        {
            Added = 1,
            Updated = 2,
            Deleted = 3,
            Changed = 4,
        }

        public enum DatabaseLogType
        {
            Restored = 1,
            BackedUp = 2,
        }

        public enum FileTypes
        {
            Dashboard = 1,
            Report = 2,
            Datasource = 3,
            Dataset = 4,
            File = 5,
            Schedule = 6,
            Folder = 7
        }

        public enum PermissionList
        {
            FolderAdmin = 1,
            FileCreate = 2,
            FileEdit = 3,
            FileMove = 4,
            FileClone = 5,
            FileCopy = 6,
            FileDelete = 7,
            FileShare = 8
        }

        public enum UploadImageTypes
        {
            LoginLogo = 1,
            MainScreenLogo = 2,
            Favicon = 3,
            ProfilePicture = 4
        }

        public enum ExportFormatTypes
        {
            Excel = 1,
            HTML = 2,
            PDF = 3,
            Word = 4
        }
    }
}