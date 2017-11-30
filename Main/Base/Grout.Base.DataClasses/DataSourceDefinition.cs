using System;
using System.Xml.Serialization;

namespace Grout.Base.DataClasses
{
    [Serializable]
    public class DataSourceDefinition
    {
        private string extensionField;
        private string connectStringField;
        private bool useOriginalConnectStringField;
        private bool originalConnectStringExpressionBasedField;
        private CredentialRetrievalEnum credentialRetrievalField;
        private bool windowsCredentialsFieldSpecified;
        private bool windowsCredentialsField;
        private bool impersonateUserField;
        private bool impersonateUserFieldSpecified;
        private string promptField;
        private string userNameField;
        private string passwordField;
        private bool enabledField;
        private bool enabledFieldSpecified;
        public string Extension
        {
            get
            {
                return extensionField;
            }
            set
            {
                extensionField = value;
            }
        }
        public string ConnectString
        {
            get
            {
                return connectStringField;
            }
            set
            {
                connectStringField = value;
            }
        }
        public bool UseOriginalConnectString
        {
            get
            {
                return useOriginalConnectStringField;
            }
            set
            {
                useOriginalConnectStringField = value;
            }
        }
        public bool OriginalConnectStringExpressionBased
        {
            get
            {
                return originalConnectStringExpressionBasedField;
            }
            set
            {
                originalConnectStringExpressionBasedField = value;
            }
        }
        public CredentialRetrievalEnum CredentialRetrieval
        {
            get
            {
                return credentialRetrievalField;
            }
            set
            {
                credentialRetrievalField = value;
            }
        }
        public bool WindowsCredentials
        {
            get
            {
                return windowsCredentialsField;
            }
            set
            {
                windowsCredentialsField = value;
            }
        }
        [XmlIgnore()]
        public bool WindowsCredentialsSpecified
        {
            get
            {
                return windowsCredentialsFieldSpecified;
            }
            set
            {
                windowsCredentialsFieldSpecified = value;
            }
        }
        public bool ImpersonateUser
        {
            get
            {
                return impersonateUserField;
            }
            set
            {
                impersonateUserField = value;
            }
        }
        [XmlIgnore()]
        public bool ImpersonateUserSpecified
        {
            get
            {
                return impersonateUserFieldSpecified;
            }
            set
            {
                impersonateUserFieldSpecified = value;
            }
        }
        public string Prompt
        {
            get
            {
                return promptField;
            }
            set
            {
                promptField = value;
            }
        }
        public string UserName
        {
            get
            {
                return userNameField;
            }
            set
            {
                userNameField = value;
            }
        }
        public string Password
        {
            get
            {
                return passwordField;
            }
            set
            {
                passwordField = value;
            }
        }
        public bool Enabled
        {
            get { return enabledField; }
            set
            {
                enabledField = Convert.ToBoolean(value.ToString().ToLower());
            }
        }
        [XmlIgnore()]
        public bool EnabledSpecified
        {
            get
            {
                return enabledFieldSpecified;
            }
            set
            {
                enabledFieldSpecified = value;
            }
        }
    }

    public enum CredentialRetrievalEnum
    {
        Prompt,
        Store,
        Integrated,
        None,
    }
}
