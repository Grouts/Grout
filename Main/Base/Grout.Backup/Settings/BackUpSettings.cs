using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Syncfusion.UMP.Backup.Base
{
    public class BackUpSettings
    {
        /// <summary>
        /// Saves the publish settings in disk as xml file
        /// </summary>
        /// <param name="settingsObject">PublishSettings class object</param>
        /// <param name="path">Path to save the settings</param>
        public static void SaveBackupSettings(BackUpandRestoreSetting settingsObject, string path)
        {
            try
            {
                if (settingsObject != null)
                {
                    using (Stream fileStream = File.Create(path))
                    {
                        GetSerializer().Serialize(fileStream, settingsObject);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Gets the publish settings from disk
        /// </summary>
        /// <param name="path">Path of the settings file in disk</param>
        public static BackUpandRestoreSetting GetBackupSettings(string path)
        {
            try
            {
                using (Stream fileStream = File.OpenRead(path))
                {
                    return (BackUpandRestoreSetting)GetSerializer().Deserialize(fileStream);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Returns an instance of xmlserializer class
        /// </summary>
        /// <returns>Returns an instance of xmlserializer class</returns>
        private static XmlSerializer GetSerializer()
        {
            return new XmlSerializer(typeof(BackUpandRestoreSetting));
        }
    }
}
