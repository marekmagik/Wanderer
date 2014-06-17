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
        //private List<Place> places;
        private List<ImageMetadata> places;
        private List<ImageMetadata> points;
        //private Place actualPlace;
        //private DAO dao;
        private int actualIndex;
        private NavigationService navigationService;

        public ListOfPlaces()
        {
            InitializeComponent();
            places = new List<ImageMetadata>();
            //places = new List<Place>();
            //dao = new DAO();
            DAO.GetDataFromServer(this, 20.5, 40.6, 100000000);
            this.DataContext = places;
            actualIndex = 0;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
           
            this.DataContext = null;
            base.OnNavigatedTo(e);
            this.DataContext = places;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }

        public void ReloadContent()
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        PlacesListBox.ItemsSource = null;
                        PlacesListBox.ItemsSource = places;
                    });
        }

        public void RequestCallback(IAsyncResult result)
        {
            HttpWebRequest request = result.AsyncState as HttpWebRequest;
            if (request != null)
            {
                try
                {
                    WebResponse response = request.EndGetResponse(result);
                    Stream stream = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(stream);
                    string json = streamReader.ReadToEnd();
                    JSONParser parser = new JSONParser();
                    places = parser.ParsePlacesJSON(json);

                    if (places.Count > 0)
                    {
                        actualIndex = 0;
                        DAO.LoadImage(this, places.ElementAt(actualIndex).IdInDatabase);
                    }

                }
                catch (WebException)
                {
                  // odkomentuj do testów:
                  //  DAO.LoadImage(this, 0);
                    return;
                }
            }
        }

        public void ThumbRequestCallback(IAsyncResult result)
        {
            HttpWebRequest request = result.AsyncState as HttpWebRequest;
            if (request != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        try
                        {
                            WebResponse response = request.EndGetResponse(result);
                            Stream stream = response.GetResponseStream();

                            BitmapImage image = new BitmapImage();
                            image.SetSource(stream);
                            Debug.WriteLine(actualIndex);
                            places.ElementAt(actualIndex).Image = image;

                            if (actualIndex == places.Count - 1)
                            {
                                ReloadContent();
                            }
                            else
                            {
                                actualIndex++;
                                DAO.LoadImage(this, places.ElementAt(actualIndex).IdInDatabase);
                            }
                        }
                        catch (WebException)
                        {
                            Debug.WriteLine("wyjatek wewnatrz UI!");
                            return;
                        }

                    });

            }
        }

        void PlacesListBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Uri uri = new Uri("/PanoramaView.xaml?photoID=" + places.ElementAt(PlacesListBox.SelectedIndex).IdInDatabase + "&hash="+places.ElementAt(PlacesListBox.SelectedIndex).PictureSHA256+"&useLocalDatabase=false", UriKind.Relative);
            
            Debug.WriteLine("photo_id=" + places.ElementAt(PlacesListBox.SelectedIndex).IdInDatabase);
            Debug.WriteLine("HASH: " + places.ElementAt(PlacesListBox.SelectedIndex).PictureSHA256);
            //   NavigationService.Navigate(new Uri("/PanoramaView.xaml?photoID=" + places.ElementAt(PlacesListBox.SelectedIndex).IdInDatabase + "&hash="+places.ElementAt(PlacesListBox.SelectedIndex).PictureSHA256+"&useLocalDatabase=false", UriKind.Relative));
           if(NavigationService == null){
               Debug.WriteLine("Ni huja");
           }
            NavigationService.Navigate(uri);
          //  navigationService.Navigate(uri);
        }


    }
}