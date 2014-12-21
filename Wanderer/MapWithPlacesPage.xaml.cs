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

namespace Wanderer
{
    public partial class MapWithPlacesPage : PhoneApplicationPage
    {
        public MapWithPlacesPage()
        {
            InitializeComponent();
            //na razie testowo ustawione na krakow
            Map.CartographicMode = MapCartographicMode.Terrain;
            Map.Center = new GeoCoordinate(50.03, 19.9);
            Map.ZoomLevel = 11;

            ShowLocationOnMap();
        }

        private void ShowLocationOnMap()
        {
            Ellipse myCircle = new Ellipse();
            myCircle.Fill = new SolidColorBrush(Colors.Blue);
            myCircle.Height = 20;
            myCircle.Width = 20;
            myCircle.Opacity = 50;

            MapOverlay myLocationOverlay = new MapOverlay();
            myLocationOverlay.Content = myCircle;
            myLocationOverlay.PositionOrigin = new System.Windows.Point(0.5, 0.5);
            myLocationOverlay.GeoCoordinate = new GeoCoordinate(50.03, 19.9);

            MapLayer myLocationLayer = new MapLayer();
            myLocationLayer.Add(myLocationOverlay);

            Map.Layers.Add(myLocationLayer);
        }
    }
}