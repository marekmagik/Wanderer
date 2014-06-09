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

namespace Wanderer
{
    public partial class ListOfPlaces : PhoneApplicationPage
    {
        private List<Place> places;
        private List<Point> points;
        private const string address = "10.22.115.27";
        private Place actualPlace;

        public ListOfPlaces()
        {
            places = new List<Place>();
            GetData(20.5,40.6,10000);
            this.DataContext = places;
            this.points = DAO.getPointsInRange(20.5, 40.6, 10000);
            //this.DataContext = points;
            InitializeComponent();

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
            string uri = "http://"+address+":7001/Wanderer/api/places/get/" + lon + "/" + lat + "/" + distance;
            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(RequestCallback, request);
        }


        void RequestCallback(IAsyncResult result)
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

                    // BeginLoadingImages();
                     //   
                }
                catch (WebException e)
                {
                    return;
                }
            }
        }

        private void BeginLoadingImages();

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
                    BitmapImage image = new BitmapImage();
                    image.SetSource(stream);
                    actualPlace.Image = image;
                }
                catch (WebException e)
                {
                    return;
                }
            }
        }
    }
}