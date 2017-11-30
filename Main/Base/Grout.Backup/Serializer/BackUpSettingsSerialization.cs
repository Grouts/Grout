using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
namespace Syncfusion.UMP.Backup.Base
{
    [Serializable]
    public class BackUpandRestoreSetting
    {
        /// <summary>
        /// Source for Backup
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// Destination for Back up
        /// </summary>
        public string Destination { get; set; }
        /// <summary>
        /// Connection String for Back up
        /// </summary>
        public string DataSource { get; set; }

    }
}
