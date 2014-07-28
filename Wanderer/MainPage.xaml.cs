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
        //private static GPSTracker gpsTracker = new GPSTracker();
        
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            Configuration.PrimaryDescriptionFontSize = 18;
            Configuration.SecondaryDescriptionFontSize = 13;

            ListOfPlacesPanoraaItem.Content = (new ListOfPlaces(this));
            IsolatedStorageDAO.InitIsolatedStorageDAO();
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }
        

        private void showPanoramaViewPage(Object sender, RoutedEventArgs e)
        {
            /* Wywołanie Garbage Collectora jest konieczne - gdy ładujemy duże zdjęcia, a poprzednie jest w pamięci 
             * może wystąpić naruszenie pamięci (core dump).
             */
            GC.Collect();
            GC.WaitForPendingFinalizers();
            NavigationService.Navigate(new Uri("/PanoramaView.xaml?photoID=1&useLocalDatabase=true", UriKind.Relative));  
        }

        private void showListOfPlaces(Object sender, RoutedEventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            NavigationService.Navigate(new Uri("/ListOfPlaces.xaml", UriKind.Relative));
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("on navigated main");

       //     Deployment.Current.Dispatcher.BeginInvoke(delegate
       //     {
                ((ListOfPlaces)ListOfPlacesPanoraaItem.Content).ReloadContent();
             //   UpdateLayout();


      //      });
            base.OnNavigatedTo(e);
        }
    }
}