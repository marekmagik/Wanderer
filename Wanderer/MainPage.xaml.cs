using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Wanderer.Resources;

namespace Wanderer
{
    public partial class MainPage : PhoneApplicationPage
    {
        private GPSTracker _gpsTracker;
        private static bool _initialized = false;

        public MainPage()
        {
            InitializeComponent();

            Configuration.PrimaryDescriptionFontSize = 18;
            Configuration.SecondaryDescriptionFontSize = 13;
            Configuration.ServerAddress = "192.168.1.103";
            Configuration.UseGPS = false;

            ListOfPlaces listOfPlaces = new ListOfPlaces(this);
            CategoriesBudlesPage categoriesPage = new CategoriesBudlesPage();
            MapWithPlacesPage mapPage = new MapWithPlacesPage(this);

            ListOfPlacesPanoraaItem.Content = listOfPlaces;
            CategoriesBundlesPanoramaItem.Content = categoriesPage;
            MapWithPlacesItem.Content = mapPage;
            IsolatedStorageDAO.InitIsolatedStorageDAO();

            _gpsTracker = new GPSTracker(listOfPlaces);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ((ListOfPlaces)ListOfPlacesPanoraaItem.Content).ReloadContent();
            if (_initialized)
            {
                _gpsTracker.activateGPSTracker();
            }
            _initialized = true;
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _gpsTracker.deactivateGPSTracker();
            base.OnNavigatedFrom(e);
        }

        private void UseGPSCheckboxChecked(object sender, RoutedEventArgs e)
        {
            if (!Configuration.UseGPS)
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    _gpsTracker.activateGPSTracker();
                    Configuration.UseGPS = true;
                });
            }
        }

        private void UseGPSCheckboxUnchecked(object sender, RoutedEventArgs e)
        {
            if (Configuration.UseGPS)
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        _gpsTracker.deactivateGPSTracker();
                        Configuration.UseGPS = false;
                    });
            }
        }
    }
}