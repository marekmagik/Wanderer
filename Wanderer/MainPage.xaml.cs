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
        private Int32 GPSRange
        {
            get
            {
                return Configuration.GPSRange;
            }
            set
            {
                Configuration.GPSRange = value;
            }
        }


        private List<int> PossiblePrimaryDescriptionFontSizes = new List<int>() { 15, 20, 25, 30, 35 };
        private List<int> PossibleSecondaryDescriptionFontSizes = new List<int>() { 10, 15, 20, 25, 30 };
        public MainPage()
        {
            InitializeComponent();

            Configuration.loadConfiguration();
            GPSTracker.LoadLastKnownPositionIfExists();
            
            PrimaryDescriptionFontSizePicker.ItemsSource = PossiblePrimaryDescriptionFontSizes;
            PrimaryDescriptionFontSizePicker.SelectedItem = PrimaryDescriptionFontSize;
            SecondaryDescriptionFontSizePicker.ItemsSource = PossibleSecondaryDescriptionFontSizes;
            SecondaryDescriptionFontSizePicker.SelectedItem = SecondaryDescriptionFontSize;

            IsolatedStorageDAO.InitIsolatedStorageDAO();

            ListOfPlaces listOfPlaces = new ListOfPlaces(this);
            CategoriesBudlesPage categoriesPage = new CategoriesBudlesPage();
            MapWithPlacesPage mapPage = new MapWithPlacesPage(this);

            ListOfPlacesPanoraaItem.Content = listOfPlaces;
//<<<<<<< HEAD
///=======
            CategoriesBundlesPanoramaItem.Content = categoriesPage;
            MapWithPlacesItem.Content = mapPage;
//            IsolatedStorageDAO.InitIsolatedStorageDAO();
//>>>>>>> 2ee94d284e8ac258dbc28922c767b943f64be3ca

            _gpsTracker = new GPSTracker(listOfPlaces);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ((ListOfPlaces)ListOfPlacesPanoraaItem.Content).ReloadContent();
            if (_initialized)
            {
                _gpsTracker.activateGPSTracker();
            }
            else {
                if (Configuration.UseGPS) {
                    Configuration.UseGPS = false;
                    UseGPSCheckbox.IsChecked = true;
                }
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
            lock (this)
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
        }

        private void UseGPSCheckboxUnchecked(object sender, RoutedEventArgs e)
        {
            lock (this)
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

        private void PrimaryDescriptionFontSizeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Configuration.PrimaryDescriptionFontSize = (int)((ListPicker)sender).SelectedItem;
        }

        private void SecondaryDescriptionFontSizeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Configuration.SecondaryDescriptionFontSize = (int)((ListPicker)sender).SelectedItem;
        }

    }
}