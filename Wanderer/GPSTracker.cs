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
        public static readonly double LocationUnknown = 1000.0;
        private static readonly double Degrees_To_Kilometers_Factor = 111;

        private static double _currentLongitude;
        private static double _currentLatitude;

        private readonly ListOfPlaces _listOfPlaces;
        private readonly MapWithPlacesPage _mapPage;
        private Geolocator _geolocator = null;

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
                Debug.WriteLine("Last known longitude loaded : " + CurrentLongitude);
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
            if (Configuration.UseGPS)
            {
                _geolocator = new Geolocator();
                _geolocator.DesiredAccuracy = PositionAccuracy.High;
                _geolocator.MovementThreshold = 10;

                _listOfPlaces.GpsSign.Source = null;
                _listOfPlaces.GpsSign.Source = new BitmapImage(new Uri("Images/GpsOnline.png", UriKind.Relative));

                _geolocator.StatusChanged += GeolocatorStatusChanged;
                _geolocator.PositionChanged += GeolocatorPositionChanged;

            }
            else
            {
                _listOfPlaces.GpsSign.Source = null;
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
        }

        private void GeolocatorPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            CurrentLongitude = args.Position.Coordinate.Longitude;
            CurrentLatitude = args.Position.Coordinate.Latitude;
            _listOfPlaces.UpdateDistanceForAllPlaces(CurrentLongitude, CurrentLatitude);
            _mapPage.NotifyGeolocatorPositionChanged(CurrentLongitude, CurrentLatitude);
            Debug.WriteLine("Odebrano współrzędne. Longitude: " + CurrentLongitude + ", Latitude: " + CurrentLatitude);
        }

        private void GeolocatorStatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            String status = "";
            bool errorOccured = false;

            switch (args.Status)
            {
                case PositionStatus.Disabled:
                    status = "GPS wyłączony w ustawieniach urządzenia.";
                    errorOccured = true;
                    break;
                case PositionStatus.Initializing:
                    status = "Inicjalizacja w toku.";
                    break;
                case PositionStatus.NoData:
                    status = "Odbiór danych GPS jest niemożliwy.";
                    errorOccured = true;
                    break;
                case PositionStatus.Ready:
                    status = "Odbiornik GPS pracuje poprawnie.";
                    break;
                case PositionStatus.NotInitialized:
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
