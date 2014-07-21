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
    public partial class ListOfPlaces : PhoneApplicationPage
    {
        //private List<Place> places;
        private List<ImageMetadata> places;
        private List<ImageMetadata> notCachedPlaces;
        private List<ImageMetadata> points;
        private List<ImageMetadata> allPlaces;
        //private Place actualPlace;
        //private DAO dao;
        private int actualIndex;
        private MainPage mainPage;
        private int increaseAmount = 1;
        private int actualNumberOfElementsInList = 1;


        public ListOfPlaces(MainPage mainPage)
        {
            InitializeComponent();
            places = new List<ImageMetadata>();
            allPlaces = new List<ImageMetadata>();
            notCachedPlaces = new List<ImageMetadata>();
            DAO.GetDataFromServer(this, 20.5, 40.6, 100000000);
            this.DataContext = places;
            actualIndex = 0;

            this.mainPage = mainPage;
        }


        public void ReloadContent()
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        PlacesListBox.ItemsSource = null;
                        PlacesListBox.ItemsSource = places;
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

                    JSONParser parser = new JSONParser();
                    allPlaces = parser.ParsePlacesJSON(json);

                    if (allPlaces.Count > 0)
                    {
                        actualIndex = 0;
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
            if (actualIndex != actualNumberOfElementsInList)
            {
                ImageMetadata place = allPlaces.ElementAt(actualIndex);
                places.Add(place);
                if (IsolatedStorageDAO.IsThumbnailCached(place.PictureSHA256))
                {
                    LoadPhotoFromIsolatedStorage(place);
                }
                else
                {
                    DAO.LoadImage(this, places.ElementAt(actualIndex).IdInDatabase);
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
                actualIndex++;
                ProcessNextPlace();
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

                            ImageMetadata place = places.ElementAt(actualIndex);
                            IsolatedStorageDAO.CacheThumbnail(stream, place.Width, place.Height, place.PictureSHA256);

                            BitmapImage image = new BitmapImage();
                            image.SetSource(stream);
                            Debug.WriteLine(actualIndex);
                            place.Image = image;

                            ReloadContent();
                            Debug.WriteLine("elemebts returned " + places.Count);
                            actualIndex++;
                            ProcessNextPlace();
                        }
                        catch (WebException)
                        {
                            Debug.WriteLine("wyjatek wewnatrz UI!");
                            return;
                        }

                    });

            }
        }

        void PlacesListBox_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            if (PlacesListBox.SelectedIndex != -1)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Uri uri = new Uri("/PanoramaView.xaml?photoID=" + places.ElementAt(PlacesListBox.SelectedIndex).IdInDatabase + "&hash=" + places.ElementAt(PlacesListBox.SelectedIndex).PictureSHA256 + "&useLocalDatabase=false", UriKind.Relative);

                Debug.WriteLine("photo_id=" + places.ElementAt(PlacesListBox.SelectedIndex).IdInDatabase);
                Debug.WriteLine("HASH: " + places.ElementAt(PlacesListBox.SelectedIndex).PictureSHA256);
                //   NavigationService.Navigate(new Uri("/PanoramaView.xaml?photoID=" + places.ElementAt(PlacesListBox.SelectedIndex).IdInDatabase + "&hash="+places.ElementAt(PlacesListBox.SelectedIndex).PictureSHA256+"&useLocalDatabase=false", UriKind.Relative));
                if (NavigationService == null)
                {
                    Debug.WriteLine("Ni huja");
                }
                //MainPage.navigateToPage(uri);
                PlacesListBox.SelectedIndex = -1;
                mainPage.NavigationService.Navigate(uri);
                //  navigationService.Navigate(uri);
            }
        }

        private void PlacesListBox_ImageTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            FrameworkElement element = (FrameworkElement)sender;
            ImageMetadata image = (ImageMetadata)element.DataContext;
            Uri uri = new Uri("/PanoramaView.xaml?photoID=" + image.IdInDatabase + "&hash=" + image.PictureSHA256 + "&useLocalDatabase=false", UriKind.Relative);

            Debug.WriteLine("photo_id=" + image.IdInDatabase);
            Debug.WriteLine("HASH: " + image.PictureSHA256);
            //   NavigationService.Navigate(new Uri("/PanoramaView.xaml?photoID=" + places.ElementAt(PlacesListBox.SelectedIndex).IdInDatabase + "&hash="+places.ElementAt(PlacesListBox.SelectedIndex).PictureSHA256+"&useLocalDatabase=false", UriKind.Relative));
            if (NavigationService == null)
            {
                Debug.WriteLine("Ni huja");
            }
            //MainPage.navigateToPage(uri);
            PlacesListBox.SelectedIndex = -1;
            mainPage.NavigationService.Navigate(uri);
        }

        private void PlacesListBox_TextTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                if (sender != null)
                {
                    FrameworkElement element = (FrameworkElement)sender;
                    ImageMetadata image = (ImageMetadata)element.DataContext;
                    image.PictureAdditionalDescription = "hejoo";
                    PlacesListBox.ItemsSource = null;
                    PlacesListBox.ItemsSource = places;
                }
                //Debug.WriteLine(image);
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (actualNumberOfElementsInList != allPlaces.Count)
            {
                actualNumberOfElementsInList += increaseAmount;
                ProcessNextPlace();
            }
        }

    }
}