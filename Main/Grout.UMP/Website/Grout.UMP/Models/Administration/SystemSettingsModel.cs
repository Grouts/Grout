using System;
using System.Data;
using System.Linq;
using Grout.Base;
using Grout.Base.DataClasses;
using Grout.Base.Encryption;

namespace Grout.UMP.Models
{
    public class SystemSettingsModel
    {
        public static SystemSettings GetSystemSettings()
        {
            var systemSettingsResult = new GlobalAppSettings().GetSystemSettings().DataTable.AsEnumerable()
                .Select(a => new
                {
                    Key = a.Field<string>(GlobalAppSettings.DbColumns.DB_SystemSettings.Key),
                    Value = a.Field<string>(GlobalAppSettings.DbColumns.DB_SystemSettings.Value)
                }
                ).ToDictionary(a => a.Key, a => a.Value);

            var systemSettings = new SystemSettings
            {
                OrganizationName = systemSettingsResult[SystemSettingKeys.OrganizationName.ToString()],
                LoginLogo = systemSettingsResult[SystemSettingKeys.LoginLogo.ToString()],
                MainScreenLogo = systemSettingsResult[SystemSettingKeys.MainScreenLogo.ToString()],
                FavIcon = systemSettingsResult[SystemSettingKeys.FavIcon.ToString()],
                WelcomeNoteText = systemSettingsResult[SystemSettingKeys.WelcomeNoteText.ToString()],
                Language = systemSettingsResult[SystemSettingKeys.Language.ToString()],
                TimeZone = systemSettingsResult[SystemSettingKeys.TimeZone.ToString()],
                DateFormat = systemSettingsResult[SystemSettingKeys.DateFormat.ToString()],
                BaseUrl = systemSettingsResult[SystemSettingKeys.BaseUrl.ToString()],
                ActivationExpirationDays =
                    Convert.ToInt32(systemSettingsResult[SystemSettingKeys.ActivationExpirationDays.ToString()]),
                MailSettingsAddress = systemSettingsResult[SystemSettingKeys.MailSettingsAddress.ToString()],
                MailSettingsHost = systemSettingsResult[SystemSettingKeys.MailSettingsHost.ToString()],
                MailSettingsSenderName = systemSettingsResult[SystemSettingKeys.MailSettingsSenderName.ToString()],
                MailSettingsPort =Convert.ToInt32(systemSettingsResult[SystemSettingKeys.MailSettingsPort.ToString()])==0?25:Convert.ToInt32(systemSettingsResult[SystemSettingKeys.MailSettingsPort.ToString()]),
                MailSettingsIsSecureAuthentication =
                    Convert.ToBoolean(
                        systemSettingsResult[SystemSettingKeys.MailSettingsIsSecureAuthentication.ToString()])
            };

            return systemSettings;
        }

        public static void UpdateSystemSettings(SystemSettings updatedSystemSettings)
        {
            var tokenCryptography = new TokenCryptography();
            var systemManagement = new SystemManagement();
            var serializer = new SystemSettingsSerializer();

            var systemSettings = serializer.Deserialize(GlobalAppSettings.GetConfigFilepath());

            systemManagement.UpdateSystemSetting(updatedSystemSettings.MailSettingsHost,
                SystemSettingKeys.MailSettingsHost.ToString());
            systemManagement.UpdateSystemSetting(updatedSystemSettings.MailSettingsPort.ToString(),
                SystemSettingKeys.MailSettingsPort.ToString());
            systemManagement.UpdateSystemSetting(updatedSystemSettings.MailSettingsSenderName,
                SystemSettingKeys.MailSettingsSenderName.ToString());
            if (!String.IsNullOrEmpty(updatedSystemSettings.MailSettingsPassword))
                systemManagement.UpdateSystemSetting(
                    tokenCryptography.DoEncryption(updatedSystemSettings.MailSettingsPassword),
                    SystemSettingKeys.MailSettingsPassword.ToString());
            systemManagement.UpdateSystemSetting(updatedSystemSettings.MailSettingsIsSecureAuthentication.ToString(),
                SystemSettingKeys.MailSettingsIsSecureAuthentication.ToString());
            systemManagement.UpdateSystemSetting(updatedSystemSettings.MailSettingsAddress,
                SystemSettingKeys.MailSettingsAddress.ToString());
            systemManagement.UpdateSystemSetting(updatedSystemSettings.OrganizationName,
                SystemSettingKeys.OrganizationName.ToString());
            systemManagement.UpdateSystemSetting(updatedSystemSettings.LoginLogo,
                SystemSettingKeys.LoginLogo.ToString());
            systemManagement.UpdateSystemSetting(updatedSystemSettings.MainScreenLogo,
                SystemSettingKeys.MainScreenLogo.ToString());
            systemManagement.UpdateSystemSetting(updatedSystemSettings.FavIcon,
                SystemSettingKeys.FavIcon.ToString());
            systemManagement.UpdateSystemSetting(updatedSystemSettings.WelcomeNoteText,
                SystemSettingKeys.WelcomeNoteText.ToString());
            systemManagement.UpdateSystemSetting(updatedSystemSettings.DateFormat,
                SystemSettingKeys.DateFormat.ToString());                        
            systemManagement.UpdateSystemSetting(updatedSystemSettings.BaseUrl,
                SystemSettingKeys.BaseUrl.ToString());
            systemManagement.UpdateSystemSetting(updatedSystemSettings.TimeZone,
                SystemSettingKeys.TimeZone.ToString());
        }

        public static string MailSettingsExist()
        {
            try
            {
                if (String.IsNullOrWhiteSpace(GlobalAppSettings.SystemSettings.MailSettingsAddress) != true &&
                    String.IsNullOrWhiteSpace(GlobalAppSettings.SystemSettings.MailSettingsHost) != true &&
                    String.IsNullOrWhiteSpace(GlobalAppSettings.SystemSettings.MailSettingsSenderName) != true &&
                    String.IsNullOrWhiteSpace(GlobalAppSettings.SystemSettings.MailSettingsPort.ToString()) != true &&
                    String.IsNullOrWhiteSpace(GlobalAppSettings.SystemSettings.MailSettingsPassword) != true)
                {
                    return "success";
                }
                return "failure";

            }
            catch (Exception ex)
            {
                return "error";
            }
        }

    }
}