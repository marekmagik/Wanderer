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
using System.Diagnostics;

namespace Wanderer
{
    public partial class CategoriesBudlesPage : PhoneApplicationPage
    {

        public List<String> _categories = new List<String>();
        public IEnumerator<ImageMetadata> _metadataEnumerator = null; 

        public CategoriesBudlesPage()
        {

            InitializeComponent();
            CategoriesListBox.DataContext = _categories;
            DAO.SendRequestForCategories(this);

        }


        public void CategoriesRequestCallback(IAsyncResult result)
        {
            HttpWebRequest request = result.AsyncState as HttpWebRequest;
            if (request != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    WebResponse response = request.EndGetResponse(result);
                    Stream stream = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(stream);
                    string json = streamReader.ReadToEnd();

                    JSONParser parser = new JSONParser();
                    _categories.AddRange(parser.ParceCategoriesJSON(json));

                    CategoriesListBox.DataContext = null;
                    CategoriesListBox.DataContext = _categories;

                    Debug.WriteLine("---JSON, req : " + json);
                });
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

                    ImageMetadata place = _metadataEnumerator.Current;
                    IsolatedStorageDAO.CacheThumbnail(stream, place.Width, place.Height, place.PictureSHA256);

                    DAO.SendRequestForPanorama(this, place.PictureSHA256);

                }
                catch (WebException)
                {
                    Debug.WriteLine("wyjatek wewnatrz UI!");
                    return;
                }
            }
        }

        public void ImageRequestCallback(IAsyncResult result)
        {
            HttpWebRequest request = result.AsyncState as HttpWebRequest;
            if (request != null)
            {
                try
                {
                    WebResponse response = request.EndGetResponse(result);
                    Stream stream = response.GetResponseStream();

                    ImageMetadata place = _metadataEnumerator.Current;
                    IsolatedStorageDAO.CachePhoto(stream, place.Width, place.Height, place);

                    if (_metadataEnumerator.MoveNext())
                        DAO.SendRequestForThumbnail(this, _metadataEnumerator.Current.PictureSHA256);

                }
                catch (WebException)
                {
                    Debug.WriteLine("wyjatek wewnatrz UI!");
                    return;
                }
            }
        }

        public void PlacesRequestCallback(IAsyncResult result)
        {
            HttpWebRequest request = result.AsyncState as HttpWebRequest;
            if (request != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    WebResponse response = request.EndGetResponse(result);
                    Stream stream = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(stream);
                    string json = streamReader.ReadToEnd();

                    Debug.WriteLine("---JSON, req : " + json);

                    JSONParser parser = new JSONParser();
                    List<ImageMetadata> metadataList = parser.ParsePlacesJSON(json);
                    IsolatedStorageDAO.CacheMetadata(json);

                    _metadataEnumerator = metadataList.GetEnumerator();
                    if (_metadataEnumerator.MoveNext())
                    {
                        Debug.WriteLine(" Sending request for thumbnail ");
                        DAO.SendRequestForThumbnail(this, _metadataEnumerator.Current.PictureSHA256);
                    }

                });
            }
        }

        private void DownloadBundleClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            String category = (String)button.DataContext;
            Debug.WriteLine(" Selected category: " + category);
            DAO.SendRequestForPlacesWithCategory(this, category);
        }
    }
}