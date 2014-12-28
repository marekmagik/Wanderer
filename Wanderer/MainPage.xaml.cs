using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private ListOfPlaces _listOfPlaces = null;
        private CategoriesBudlesPage _categoriesPage;
        private MapWithPlacesPage _mapPage = null;

        public int PrimaryDescriptionFontSize
        {
            get
            {
                return Configuration.PrimaryDescriptionFontSize;
            }
            set
            {
                Configuration.PrimaryDescriptionFontSize = value;
            }
        }
        private int SecondaryDescriptionFontSize
        {
            get
            {
                return Configuration.SecondaryDescriptionFontSize;
            }
            set
            {
                Configuration.SecondaryDescriptionFontSize = value;
            }
        }
        private String GPSRange
        {
            get
            {
                return Convert.ToString(Configuration.GPSRange);
            }
            set
            {
                Configuration.GPSRange = Convert.ToInt32(value);
                _listOfPlaces.UpdateDistanceForAllPlaces(GPSTracker.CurrentLongitude, GPSTracker.CurrentLatitude);
            }
        }

        public GPSTracker GPSTracker
        {
            get
            {
                return _gpsTracker;
            }
        }

        private List<int> PossiblePrimaryDescriptionFontSizes = new List<int>() { 15, 20, 25, 30, 35 };
        private List<int> PossibleSecondaryDescriptionFontSizes = new List<int>() { 10, 15, 20, 25, 30 };
        public MainPage()
        {
            Configuration.loadConfiguration();

            InitializeComponent();

            GPSTracker.LoadLastKnownPositionIfExists();

            PrimaryDescriptionFontSizePicker.ItemsSource = PossiblePrimaryDescriptionFontSizes;
            PrimaryDescriptionFontSizePicker.SelectedItem = PrimaryDescriptionFontSize;
            SecondaryDescriptionFontSizePicker.ItemsSource = PossibleSecondaryDescriptionFontSizes;
            SecondaryDescriptionFontSizePicker.SelectedItem = SecondaryDescriptionFontSize;

            IsolatedStorageDAO.InitIsolatedStorageDAO();

            _listOfPlaces = new ListOfPlaces(this);
            _categoriesPage = new CategoriesBudlesPage(_listOfPlaces);

            _mapPage = new MapWithPlacesPage(this, _listOfPlaces);

            GPSRangeTextBox.Text = GPSRange;
            WorkOnlineCheckbox.IsChecked = Configuration.WorkOnline;

            ListOfPlacesPanoraaItem.Content = _listOfPlaces;
            CategoriesBundlesPanoramaItem.Content = _categoriesPage;
            MapWithPlacesItem.Content = _mapPage;

            _gpsTracker = new GPSTracker(_listOfPlaces, _mapPage);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ((ListOfPlaces)ListOfPlacesPanoraaItem.Content).UpdateDistanceForAllPlaces(GPSTracker.CurrentLongitude, GPSTracker.CurrentLatitude);

            if (Configuration.UseGPS)
            {
                UseGPSCheckbox.IsChecked = true;
                _gpsTracker.activateGPSTracker();
            }
            if (Configuration.WorkOnline)
            {
                _listOfPlaces.InternetSign.Visibility = Visibility.Visible;
            }
            else {
                _listOfPlaces.InternetSign.Visibility = Visibility.Collapsed;
            }
            if (Configuration.InternetExceptionOccured)
            {
                _listOfPlaces.setInternetSignOffline();
                Configuration.InternetExceptionOccured = false;
            }
            else {
                _listOfPlaces.setInternetSignOnline();
            }
            _listOfPlaces.refreshCollectionElements();

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _gpsTracker.deactivateGPSTracker();

            base.OnNavigatedFrom(e);
        }

        private void UseGPSCheckboxChecked(object sender, RoutedEventArgs e)
        {
            lock (this)
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Configuration.UseGPS = true;
                    _gpsTracker.activateGPSTracker();
                });
            }
        }

        private void UseGPSCheckboxUnchecked(object sender, RoutedEventArgs e)
        {
            lock (this)
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        _gpsTracker.deactivateGPSTracker();
                        Configuration.UseGPS = false;
                    });
            }
        }


        private void WorkOnlineCheckboxChecked(object sender, RoutedEventArgs e)
        {
            Configuration.WorkOnline = true;
            if (_mapPage.Count == 0 && GPSTracker != null && GPSTracker.IsEnabled)
                DAO.SendRequestForMetadataOfPlacesWithinRange(_mapPage, GPSTracker.CurrentLongitude, GPSTracker.CurrentLatitude, Configuration.GPSRange);
            Deployment.Current.Dispatcher.BeginInvoke(delegate {
                _listOfPlaces.InternetSign.Visibility = Visibility.Visible;
            });
        }

        private void WorkOnlineCheckboxUnchecked(object sender, RoutedEventArgs e)
        {
            Configuration.WorkOnline = false;
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                _listOfPlaces.InternetSign.Visibility = Visibility.Collapsed;
            });
        }

        private void PrimaryDescriptionFontSizeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Configuration.PrimaryDescriptionFontSize = (int)((ListPicker)sender).SelectedItem;
        }

        private void SecondaryDescriptionFontSizeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Configuration.SecondaryDescriptionFontSize = (int)((ListPicker)sender).SelectedItem;
        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (GPSRangeTextBox.Text.Equals("0"))
            {
                return;
            }
            if (GPSRangeTextBox.Text.StartsWith("0"))
            {
                GPSRangeTextBox.Text = GPSRangeTextBox.Text.Replace("0", "");
            }
            int position = GPSRangeTextBox.Text.IndexOf(",");
            if (position >= 0)
            {
                GPSRangeTextBox.Text = GPSRangeTextBox.Text.Substring(0, position);
                GPSRangeTextBox.Select(GPSRangeTextBox.Text.Length, 0);
            }
            if (GPSRangeTextBox.Text.Length > 0)
            {
                GPSRange = GPSRangeTextBox.Text;
            }
            else
            {
                GPSRangeTextBox.Text = "0";
                GPSRangeTextBox.Select(GPSRangeTextBox.Text.Length, 0);
            }
        }

    }
}