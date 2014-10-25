using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Wanderer
{
    public class Point
    {
        public String PrimaryDescription { get; set; }
        public String SecondaryDescription { get; set; }
        public Category Category { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public byte Alignment { get; set; }
        public Color Color { get; set; }
        public double LineLength { get; set; }
        public double Angle { get; set; }
        public Line LeftPanoramaLine { get; set; }
        public Line RightPanoramaLine { get; set; }
        public Canvas LeftCanvas { get; set; }
        public Canvas RightCanvas { get; set; }
        public StackPanel LeftStackPanel { get; set; }
        public StackPanel RightStackPanel { get; set; }
        public TextBlock LeftPrimaryDescriptionTextBlock { get; set; }
        public TextBlock RightPrimaryDescriptionTextBlock { get; set; }
        public TextBlock LeftSecondaryDescriptionTextBlock { get; set; }
        public TextBlock RightSecondaryDescriptionTextBlock { get; set; }
        public double MinimumScaleDescriptionVisibility { get; set; }
        public Ellipse LeftBall { get; set; }
        public Ellipse RightBall { get; set; }
        //TODO: delete following
        public Line BottomLine { get; set; }
        public Line TopLine { get; set; }

        public Point(double x, double y, Category category, String primaryDescription, String secondaryDescription)
        {
            this.X = x;
            this.Y = y;
            this.Category = category;
            this.PrimaryDescription = primaryDescription;
            this.SecondaryDescription = secondaryDescription;

            MinimumScaleDescriptionVisibility = 1.0;
        }


        public override string ToString()
        {
            return PrimaryDescription;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == this.GetType())
            {
                if (((Point)obj).X == this.X && ((Point)obj).Y == this.Y)
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

    }
}
