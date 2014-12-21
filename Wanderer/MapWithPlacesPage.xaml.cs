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

namespace Wanderer
{
    public partial class MapWithPlacesPage : PhoneApplicationPage
    {

        private GeoCoordinate _actualPosition;
        private List<ImageMetadata> _metadataList = new List<ImageMetadata>();

        public MapWithPlacesPage()
        {
            InitializeComponent();
            //na razie testowo ustawione na krakow
            _actualPosition = new GeoCoordinate(50.3,19.9);
            Map.CartographicMode = MapCartographicMode.Terrain;
            Map.Center = _actualPosition;
            //Map.ZoomLevel = 11;

            ShowLocationOnMap(_actualPosition);
            DAO.SendRequestForMetadataOfPlacesWithinRange(this, _actualPosition.Longitude, _actualPosition.Latitude, 100000000);
        }

        private void ShowLocationOnMap(GeoCoordinate position)
        {
            Ellipse myCircle = new Ellipse();
            myCircle.Fill = new SolidColorBrush(Colors.Blue);
            myCircle.Height = 20;
            myCircle.Width = 20;
            myCircle.Opacity = 50;

            MapOverlay myLocationOverlay = new MapOverlay();
            myLocationOverlay.Content = myCircle;
            myLocationOverlay.PositionOrigin = new System.Windows.Point(0.5, 0.5);
            myLocationOverlay.GeoCoordinate = position;

            MapLayer myLocationLayer = new MapLayer();
            myLocationLayer.Add(myLocationOverlay);

            Map.Layers.Add(myLocationLayer);
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
                    _metadataList.AddRange(parser.ParsePlacesJSON(json));
                    ShowPointsOnMap();

                }
                catch (WebException)
                {
                    return;
                }
            }
        }

        private void ShowPointsOnMap()
        {
            Debug.WriteLine(" Creating points overlay "+_metadataList.Count);
            Deployment.Current.Dispatcher.BeginInvoke(delegate
                {

                    MapLayer pointsLayer = new MapLayer();
                    foreach (ImageMetadata metadata in _metadataList)
                    {
                        Ellipse myCircle = new Ellipse();
                        myCircle.Fill = new SolidColorBrush(Colors.Red);
                        myCircle.Height = 20;
                        myCircle.Width = 20;
                        myCircle.Opacity = 50;
                        myCircle.Tap += myCircle_Tap;
                        myCircle.DataContext = metadata;

                        MapOverlay myPointOverlay = new MapOverlay();
                        myPointOverlay.Content = myCircle;
                        myPointOverlay.PositionOrigin = new System.Windows.Point(0.5, 0.5);
                        myPointOverlay.GeoCoordinate = new GeoCoordinate(metadata.Latitude, metadata.Longitude);

                        pointsLayer.Add(myPointOverlay);
                    }
                    Map.Layers.Add(pointsLayer);
                });
        }

        void myCircle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Debug.WriteLine(" Tap on point handler ");
            Ellipse ellipse = sender as Ellipse;
            ImageMetadata metadata = ellipse.DataContext as ImageMetadata;
            Debug.WriteLine(metadata.PictureDescription);
        }

    }
}