﻿using System;
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

        private List<String> _categories = new List<String>();
        private IEnumerator<ImageMetadata> _metadataEnumerator = null;
        private Boolean _isDownloadingInProgress = false;
        private Button _hiddenButton = null;
        private ProgressBar _progressBar = null;
        private Grid _parentGrid = null;
        private String _actualCategory;
        private int _countOfCategory;
        private List<String> _cachedCategories;

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
                    }catch (WebException)
                {
                    //HandleWebException();
                    Debug.WriteLine("wyjatek wewnatrz UI!");
                    return;
                }
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
                    _countOfCategory++;
                    if (_metadataEnumerator.MoveNext())
                    {
                            DAO.SendRequestForThumbnail(this, _metadataEnumerator.Current.PictureSHA256);
                    }
                    else
                    {
                        _isDownloadingInProgress = false;
                        ChangeUIElements();
                        IsolatedStorageDAO.CacheCategory(_actualCategory, _countOfCategory);
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
                    List<ImageMetadata> metadataList = parser.ParsePlacesJSON(json);
                    IsolatedStorageDAO.CacheMetadata(json);

                    _metadataEnumerator = metadataList.GetEnumerator();
                    if (_metadataEnumerator.MoveNext())
                    {
                        Debug.WriteLine(" Sending request for thumbnail ");
                        try
                        {
                            DAO.SendRequestForThumbnail(this, _metadataEnumerator.Current.PictureSHA256);
                        }
                        catch (WebException)
                        {
                            HandleWebException();
                        }
                    }

                });
            }
        }

        private void DownloadBundleClick(object sender, RoutedEventArgs e)
        {
            if (!_isDownloadingInProgress)
            {
                _countOfCategory = 0;
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
            Button button = (Button)grid.Children[1];
            Debug.WriteLine(textBlock.Text);
            if (IsolatedStorageDAO.IsCategoryCached(textBlock.Text))
            {
                SetCategoryCached(grid, button, null);
            }
        }

        private void SetCategoryCached(Grid grid, Button button, ProgressBar progressBar)
        {
            grid.Height = grid.ActualHeight;

            if(progressBar!=null)
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
    }
}