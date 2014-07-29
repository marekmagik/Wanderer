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
        //private static GPSTracker _gpsTracker = new GPSTracker();

        public MainPage()
        {
            InitializeComponent();

            Configuration.PrimaryDescriptionFontSize = 18;
            Configuration.SecondaryDescriptionFontSize = 13;
            Configuration.ServerAddress = "192.168.1.102";

            ListOfPlacesPanoraaItem.Content = (new ListOfPlaces(this));
            IsolatedStorageDAO.InitIsolatedStorageDAO();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ((ListOfPlaces)ListOfPlacesPanoraaItem.Content).ReloadContent();
            base.OnNavigatedTo(e);
        }

    }
}