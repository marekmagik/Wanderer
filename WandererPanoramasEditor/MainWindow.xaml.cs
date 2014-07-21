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

        private ImageSource panorama;
        private double height;
        private double width;
        private double realX;
        private double realY;
        private double currentScale;
        private bool crossMovingInProgress;
        private double marginToTopOfCanvas;
        private double marginToLeftOfCanvas;
        private ImageMetadata metadata;
        private Point selectedPoint;
        private string imageFileName;

        public ImageSource PanoramaImage
        {
            get
            {
                return panorama;
            }
            set
            {
                panorama = value;
            }
        }


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
            metadata = new ImageMetadata();
            metadata.addCategory(new Category("Szczyty"));
            metadata.addCategory(new Category("Przełęcze"));
            metadata.addCategory(new Category("Doliny"));
            PointsComboBox.DataContext = metadata.Points;
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
                imageFileName = dialog.FileName;
                using (FileStream stream = OpenFileStream(dialog.FileName))
                {

                    resetUIAndData();

                    JpegBitmapDecoder decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    BitmapSource bitmapSource = decoder.Frames[0];
                    WriteableBitmap bitmapImage = new WriteableBitmap(bitmapSource);

                    PanoramaImage = bitmapImage;
                    Panorama.DataContext = PanoramaImage;

                    height = panorama.Height;
                    width = panorama.Width;

                    ScrollViewer.Height = ImageCanvas.ActualHeight;
                    ScrollViewer.Width = ImageCanvas.ActualWidth;

                    crossMovingInProgress = false;
                    currentScale = 1.0;

                    ScaleTransform scaleTransform = Panorama.RenderTransform as ScaleTransform;
                    scaleTransform.ScaleX = 1.0;
                    scaleTransform.ScaleY = 1.0;


                    TranslateTransform transform = Cross.RenderTransform as TranslateTransform;


                    marginToTopOfCanvas = (ImageCanvas.ActualHeight - height) / 2.0;
                    marginToLeftOfCanvas = (ImageCanvas.ActualWidth - width) / 2.0;


                    if (marginToTopOfCanvas > 0 && marginToLeftOfCanvas < 0)
                    {
                        marginToTopOfCanvas -= (SystemParameters.HorizontalScrollBarHeight / 2.0);
                    }
                    if (marginToLeftOfCanvas > 0 && marginToTopOfCanvas < 0)
                    {
                        marginToLeftOfCanvas -= (SystemParameters.VerticalScrollBarWidth / 2.0);
                    }


                    if (marginToTopOfCanvas < 0)
                    {
                        marginToTopOfCanvas = 0;
                    }
                    if (marginToLeftOfCanvas < 0)
                    {
                        marginToLeftOfCanvas = 0;
                    }

                    transform.X = marginToLeftOfCanvas + 20;
                    transform.Y = marginToTopOfCanvas + 20;

                    UpdatePositionDisplay();

                    GenerateJSONButton.IsEnabled = true;
                    MetaDataMenuItem.IsEnabled = true;
                }

            }
        }

        private void resetUIAndData()
        {
            ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            ScrollViewer.Height = 0;
            ScrollViewer.Width = 0;
            ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            metadata = new ImageMetadata();

            RemovePointButton.IsEnabled = false;
            EditPointButton.IsEnabled = false;
            PointsComboBox.ItemsSource = metadata.Points;
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

                    // System.Windows.Forms.ToolTip tooltip = new System.Windows.Forms.ToolTip();
                    //tooltip.SetToolTip((System.Windows.Forms.TextBox) sender, "Podaj liczbę całkowitą z przedziału [10, 100]");
                    //tooltip.Show("Podaj liczbę całkowitą z przedziału [10, 100]", sender, 1000);

                    ToolTip toolTip = new ToolTip() { Content = "Podaj liczbę całkowitą z przedziału [10, 100]" };
                    ScaleTextBox.ToolTip = toolTip;

                    toolTip.IsMouseDirectlyOverChanged += toolTip_IsMouseDirectlyOverChanged;
                    toolTip.IsOpen = true;
                    //   toolTip.Visibility = Visibility.Visible;

                    if (!toolTip.Focus())
                    {
                        Debug.WriteLine("cannot set tooltip focus");
                    }
                    //     toolTip.Visibility = Visibility.Visible;
                    //System.Threading.Thread.Sleep(3000);
                    //toolTip.IsOpen = false;
                }

            }
        }

        private void toolTip_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
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

            scaleTransformPanorama.CenterX = width * 0.5;
            scaleTransformPanorama.CenterY = height * 0.5;
            scaleTransformPanorama.ScaleX = newScale;
            scaleTransformPanorama.ScaleY = newScale;


            translateTransformCross.X = computeAbsoluteX(translateTransformCross.X, currentScale, newScale);
            translateTransformCross.Y = computeAbsoluteY(translateTransformCross.Y, currentScale, newScale);


            Debug.WriteLine("pointToTextLine: " + Math.Abs(PointToTextLine.X1 - PointToTextLine.X2) / currentScale);

            PointToTextLine.X1 = computeAbsoluteX(PointToTextLine.X1, currentScale, newScale);
            PointToTextLine.Y1 = computeAbsoluteY(PointToTextLine.Y1, currentScale, newScale);
            PointToTextLine.X2 = computeAbsoluteX(PointToTextLine.X2, currentScale, newScale);
            PointToTextLine.Y2 = computeAbsoluteY(PointToTextLine.Y2, currentScale, newScale);

            currentScale = newScale;

            UpdatePositionDisplay();
        }

        #region CrossManualMovingFunctions

        private void CrossMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            crossMovingInProgress = true;
        }

        private void CrossMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            crossMovingInProgress = false;
        }

        private void CrossMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!crossMovingInProgress)
            {
                return;
            }

            System.Windows.Point mousePos = e.GetPosition(Panorama);

            TranslateTransform transform = Cross.RenderTransform as TranslateTransform;

            transform.X = ((width / 2.0 - (width * currentScale) / 2.0)) + (mousePos.X * currentScale) - (Cross.Width / 2) - ScrollViewer.HorizontalOffset + marginToLeftOfCanvas + 20;
            transform.Y = ((height / 2.0 - (height * currentScale) / 2.0)) + (mousePos.Y * currentScale) - (Cross.Height / 2) - ScrollViewer.VerticalOffset + marginToTopOfCanvas + 20;

            UpdatePositionDisplay();
            if (realX < 0)
            {
                moveCursorToSpecifiedPosition(0, realY);
                UpdatePositionDisplay();
            }
            if (realY < 0)
            {
                moveCursorToSpecifiedPosition(realX, 0);
                UpdatePositionDisplay();
            }
            if (realX > width)
            {
                moveCursorToSpecifiedPosition(width, realY);
                UpdatePositionDisplay();
            }
            if (realY > height)
            {
                moveCursorToSpecifiedPosition(realX, height);
                UpdatePositionDisplay();
            }
        }

        private void CrossMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            crossMovingInProgress = false;
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

        //    descriptionTransform.X -= e.HorizontalChange;
        //    descriptionTransform.Y -= e.VerticalChange;

            UpdatePositionDisplay();
        }

        private void UpdatePositionDisplay()
        {
            TranslateTransform transform = Cross.RenderTransform as TranslateTransform;

            realX = computeRealX(transform.X);
            realY = computeRealY(transform.Y);

            xCursorPosition.Text = Convert.ToString((int)realX);
            yCursorPosition.Text = Convert.ToString((int)realY);
        }

        #region ComputePositionFunctions

        public double computeRealX(double relativeX)
        {
            double distanceFromCenterToTopBorderX = currentScale * (width / 2.0);
            double realX = ((relativeX - (width / 2.0 - distanceFromCenterToTopBorderX)) / (currentScale)) + (ScrollViewer.HorizontalOffset / (currentScale)) - marginToLeftOfCanvas / currentScale;
            return realX;
        }

        public double computeRealY(double relativeY)
        {
            double distanceFromCenterToTopBorderY = currentScale * (height / 2.0);
            double realY = ((relativeY - (height / 2.0 - distanceFromCenterToTopBorderY)) / (currentScale)) + (ScrollViewer.VerticalOffset / (currentScale)) - marginToTopOfCanvas / currentScale;
            return realY;
        }


        public double computeAbsoluteX(double transformX, double currentScale, double newScale)
        {

            double distanceFromCenterToLeftBorderX = currentScale * (width / 2.0);
            double newDiscanceFromCenterToLeftBorderX = newScale * (width / 2.0);
            double realX = ((transformX - marginToLeftOfCanvas - (width / 2.0 - distanceFromCenterToLeftBorderX)) / (currentScale)) + (ScrollViewer.HorizontalOffset / (currentScale));
            double newX = (newScale * realX) - ScrollViewer.HorizontalOffset + marginToLeftOfCanvas;
            double newAbsoluteX = (width / 2.0) - newDiscanceFromCenterToLeftBorderX + newX;

            return newAbsoluteX;
        }

        public double computeAbsoluteY(double transformY, double currentScale, double newScale)
        {

            double distanceFromCenterToTopBorderY = currentScale * (height / 2.0);
            double newDiscanceFromCenterToTopBorderY = newScale * (height / 2.0);
            double realY = ((transformY - marginToTopOfCanvas - (height / 2.0 - distanceFromCenterToTopBorderY)) / (currentScale)) + (ScrollViewer.VerticalOffset / (currentScale));
            double newY = (newScale * realY) - ScrollViewer.VerticalOffset + marginToTopOfCanvas;
            double newAbsoluteY = (height / 2.0) - newDiscanceFromCenterToTopBorderY + newY;

            return newAbsoluteY;
        }

        public double computeRelativeX(double realX)
        {

            double distanceFromCenterToTopBorderX = currentScale * (width / 2.0);
            double newX = (currentScale * realX) - ScrollViewer.HorizontalOffset + marginToLeftOfCanvas;
            double newRelativeX = (width / 2.0) - distanceFromCenterToTopBorderX + newX;

            return newRelativeX;
        }

        public double computeRelativeY(double realY)
        {

            double distanceFromCenterToTopBorderY = currentScale * (height / 2.0);
            double newY = (currentScale * realY) - ScrollViewer.VerticalOffset + marginToTopOfCanvas;
            double newRelativeY = (height / 2.0) - distanceFromCenterToTopBorderY + newY;

            return newRelativeY;
        }

        #endregion

        #region CrossColorMenuItems

        private void blackColorMenuItemClick(object sender, RoutedEventArgs e)
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

        private void redColorMenuItemClick(object sender, RoutedEventArgs e)
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

        private void yellowColorMenuItemClick(object sender, RoutedEventArgs e)
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

        private void greenColorMenuItemClick(object sender, RoutedEventArgs e)
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

        private void moveCursorToSpecifiedPosition(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                try
                {
                    int xPosition = Convert.ToInt32(xCursorPosition.Text);
                    int yPosition = Convert.ToInt32(yCursorPosition.Text);
                    if (xPosition < 0 || yPosition < 0 || xPosition > width || yPosition > height)
                    {
                        throw new Exception();
                    }
                    moveCursorToSpecifiedPosition(xPosition, yPosition);

                }
                catch (Exception)
                {
                    ToolTip toolTip = new ToolTip() { Content = "Podaj poprawne współrzędne" };
                    ((TextBox)sender).ToolTip = toolTip;
                    toolTip.IsOpen = true;
                }
            }
        }


        private void moveCursorToSpecifiedPosition(double newXAbsolutePosition, double newYAbsolutePosition)
        {
            TranslateTransform transform = Cross.RenderTransform as TranslateTransform;

            transform.X = computeRelativeX(newXAbsolutePosition);
            transform.Y = computeRelativeY(newYAbsolutePosition);
            UpdatePositionDisplay();
        }


        private void manageCategories(object sender, RoutedEventArgs e)
        {
            CategoriesManager categoriesManager = new CategoriesManager(metadata);
            categoriesManager.ShowDialog();
        }

        public void resetSelectedPoint()
        {
            selectedPoint = null;
            EditPointButton.IsEnabled = false;
            RemovePointButton.IsEnabled = false;
        }

        private void loadMetadataFile(object sender, RoutedEventArgs e)
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
                            this.metadata = loadedMetadata;
                            refreshPointsComboBox();
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Format pliku nieznany.", "Błąd");
                        }
                    }

                }
            }

        }

        private void addPoint(object sender, RoutedEventArgs e)
        {
            Cross.Visibility = Visibility.Hidden;
            PointToTextLine.Visibility = Visibility.Visible;
            DescriptionCanvas.Visibility = Visibility.Visible;
            AddEditPointToListWindow addPointWindow = new AddEditPointToListWindow(metadata, realX, realY, null, this);
            addPointWindow.ShowDialog();
            refreshPointsComboBox();
            resetSelectedPoint();
            Cross.Visibility = Visibility.Visible;
            PointToTextLine.Visibility = Visibility.Hidden;
            DescriptionCanvas.Visibility = Visibility.Hidden;
        }


        private void PointsComboBoxDropDownClosed(object sender, EventArgs e)
        {
            if (PointsComboBox.SelectedItem != null)
            {
                selectedPoint = (Point)PointsComboBox.SelectedItem;
                moveCursorToSpecifiedPosition(selectedPoint.X, selectedPoint.Y);
                EditPointButton.IsEnabled = true;
                RemovePointButton.IsEnabled = true;
            }
            else
            {
                resetSelectedPoint();
            }
        }

        private void editPoint(object sender, RoutedEventArgs e)
        {
            if (PointsComboBox.SelectedItem == null)
            {
                return;
            }
            Cross.Visibility = Visibility.Hidden;
            PointToTextLine.Visibility = Visibility.Visible;
            DescriptionCanvas.Visibility = Visibility.Visible;
            AddEditPointToListWindow editPointWindow = new AddEditPointToListWindow(metadata, realX, realY, selectedPoint, this);
            editPointWindow.ShowDialog();
            refreshPointsComboBox();
            resetSelectedPoint();
            Cross.Visibility = Visibility.Visible;
            PointToTextLine.Visibility = Visibility.Hidden;
            DescriptionCanvas.Visibility = Visibility.Hidden;
        }

        private void removePoint(object sender, RoutedEventArgs e)
        {
            if (PointsComboBox.SelectedItem != null)
            {
                metadata.Points.Remove((Point)PointsComboBox.SelectedItem);
                resetSelectedPoint();
                refreshPointsComboBox();
            }
        }


        public void refreshPointsComboBox()
        {
            PointsComboBox.ItemsSource = null;
            PointsComboBox.ItemsSource = metadata.Points;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /*
            Panorama.DataContext = null;
            ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            ScrollViewer.Height = 0;
            ScrollViewer.Width = 0;
            ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Panorama.DataContext = PanoramaImage;
            */
        }

        private void GenerateJSONClick(object sender, RoutedEventArgs e)
        {
            GenerateJSONWindow jsonWindow = new GenerateJSONWindow(metadata, imageFileName);
            jsonWindow.ShowDialog();
        }



    }
}
