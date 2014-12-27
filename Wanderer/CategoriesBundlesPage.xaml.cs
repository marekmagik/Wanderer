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
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Wanderer
{
    public partial class CategoriesBudlesPage : PhoneApplicationPage
    {
        private const String ActionDownloadThumbnail = "thumb";
        private const String _actionDownloadPanorama = "pano";

        private List<String> _categories = new List<String>();
        private IEnumerator<ImageMetadata> _metadataEnumerator = null;
        private Boolean _isDownloadingInProgress = false;
        private Button _hiddenButton = null;
        private ProgressBar _progressBar = null;
        private Grid _parentGrid = null;
        private String _actualCategory;
        private List<String> _cachedCategories;
        private List<ImageMetadata> _metadataForActualCategory;

        private IEnumerator<String> _categoriesEnumerator = null;
        private Dictionary<String, Grid> _gridMap = new Dictionary<string, Grid>();
        private ListOfPlaces _listOfPlaces;

        public CategoriesBudlesPage(ListOfPlaces listOfPlaces)
        {
            this._listOfPlaces = listOfPlaces;
            InitializeComponent();
            CategoriesListBox.DataContext = _categories;
            if (Configuration.WorkOnline)
            {
                DAO.SendRequestForCategories(this);
            }
        }


        public void CategoriesRequestCallback(IAsyncResult result)
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
                        StreamReader streamReader = new StreamReader(stream);
                        string json = streamReader.ReadToEnd();

                        JSONParser parser = new JSONParser();
                        _categories.AddRange(parser.ParceCategoriesJSON(json));

                        CategoriesListBox.DataContext = null;
                        CategoriesListBox.DataContext = _categories;


                        Debug.WriteLine("---JSON, req : " + json);
                    }
                    catch (WebException)
                    {
                        //HandleWebException();
                        Debug.WriteLine("wyjatek wewnatrz UI!");
                        return;
                    }
                    _isDownloadingInProgress = false;
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

                    PerformNearestAction(_actionDownloadPanorama);
                    //DAO.SendRequestForPanorama(this, place.PictureSHA256);

                    //_listOfPlaces.insertPlace(place);

                }
                catch (WebException)
                {
                    HandleWebException();
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

                    _listOfPlaces.LoadPhotoFromIsolatedStorage(place);
                    _listOfPlaces.insertPlace(place);

                    if (_metadataEnumerator.MoveNext())
                    {
                        // DAO.SendRequestForThumbnail(this, _metadataEnumerator.Current.PictureSHA256);
                        PerformNearestAction(ActionDownloadThumbnail);
                    }
                    else
                    {
                        _isDownloadingInProgress = false;
                        ChangeUIElements();
                        IsolatedStorageDAO.CacheCategory(_actualCategory, _metadataForActualCategory);
                    }

                }
                catch (WebException)
                {
                    HandleWebException();
                    Debug.WriteLine("wyjatek wewnatrz UI!");
                    return;
                }
            }
        }

        private void ChangeUIElements()
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                SetCategoryCached(_parentGrid, _hiddenButton, _progressBar);
            });
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
                    _metadataForActualCategory = parser.ParsePlacesJSON(json);


                    IsolatedStorageDAO.CacheMetadata(json);

                    _metadataEnumerator = _metadataForActualCategory.GetEnumerator();
                    if (_metadataEnumerator.MoveNext())
                    {
                        Debug.WriteLine(" Sending request for thumbnail ");
                        try
                        {
                            PerformNearestAction(ActionDownloadThumbnail);
                            //DAO.SendRequestForThumbnail(this, _metadataEnumerator.Current.PictureSHA256);
                        }
                        catch (WebException)
                        {
                            HandleWebException();
                        }
                    }

                });
            }
        }

        private void PerformNearestAction(String action)
        {
            bool shouldRunning = true;
            String currentAction = action;
            while (shouldRunning)
            {
                ImageMetadata metadata = _metadataEnumerator.Current;
                if (currentAction.Equals(ActionDownloadThumbnail))
                {
                    if (!IsolatedStorageDAO.IsThumbnailCached(metadata.PictureSHA256))
                    {
                        DAO.SendRequestForThumbnail(this, metadata.PictureSHA256);
                        shouldRunning = false;
                    }
                    else
                        currentAction = _actionDownloadPanorama;
                }
                else if (currentAction.Equals(_actionDownloadPanorama))
                {
                    if (!IsolatedStorageDAO.IsPhotoCached(metadata.PictureSHA256))
                    {
                        DAO.SendRequestForPanorama(this, metadata.PictureSHA256);
                        shouldRunning = false;
                    }
                    else
                    {
                        if (_metadataEnumerator.MoveNext())
                        {
                            currentAction = ActionDownloadThumbnail;
                        }
                        else
                        {
                            //_listOfPlaces.insertPlace(metadata);
                            _isDownloadingInProgress = false;
                            ChangeUIElements();
                            IsolatedStorageDAO.CacheCategory(_actualCategory, _metadataForActualCategory);
                            shouldRunning = false;
                        }
                    }
                }
            }
        }

        private void DownloadBundleClick(object sender, RoutedEventArgs e)
        {
            if (!_isDownloadingInProgress)
            {
                _isDownloadingInProgress = true;
                Button button = sender as Button;
                String category = (String)button.DataContext;
                _actualCategory = category;
                CreateProgressBar(button, category);
                Debug.WriteLine(" Selected category: " + category);

                try
                {
                    DAO.SendRequestForPlacesWithCategory(this, category);
                }
                catch (WebException)
                {
                    HandleWebException();
                }
            }
        }

        private void MarkBundlesToUpdate(object sender, RoutedEventArgs e)
        {
            if (_categories.Count == 0)
            {
                if (!_isDownloadingInProgress && Configuration.WorkOnline)
                {
                    _isDownloadingInProgress = true;
                    DAO.SendRequestForCategories(this);
                }
                return;
            }

            List<String> categoriesToCheck = new List<String>();
            foreach (String category in _categories)
            {
                if (IsCategoryReallyCached(category))
                    categoriesToCheck.Add(category);
            }
            _categoriesEnumerator = categoriesToCheck.GetEnumerator();
            if (_categoriesEnumerator.MoveNext())
            {
                DAO.SendRequestForPlacesWithCategoryForUpdate(this, _categoriesEnumerator.Current);
            }

        }

        private bool IsCategoryReallyCached(String category)
        {
            bool isReallyCached = false;
            if (IsolatedStorageDAO.IsCategoryCached(category))
            {
                List<String> hashes = IsolatedStorageDAO.GetCachedPlacesForCategory(category);
                isReallyCached = true;
                foreach (String hash in hashes)
                {
                    if (!IsolatedStorageDAO.IsPhotoCached(hash))
                        isReallyCached = false;
                    if (!IsolatedStorageDAO.IsThumbnailCached(hash))
                        isReallyCached = false;
                    if (!IsolatedStorageDAO.IsMetadataCached(hash))
                        isReallyCached = false;
                }
            }
            return isReallyCached;
        }

        private void UpdateBundleClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(" Update ");
        }

        private void HandleWebException()
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                _isDownloadingInProgress = false;
                _parentGrid.Children.Remove(_progressBar);
                _hiddenButton.Visibility = Visibility.Visible;
            });
        }

        private void CreateProgressBar(Button button, string category)
        {
            button.Visibility = Visibility.Collapsed;
            Grid grid = (Grid)button.Parent;
            ProgressBar progressBar = new ProgressBar();
            progressBar.IsEnabled = true;
            progressBar.IsIndeterminate = true;
            progressBar.Visibility = Visibility.Visible;
            progressBar.Height = button.ActualHeight;
            progressBar.HorizontalAlignment = button.HorizontalAlignment;
            progressBar.Width = button.ActualWidth;

            _parentGrid = grid;
            _progressBar = progressBar;
            _hiddenButton = button;

            grid.Children.Add(progressBar);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Grid grid = (Grid)sender;
            TextBlock textBlock = (TextBlock)grid.Children[0];
            if (!_gridMap.ContainsKey(textBlock.Text))
                _gridMap.Add(textBlock.Text, grid);
            if (grid.Children[1] is Button)
            {
                Button button = (Button)grid.Children[1];
                String category = textBlock.Text;
                Debug.WriteLine(textBlock.Text);
                if (IsolatedStorageDAO.IsCategoryCached(category))
                {
                    List<String> hashes = IsolatedStorageDAO.GetCachedPlacesForCategory(category);
                    bool isReallyCached = true;
                    foreach (String hash in hashes)
                    {
                        if (!IsolatedStorageDAO.IsPhotoCached(hash))
                            isReallyCached = false;
                        if (!IsolatedStorageDAO.IsThumbnailCached(hash))
                            isReallyCached = false;
                        if (!IsolatedStorageDAO.IsMetadataCached(hash))
                            isReallyCached = false;
                    }

                    if (isReallyCached)
                        SetCategoryCached(grid, button, null);
                    else
                    {
                        button.Content = "Aktualizuj";
                        //button.Click -= DownloadBundleClick;
                        //button.Click += UpdateBundleClick;
                    }
                }
            }
        }

        private void SetCategoryCached(Grid grid, Button button, ProgressBar progressBar)
        {
            grid.Height = grid.ActualHeight;

            if (progressBar != null)
                grid.Children.Remove(progressBar);

            Image image = new Image();
            image.HorizontalAlignment = button.HorizontalAlignment;
            image.Width = button.ActualHeight / 2;
            image.Height = button.ActualHeight / 2;
            image.Source = new BitmapImage(new Uri("/Images/PanoramaCached.png", UriKind.Relative));
            image.Margin = new Thickness(0, 0, 10.0, 0);
            grid.Children.Remove(button);

            grid.Children.Add(image);
        }

        internal void PlacesUpdateRequestCallback(IAsyncResult result)
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
                    List<ImageMetadata> metadatas = parser.ParsePlacesJSON(json);

                    List<String> cachedHashes = IsolatedStorageDAO.GetCachedPlacesForCategory(_categoriesEnumerator.Current);

                    Boolean toUpdate = false;
                    foreach (ImageMetadata metadata in metadatas)
                    {
                        if (!cachedHashes.Contains(metadata.PictureSHA256))
                            toUpdate = true;
                    }

                    if (toUpdate)
                        MarkCategoryToUpdate(_categoriesEnumerator.Current);

                    if (_categoriesEnumerator.MoveNext())
                        DAO.SendRequestForPlacesWithCategoryForUpdate(this, _categoriesEnumerator.Current);
                });
            }
        }

        private void MarkCategoryToUpdate(String category)
        {
            Grid grid = _gridMap[category];
            grid.Children.RemoveAt(1);
            Button button = new Button();
            button.Content = "Aktualizuj";
            button.Click += DownloadBundleClick;
            button.HorizontalAlignment = HorizontalAlignment.Right;
            grid.Children.Add(button);
            Debug.WriteLine(" category to update " + category);
        }
    }
}