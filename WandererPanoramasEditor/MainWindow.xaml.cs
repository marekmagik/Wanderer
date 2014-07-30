using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.Diagnostics;

namespace WandererPanoramasEditor
{

    public partial class MainWindow : Window
    {

        private ImageSource PanoramaImage { get; set; }
        private double _height;
        private double _width;
        private double _realX;
        private double _realY;
        private double _currentScale;
        private bool _crossMovingInProgress;
        private double _marginToTopOfCanvas;
        private double _marginToLeftOfCanvas;
        private ImageMetadata _metadata;
        private Point _selectedPoint;
        private string _imageFileName;

        public MainWindow()
        {
            InitializeComponent();

            this.Width = System.Windows.SystemParameters.WorkArea.Width;
            this.Height = System.Windows.SystemParameters.WorkArea.Height + 6;
            this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Left;
            this.Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Top - 3;
            this.ResizeMode = ResizeMode.CanMinimize;
            this.WindowState = WindowState.Normal;


            this.Closing += new CancelEventHandler(closeApplication);
            Panorama.DataContext = PanoramaImage;
        }

        private void closeApplication(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }

        private void loadPanoramaFile(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Zdjęcia|*.jpg; *.jpeg";
            dialog.ShowDialog();

            if (File.Exists(dialog.FileName))
            {
                _imageFileName = dialog.FileName;
                using (FileStream stream = OpenFileStream(dialog.FileName))
                {

                    ResetUIAndData();

                    JpegBitmapDecoder decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    BitmapSource bitmapSource = decoder.Frames[0];
                    WriteableBitmap bitmapImage = new WriteableBitmap(bitmapSource);

                    PanoramaImage = bitmapImage;
                    Panorama.DataContext = PanoramaImage;

                    _height = PanoramaImage.Height;
                    _width = PanoramaImage.Width;

                    ScrollViewer.Height = ImageCanvas.ActualHeight;
                    ScrollViewer.Width = ImageCanvas.ActualWidth;

                    _crossMovingInProgress = false;
                    _currentScale = 1.0;

                    ScaleTransform scaleTransform = Panorama.RenderTransform as ScaleTransform;
                    scaleTransform.ScaleX = 1.0;
                    scaleTransform.ScaleY = 1.0;


                    TranslateTransform transform = Cross.RenderTransform as TranslateTransform;


                    _marginToTopOfCanvas = (ImageCanvas.ActualHeight - _height) / 2.0;
                    _marginToLeftOfCanvas = (ImageCanvas.ActualWidth - _width) / 2.0;


                    if (_marginToTopOfCanvas > 0 && _marginToLeftOfCanvas < 0)
                    {
                        _marginToTopOfCanvas -= (SystemParameters.HorizontalScrollBarHeight / 2.0);
                    }
                    if (_marginToLeftOfCanvas > 0 && _marginToTopOfCanvas < 0)
                    {
                        _marginToLeftOfCanvas -= (SystemParameters.VerticalScrollBarWidth / 2.0);
                    }


                    if (_marginToTopOfCanvas < 0)
                    {
                        _marginToTopOfCanvas = 0;
                    }
                    if (_marginToLeftOfCanvas < 0)
                    {
                        _marginToLeftOfCanvas = 0;
                    }

                    transform.X = _marginToLeftOfCanvas + 20;
                    transform.Y = _marginToTopOfCanvas + 20;

                    UpdatePositionDisplay();

                    GenerateJSONButton.IsEnabled = true;
                    MetaDataMenuItem.IsEnabled = true;
                }

            }
        }

        private void ResetUIAndData()
        {
            ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            ScrollViewer.Height = 0;
            ScrollViewer.Width = 0;
            ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            _metadata = new ImageMetadata();
            _metadata.addCategory(new Category("Szczyty"));
            _metadata.addCategory(new Category("Przełęcze"));
            _metadata.addCategory(new Category("Doliny"));


            RemovePointButton.IsEnabled = false;
            EditPointButton.IsEnabled = false;
            PointsComboBox.ItemsSource = _metadata.Points;
            Cross.Visibility = Visibility.Visible;
            AddPointButton.IsEnabled = true;
            ManageCategoriesButton.IsEnabled = true;
        }

        private FileStream OpenFileStream(string filename)
        {
            if (filename == null || !File.Exists(filename))
            {
                throw new ArgumentException("Cannot open File");
            }
            return File.Open(filename, System.IO.FileMode.Open, FileAccess.Read);
        }

        private void ImageCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScrollViewer.Height = ImageCanvas.ActualHeight;
            ScrollViewer.Width = ImageCanvas.ActualWidth;
        }

