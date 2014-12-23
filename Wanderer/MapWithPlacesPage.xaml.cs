using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Shapes;
using System.Windows.Media;
using Microsoft.Phone.Maps.Controls;
using System.Device.Location;
using System.IO;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace Wanderer
{
    public partial class MapWithPlacesPage : PhoneApplicationPage
    {

        private GeoCoordinate _actualPosition;
        private List<ImageMetadata> _metadataList = new List<ImageMetadata>();
        private ImageMetadata _selectedMetadata;
        private MainPage _mainPage;
        private MapOverlay myLocationOverlay = null;
        private Dictionary<ImageMetadata, MapOverlay> _pointOnMapDictionary = new Dictionary<ImageMetadata, MapOverlay>();
        private MapLayer _pointsLayer = new MapLayer();

        public int Count 
        {
            get
            {
                return _metadataList.Count;
            }
        }

        public MapWithPlacesPage(MainPage mainPage)
        {
            InitializeComponent();

            _mainPage = mainPage;
            Map.CartographicMode = MapCartographicMode.Terrain;
            if (GPSTracker.CurrentLatitude != GPSTracker.LocationUnknown && GPSTracker.CurrentLongitude != GPSTracker.LocationUnknown)
            {
                _actualPosition = new GeoCoordinate(GPSTracker.CurrentLatitude, GPSTracker.CurrentLongitude);
                ShowLocationOnMap(_actualPosition);
            }
            
        }

        private void ShowLocationOnMap(GeoCoordinate position)
        {
            Map.Center = _actualPosition;
            Map.ZoomLevel = 10;

            Ellipse myCircle = new Ellipse();
            myCircle.Fill = new SolidColorBrush(Colors.Blue);
            myCircle.Height = 20;
            myCircle.Width = 20;
            myCircle.Opacity = 50;

            myLocationOverlay = new MapOverlay();
            myLocationOverlay.Content = myCircle;
            myLocationOverlay.PositionOrigin = new System.Windows.Point(0.5, 0.5);
            myLocationOverlay.GeoCoordinate = position;

            MapLayer myLocationLayer = new MapLayer();
            myLocationLayer.Add(myLocationOverlay);

            Map.Layers.Add(myLocationLayer);

            DAO.SendRequestForMetadataOfPlacesWithinRange(this, _actualPosition.Longitude, _actualPosition.Latitude, Configuration.GPSRange);
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
                    JSONParser parser = new JSONParser();
                    if (_metadataList.Count == 0)
                    {
                        _metadataList.AddRange(parser.ParsePlacesJSON(json));
                        ShowPointsOnMap();
                    }
                    else
                    {
                        NotifyPlacesListUpdated(parser.ParsePlacesJSON(json));
                    }
                    

                }
                catch (WebException)
                {
                    _metadataList.AddRange(IsolatedStorageDAO.getAllCachedMetadatas());
                    ShowPointsOnMap();
                    return;
                }
            }
        }

        private void ShowPointsOnMap()
        {
            Debug.WriteLine(" Creating points overlay " + _metadataList.Count);
            Deployment.Current.Dispatcher.BeginInvoke(delegate
                {


                    foreach (ImageMetadata metadata in _metadataList)
                    {
                        Ellipse myCircle = new Ellipse();
                        myCircle.Fill = new SolidColorBrush(Colors.Red);
                        myCircle.Height = 20;
                        myCircle.Width = 20;
                        myCircle.Opacity = 50;
                        myCircle.Tap += ShowContextMenu;
                        myCircle.DataContext = metadata;

                        MapOverlay myPointOverlay = new MapOverlay();
                        myPointOverlay.Content = myCircle;
                        myPointOverlay.PositionOrigin = new System.Windows.Point(0.5, 0.5);
                        myPointOverlay.GeoCoordinate = new GeoCoordinate(metadata.Latitude, metadata.Longitude);

                        _pointOnMapDictionary.Add(metadata, myPointOverlay);

                        _pointsLayer.Add(myPointOverlay);
                    }
                    Map.Layers.Add(_pointsLayer);
                });
        }

        void ShowContextMenu(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ContextMenu.Width = MainGrid.ActualWidth - 10;
            Thumbnail.Source = null;

            Debug.WriteLine(" Tap on point handler ");
            Ellipse ellipse = sender as Ellipse;
            ImageMetadata metadata = ellipse.DataContext as ImageMetadata;
            _selectedMetadata = metadata;

            if(IsolatedStorageDAO.IsThumbnailCached(metadata.PictureSHA256)){
                WriteableBitmap image = IsolatedStorageDAO.loadThumbnail(metadata.PictureSHA256);
                Thumbnail.Source = image;
            }
            else if(Configuration.WorkOnline)
                DAO.SendRequestForThumbnail(this, metadata.PictureSHA256);

            Debug.WriteLine(metadata.PictureDescription);
            PrimaryDescription.Text = metadata.PictureDescription;
            SecondaryDescription.Text = metadata.PictureAdditionalDescription;
            CategoryTextBlock.Text = metadata.Category;
            ContextMenu.Visibility = Visibility.Visible;
        }

        private void HideContextMenu(object sender, RoutedEventArgs e)
        {
            ContextMenu.Visibility = Visibility.Collapsed;
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
                        IsolatedStorageDAO.CacheThumbnail(stream,_selectedMetadata.Width,_selectedMetadata.Height ,_selectedMetadata.PictureSHA256);
                        BitmapImage image = new BitmapImage();
                        image.SetSource(stream);
                        

                        Thumbnail.Source = image;
                    }
                    catch (WebException)
                    {
                        Debug.WriteLine("wyjatek wewnatrz UI!");
                        return;
                    }
                });
            }
        }

        private void ShowPanorama(object sender, RoutedEventArgs e)
        {
            ContextMenu.Visibility = Visibility.Collapsed;
            Uri uri = new Uri("/PanoramaView.xaml?&hash=" + _selectedMetadata.PictureSHA256, UriKind.Relative);
            _mainPage.NavigationService.Navigate(uri);
        }

        public void NotifyGeolocatorPositionChanged(double longitude, double latitude)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    if (myLocationOverlay == null)
                    {
                        _actualPosition = new GeoCoordinate(latitude, longitude);
                        ShowLocationOnMap(_actualPosition);
                    }
                    else
                        myLocationOverlay.GeoCoordinate = new GeoCoordinate(latitude, longitude);
                });
        }

        public void NotifyPlacesListUpdated(List<ImageMetadata> newList)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    List<ImageMetadata> toRemove = new List<ImageMetadata>();

                    foreach (ImageMetadata metadata in _pointOnMapDictionary.Keys)
                    {
                        if (!newList.Contains(metadata))
                        {
                            _pointsLayer.Remove(_pointOnMapDictionary[metadata]);
                            toRemove.Add(metadata);

                        }
                    }

                    foreach (ImageMetadata metadata in toRemove)
                        _pointOnMapDictionary.Remove(metadata);

                    foreach (ImageMetadata metadata in newList)
                    {
                        if (!_pointOnMapDictionary.Keys.Contains(metadata))
                        {
                            Ellipse myCircle = new Ellipse();
                            myCircle.Fill = new SolidColorBrush(Colors.Red);
                            myCircle.Height = 20;
                            myCircle.Width = 20;
                            myCircle.Opacity = 50;
                            myCircle.Tap += ShowContextMenu;
                            myCircle.DataContext = metadata;

                            MapOverlay myPointOverlay = new MapOverlay();
                            myPointOverlay.Content = myCircle;
                            myPointOverlay.PositionOrigin = new System.Windows.Point(0.5, 0.5);
                            myPointOverlay.GeoCoordinate = new GeoCoordinate(metadata.Latitude, metadata.Longitude);

                            _pointOnMapDictionary.Add(metadata, myPointOverlay);

                            _pointsLayer.Add(myPointOverlay);
                        }
                    }
                });
        }
    }
}