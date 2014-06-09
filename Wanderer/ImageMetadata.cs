using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Wanderer
{
    public class ImageMetadata
    {
        private List<Point> points;
        private List<Category> categories;
        private double orientationOfLeftBorder;
        private double coverageInPercent;
        private double longitude;
        private double latitude;
        private String pictureDescription;
        private int idFromServerDatabase;

        public int Width { get; set; }
        public int Height { get; set; }


        public List<Point> Points
        {
            get
            {
                return points;
            }
            set
            {
                points = value;
            }
        }

        public List<Category> Categories
        {
            get
            {
                return categories;
            }
            set
            {
                categories = value;
            }
        }

        public double OrientationOfLeftBorder
        {
            get
            {
                return orientationOfLeftBorder;
            }
            set
            {
                orientationOfLeftBorder = value;
            }
        }


        public double CoverageInPercent
        {
            get
            {
                return coverageInPercent;
            }
            set
            {
                coverageInPercent = value;
            }
        }
        public String PictureDescription
        {
            get
            {
                return pictureDescription;
            }
            set
            {
                pictureDescription = value;
            }
        }
        public int IdFromServerDatabase
        {
            get
            {
                return idFromServerDatabase;
            }
            set
            {
                idFromServerDatabase = value;
            }
        }

        public double Longitude
        {
            get
            {
                return longitude;
            }
            set
            {
                longitude = value;
            }
        }

        public double Latitude
        {
            get
            {
                return latitude;
            }
            set
            {
                latitude = value;
            }
        }

        public ImageMetadata()
        {
            points = new List<Point>();
            categories = new List<Category>();
        }

        public bool addCategory(Category category)
        {
            if (categories.Contains(category))
            {
                return false;
            }
            else
            {
                categories.Add(category);
                return true;
            }
        }

        public bool addPoint(Point point)
        {
            bool canAddPoint = true;
            foreach (Point p in points)
            {
                if (Math.Abs(p.X - point.X) < 10 && Math.Abs(p.Y - point.Y) < 10)
                {
                    canAddPoint = false;
                }
            }
            if (canAddPoint)
            {
                points.Add(point);
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
            foreach (Point p in points)
            {
                if (p.Category.Equals(category))
                {
                    canDelete = false;
                }
            }
            if (canDelete)
            {
                return categories.Remove(category);
            }
            else
            {
                return false;
            }
        }
    }
}
