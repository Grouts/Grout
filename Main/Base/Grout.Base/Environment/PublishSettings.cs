using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Grout.Base
{
    [Serializable]
    public class PublishSettings
    {
        /// <summary>
        /// Gets or sets the list of report file extensions supported
        /// </summary>
        [XmlArrayItem("Extension")]
        public List<string> ReportExtensions { get; set; }

        /// <summary>
        /// Gets or sets the list of db file extensions supported
        /// </summary>
        [XmlArrayItem("Extension")]
        public List<string> DBExtensions { get; set; }

        /// <summary>
        /// Gets or sets the list of map shape file extensions supported
        /// </summary>
        [XmlArrayItem("Extension")]
        public List<string> MapShapeExtensions { get; set; }
    }
}