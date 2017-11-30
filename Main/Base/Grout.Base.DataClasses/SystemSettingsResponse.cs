using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grout.Base.DataClasses
{
    public class SystemSettingsResponse
    {
        public string DateFormat { get; set; }
        public byte[] FavIcon { get; set; }
        public byte[] LoginLogo { get; set; }
        public byte[] MainScreenLogo { get; set; }
        public string OrganizationName { get; set; }
        public string TimeZone { get; set; }
        public string WelcomeNoteText { get; set; }
    }
}
