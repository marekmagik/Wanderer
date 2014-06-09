using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Wanderer
{
    public partial class ListOfPlaces : PhoneApplicationPage
    {
        private List<Place> places;
        private List<ImageMetadata> points;
        private Place actualPlace;

        public ListOfPlaces()
        {
            InitializeComponent();
            places = new List<Place>();
            DAO dao = new DAO();
            dao.GetDataFromServer(places,20.5,40.6,1000000);
            this.DataContext = places;
            //this.points = dao.getPointsInRange(20.5, 40.6, 1000000);
            //this.DataContext = points;
        }

        /*Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        PlacesListBox.ItemsSource = null;
                        PlacesListBox.ItemsSource = places2;
                    });*/
        
    }
}