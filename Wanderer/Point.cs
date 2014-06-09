using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanderer
{
    public class Point
    {
        private double x;
        private double y;
        private Category category;
        private String primaryDescrption;
        private String secondaryDescription;

        public String PrimaryDescription
        {
            get
            {
                return primaryDescrption;
            }
            set
            {
                primaryDescrption = value;
            }
        }

        public String SecondaryDescription
        {
            get
            {
                return secondaryDescription;
            }
            set
            {
                secondaryDescription = value;
            }
        }

        public Category Category
        {
            get
            {
                return category;
            }
            set
            {
                category = value;
            }
        }

        public double X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        public double Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }

        public Point(double x, double y, Category category, String primaryDescription, String secondaryDescription)
        {
            this.x = x;
            this.y = y;
            this.category = category;
            this.primaryDescrption = primaryDescription;
            this.secondaryDescription = secondaryDescription;
        }


        public override string ToString()
        {
            return primaryDescrption;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == this.GetType())
            {
                if (((Point)obj).X == x && ((Point)obj).Y == y)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

    }
}
