using System;
using System.Reflection;
using System.Text;
using log4net;

namespace Grout.Base.Logger
{
    public static class LogExtension
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void LogInfo(string message, MethodBase methodType, string optionaldata = "")
        {            
            var nameSpace = String.Empty;
            var methodName = String.Empty;

            optionaldata = Convert.ToBoolean(System.Configuration.ConfigurationSettings.AppSettings["IsLogOptionalData"]) ? optionaldata : String.Empty;

            if (methodType != null)
            {
                methodName = !String.IsNullOrWhiteSpace(methodType.Name) ? methodType.Name : String.Empty;

                if (methodType.DeclaringType != null)
                {
                    nameSpace = !String.IsNullOrWhiteSpace(methodType.DeclaringType.FullName)
                        ? methodType.DeclaringType.FullName
                        : String.Empty;
                }
            }

            var messageBuilder = new StringBuilder();
            messageBuilder.Append(nameSpace + "\t" + methodName + "\t" + "\t" + message + optionaldata);
            Log.Info(messageBuilder);
        }

        public static void LogError(string message, Exception exception, MethodBase methodType, string optionaldata = "")
        {
            var nameSpace = String.Empty;
            var methodName = String.Empty;

            optionaldata = Convert.ToBoolean(System.Configuration.ConfigurationSettings.AppSettings["IsLogOptionalData"]) ? optionaldata : String.Empty;

            if (methodType != null)
            {
                methodName = !String.IsNullOrWhiteSpace(methodType.Name) ? methodType.Name : String.Empty;

                if (methodType.DeclaringType != null)
                {
                    nameSpace = !String.IsNullOrWhiteSpace(methodType.DeclaringType.FullName)
                        ? methodType.DeclaringType.FullName
                        : String.Empty;
                }
            }

            var messageBuilder = new StringBuilder();
            messageBuilder.Append(nameSpace + "\t" + methodName + "\t" + "\t" + message + optionaldata);
            Log.Error(messageBuilder, exception);
        }
    }
}
