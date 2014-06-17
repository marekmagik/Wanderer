using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Wanderer
{
    public class ImageMetadata
    {

        public List<Point> Points { get; set; }
        public List<Category> Categories { get; set; }
        public double OrientationOfLeftBorder { get; set; }
        public double CoverageInPercent { get; set; }
        public String PictureDescription { get; set; }
        public String PictureAdditionalDescription { get; set; }
        public int IdInDatabase { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Version { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public ImageSource Image { get; set; }
        public string PictureSHA256 { get; set; }


        public ImageMetadata()
        {
            Points = new List<Point>();
            Categories = new List<Category>();
        }

        public bool addCategory(Category category)
        {
            if (Categories.Contains(category))
            {
                return false;
            }
            else
            {
                Categories.Add(category);
                return true;
            }
        }

        public bool addPoint(Point point)
        {
            bool canAddPoint = true;
            foreach (Point p in Points)
            {
                if (Math.Abs(p.X - point.X) < 10 && Math.Abs(p.Y - point.Y) < 10)
                {
                    canAddPoint = false;
                }
            }
            if (canAddPoint)
            {
                Points.Add(point);
                return true;
            }
            else
            {
                return false;
            }

        }

        internal bool removeCategory(Category category)
        {
            bool canDelete = true;
            foreach (Point p in Points)
            {
                if (p.Category.Equals(category))
                {
                    canDelete = false;
                }
            }
            if (canDelete)
            {
                return Categories.Remove(category);
            }
            else
            {
                return false;
            }
        }
    }
}
