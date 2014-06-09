using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Wanderer
{
    
    public class DAO
    {

        private List<Place> places;
        private const string address = "10.20.120.151";//"10.22.115.27";.
        private int actualIndex;

        public static List<ImageMetadata> getPointsInRange(double longitude, double latitude, double range)
        {
            List<Point> listOfPoints = new List<Point>();
            Category category = new Category("Szczyty");

            listOfPoints.Add(new Point(100.0, 100.0, category, "Wielka góra", "napis1 normal"));
            listOfPoints.Add(new Point(300.0, 100.0, category, "Też duża", "napis2 normal"));

            ImageMetadata image1 = new ImageMetadata();
            image1.addCategory(category);
            image1.Points = listOfPoints;
            image1.PictureDescription = "Stacja kosmiczna Mir";
            List<ImageMetadata> listOfMetadata = new List<ImageMetadata>();
            listOfMetadata.Add(image1);

            return listOfMetadata;
        }

        public void GetDataFromServer(List<Place> places, double lon, double lat, int distance)
        {
            this.places = places;
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

                    actualIndex = 0;
                    LoadAllImages();              
  
                }
                catch (WebException e)
                {
                    return;
                }
            }
        }

        private void LoadAllImages()
        {
            throw new NotImplementedException();
        }

        private void LoadImage()
        {
            string uri = "http://" + address + ":7001/Wanderer/api/photos/get/thumbnail/" + places.ElementAt(actualIndex).PlaceId;
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
                    places.ElementAt(actualIndex).Image = image;

                    if (actualIndex == places.Count)
                    {
                        //callback
                    }
                    else
                    {
                        actualIndex++;
                        LoadImage();
                    }
                      

                }
                catch (WebException e)
                {
                    return;
                }
            }
        }

    }
}
