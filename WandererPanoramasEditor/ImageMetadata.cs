using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WandererPanoramasEditor
{
    public class ImageMetadata : INotifyPropertyChanged
    {

        public List<Point> Points { get; private set; }
        public List<Category> Categories { get; private set; }
        public double OrientationOfLeftBorder { get; set; }
        public double CoverageInPercent { get; set; }
        public String PictureDescription { get; set; }
        public String PictureAdditionalDescription { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Version { get; set; }
        public String PictureSHA256 { get; set; }

        public String Category { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
        //public ImageSource Thumbnail { get; set; }

        private ImageSource m_thumbnail=null;
        public ImageSource Thumbnail
        {
            get { return m_thumbnail; }
            set
            {
                if (value != this.m_thumbnail)
                {
                    this.m_thumbnail = value;
                    NotifyPropertyChanged("Thumbnail");
                }
            }
        }

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

        public bool removeCategory(Category category)
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