        private void closeApplication(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ScaleTextBoxKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                int scale;
                try
                {
                    scale = Convert.ToInt32(ScaleTextBox.Text.Replace('%', ' '));
                    if (scale < 10 || scale > 100)
                    {
                        throw new Exception();
                    }
                    rescale(scale);
                }
                catch (Exception)
                {
                    ToolTip toolTip = new ToolTip() { Content = "Podaj liczbę całkowitą z przedziału [10, 100]" };
                    ScaleTextBox.ToolTip = toolTip;

                    toolTip.IsMouseDirectlyOverChanged += ToolTipIsMouseDirectlyOverChanged;
                    toolTip.IsOpen = true;

                    if (!toolTip.Focus())
                    {
                        Debug.WriteLine("cannot set tooltip focus");
                    }
                }

            }
        }

        private void ToolTipIsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine("toolTip left");
            ((ToolTip)sender).IsOpen = false;

            ScaleTextBox.ToolTip = null;
        }

        private void rescale(double newScale)
        {
            ScaleTransform scaleTransformPanorama = Panorama.RenderTransform as ScaleTransform;
            ScaleTransform scaleTransformPointToTextLine = PointToTextLine.RenderTransform as ScaleTransform;
            TranslateTransform translateTransformCross = Cross.RenderTransform as TranslateTransform;
            TranslateTransform translateTransformPointToTextLine = PointToTextLine.RenderTransform as TranslateTransform;

            newScale = ((newScale) / (100.0));

            scaleTransformPanorama.CenterX = _width * 0.5;
            scaleTransformPanorama.CenterY = _height * 0.5;
            scaleTransformPanorama.ScaleX = newScale;
            scaleTransformPanorama.ScaleY = newScale;


            translateTransformCross.X = ComputeAbsoluteX(translateTransformCross.X, _currentScale, newScale);
            translateTransformCross.Y = ComputeAbsoluteY(translateTransformCross.Y, _currentScale, newScale);


            Debug.WriteLine("pointToTextLine: " + Math.Abs(PointToTextLine.X1 - PointToTextLine.X2) / _currentScale);

            PointToTextLine.X1 = ComputeAbsoluteX(PointToTextLine.X1, _currentScale, newScale);
            PointToTextLine.Y1 = ComputeAbsoluteY(PointToTextLine.Y1, _currentScale, newScale);
            PointToTextLine.X2 = ComputeAbsoluteX(PointToTextLine.X2, _currentScale, newScale);
            PointToTextLine.Y2 = ComputeAbsoluteY(PointToTextLine.Y2, _currentScale, newScale);

            _currentScale = newScale;

            UpdatePositionDisplay();
        }

        #region CrossManualMovingFunctions

        private void CrossMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _crossMovingInProgress = true;
        }

        private void CrossMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _crossMovingInProgress = false;
        }

        private void CrossMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_crossMovingInProgress)
            {
                return;
            }

            System.Windows.Point mousePos = e.GetPosition(Panorama);

            TranslateTransform transform = Cross.RenderTransform as TranslateTransform;

            transform.X = ((_width / 2.0 - (_width * _currentScale) / 2.0)) + (mousePos.X * _currentScale) - (Cross.Width / 2) - ScrollViewer.HorizontalOffset + _marginToLeftOfCanvas + 20;
            transform.Y = ((_height / 2.0 - (_height * _currentScale) / 2.0)) + (mousePos.Y * _currentScale) - (Cross.Height / 2) - ScrollViewer.VerticalOffset + _marginToTopOfCanvas + 20;

            UpdatePositionDisplay();
            if (_realX < 0)
            {
                MoveCursorToSpecifiedPosition(0, _realY);
                UpdatePositionDisplay();
            }
            if (_realY < 0)
            {
                MoveCursorToSpecifiedPosition(_realX, 0);
                UpdatePositionDisplay();
            }
            if (_realX > _width)
            {
                MoveCursorToSpecifiedPosition(_width, _realY);
                UpdatePositionDisplay();
            }
            if (_realY > _height)
            {
                MoveCursorToSpecifiedPosition(_realX, _height);
                UpdatePositionDisplay();
            }
        }

        private void CrossMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _crossMovingInProgress = false;
        }

        #endregion

        private void ScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            TranslateTransform crossTransform = Cross.RenderTransform as TranslateTransform;
            TranslateTransform descriptionTransform = DescriptionCanvas.RenderTransform as TranslateTransform;

            crossTransform.X -= e.HorizontalChange;
            crossTransform.Y -= e.VerticalChange;

            PointToTextLine.X1 -= e.HorizontalChange;
            PointToTextLine.Y1 -= e.VerticalChange;
            PointToTextLine.X2 -= e.HorizontalChange;
            PointToTextLine.Y2 -= e.VerticalChange;

            UpdatePositionDisplay();
        }

        private void UpdatePositionDisplay()
        {
            TranslateTransform transform = Cross.RenderTransform as TranslateTransform;

            _realX = ComputeRealX(transform.X);
            _realY = ComputeRealY(transform.Y);

            xCursorPosition.Text = Convert.ToString((int)_realX);
            yCursorPosition.Text = Convert.ToString((int)_realY);
        }

        #region ComputePositionFunctions

        public double ComputeRealX(double relativeX)
        {
            double distanceFromCenterToTopBorderX = _currentScale * (_width / 2.0);
            double realX = ((relativeX - (_width / 2.0 - distanceFromCenterToTopBorderX)) / (_currentScale)) + (ScrollViewer.HorizontalOffset / (_currentScale)) - _marginToLeftOfCanvas / _currentScale;
            return realX;
        }

        public double ComputeRealY(double relativeY)
        {
            double distanceFromCenterToTopBorderY = _currentScale * (_height / 2.0);
            double realY = ((relativeY - (_height / 2.0 - distanceFromCenterToTopBorderY)) / (_currentScale)) + (ScrollViewer.VerticalOffset / (_currentScale)) - _marginToTopOfCanvas / _currentScale;
            return realY;
        }


        public double ComputeAbsoluteX(double transformX, double currentScale, double newScale)
        {

            double distanceFromCenterToLeftBorderX = currentScale * (_width / 2.0);
            double newDiscanceFromCenterToLeftBorderX = newScale * (_width / 2.0);
            double realX = ((transformX - _marginToLeftOfCanvas - (_width / 2.0 - distanceFromCenterToLeftBorderX)) / (currentScale)) + (ScrollViewer.HorizontalOffset / (currentScale));
            double newX = (newScale * realX) - ScrollViewer.HorizontalOffset + _marginToLeftOfCanvas;
            double newAbsoluteX = (_width / 2.0) - newDiscanceFromCenterToLeftBorderX + newX;

            return newAbsoluteX;
        }

        public double ComputeAbsoluteY(double transformY, double currentScale, double newScale)
        {

            double distanceFromCenterToTopBorderY = currentScale * (_height / 2.0);
            double newDiscanceFromCenterToTopBorderY = newScale * (_height / 2.0);
            double realY = ((transformY - _marginToTopOfCanvas - (_height / 2.0 - distanceFromCenterToTopBorderY)) / (currentScale)) + (ScrollViewer.VerticalOffset / (currentScale));
            double newY = (newScale * realY) - ScrollViewer.VerticalOffset + _marginToTopOfCanvas;
            double newAbsoluteY = (_height / 2.0) - newDiscanceFromCenterToTopBorderY + newY;

            return newAbsoluteY;
        }

        public double ComputeRelativeX(double realX)
        {

            double distanceFromCenterToTopBorderX = _currentScale * (_width / 2.0);
            double newX = (_currentScale * realX) - ScrollViewer.HorizontalOffset + _marginToLeftOfCanvas;
            double newRelativeX = (_width / 2.0) - distanceFromCenterToTopBorderX + newX;

            return newRelativeX;
        }

        public double ComputeRelativeY(double realY)
        {

            double distanceFromCenterToTopBorderY = _currentScale * (_height / 2.0);
            double newY = (_currentScale * realY) - ScrollViewer.VerticalOffset + _marginToTopOfCanvas;
            double newRelativeY = (_height / 2.0) - distanceFromCenterToTopBorderY + newY;

            return newRelativeY;
        }

        #endregion

        #region CrossColorMenuItems

        private void BlackColorMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (blackColorMenuItem.IsChecked == true)
            {
                redColorMenuItem.IsChecked = false;
                yellowColorMenuItem.IsChecked = false;
                greenColorMenuItem.IsChecked = false;
                CrossColor.Stroke = Brushes.Black;
            }
            else
            {
                blackColorMenuItem.IsChecked = true;
            }
        }

        private void RedColorMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (redColorMenuItem.IsChecked == true)
            {
                blackColorMenuItem.IsChecked = false;
                yellowColorMenuItem.IsChecked = false;
                greenColorMenuItem.IsChecked = false;
                CrossColor.Stroke = Brushes.Red;
            }
            else
            {
                redColorMenuItem.IsChecked = true;
            }
        }

        private void YellowColorMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (yellowColorMenuItem.IsChecked == true)
            {
                blackColorMenuItem.IsChecked = false;
                redColorMenuItem.IsChecked = false;
                greenColorMenuItem.IsChecked = false;
                CrossColor.Stroke = Brushes.Yellow;
            }
            else
            {
                yellowColorMenuItem.IsChecked = true;
            }
        }

        private void GreenColorMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (greenColorMenuItem.IsChecked == true)
            {
                blackColorMenuItem.IsChecked = false;
                yellowColorMenuItem.IsChecked = false;
                redColorMenuItem.IsChecked = false;
                CrossColor.Stroke = Brushes.Green;
            }
            else
            {
                greenColorMenuItem.IsChecked = true;
            }
        }

        #endregion

        private void MoveCursorToSpecifiedPosition(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                try
                {
                    int xPosition = Convert.ToInt32(xCursorPosition.Text);
                    int yPosition = Convert.ToInt32(yCursorPosition.Text);
                    if (xPosition < 0 || yPosition < 0 || xPosition > _width || yPosition > _height)
                    {
                        throw new Exception();
                    }
                    MoveCursorToSpecifiedPosition(xPosition, yPosition);

                }
                catch (Exception)
                {
                    ToolTip toolTip = new ToolTip() { Content = "Podaj poprawne współrzędne" };
                    ((TextBox)sender).ToolTip = toolTip;
                    toolTip.IsOpen = true;
                }
            }
        }


        private void MoveCursorToSpecifiedPosition(double newXAbsolutePosition, double newYAbsolutePosition)
        {
            TranslateTransform transform = Cross.RenderTransform as TranslateTransform;

            transform.X = ComputeRelativeX(newXAbsolutePosition);
            transform.Y = ComputeRelativeY(newYAbsolutePosition);
            UpdatePositionDisplay();
        }


        private void ManageCategories(object sender, RoutedEventArgs e)
        {
            CategoriesManager categoriesManager = new CategoriesManager(_metadata);
            categoriesManager.ShowDialog();
        }

        public void ResetSelectedPoint()
        {
            _selectedPoint = null;
            EditPointButton.IsEnabled = false;
            RemovePointButton.IsEnabled = false;
        }

        private void LoadMetadataFile(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Metadane programu Wanderer|*.wan";
            dialog.ShowDialog();

            if (File.Exists(dialog.FileName))
            {
                using (FileStream stream = OpenFileStream(dialog.FileName))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        try
                        {
                            ImageMetadata loadedMetadata = JsonConvert.DeserializeObject<ImageMetadata>(reader.ReadToEnd());
                            this._metadata = loadedMetadata;
                            RefreshPointsComboBox();
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Format pliku nieznany.", "Błąd");
                        }
                    }
                }
            }
        }

        private void AddPoint(object sender, RoutedEventArgs e)
        {
            Cross.Visibility = Visibility.Hidden;
            PointToTextLine.Visibility = Visibility.Visible;
            DescriptionCanvas.Visibility = Visibility.Visible;
            AddOrEditPointToListWindow addPointWindow = new AddOrEditPointToListWindow(_metadata, _realX, _realY, null, this);
            addPointWindow.ShowDialog();
            RefreshPointsComboBox();
            ResetSelectedPoint();
            Cross.Visibility = Visibility.Visible;
            PointToTextLine.Visibility = Visibility.Hidden;
            DescriptionCanvas.Visibility = Visibility.Hidden;
        }


        private void PointsComboBoxDropDownClosed(object sender, EventArgs e)
        {
            if (PointsComboBox.SelectedItem != null)
            {
                _selectedPoint = (Point)PointsComboBox.SelectedItem;
                MoveCursorToSpecifiedPosition(_selectedPoint.X, _selectedPoint.Y);
                EditPointButton.IsEnabled = true;
                RemovePointButton.IsEnabled = true;
            }
            else
            {
                ResetSelectedPoint();
            }
        }

        private void EditPoint(object sender, RoutedEventArgs e)
        {
            if (PointsComboBox.SelectedItem == null)
            {
                return;
            }
            Cross.Visibility = Visibility.Hidden;
            PointToTextLine.Visibility = Visibility.Visible;
            DescriptionCanvas.Visibility = Visibility.Visible;
            AddOrEditPointToListWindow editPointWindow = new AddOrEditPointToListWindow(_metadata, _realX, _realY, _selectedPoint, this);
            editPointWindow.ShowDialog();
            RefreshPointsComboBox();
            ResetSelectedPoint();
            Cross.Visibility = Visibility.Visible;
            PointToTextLine.Visibility = Visibility.Hidden;
            DescriptionCanvas.Visibility = Visibility.Hidden;
        }

        private void RemovePoint(object sender, RoutedEventArgs e)
        {
            if (PointsComboBox.SelectedItem != null)
            {
                _metadata.Points.Remove((Point)PointsComboBox.SelectedItem);
                ResetSelectedPoint();
                RefreshPointsComboBox();
            }
        }


        public void RefreshPointsComboBox()
        {
            PointsComboBox.ItemsSource = null;
            PointsComboBox.ItemsSource = _metadata.Points;
        }

        private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            /* Narazie rozmiar okna jest usatalony (maksymalizacja okna).
             * NOTE: Client approved. 
             */
        }

        private void GenerateJSONClick(object sender, RoutedEventArgs e)
        {
            GenerateJSONWindow jsonWindow = new GenerateJSONWindow(_metadata, _imageFileName);
            jsonWindow.ShowDialog();
        }



    }
}
