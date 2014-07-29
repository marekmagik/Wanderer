using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanderer
{
    public class Category
    {
        public String Name { get; private set; }

        public Category(String name)
        {
            Name = name;
        }

        public override String ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !obj.GetType().Equals(this.GetType()))
            {
                return false;
            }
            else
            {
                if (((Category)obj).ToString().Trim().ToUpper().Equals(Name.Trim().ToUpper()))
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
            hash = (hash * 7) + Name.GetHashCode();
            return hash;
        }

    }
}
