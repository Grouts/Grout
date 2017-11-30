using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using Grout.Base.Logger;
using System.Reflection;

namespace Grout.Base
{
    public class MailSender : EmailConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="toAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public void SendEmail(string toAddress, string subject, string body)
        {
            try
            {
                var messageThread = new Thread(SendEmail);

                MailMessage.From = new MailAddress(GlobalAppSettings.SystemSettings.MailSettingsAddress);
                MailMessage.Subject = subject;
                MailMessage.Body = body;
                MailMessage.IsBodyHtml = true;
                MailMessage.To.Add(toAddress);

                messageThread.Start();
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception is thrown while sending Email", e, MethodBase.GetCurrentMethod(), " ToAddress - " + toAddress + " Subject - " + subject + " Body - " + body);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mailMessage"></param>
        public void SendEmail(MailMessage mailMessage)
        {
            var messageThread = new Thread(SendEmail);

            MailMessage = mailMessage;

            messageThread.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        private void SendEmail()
        {
            try
            {
                SmtpClient.Send(MailMessage);                
            }
            catch (Exception ex)
            {
                //Exception has been thrown as this is needed for logging in Schedulers                
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class EmailConfiguration
    {
        protected SmtpClient SmtpClient;
        protected NetworkCredential BasicAuthenticationInfo;
        protected MailMessage MailMessage;

        public EmailConfiguration()
        {
            try
            {
                SmtpClient = new SmtpClient
                {
                    Host = GlobalAppSettings.SystemSettings.MailSettingsHost,
                    Port = GlobalAppSettings.SystemSettings.MailSettingsPort,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(GlobalAppSettings.SystemSettings.MailSettingsAddress,
                        GlobalAppSettings.SystemSettings.MailSettingsPassword),
                    EnableSsl = GlobalAppSettings.SystemSettings.MailSettingsIsSecureAuthentication
                };

                MailMessage = new MailMessage { IsBodyHtml = true };
            }
            catch (Exception e)
            {
                LogExtension.LogError("Exception is thrown while configuring Email", e, MethodBase.GetCurrentMethod(), " Host - " + SmtpClient.Host + " Port - " + SmtpClient.Port + " UseDefaultCredentials - " + SmtpClient.UseDefaultCredentials + " MailSettingsAddress - " + GlobalAppSettings.SystemSettings.MailSettingsAddress + " MailSettingsPassword - " + GlobalAppSettings.SystemSettings.MailSettingsPassword + " EnableSsl - " + GlobalAppSettings.SystemSettings.MailSettingsIsSecureAuthentication);
            }
        }
    }
}