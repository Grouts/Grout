using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Grout.Base.DataClasses;
using Grout.Base.Logger;

namespace Grout.Base
{
    public class SystemSettingsSerializer
    {
        /// <summary>
        /// XML Seralize
        /// </summary>
        /// <param name="obj">Obj is SystemSetting properties</param>
        /// <param name="path">Path which is used for store the xml file</param>
        public void Serialize(SystemSettings obj, String path)
        {
            var xmlserializer = new XmlSerializer(typeof(SystemSettings));
            using (TextWriter writer = new StreamWriter(path))
            {
                xmlserializer.Serialize(writer, obj);
                writer.Close();
            }
            LogExtension.LogInfo("Created config.xml with connection properties", MethodBase.GetCurrentMethod());
        }

        /// <summary>
        /// XML Deseralize
        /// </summary>
        /// <param name="path">Path which is used for retrieve the xml file location</param>
        /// <returns></returns>
        public SystemSettings Deserialize(string path)
        {
            SystemSettings data = null;
            var xmlSerializer = new XmlSerializer(typeof(SystemSettings));
            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                {
                    data = (SystemSettings)xmlSerializer.Deserialize(reader);
                    reader.Close();
                }
                return data;
            }
            return null;
        }
    }
}