using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WandererPanoramasEditor
{
    class ConfigurationFactory
    {
        #region Members
        private static Configuration instance;
        #endregion

        #region Methods
        public static Configuration GetConfiguration()
        {
            if (instance == null)
                instance = new Configuration();
            return instance;
        }
        #endregion
    }
}
