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

namespace Wanderer
{
    public partial class ListOfPlaces : PhoneApplicationPage
    {
        private List<Place> places;

        public ListOfPlaces()
        {
            places = new List<Place>();
            GetData(20.5,40.6,10000);
            this.DataContext = places;

            InitializeComponent();

        }

        //tymczasowo, zeby cos sie wypisalo, na razie jeszcze nie ma sciagania obrazka
        private void GetData(double lon, double lat, int distance)
        {
            Place place = new Place();
            place.Desc = " Super Gory";
            place.Distance = 1.0;

            places.Add(place);

            Place place2 = new Place();
            place2.Desc = " Bar mleczny ";
            place.Distance = 2.6;

            places.Add(place2);
        }

        // nie przetestowane, testowalem na desktopie i dzialalo ale tam sie to robilo inaczej wiec ten kod moze byc wyjatkogenny ;)
        // zamiast localhost oczywiscie jakis tam adres :P
        /*private void GetData(double lon, double lat, int distance)
        {
            string uri = "http://localhost:7001/Wanderer/api/places/get/" + lon + "/" + lat + "/" + distance;
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
                }
                catch (WebException e)
                {
                    return;
                }
            }
        }*/
    }
}