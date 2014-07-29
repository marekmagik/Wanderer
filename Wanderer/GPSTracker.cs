using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Device.Location;
using System.ComponentModel;
using System.Windows.Controls;

namespace Wanderer
{
    public class GPSTracker : UserControl
    {
        private bool _useGPS;
        public double Longitude { get; private set; }
        public double Latitude { get; private set; }
        public double Altitude { get; private set; }

        public bool UseGPS
        {
            get
            { return _useGPS; }
            set
            {
                if (_useGPS && !value)
                {
 //                   gpsTracker.Stop();
                }
                else
                {
                    if (!_useGPS && value)
                    {
//                        gpsTracker.Start();
                    }
                }
                _useGPS = value;
            }
        }

//        private readonly GeoCoordinateWatcher gpsTracker;

        public GPSTracker()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
//                gpsTracker = new GeoCoordinateWatcher(GeoPositionAccuracy.High) { MovementThreshold = 100 };
//                gpsTracker.PositionChanged += gpsTrackerPositionChanged;
            }

 //           gpsTracker = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
 //           gpsTracker.MovementThreshold = 100;
            _useGPS = false;
        }
/*
        private void gpsTrackerPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            Longitude = e.Position.Location.Longitude;
            Latitude = e.Position.Location.Latitude;
            Altitude = e.Position.Location.Altitude;
        }
*/

        public static double ComputeDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(x1 - x2), 2) + Math.Pow(Math.Abs(y1 - y2), 2));
        }

    }
}
