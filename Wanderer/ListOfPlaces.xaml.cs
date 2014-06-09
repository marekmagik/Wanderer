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
        private DAO dao;
        private int actualIndex;

        public ListOfPlaces()
        {
            InitializeComponent();
            places = new List<Place>();
            dao = new DAO();
            dao.GetDataFromServer(this,20.5,40.6,1000000);
            this.DataContext = places;
            actualIndex = 0;
        }

        public void ReloadContent(){
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
                        dao.LoadImage(actualIndex);
                    }

                }
                catch (WebException )
                {
                    return;
                }
            }
        }

        public void ThumbRequestCallback(IAsyncResult result)
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
                    places.ElementAt(actualIndex).Image = image;

                    if (actualIndex == places.Count)
                    {
                        ReloadContent();
                    }
                    else
                    {
                        actualIndex++;
                        dao.LoadImage(actualIndex);
                    }


                }
                catch (WebException)
                {
                    return;
                }
            }
        }
        
    }
}