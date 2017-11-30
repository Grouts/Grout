using System;
using System.IO;
using System.Xml.Serialization;

namespace Grout.Base
{
    public class PublishSettingsSerializer
    {
        /// <summary>
        /// Saves the publish settings in disk as xml file
        /// </summary>
        /// <param name="settingsObject">PublishSettings class object</param>
        /// <param name="path">Path to save the settings</param>
        public static void SavePublishSettings(PublishSettings settingsObject, string path)
        {
            var xmlSerializer = new XmlSerializer(typeof(PublishSettings));
            try
            {
                if (settingsObject != null)
                {
                    using (Stream fileStream = File.Create(path))
                    {
                        xmlSerializer.Serialize(fileStream, settingsObject);
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
        public static PublishSettings GetPublishSettings(string path)
        {
            var xmlSerializer = new XmlSerializer(typeof(PublishSettings));
            try
            {
                using (Stream fileStream = File.OpenRead(path))
                {
                    return (PublishSettings)xmlSerializer.Deserialize(fileStream);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}