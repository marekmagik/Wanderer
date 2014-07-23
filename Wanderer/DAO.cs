using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private const string address = "192.168.0.105";//192.168.1.100";//"192.168.1.100";//"10.20.116.217";// "10.20.120.151";//"10.22.115.27";.

        public static void GetDataFromServer(ListOfPlaces callback, double lon, double lat, int distance)
        {
            string longitude = Convert.ToString(lon).Replace(',', '.');
            string latitude = Convert.ToString(lat).Replace(',', '.');

            string uri = "http://" + address + ":7001/Wanderer/api/places/get/" + longitude + "/" + latitude + "/" + distance;
            
            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "GET";
            request.BeginGetResponse(new AsyncCallback(callback.RequestCallback), request);

        }


        public static void LoadImage(ListOfPlaces callback, int placeId)
        {
            string uri = "http://" + address + ":7001/Wanderer/api/photos/get/thumbnail/" + placeId;
            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(callback.ThumbRequestCallback, request);
        }

        public static void GetPhotoById(PanoramaView callback, int id)
        {

            string uri = "http://" + address + ":7001/Wanderer/api/photos/get/" +id;

            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "GET";
            request.BeginGetResponse(new AsyncCallback(callback.ImageRequestCallback), request);
            
        }

        public static void GetPhotoDescById(PanoramaView callback, int id)
        {

            Debug.WriteLine("getPhotoDesc  DAO");
            string uri = "http://" + address + ":7001/Wanderer/api/photos/get/meta/" +id;

            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "GET";
            request.BeginGetResponse(new AsyncCallback(callback.DescRequestCallback), request);

        }

        public static void GetDataFromServer(PanoramaView callback, int placeId)
        {

            string uri = "http://" + address + ":7001/Wanderer/api/photos/get/meta" + placeId;

            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "GET";
            request.BeginGetResponse(new AsyncCallback(callback.DescRequestCallback), request);

        }
        

    }
}
