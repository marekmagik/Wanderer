using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Location;

namespace Wanderer
{
    public class GPSTracker
    {
        private bool useGPS;
        public double longitude;
        public double latitude;
        public double altitude;

        public bool UseGPS
        {
            get
            { return useGPS; }
            set
            {
                if (useGPS && !value)
                {
                    gpsTracker.Stop();
                }
                else
                {
                    if (!useGPS && value)
                    {
                        gpsTracker.Start();
                    }
                }
                useGPS = value;
            }
        }

        public double Longitude
        {
            get
            {
                return longitude;
            }
        }
        public double Latitude
        {
            get
            {
                return latitude;
            }
        }
        public double Altitude
        {
            get
            {
                return altitude;
            }
        }


        private readonly GeoCoordinateWatcher gpsTracker;

        public GPSTracker()
        {
            gpsTracker = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            gpsTracker.MovementThreshold = 100;
            gpsTracker.PositionChanged += gpsTrackerPositionChanged;
            useGPS = false;
        }

        private void gpsTrackerPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            longitude = e.Position.Location.Longitude;
            latitude = e.Position.Location.Latitude;
            altitude = e.Position.Location.Altitude;
        }

        public static double computeDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(x1 - x2), 2) + Math.Pow(Math.Abs(y1 - y2), 2));
        }

    }
}
