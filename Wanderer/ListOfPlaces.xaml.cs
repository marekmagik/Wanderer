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
    public partial class ListOfPlaces : PhoneApplicationPage, IThumbnailCallbackReceiver
    {
        private List<ImageMetadata> _places;
        private List<ImageMetadata> _notCachedPlaces;
        private List<ImageMetadata> _points;
        private List<ImageMetadata> _allPlaces;
        private int _actualIndex;
        private MainPage _mainPage;
        private int _increaseAmount = 1;
        private int _actualNumberOfElementsInList = 1;


        public ListOfPlaces(MainPage mainPage)
        {
            InitializeComponent();
            _places = new List<ImageMetadata>();
            _allPlaces = new List<ImageMetadata>();
            _notCachedPlaces = new List<ImageMetadata>();
            DAO.SendRequestForMetadataOfPlacesWithinRange(this, 20.5, 40.6, 100000000);
            this.DataContext = _places;
            _actualIndex = 0;

            this._mainPage = mainPage;
        }

        public void ReloadContent()
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    PlacesListBox.ItemsSource = null;
                    PlacesListBox.ItemsSource = _places;
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

                    Debug.WriteLine("---JSON, req : " + json);

                    IsolatedStorageDAO.CacheMetadata(json);
                    JSONParser parser = new JSONParser();
                    _allPlaces = parser.ParsePlacesJSON(json);

                    if (_allPlaces.Count > 0)
                    {
                        _actualIndex = 0;
                        ProcessNextPlace();
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

        private void ProcessNextPlace()
        {
            if (_actualIndex < _actualNumberOfElementsInList)
            {
                ImageMetadata place = _allPlaces.ElementAt(_actualIndex);
                _places.Add(place);
                if (IsolatedStorageDAO.IsThumbnailCached(place.PictureSHA256))
                {
                    LoadPhotoFromIsolatedStorage(place);
                }
                else
                {
                    DAO.SendRequestForThumbnail(this, _places.ElementAt(_actualIndex).PictureSHA256);
                }
            }
        }

        private void LoadPhotoFromIsolatedStorage(ImageMetadata place)
        {
            Debug.WriteLine(" Loading from iso ");
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                WriteableBitmap bitmapImage = IsolatedStorageDAO.loadThumbnail(place.PictureSHA256);
                place.Image = bitmapImage;
                ReloadContent();
                _actualIndex++;
                UpdateDistanceForAllPlaces(GPSTracker.CurrentLongitude, GPSTracker.CurrentLatitude);
            });
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

                            ImageMetadata place = _places.ElementAt(_actualIndex);
                            IsolatedStorageDAO.CacheThumbnail(stream, place.Width, place.Height, place.PictureSHA256);

                            BitmapImage image = new BitmapImage();
                            image.SetSource(stream);
                            Debug.WriteLine(_actualIndex);
                            place.Image = image;

                            ReloadContent();
                            Debug.WriteLine("elements returned " + _places.Count);
                            _actualIndex++;
                            ProcessNextPlace();
                        }
                        catch (WebException)
                        {
                            Debug.WriteLine("wyjatek wewnatrz UI!");
                            return;
                        }
                        UpdateDistanceForAllPlaces(GPSTracker.CurrentLongitude, GPSTracker.CurrentLatitude);
                    });
            }
        }

        private void PlacesListBoxImageTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            FrameworkElement element = (FrameworkElement)sender;
            ImageMetadata image = (ImageMetadata)element.DataContext;
            Uri uri = new Uri("/PanoramaView.xaml?&hash=" + image.PictureSHA256, UriKind.Relative);

            Debug.WriteLine("HASH: " + image.PictureSHA256);
            PlacesListBox.SelectedIndex = -1;
            _mainPage.NavigationService.Navigate(uri);
        }

        private void PlacesListBoxTextTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                if (sender != null)
                {
                    FrameworkElement element = (FrameworkElement)sender;
                    ImageMetadata metadata = (ImageMetadata)element.DataContext;
                    metadata.ToggleDescriptions();
                    PlacesListBox.ItemsSource = null;
                    PlacesListBox.ItemsSource = _places;
                }
            });
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            if (_actualNumberOfElementsInList != _allPlaces.Count)
            {
                _actualNumberOfElementsInList += _increaseAmount;
                ProcessNextPlace();
            }
        }

        private void PlacesListBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (_allPlaces.Count > 0)
            {
                if (_actualNumberOfElementsInList != _allPlaces.Count)
                {
                    _actualNumberOfElementsInList += _increaseAmount;
                    ProcessNextPlace();
                }
            }
        }

        public void UpdateDistanceForAllPlaces(double currentLogitude, double currentLatitude)
        {
            foreach (ImageMetadata place in _places)
            {
                place.UpdateDistance(currentLogitude, currentLatitude);
            }
        }

        private void PlacesListBoxManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            Debug.WriteLine(" Handler ");
        }

    }
}