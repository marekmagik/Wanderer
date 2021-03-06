﻿using System;
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
        public static void SendRequestForMetadataOfPlacesWithinRange(ListOfPlaces callback, double lon, double lat, int distance)
        {
            string longitude = Convert.ToString(lon).Replace(',', '.');
            string latitude = Convert.ToString(lat).Replace(',', '.');

            string uri = "http://" + Configuration.ServerAddress + ":7001/Wanderer/api/places/get/" + longitude + "/" + latitude + "/" + distance;
            
            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "GET";
            request.BeginGetResponse(new AsyncCallback(callback.RequestCallback), request);
        }


        public static void SendRequestForThumbnail(ListOfPlaces callback, string pictureSHA256)
        {
            string uri = "http://" + Configuration.ServerAddress + ":7001/Wanderer/api/photos/get/thumbnail/" + pictureSHA256;
            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(callback.ThumbRequestCallback, request);
        }

        public static void SendRequestForPanorama(PanoramaView callback, string pictureSHA256)
        {
            string uri = "http://" + Configuration.ServerAddress + ":7001/Wanderer/api/photos/get/" +pictureSHA256;

            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "GET";
            request.BeginGetResponse(new AsyncCallback(callback.ImageRequestCallback), request);
        }

        public static void SendRequestForCategories(CategoriesBudlesPage callback)
        {
            string uri = "http://" + Configuration.ServerAddress + ":7001/Wanderer/api/places/get/categories";

            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "GET";
            request.BeginGetResponse(new AsyncCallback(callback.CategoriesRequestCallback), request);
        }

        public static void SendRequestForPlacesWithCategory(CategoriesBudlesPage callback, String category)
        {
            string uri = "http://" + Configuration.ServerAddress + ":7001/Wanderer/api/places/get/category/";

            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri+category);
            request.Method = "GET";
            request.BeginGetResponse(new AsyncCallback(callback.PlacesRequestCallback), request);
        }

        public static void SendRequestForThumbnail(CategoriesBudlesPage callback, string pictureSHA256)
        {
            string uri = "http://" + Configuration.ServerAddress + ":7001/Wanderer/api/photos/get/thumbnail/" + pictureSHA256;
            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(callback.ThumbRequestCallback, request);
        }

        public static void SendRequestForPanorama(CategoriesBudlesPage callback, string pictureSHA256)
        {
            string uri = "http://" + Configuration.ServerAddress + ":7001/Wanderer/api/photos/get/" + pictureSHA256;

            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "GET";
            request.BeginGetResponse(new AsyncCallback(callback.ImageRequestCallback), request);
        }


        public static void SendRequestForMetadataOfPlacesWithinRange(MapWithPlacesPage callback, double lon, double lat, int distance)
        {
            string longitude = Convert.ToString(lon).Replace(',', '.');
            string latitude = Convert.ToString(lat).Replace(',', '.');

            string uri = "http://" + Configuration.ServerAddress + ":7001/Wanderer/api/places/get/" + longitude + "/" + latitude + "/" + distance;

            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = "GET";
            request.BeginGetResponse(new AsyncCallback(callback.RequestCallback), request);
        }

        public static void SendRequestForThumbnail(MapWithPlacesPage callback, string pictureSHA256)
        {
            string uri = "http://" + Configuration.ServerAddress + ":7001/Wanderer/api/photos/get/thumbnail/" + pictureSHA256;
            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri);
            request.BeginGetResponse(callback.ThumbRequestCallback, request);
        }

        public static void SendRequestForPlacesWithCategoryForUpdate(CategoriesBudlesPage callback, String category)
        {
            string uri = "http://" + Configuration.ServerAddress + ":7001/Wanderer/api/places/get/category/";

            HttpWebRequest request =
                (HttpWebRequest)HttpWebRequest.Create(uri + category);
            request.Method = "GET";
            request.BeginGetResponse(new AsyncCallback(callback.PlacesUpdateRequestCallback), request);
        }
    }
}
