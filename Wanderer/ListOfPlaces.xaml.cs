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
        private const string address = "192.168.1.100";//"10.20.120.151";//"10.22.115.27";.
        private Place actualPlace;

        public ListOfPlaces()
        {
            InitializeComponent();
            places = new List<Place>();
            GetDataFromServer(20.5,40.6,1000000);
            this.DataContext = places;
            this.points = DAO.getPointsInRange(20.5, 40.6, 1000000);
            //this.DataContext = points;
        }

        //tymczasowo, zeby cos sie wypisalo, na razie jeszcze nie ma sciagania obrazka
        private void GetData(double lon, double lat, int distance)
        {
            Place place = new Place();
            place.Desc = " Super Gory";
            place.Distance = 1.0;
            place.Image = null;

            places.Add(place);

            Place place2 = new Place();
            place2.Desc = " Bar mleczny ";
            place.Distance = 2.6;
            place.Image = null;

            places.Add(place2);
        }

        // nie przetestowane, testowalem na desktopie i dzialalo ale tam sie to robilo inaczej wiec ten kod moze byc wyjatkogenny ;)
        // zamiast localhost oczywiscie jakis tam adres :P
        private void GetDataFromServer(double lon, double lat, int distance)
        {
            //string uri = "http://"+address+":7001/Wanderer/api/places/get/" + lon + "/" + lat + "/" + distance;
            string longitude = Convert.ToString(lon).Replace(',', '.');
            string latitude = Convert.ToString(lat).Replace(',', '.');

            string uri = "http://" + address + ":7001/Wanderer/api/places/get/" + longitude + "/" + latitude + "/" + distance;

            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "GET";
            request.BeginGetResponse(new AsyncCallback(RequestCallback), request);

        }


        void RequestCallback(IAsyncResult result)
        {
            Debug.WriteLine("Hello2");
            HttpWebRequest request = result.AsyncState as HttpWebRequest;
            if (request != null)
            {
                try
                {
                    Debug.WriteLine("Hello!");
                    WebResponse response = request.EndGetResponse(result);
                    Stream stream = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(stream);
                    string json = streamReader.ReadToEnd();
                    JSONParser parser = new JSONParser();
                    List<Place> places2 = parser.ParsePlacesJSON(json);
                    foreach (Place p in places2)
                    {
                        places.Add(p);
                        Debug.WriteLine(p);
                    }
                   
                    LoadImage(places.ElementAt(4));

                    Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        PlacesListBox.ItemsSource = null;
                        PlacesListBox.ItemsSource = places2;
                    });
                    
                   
                    // BeginLoadingImages();
                     //   
                }
                catch (WebException e)
                {
                    return;
                }
            }
        }



        private void BeginLoadingImages(){
        }

        private void LoadImage(Place place)
        {
            actualPlace = place;
            string uri = "http://" + address + ":7001/Wanderer/api/photos/get/thumbnail/" + place.PlaceId;
            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(ThumbRequestCallback, request);
        }

        void ThumbRequestCallback(IAsyncResult result)
        {
            HttpWebRequest request = result.AsyncState as HttpWebRequest;
            if (request != null)
            {
                try
                {
                    WebResponse response = request.EndGetResponse(result);
                    Stream stream = response.GetResponseStream();
                   // BitmapImage image = new BitmapImage();
                   // image.SetSource(stream);
                   // actualPlace.Image = image;

                    Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        BitmapImage image = new BitmapImage();
                        image.SetSource(stream);
                        actualPlace.Image = image;

                        PlacesListBox.ItemsSource = null;
                        PlacesListBox.ItemsSource = places;
                    });

                    Debug.WriteLine("obrazek pobrany!");

                }
                catch (WebException e)
                {
                    return;
                }
            }
        }
    }
}