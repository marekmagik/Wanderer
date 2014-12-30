using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WandererPanoramasEditor
{
    class PropertiesFileParser
    {
        #region Members
        private Dictionary<String, String> _properties;
        #endregion

        #region Constructors
        public PropertiesFileParser(String filename)
        {
            _properties = new Dictionary<String, String>();
            ParseFile(filename);
        }
        #endregion

        #region Methods
        private void ParseFile(String filename)
        {
            foreach (var row in File.ReadAllLines(filename))
            {
                String[] values = row.Split('=');
                if(values.Length==2)
                    _properties.Add(values[0], values[1]);
            }
        }

        public String GetProperty(String propertyName)
        {
            return _properties[propertyName];
        }
        #endregion

    }
}
