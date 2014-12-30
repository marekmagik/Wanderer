using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WandererPanoramasEditor
{
    class Configuration
    {
        #region Members
        public String CONFIG_FILENAME = "./config.properties";
        #endregion

        #region Properties
        public String Address { get; private set;}
        public String Port { get; private set; }
        public String Mode { get; private set; }
        #endregion

        #region Constructors
        public Configuration()
        {
            PropertiesFileParser parser = new PropertiesFileParser(CONFIG_FILENAME);
            Address = parser.GetProperty("server.address");
            Port = parser.GetProperty("server.port");
            Mode = Modes.AdminMode;
        }
        #endregion

    }

    class Modes
    {
        #region Static Members
        public static String AdminMode = "ADMIN_MODE";
        public static String NormalMode = "NORMAL_MODE";
        #endregion
    }

    class AvailableColors
    {
        #region Static Members
        public static String Yellow = "Żółty";
        public static String Black = "Czarny";
        public static String White = "Biały";
        public static List<String> AvailableColorsList = new List<string> { Yellow, Black, White };
        #endregion
    }
}
