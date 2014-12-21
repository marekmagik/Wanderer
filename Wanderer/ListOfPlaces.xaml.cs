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
using System.ComponentModel;

namespace Wanderer
{
    public partial class ListOfPlaces : PhoneApplicationPage
    {
        private List<ImageMetadata> _places = new List<ImageMetadata>();
        public List<ImageMetadata> Places
        {
            get
            {
                return _places;
            }
            private set
            {
                _places = value;            
            }
        }
        private List<ImageMetadata> _notCachedPlaces;
        private List<ImageMetadata> _allPlaces;
        private int _actualIndex;
        private MainPage _mainPage;
        private int _increaseAmount = 1;
        private int _actualNumberOfElementsInList = 1;

        public ListOfPlaces(MainPage mainPage)
        {
            InitializeComponent();

            DataContext = this;

            _allPlaces = new List<ImageMetadata>();
            Places = IsolatedStorageDAO.getAllCachedMetadatas();

            UpdateDistanceForAllPlaces(GPSTracker.CurrentLongitude, GPSTracker.CurrentLatitude);

            foreach (ImageMetadata place in Places)
            {
                Debug.WriteLine("deb: " + place.CurrentDistance);
                LoadPhotoFromIsolatedStorage(place);
            }
            ReloadContent();

            _notCachedPlaces = new List<ImageMetadata>();
            DAO.SendRequestForMetadataOfPlacesWithinRange(this, GPSTracker.CurrentLongitude, GPSTracker.CurrentLatitude, Configuration.GPSRange);

            _actualIndex = 0;

            this._mainPage = mainPage;
        }

        public void ReloadContent()
        {
            PlacesListBox.ItemsSource = null;
            PlacesListBox.ItemsSource = Places;
                
           // NotifyPropertyChanged("Places");
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
                Places.Add(place);
                if (IsolatedStorageDAO.IsThumbnailCached(place.PictureSHA256))
                {
                    LoadPhotoFromIsolatedStorage(place);
                }
                else
                {
                    DAO.SendRequestForThumbnail(this, Places.ElementAt(_actualIndex).PictureSHA256);
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
                //ReloadContent();
                _actualIndex++;
                UpdateDistanceForAllPlaces(GPSTracker.CurrentLongitude, GPSTracker.CurrentLatitude);
                //ReloadContent();
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

                            ImageMetadata place = Places.ElementAt(_actualIndex);
                            IsolatedStorageDAO.CacheThumbnail(stream, place.Width, place.Height, place.PictureSHA256);

                            BitmapImage image = new BitmapImage();
                            image.SetSource(stream);
                            Debug.WriteLine(_actualIndex);
                            place.Image = image;

                            //ReloadContent();
                            Debug.WriteLine("elements returned " + Places.Count);
                            _actualIndex++;
                            ProcessNextPlace();
                        }
                        catch (WebException)
                        {
                            Debug.WriteLine("wyjatek wewnatrz UI!");
                            return;
                        }
                        UpdateDistanceForAllPlaces(GPSTracker.CurrentLongitude, GPSTracker.CurrentLatitude);
                        //ReloadContent();
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
                 
                    ReloadContent();
                //    PlacesListBox.ItemsSource = null;
                //    PlacesListBox.ItemsSource = Places;
                }
            });
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            lock (this)
            {
                if (_actualNumberOfElementsInList != _allPlaces.Count)
                {
                    _actualNumberOfElementsInList += _increaseAmount;
                    ProcessNextPlace();
                }
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
            lock (this)
            {
                foreach (ImageMetadata place in Places)
                {
                    place.UpdateDistance(currentLogitude, currentLatitude);
                }
                sortByDistance(_allPlaces);
                sortByDistance(Places);
            }
        }

        private void PlacesListBoxManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            Debug.WriteLine(" Handler ");
        }

        private void sortByDistance(List<ImageMetadata> listToSort)
        {
            listToSort.Sort(delegate(ImageMetadata x, ImageMetadata y)
            {
                if (x.CurrentDistance == null && y.CurrentDistance == null
                    || x.CurrentDistance.Equals(y.CurrentDistance))
                {
                    return 0;
                }
                else if (x.CurrentDistance == null)
                {
                    return -1;
                }
                else if (y.CurrentDistance == null)
                {
                    return 1;
                }
                else
                {
                    return Convert.ToDouble(x.CurrentDistance.Replace(" km", "")) > Convert.ToDouble(y.CurrentDistance.Replace(" km", "")) ? 1 : -1;
                }
            });
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                ReloadContent();
            });
        }

    }
}