using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Location;
using System.IO.IsolatedStorage;

namespace Wanderer
{
    public static class Configuration
    {
        private static readonly IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        private static int _primaryDescriptionFontSize;
        private static int _secondaryDescriptionFontSize;
        private static bool _useGPS;
        private static bool _workOnline;
        private static Int32 _GPSRange;
        private static String _serverAddress;

        public static int PrimaryDescriptionFontSize
        {
            get
            {
                return _primaryDescriptionFontSize;
            }
            set
            {
                _primaryDescriptionFontSize = value;
                saveSettingProperty("primaryDescriptionFontSize", value);
            }
        }

        public static int SecondaryDescriptionFontSize
        {
            get
            {
                return _secondaryDescriptionFontSize;
            }
            set
            {
                _secondaryDescriptionFontSize = value;
                saveSettingProperty("secondaryDescriptionFontSize", value);
            }
        }

        public static bool UseGPS
        {
            get
            {
                return _useGPS;
            }
            set
            {
                _useGPS = value;
                saveSettingProperty("useGPS", value);
            }
        }

        public static bool WorkOnline {
            get {
                return _workOnline;
            }
            set {
                _workOnline = value;
                saveSettingProperty("workOnline", value);
            }
        }

        public static Int32 GPSRange
        {
            get
            {
                return _GPSRange;
            }
            set
            {
                _GPSRange = value;
                saveSettingProperty("GPSRange", value);
            }
        }

        public static String ServerAddress
        {
            get
            {
                return _serverAddress;
            }
            set
            {
                _serverAddress = value;
                saveSettingProperty("serverAddress", value);
            }
        }


        public static void saveSettingProperty(String property, Object value)
        {
            if (!settings.Contains(property))
            {
                settings.Add(property, value);
            }
            else
            {
                settings[property] = value;
            }
            try
            {
                settings.Save();
            }
            catch (Exception) { }
        }

        public static object getSettingProperty(String property)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(property))
            {
                return IsolatedStorageSettings.ApplicationSettings[property] as object;
            }
            return null;
        }

        public static void loadConfiguration()
        {
            if (getSettingProperty("GPSRange") != null)
            {
                Configuration.PrimaryDescriptionFontSize = (int)getSettingProperty("primaryDescriptionFontSize");
                Configuration.SecondaryDescriptionFontSize = (int)getSettingProperty("secondaryDescriptionFontSize");
                Configuration.ServerAddress = (String)getSettingProperty("serverAddress");
                Configuration.UseGPS = (bool)getSettingProperty("useGPS");
                Configuration.GPSRange = (Int32)getSettingProperty("GPSRange");
                Configuration.WorkOnline = (bool)getSettingProperty("workOnline");
            }
            else
            {
                Configuration.PrimaryDescriptionFontSize = 20;
                Configuration.SecondaryDescriptionFontSize = 15;
                Configuration.ServerAddress = "192.168.1.100";
                Configuration.UseGPS = false;
                Configuration.WorkOnline = false;
                Configuration.GPSRange = 100000000;
            }
        }

    }
}
