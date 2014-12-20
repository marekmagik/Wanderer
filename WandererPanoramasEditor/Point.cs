using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WandererPanoramasEditor
{
    public class Point
    {
        public String PrimaryDescription { get; set; }
        public String SecondaryDescription { get; set; }
        public Category Category { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public byte Alignment { get; set; }
        public Char Color { get; set; }
        public double LineLength { get; set; }
        public double Angle { get; set; }

        public Point(double x, double y, Category category, String primaryDescription, String secondaryDescription)
        {
            this.X = x;
            this.Y = y;
            this.Category = category;
            this.PrimaryDescription = primaryDescription;
            this.SecondaryDescription = secondaryDescription;
        }

        public Point()
        {

        }

        public override string ToString()
        {
            return PrimaryDescription;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == this.GetType())
            {
                if (((Point)obj).X == X && ((Point)obj).Y == Y)
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

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + Convert.ToInt32(X);
            hash = (hash * 7) + Convert.ToInt32(Y);
            hash = (hash * 7) + PrimaryDescription.GetHashCode();
            hash = (hash * 7) + SecondaryDescription.GetHashCode();
            return hash;
        }

        public void SetValues(Point point)
        {
            this.PrimaryDescription = point.PrimaryDescription;
            this.SecondaryDescription = point.SecondaryDescription;
            this.Category = point.Category;
            this.X = point.X;
            this.Y = point.Y;
            this.Alignment = point.Alignment;
            this.Color = point.Color;
            this.LineLength = point.LineLength;
            this.Angle = point.Angle;
        }
    }
}
