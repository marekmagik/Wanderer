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
using System.Net;
using System.Collections.ObjectModel;
using System.Windows.Markup;
using System.Xml;

namespace WandererPanoramasEditor
{

    public partial class MainWindow : Window, IServerCallback
    {
        #region Members
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
        private byte[] _imageBytes;
        private BindingList<ImageMetadata> _waitingRoomList;
        private int _actualProcessedIndex;
        private Boolean _isWaitingListInitialized;
        private ImageMetadata _metadataTempHolder = null;
        private Boolean _isPlaceFromWaitingList = false;

        private int _previousClick = System.Windows.Forms.SystemInformation.DoubleClickTime;
        private List<Canvas> _descriptionCanvasList = new List<Canvas>();
        private List<Line> _pointToTextLinesList = new List<Line>();

        private ServerConnector _serverConnector;
        #endregion

        #region Properties
        private ImageSource PanoramaImage { get; set; }
        public double DisplayedImageScale { get; set; }
        public bool ResetPoint { get; set; }
        public int SelectedIndex { get; set; }
        #endregion

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();

            this._serverConnector = new ServerConnector();

            this.Width = System.Windows.SystemParameters.WorkArea.Width;
            this.Height = System.Windows.SystemParameters.WorkArea.Height + 6;
            this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Left;
            this.Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Top - 3;
            this.ResizeMode = ResizeMode.CanMinimize;
            this.WindowState = WindowState.Normal;
            this.ResetPoint = false;

            this.Closing += new CancelEventHandler(CloseApplication);
            Panorama.DataContext = PanoramaImage;

            if (ConfigurationFactory.GetConfiguration().Mode.Equals(Modes.AdminMode))
            {
                AdminPanelButton.Visibility = Visibility.Visible;
                RejectImageButton.Visibility = Visibility.Visible;
            }
            this._waitingRoomList = new BindingList<ImageMetadata>();
            this.DataContext = _waitingRoomList;
            this._actualProcessedIndex = 0;
            this._isWaitingListInitialized = false;
        }
        #endregion

        #region CrossManualMovingFunctions
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

