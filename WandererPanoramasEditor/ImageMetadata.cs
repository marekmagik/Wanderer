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
        #region Members
        private ImageSource m_thumbnail = null;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Properties
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
        #endregion

        #region Constructors
        public ImageMetadata()
        {
            Points = new List<Point>();
            Categories = new List<Category>();
        }
        #endregion

        #region Methods
        public bool AddCategory(Category category)
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

        public bool AddPoint(Point point)
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

        public bool RemoveCategory(Category category)
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

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion
    }
}
