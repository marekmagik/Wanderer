using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Location;

namespace Wanderer
{
    public static class Configuration
    {
        public static int PrimaryDescriptionFontSize { get; set; }
        public static int SecondaryDescriptionFontSize { get; set; }
        public static string ServerAddress { get; set; }
        public static bool UseGPS { get; set; }

    }
}
