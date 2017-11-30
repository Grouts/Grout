using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Grout.Base.DataClasses;
using Grout.Base.Logger;

namespace Grout.Base.Data
{
    public class DbColumnDeSerializer
    {
        public DB_SyncUMP DeserializeTables(string filePath)
        {
            DB_SyncUMP data = null;
            var xmlSerializer = new XmlSerializer(typeof(DB_SyncUMP));
            if (File.Exists(filePath))
            {
                try
                {
                    using (var reader = new StreamReader(filePath))
                    {
                        data = (DB_SyncUMP) xmlSerializer.Deserialize(reader);
                        reader.Close();
                    }
                }
                catch (Exception e)
                {
                    LogExtension.LogError("Exception is thrown while deserializing DB_Schema.xml", e, MethodBase.GetCurrentMethod(), "FilePath - " + filePath);
                }
            }
            else
            {
                LogExtension.LogInfo("DB_Schema.xml file not found", MethodBase.GetCurrentMethod(), "FilePath - " + filePath);
            }
            return data;
        }
    }
}