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
using System.Threading;
using System.Collections.ObjectModel;

namespace Wanderer
{
    public partial class ListOfPlaces : PhoneApplicationPage, IThumbnailCallbackReceiver, INotifyPropertyChanged
    {
        private ObservablePlacesCollection _places = new ObservablePlacesCollection();
        public ObservablePlacesCollection Places
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
        private List<ImageMetadata> _invisiblePlaces = new List<ImageMetadata>();

        private List<ImageMetadata> _notCachedPlaces;
        private List<ImageMetadata> _allPlaces;
        private int _actualIndex;
        private MainPage _mainPage;
        private int _increaseAmount = 1;
        private int _actualNumberOfElementsInList = 1;
        static ImageMetadata placeWaitingForThumbnail = null;

        public event PropertyChangedEventHandler PropertyChanged;

        public ListOfPlaces(MainPage mainPage)
        {
            InitializeComponent();


            DataContext = this;

            _allPlaces = new List<ImageMetadata>();
            CopyElementsToObservableCollection(Places, IsolatedStorageDAO.getAllCachedMetadatas());

            foreach (ImageMetadata place in Places)
            {
                Debug.WriteLine("deb: " + place.CurrentDistance);
                LoadPhotoFromIsolatedStorage(place);
            }

            UpdateDistanceForAllPlaces(GPSTracker.CurrentLongitude, GPSTracker.CurrentLatitude);

            _notCachedPlaces = new List<ImageMetadata>();

            _actualIndex = 0;

            this._mainPage = mainPage;
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

                    foreach (ImageMetadata place in parser.ParsePlacesJSON(json))
                    {
                        if (!_invisiblePlaces.Contains(place) && !Places.Contains(place))
                        {
                            if (IsolatedStorageDAO.IsThumbnailCached(place.PictureSHA256))
                            {
                                LoadPhotoFromIsolatedStorage(place);
                            }
                            else
                            {
                                placeWaitingForThumbnail = place;
                                DAO.SendRequestForThumbnail(this, place.PictureSHA256);
                                while (placeWaitingForThumbnail != null) { }
                            }
                            _invisiblePlaces.Add(place);
                        }
                    }

                    UpdateDistanceForAllPlaces(GPSTracker.CurrentLongitude, GPSTracker.CurrentLatitude);

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
                if (!Places.Contains(place))
                {
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
                else
                {
                    _actualIndex++;
                    ProcessNextPlace();
                }
            }
        }

        public void LoadPhotoFromIsolatedStorage(ImageMetadata place)
        {
            Debug.WriteLine(" Loading from iso ");
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                WriteableBitmap bitmapImage = IsolatedStorageDAO.loadThumbnail(place.PictureSHA256);
                place.Image = bitmapImage;
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

                            //ImageMetadata place = Places.ElementAt(_actualIndex);
                            IsolatedStorageDAO.CacheThumbnail(stream, placeWaitingForThumbnail.Width, placeWaitingForThumbnail.Height, placeWaitingForThumbnail.PictureSHA256);

                            BitmapImage image = new BitmapImage();
                            image.SetSource(stream);
                            Debug.WriteLine(_actualIndex);
                            placeWaitingForThumbnail.Image = image;

                            Debug.WriteLine("elements returned " + Places.Count);
                            //_actualIndex++;
                            //ProcessNextPlace();

                        }
                        catch (WebException)
                        {
                            Debug.WriteLine("wyjatek wewnatrz UI!");
                            return;
                        }
                        placeWaitingForThumbnail = null;
                        //UpdateDistanceForAllPlaces(GPSTracker.CurrentLongitude, GPSTracker.CurrentLatitude);
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

            Configuration.UseGPS = false;
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
                }
            });
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            if (Configuration.WorkOnline)
            {
                DAO.SendRequestForMetadataOfPlacesWithinRange(this, GPSTracker.CurrentLongitude, GPSTracker.CurrentLatitude, Configuration.GPSRange);
            }
            /*
                lock (this)
                {
                    if (_actualNumberOfElementsInList != _allPlaces.Count)
                    {
                        _actualNumberOfElementsInList += _increaseAmount;
                        ProcessNextPlace();
                    }
                }
             */
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
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                foreach (ImageMetadata place in Places)
                {
                    place.UpdateDistance(currentLogitude, currentLatitude);
                }
                foreach (ImageMetadata place in _invisiblePlaces)
                {
                    place.UpdateDistance(currentLogitude, currentLatitude);
                }

                sortByDistance(Places);

                //            OnPropertyChanged("Places");
            });
        }

        private void sortByDistance(ObservablePlacesCollection listToSort)
        {
            /*            lock (this)
                        {
                            Dispatcher.BeginInvoke(delegate
                            {
             * 
             * 
            */

            for (int i = 0; i < _invisiblePlaces.Count; )
            {
                if (_invisiblePlaces.ElementAt(i).IsImageInDesiredRange)
                {
                    Places.Add(_invisiblePlaces.ElementAt(i));
                    _invisiblePlaces.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
            for (int i = 0; i < Places.Count; )
            {
                if (!Places.ElementAt(i).IsImageInDesiredRange)
                {
                    _invisiblePlaces.Add(Places.ElementAt(i));
                    Places.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            listToSort.Sort();
            //                });
            //           }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public void insertPlace(ImageMetadata place) { 
            if(!_invisiblePlaces.Contains(place) && !Places.Contains(place)){
                _invisiblePlaces.Add(place);
            }
            UpdateDistanceForAllPlaces(GPSTracker.CurrentLongitude, GPSTracker.CurrentLatitude);
        }

        public static void CopyElementsToObservableCollection(ObservableCollection<ImageMetadata> target, List<ImageMetadata> source)
        {
            foreach (ImageMetadata place in source)
            {
                target.Add(place);
            }
        }
    }
}