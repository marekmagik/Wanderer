using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Controls;
using Windows.Devices.Geolocation;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Diagnostics;

namespace Wanderer
{
    public class GPSTracker : UserControl
    {
        public static double LocationUnknown = 1000.0;

        private static readonly double Degrees_To_Kilometers_Factor = 111;

        private static double _currentLongitude;
        private static double _currentLatitude;


        public static double CurrentLongitude
        {
            get
            {
                return _currentLongitude;
            }
            private set
            {
                _currentLongitude = value;
                Configuration.saveSettingProperty("LastKnownLogitude", value);
            }
        }
        public static double CurrentLatitude
        {
            get
            {
                return _currentLatitude;
            }
            private set
            {
                _currentLatitude = value;
                Configuration.saveSettingProperty("LastKnownLatitude", value);
            }
        }

        private readonly ListOfPlaces _listOfPlaces;
        private readonly MapWithPlacesPage _mapPage;
        private Geolocator _geolocator = null;

        public GPSTracker(ListOfPlaces listOfPlaces, MapWithPlacesPage mapPage)
        {
            this._listOfPlaces = listOfPlaces;
            this._mapPage = mapPage;
        }

        public static void LoadLastKnownPositionIfExists()
        {
            if (Configuration.getSettingProperty("LastKnownLogitude") != null)
            {
                CurrentLongitude = (double)Configuration.getSettingProperty("LastKnownLogitude");
                Debug.WriteLine("LAST KNOWN LONGITUDE : " + CurrentLongitude + "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
            }
            else
            {
                CurrentLongitude = LocationUnknown;
            }
            if (Configuration.getSettingProperty("LastKnownLatitude") != null)
            {
                CurrentLatitude = (double)Configuration.getSettingProperty("LastKnownLatitude");
            }
            else
            {
                CurrentLatitude = LocationUnknown;
            }
        }

        public static double ComputeDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(x1 - x2) * Degrees_To_Kilometers_Factor, 2)
                + Math.Pow(Math.Abs(y1 - y2) * Degrees_To_Kilometers_Factor, 2));
        }

        public void activateGPSTracker()
        {
            if (!Configuration.UseGPS)
            {
                _geolocator = new Geolocator();
                _geolocator.DesiredAccuracy = PositionAccuracy.High;
                _geolocator.MovementThreshold = 10;

                _geolocator.StatusChanged += GeolocatorStatusChanged;
                _geolocator.PositionChanged += GeolocatorPositionChanged;

                _listOfPlaces.GpsSign.Source = null;
                _listOfPlaces.GpsSign.Source = new BitmapImage(new Uri("Images/GpsOnline.png", UriKind.Relative));
            }
            else
            {
                _listOfPlaces.GpsSign.Source = null;
                CurrentLongitude = LocationUnknown;
                CurrentLatitude = LocationUnknown;
            }
        }

        public void deactivateGPSTracker()
        {
            if (Configuration.UseGPS)
            {
                _geolocator.PositionChanged -= GeolocatorPositionChanged;
                _geolocator.StatusChanged -= GeolocatorStatusChanged;

                _geolocator = null;

            }
            _listOfPlaces.GpsSign.Source = null;
            CurrentLongitude = LocationUnknown;
            CurrentLatitude = LocationUnknown;
        }

        private void GeolocatorPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            CurrentLongitude = args.Position.Coordinate.Longitude;
            CurrentLatitude = args.Position.Coordinate.Latitude;
            _listOfPlaces.UpdateDistanceForAllPlaces(CurrentLongitude, CurrentLatitude);
            _mapPage.NotifyGeolocatorPositionChanged(CurrentLongitude, CurrentLatitude);
            Debug.WriteLine("Lon: " + CurrentLongitude + ", Lat: " + CurrentLatitude);
        }

        private void GeolocatorStatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            String status = "";
            bool errorOccured = false;

            switch (args.Status)
            {
                case PositionStatus.Disabled:
                    // the application does not have the right capability or the location master switch is off
                    status = "GPS wyłączony w ustawieniach urządzenia.";
                    errorOccured = true;
                    break;
                case PositionStatus.Initializing:
                    // the geolocator started the tracking operation
                    status = "Inicjalizacja w toku.";
                    break;
                case PositionStatus.NoData:
                    // the location service was not able to acquire the location
                    status = "Odbiór danych GPS jest niemożliwy.";
                    errorOccured = true;
                    break;
                case PositionStatus.Ready:
                    // the location service is generating geopositions as specified by the tracking parameters
                    status = "Odbiornik GPS pracuje poprawnie.";
                    break;
                case PositionStatus.NotInitialized:
                    // the initial state of the geolocator, once the tracking operation is stopped by the user the geolocator moves back to this state
                    status = "Urządzenie nie zainicjalizowane.";
                    break;
            }
            Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    if (Configuration.UseGPS)
                    {
                        if (errorOccured)
                        {
                            CurrentLongitude = LocationUnknown;
                            CurrentLatitude = LocationUnknown;
                            _listOfPlaces.GpsSign.Source = null;
                            _listOfPlaces.GpsSign.Source = new BitmapImage(new Uri("Images/GpsOffline.png", UriKind.Relative));
                        }
                        else
                        {
                            _listOfPlaces.GpsSign.Source = new BitmapImage(new Uri("Images/GpsOnline.png", UriKind.Relative));
                        }
                    }
                    else
                    {
                        _listOfPlaces.GpsSign.Source = null;
                        _listOfPlaces.UpdateDistanceForAllPlaces(LocationUnknown, LocationUnknown);
                    }

                }
            );

            Debug.WriteLine(status);
        }
    }

}
