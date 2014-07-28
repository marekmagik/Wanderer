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

        private const string address = "192.168.1.102";

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
