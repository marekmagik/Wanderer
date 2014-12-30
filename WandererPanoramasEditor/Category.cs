﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WandererPanoramasEditor
{
    public class Category
    {
        #region Properties
        public String Name { get; set; }
        #endregion

        #region Constructors
        public Category(String name)
        {
            this.Name = name;
        }
        #endregion

        #region Methods
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

        #endregion
    }
}