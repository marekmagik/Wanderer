using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanderer
{
    public class Category
    {
        private String name;

        public String Name
        {
            get
            {
                return name;
            }
            set {
                name = value;
            }
        }

        public Category(String name)
        {
            this.name = name;
        }

        public override String ToString()
        {
            return name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !obj.GetType().Equals(this.GetType()))
            {
                return false;
            }
            else
            {
                if (((Category)obj).ToString().Trim().ToUpper().Equals(name.Trim().ToUpper()))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + name.GetHashCode();
            return hash;
        }

    }
}
