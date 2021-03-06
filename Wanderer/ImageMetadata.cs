﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Wanderer
{
    public class ImageMetadata : INotifyPropertyChanged, IComparable<ImageMetadata>
    {
        public List<Point> Points { get; set; }
        public List<Category> Categories { get; set; }
        public double OrientationOfLeftBorder { get; set; }
        public double CoverageInPercent { get; set; }
        public String PictureDescription { get; set; }
        public String PictureAdditionalDescription { get; set; }
        public String PictureDescriptionToChange { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Version { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public ImageSource Image { get; set; }
        public String PictureSHA256 { get; set; }
        public String Category { get; set; }
        public bool IsPanoramaCached
        {
            get
            {
                return IsolatedStorageDAO.IsPhotoCached(PictureSHA256);
            }
        }
        public bool IsImageInDesiredRange
        {
            get
            {
                if (CurrentDistance == null || CurrentDistance.Equals("-.-- km"))
                {
                    return true;
                }
                double distance = Convert.ToDouble(CurrentDistance.Replace(" km", ""));

                return (distance <= (Convert.ToDouble(Configuration.GPSRange) / 1000.0));
            }
        }

        private String _currentDistance;
        public String CurrentDistance
        {
            get
            {
                return _currentDistance;
            }
            set
            {
                _currentDistance = value;
                OnPropertyChanged("CurrentDistance");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ImageMetadata()
        {
            Points = new List<Point>();
            Categories = new List<Category>();
            CurrentDistance = "-.-- km";
        }

        public void ToggleDescriptions()
        {
            string temp = PictureAdditionalDescription;
            PictureAdditionalDescription = PictureDescriptionToChange;
            PictureDescriptionToChange = temp;
        }

        public void UpdateDistance(double longitude, double lattitude)
        {
            double distance = GPSTracker.ComputeDistance(Longitude, Latitude, longitude, lattitude);
            if (longitude == GPSTracker.LocationUnknown)
            {
                CurrentDistance = "-.-- km";
            }
            else
            {
                CurrentDistance = String.Format("{0:0.00}", distance) + " km";
            }
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    handler(this, new PropertyChangedEventArgs(name));
                });
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !obj.GetType().Equals(this.GetType()))
            {
                return false;
            }
            return PictureSHA256.Equals(((ImageMetadata)obj).PictureSHA256);
        }


        public int CompareTo(ImageMetadata other)
        {
            if (CurrentDistance == null || CurrentDistance.Equals("-.-- km") && (other.CurrentDistance == null || other.CurrentDistance.Equals("-.-- km")))
            {
                return 0;
            }
            if (CurrentDistance == null || CurrentDistance.Equals("-.-- km"))
            {
                return 1;
            }
            if (other.CurrentDistance == null || other.CurrentDistance.Equals("-.-- km"))
            {
                return -1;
            }
            double currentDistance = Convert.ToDouble(CurrentDistance.Replace(" km", ""));
            double otherCurrentDistance = Convert.ToDouble(other.CurrentDistance.Replace(" km", ""));

            if (currentDistance == otherCurrentDistance)
            {
                return 0;
            }

            return currentDistance < otherCurrentDistance ? -1 : 1;
        }
    }
}