        private void CrossMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _crossMovingInProgress = true;
        }

        private void CrossMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DoubleClickCount())
            {
                CrossMouseMove(sender, e);
                AddPoint(sender, e);
            }
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

            transform.X = ((_width / 2.0 - (_width * _currentScale) / 2.0)) + (mousePos.X * _currentScale * DisplayedImageScale) - (Cross.Width / 2)  + 20;
            transform.Y = ((_height / 2.0 - (_height * _currentScale) / 2.0)) + (mousePos.Y * _currentScale * DisplayedImageScale) - (Cross.Height / 2)  + 20;

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

        #region ComputePositionFunctions

        public double ComputeRealX(double relativeX)
        {
            return relativeX;
        }

        public double ComputeRealY(double relativeY)
        {
            return relativeY;
        }


        public double ComputeAbsoluteX(double transformX, double currentScale, double newScale)
        {

            double distanceFromCenterToLeftBorderX = currentScale * (_width / 2.0);
            double newDiscanceFromCenterToLeftBorderX = newScale * (_width / 2.0);
            double realX = ((transformX  - (_width / 2.0 - distanceFromCenterToLeftBorderX)) / (currentScale));
            double newX = (newScale * realX);
            double newAbsoluteX = (_width / 2.0) - newDiscanceFromCenterToLeftBorderX + newX;

            return newAbsoluteX;
        }

        public double ComputeAbsoluteY(double transformY, double currentScale, double newScale)
        {

            double distanceFromCenterToTopBorderY = currentScale * (_height / 2.0);
            double newDiscanceFromCenterToTopBorderY = newScale * (_height / 2.0);
            double realY = ((transformY - (_height / 2.0 - distanceFromCenterToTopBorderY)) / (currentScale));
            double newY = (newScale * realY);
            double newAbsoluteY = (_height / 2.0) - newDiscanceFromCenterToTopBorderY + newY;

            return newAbsoluteY;
        }

        public double ComputeRelativeX(double realX)
        {
            return realX;
        }

        public double ComputeRelativeY(double realY)
        {
            return realY;
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

        #region Top Panel Event Handlers
        private void CloseApplication(object sender, RoutedEventArgs e)
        {
            Close();
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

        private void AdminPanelButton_Click(object sender, RoutedEventArgs e)
        {
            if (ImageCanvas.Visibility.Equals(Visibility.Visible))
            {
                ImageCanvas.Visibility = Visibility.Collapsed;
                WaitingRoomListBox.Visibility = Visibility.Visible;
                AdminPanelButton.Header = "Edycja zdjęcia";
                if (!_isWaitingListInitialized)
                {
                    _isWaitingListInitialized = true;
                    _serverConnector.GetWaitingRoomContent(this);
                }
            }
            else
            {
                ImageCanvas.Visibility = Visibility.Visible;
                WaitingRoomListBox.Visibility = Visibility.Collapsed;
                AdminPanelButton.Header = "Panel administratora";
            }
        }
        #endregion

        #region Callbacks
        public void WaitingRoomListRequestCallback(IAsyncResult result)
        {
            try
            {
                HttpWebRequest request = result.AsyncState as HttpWebRequest;
                if (request != null)
                {
                    WebResponse response = request.EndGetResponse(result);
                    Stream stream = response.GetResponseStream();            
                    StreamReader streamReader = new StreamReader(stream);
                    String json = streamReader.ReadToEnd();
                    response.Close();
                    Debug.WriteLine(json);
                    JSONParser parser = new JSONParser();
                    List<ImageMetadata> metadataList = parser.ParsePlacesJSON(json);

                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                    {
                        foreach (ImageMetadata meta in metadataList)
                        {
                            _waitingRoomList.Add(meta);
                        }
                    }));

                }
                ProcessNextPlace();
            }
            catch (WebException)
            {
                MessageBox.Show("Nieudana próba połączenia z serwerem", "Wanderer");
            }
        }

        public void ThumbnailRequestCallback(IAsyncResult result)
        {
            HttpWebRequest request = result.AsyncState as HttpWebRequest;
            if (request != null)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                {

                    try
                    {
                        WebResponse response = request.EndGetResponse(result);
                        Stream stream = response.GetResponseStream();
                        JpegBitmapDecoder decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                        response.Close();
                        BitmapSource bitmapSource = decoder.Frames[0];
                        WriteableBitmap bitmapImage = new WriteableBitmap(bitmapSource);
                        _waitingRoomList.ElementAt(_actualProcessedIndex).Thumbnail = bitmapImage;
                    }
                    catch (WebException)
                    {
                        Debug.WriteLine("wyjatek wewnatrz UI!");
                        return;
                    }
                }));
            }
            _actualProcessedIndex++;
            ProcessNextPlace();
        }

        public void ImageRequestCallback(IAsyncResult result)
        {
            HttpWebRequest request = result.AsyncState as HttpWebRequest;
            if (request != null)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                {

                    try
                    {
                        WebResponse response = request.EndGetResponse(result);
                        Stream stream = response.GetResponseStream();
                        stream = CopyStreamToByteArray(stream);
                        LoadPanoramaFromStream(stream);
                        response.Close();
                        _metadata = _metadataTempHolder;

                        _metadataTempHolder = null;
                        AdminPanelButton.Header = "Panel administratora";
                        RefreshPointsComboBox();
                        RejectImageButton.IsEnabled = true;
                    }
                    catch (WebException)
                    {
                        Debug.WriteLine("wyjatek wewnatrz UI!");
                        return;
                    }
                }));
            }
            _isPlaceFromWaitingList = true;
            LoadDescriptionCanvases();
        }
        #endregion

        #region Mouse Control Handlers
        void PanoramaMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _crossMovingInProgress = true;
            CrossMouseMove(sender, e);
            _crossMovingInProgress = false;
            if (DoubleClickCount())
            {
                AddPoint(sender, e);
            }
        }

        void MouseDoubleClickHandler(object sender, MouseEventArgs e)
        {
            ListBox listbox = (ListBox)sender;
            ImageMetadata metadata = (ImageMetadata)listbox.SelectedItem;
            this._metadataTempHolder = metadata;
            ImageCanvas.Visibility = Visibility.Visible;
            WaitingRoomListBox.Visibility = Visibility.Collapsed;
            _serverConnector.GetImage(metadata.PictureSHA256, this);
        }
        #endregion

        #region Bottom Menu Event Handlers
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
                    Rescale(scale);
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

        private void ManageCategories(object sender, RoutedEventArgs e)
        {
            CategoriesManager categoriesManager = new CategoriesManager(_metadata);
            categoriesManager.ShowDialog();
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
            Debug.Write("should reset " + ResetPoint);
            if (!ResetPoint)
            {
                Canvas newCanvas = (Canvas)CopyXamlObject(DescriptionCanvas);
                Line newLine = (Line)CopyXamlObject(PointToTextLine);

                ChangeCanvasAndLineToColor(Colors.Black);

                newCanvas.Name = "canvas" + _descriptionCanvasList.Count;
                newLine.Name = "line" + _pointToTextLinesList.Count;
                WrappingCanvas.Children.Add(newCanvas);
                WrappingCanvas.Children.Add(newLine);
                _descriptionCanvasList.Add(newCanvas);
                _pointToTextLinesList.Add(newLine);
            }

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

            //pobranie aktualnych elementow i ukrycie ich
            Canvas actualCanvas = _descriptionCanvasList.ElementAt(PointsComboBox.SelectedIndex);
            Line actualLine = _pointToTextLinesList.ElementAt(PointsComboBox.SelectedIndex);
            actualCanvas.Visibility = Visibility.Collapsed;
            actualLine.Visibility = Visibility.Collapsed;

            ChangeColorOfDescriptionCanvas(_selectedPoint.Color);

            AddOrEditPointToListWindow editPointWindow = new AddOrEditPointToListWindow(_metadata, _realX, _realY, _selectedPoint, this);
            editPointWindow.ShowDialog();

            //stworzenie nowych elementow i dodanie ich zamiast starych 
            if (ResetPoint)
            {
                actualCanvas.Visibility = Visibility.Visible;
                actualLine.Visibility = Visibility.Visible;
                ResetPoint = false;
            }
            else
            {
                Canvas newCanvas = (Canvas)CopyXamlObject(DescriptionCanvas);
                Line newLine = (Line)CopyXamlObject(PointToTextLine);
                WrappingCanvas.Children.Remove(actualCanvas);
                WrappingCanvas.Children.Remove(actualLine);
                _descriptionCanvasList[SelectedIndex] = newCanvas;
                _pointToTextLinesList[SelectedIndex] = newLine;
                WrappingCanvas.Children.Add(newCanvas);
                WrappingCanvas.Children.Add(newLine);
                RefreshPointsComboBox();
            }

            ChangeCanvasAndLineToColor(Colors.Black);
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
                Canvas canvas = _descriptionCanvasList.ElementAt(PointsComboBox.SelectedIndex);
                WrappingCanvas.Children.Remove(canvas);
                _descriptionCanvasList.Remove(canvas);

                Line line = _pointToTextLinesList.ElementAt(PointsComboBox.SelectedIndex);
                WrappingCanvas.Children.Remove(line);
                _pointToTextLinesList.Remove(line);

                _metadata.Points.Remove((Point)PointsComboBox.SelectedItem);
                ResetSelectedPoint();
                RefreshPointsComboBox();
            }
        }
        private void GenerateJSONClick(object sender, RoutedEventArgs e)
        {
            GenerateJSONWindow jsonWindow = new GenerateJSONWindow(_metadata, _imageFileName, GenerateJSONWindow.GENERATE_JSON_MODE, _imageBytes, _isPlaceFromWaitingList, _waitingRoomList);
            jsonWindow.ShowDialog();
        }

        private void SendDataToServer(object sender, RoutedEventArgs e)
        {
            GenerateJSONWindow jsonWindow = new GenerateJSONWindow(_metadata, _imageFileName, GenerateJSONWindow.SEND_TO_SERVER_MODE, _imageBytes, _isPlaceFromWaitingList, _waitingRoomList);
            jsonWindow.ShowDialog();
        }
        private void RejectImage(object sender, RoutedEventArgs e)
        {
            Boolean resultOfRequest = _serverConnector.DeletePlaceFromWaitingRoom(_metadata.PictureSHA256);
            if (resultOfRequest)
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                {
                    _waitingRoomList.Remove(_metadata);
                }));

                MessageBox.Show("Zdjęcie pomyślnie usunięte", "Wanderer");
            }
            else
                MessageBox.Show("Błąd podczas usuwania zdjęcia", "Wanderer");
        }


        #endregion

        #region Help Functions
        private void ChangeColorOfDescriptionCanvas(char color)
        {
            if(color.Equals('b'))
                ChangeCanvasAndLineToColor(Colors.Black);
            else if(color.Equals('w'))
                ChangeCanvasAndLineToColor(Colors.White);
            else if(color.Equals('y'))
                ChangeCanvasAndLineToColor(Colors.Yellow);
        }

        private void ImageCanvasSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScrollViewer.Height = ImageCanvas.ActualHeight;
            ScrollViewer.Width = ImageCanvas.ActualWidth;
        }

        private void ToolTipIsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((ToolTip)sender).IsOpen = false;

            ScaleTextBox.ToolTip = null;
        }

        private void UpdatePositionDisplay()
        {
            TranslateTransform transform = Cross.RenderTransform as TranslateTransform;

            _realX = ComputeRealX(transform.X);
            _realY = ComputeRealY(transform.Y);

            xCursorPosition.Text = Convert.ToString((int)_realX);
            yCursorPosition.Text = Convert.ToString((int)_realY);
        }

        private void MoveCursorToSpecifiedPosition(double newXAbsolutePosition, double newYAbsolutePosition)
        {
            TranslateTransform transform = Cross.RenderTransform as TranslateTransform;

            transform.X = ComputeRelativeX(newXAbsolutePosition);
            transform.Y = ComputeRelativeY(newYAbsolutePosition);
            UpdatePositionDisplay();
        }

        public void ResetSelectedPoint()
        {
            _selectedPoint = null;
            EditPointButton.IsEnabled = false;
            RemovePointButton.IsEnabled = false;
        }

        private void ChangeCanvasAndLineToColor(Color color)
        {
            PrimaryDescriptionTextBlock.Foreground = new SolidColorBrush(color);
            SecondaryDescriptionTextBlock.Foreground = new SolidColorBrush(color);
            PointToTextLine.Stroke = new SolidColorBrush(color);
        }

        private void LoadDescriptionCanvases()
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {
                DescriptionCanvas.Visibility = Visibility.Visible;
                PointToTextLine.Visibility = Visibility.Visible;

                _metadata.AddCategory(new Category("Szczyty"));
                _metadata.AddCategory(new Category("Przełęcze"));
                _metadata.AddCategory(new Category("Doliny"));

                foreach (Point point in _metadata.Points)
                {
                    MoveCursorToSpecifiedPosition(point.X, point.Y);
                    AddOrEditPointToListWindow window = new AddOrEditPointToListWindow(_metadata, point.X, point.Y, point, this);

                    char colorName = point.Color;
                    Color color = Colors.Black;
                    if (colorName.Equals('w'))
                        color = Colors.White;
                    else if (colorName.Equals('y'))
                        color = Colors.Yellow;

                    ChangeCanvasAndLineToColor(color);


                    Canvas newCanvas = (Canvas)CopyXamlObject(DescriptionCanvas);
                    Line newLine = (Line)CopyXamlObject(PointToTextLine);

                    ChangeCanvasAndLineToColor(Colors.Black);

                    _descriptionCanvasList.Add(newCanvas);
                    _pointToTextLinesList.Add(newLine);
                    WrappingCanvas.Children.Add(newCanvas);
                    WrappingCanvas.Children.Add(newLine);
                    RefreshPointsComboBox();
                }
                DescriptionCanvas.Visibility = Visibility.Collapsed;
                PointToTextLine.Visibility = Visibility.Collapsed;
            }));

        }

        bool DoubleClickCount()
        {
            bool result = false;
            int now = System.Environment.TickCount;
            Debug.WriteLine(now - _previousClick);
            Debug.WriteLine(System.Windows.Forms.SystemInformation.DoubleClickTime);
            if (now - _previousClick <= System.Windows.Forms.SystemInformation.DoubleClickTime)
            {
                result = true;
            }
            _previousClick = now;

            return result;
        }

        private Stream CopyStreamToByteArray(Stream stream)
        {

            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                _imageBytes = ms.ToArray();
            }
            return new MemoryStream(_imageBytes);

        }

        private void ProcessNextPlace()
        {
            if (_actualProcessedIndex < _waitingRoomList.Count)
            {
                _serverConnector.GetThumbnail(_waitingRoomList.ElementAt(_actualProcessedIndex).PictureSHA256, this);
            }
        }

        public void RefreshPointsComboBox()
        {
            PointsComboBox.ItemsSource = null;
            PointsComboBox.ItemsSource = _metadata.Points;
        }

        private Object CopyXamlObject(Object objectToCopy)
        {
            var xaml = XamlWriter.Save(objectToCopy);

            var xamlString = new StringReader(xaml);

            var xmlTextReader = new XmlTextReader(xamlString);

            var deepCopyObject = XamlReader.Load(xmlTextReader);

            return deepCopyObject;
        }

        private void Rescale(double newScale)
        {

            ScaleTransform scaleTransformPanorama = Panorama.RenderTransform as ScaleTransform;
            ScaleTransform scaleTransformPointToTextLine = PointToTextLine.RenderTransform as ScaleTransform;
            TranslateTransform translateTransformCross = Cross.RenderTransform as TranslateTransform;
            TranslateTransform translateTransformPointToTextLine = PointToTextLine.RenderTransform as TranslateTransform;

            newScale = ((newScale) / (100.0));

            scaleTransformPanorama.CenterX = _width / DisplayedImageScale * 0.5;
            scaleTransformPanorama.CenterY = _height / DisplayedImageScale * 0.5;
            scaleTransformPanorama.ScaleX = newScale * DisplayedImageScale;
            scaleTransformPanorama.ScaleY = newScale * DisplayedImageScale;


            translateTransformCross.X = ComputeAbsoluteX(translateTransformCross.X, _currentScale, newScale);
            translateTransformCross.Y = ComputeAbsoluteY(translateTransformCross.Y, _currentScale, newScale);

            PointToTextLine.X1 = ComputeAbsoluteX(PointToTextLine.X1, _currentScale, newScale);
            PointToTextLine.Y1 = ComputeAbsoluteY(PointToTextLine.Y1, _currentScale, newScale);
            PointToTextLine.X2 = ComputeAbsoluteX(PointToTextLine.X2, _currentScale, newScale);
            PointToTextLine.Y2 = ComputeAbsoluteY(PointToTextLine.Y2, _currentScale, newScale);

            foreach (Canvas canvas in _descriptionCanvasList)
            {
                TranslateTransform translateTransformCanvas = canvas.RenderTransform as TranslateTransform;
                translateTransformCanvas.X = ComputeAbsoluteX(translateTransformCanvas.X, _currentScale, newScale);
                translateTransformCanvas.Y = ComputeAbsoluteY(translateTransformCanvas.Y, _currentScale, newScale);
            }

            foreach (Line line in _pointToTextLinesList)
            {
                line.X1 = ComputeAbsoluteX(line.X1, _currentScale, newScale);
                line.Y1 = ComputeAbsoluteY(line.Y1, _currentScale, newScale);
                line.X2 = ComputeAbsoluteX(line.X2, _currentScale, newScale);
                line.Y2 = ComputeAbsoluteY(line.Y2, _currentScale, newScale);
            }

            _currentScale = newScale;

            UpdatePositionDisplay();
        }

        private void CloseApplication(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }

        private void LoadPanoramaFile(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Zdjęcia|*.jpg; *.jpeg";
            dialog.ShowDialog();
            Debug.WriteLine(" End of dialog");
            if (File.Exists(dialog.FileName))
            {

                _imageFileName = dialog.FileName;
                _isPlaceFromWaitingList = false;
                RejectImageButton.IsEnabled = false;
                using (FileStream stream = OpenFileStream(dialog.FileName))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        _imageBytes = ms.ToArray();
                        stream.Position = 0;
                    }

                    LoadPanoramaFromStream(stream);
                }

            }
        }

        private void LoadPanoramaFromStream(Stream stream)
        {
            ResetUIAndData();

            foreach (Canvas canvas in _descriptionCanvasList)
            {
                WrappingCanvas.Children.Remove(canvas);
            }

            foreach (Line line in _pointToTextLinesList)
            {
                WrappingCanvas.Children.Remove(line);
            }

            _descriptionCanvasList.Clear();
            _pointToTextLinesList.Clear();


            JpegBitmapDecoder decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            BitmapSource bitmapSource = decoder.Frames[0];
            WriteableBitmap bitmapImage = new WriteableBitmap(bitmapSource);

            _metadata.Height = bitmapImage.PixelHeight;
            _metadata.Width = bitmapImage.PixelWidth;

            PanoramaImage = bitmapImage;
            WrappingCanvas.Width = bitmapImage.PixelWidth;
            WrappingCanvas.Height = bitmapImage.PixelHeight;
            Panorama.Width = bitmapImage.PixelWidth;
            Panorama.Height = bitmapImage.PixelHeight;
            Panorama.DataContext = bitmapImage;

            DisplayedImageScale = bitmapSource.PixelWidth / bitmapSource.Width;
            Debug.WriteLine("New scale " + DisplayedImageScale);

            _height = bitmapImage.PixelHeight;
            _width = bitmapImage.PixelWidth;

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
            SendToServerButton.IsEnabled = true;

            ScaleTransform scaleTransformPanorama = Panorama.RenderTransform as ScaleTransform;
            scaleTransformPanorama.CenterX = bitmapImage.Width * 0.5;
            scaleTransformPanorama.CenterY = bitmapImage.Height * 0.5;
            scaleTransformPanorama.ScaleX = DisplayedImageScale;
            scaleTransformPanorama.ScaleY = DisplayedImageScale;

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
            _metadata.AddCategory(new Category("Szczyty"));
            _metadata.AddCategory(new Category("Przełęcze"));
            _metadata.AddCategory(new Category("Doliny"));


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
        #endregion

    }
}
