using System;

namespace Grout.Base.DataClasses
{
    [Serializable]
    public class SystemSettings
    {
        public DataBaseConfiguration SqlConfiguration { get; set; }

        public string OrganizationName { get; set; }

        public string ApplicationKey { get; set; }

        public string LoginLogo { get; set; }

        public string MainScreenLogo { get; set; }

        public string FavIcon { get; set; }

        public string WelcomeNoteText { get; set; }

        public string Language { get; set; }

        public string TimeZone { get; set; }

        public string DateFormat { get; set; }

        public int ActivationExpirationDays { get; set; }

        public int ReportCount { get; set; }

        public string BaseUrl { get; set; }

        public string MailSettingsAddress { get; set; }

        public string MailSettingsPassword { get; set; }

        public string MailSettingsHost { get; set; }

        public string MailSettingsSenderName { get; set; }

        public int MailSettingsPort { get; set; }

        public bool MailSettingsIsSecureAuthentication { get; set; }
    
    }
}
