using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WandererPanoramasEditor
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class AddOrEditPointToListWindow : Window
    {
        private readonly ImageMetadata _metadata;
        private readonly double _x;
        private readonly double y;
        private readonly Point _pointToEdit;
        private readonly MainWindow _mainWindow;
        private readonly TranslateTransform _translateTransform;
        private readonly RotateTransform _rotateTransform; 
        private const int TopPointMargin = 50;
        private List<String> _colors = new List<String>{"Biały","Czarny","Żółty"};

        private Point _initialPoint = new Point();
        private bool _closeWindowWithButton;

        public AddOrEditPointToListWindow(ImageMetadata metadata, double x, double y, Point pointToEdit, MainWindow mainWindow)
        {
            InitializeComponent();
            _closeWindowWithButton = false;
            _translateTransform = new TranslateTransform();
            _rotateTransform = new RotateTransform();

            this._metadata = metadata;
            CategoryComboBox.ItemsSource = metadata.Categories;
            ColorComboBox.ItemsSource = _colors;
            ColorComboBox.SelectedIndex = 1;
            this._x = x;
            this.y = y;
            this._mainWindow = mainWindow;
            CoordinatesTextBlock.Text = (int)x + " x " + (int)y;
            this._pointToEdit = pointToEdit;

            mainWindow.PrimaryDescriptionTextBlock.Text = PrimaryDescription.Text;
            mainWindow.SecondaryDescriptionTextBlock.Text = SecondaryDescription.Text;

            mainWindow.ResetPoint = false;

            if ((y - TopPointMargin) <= 0)
            {
                PointToTextLineLengthSlider.Maximum = 0;
                PointToTextLineLengthSlider.IsEnabled = false;
            }
            else
            {
                PointToTextLineLengthSlider.Maximum = y - TopPointMargin;
                PointToTextLineLengthSlider.IsEnabled = true;
            }

            mainWindow.PointToTextLine.X1 = mainWindow.ComputeRelativeX(x);
            mainWindow.PointToTextLine.X2 = mainWindow.PointToTextLine.X1;

            mainWindow.PointToTextLine.Y1 = mainWindow.ComputeRelativeY(y);
            mainWindow.PointToTextLine.Y2 = mainWindow.PointToTextLine.Y1;

            if (pointToEdit != null)
            {
                mainWindow.SelectedIndex = mainWindow.PointsComboBox.SelectedIndex;
                mainWindow.PointsComboBox.SelectedIndex = -1;
                _initialPoint.SetValues(pointToEdit);
                this.Title = "Edytuj punkt";
                ConfirmButton.Content = "Zakończ edycję";
                ConfirmButton.Click += new RoutedEventHandler(EditPoint);
                CategoryComboBox.SelectedItem = pointToEdit.Category;
                PrimaryDescription.Text = pointToEdit.PrimaryDescription;
                SecondaryDescription.Text = pointToEdit.SecondaryDescription;

                if (pointToEdit.Alignment == 0)
                {
                    DescriptionAlignmentLeft.IsChecked = true;
                }
                else
                {
                    if (pointToEdit.Alignment == 1)
                    {
                        DescriptionAlignmentCenter.IsChecked = true;
                    }
                    else
                    {
                        DescriptionAlignmentRight.IsChecked = true;
                    }
                }
                PointToTextLineLengthSlider.Value = pointToEdit.LineLength;
                AngleTextSlider.Value = Math.Abs(pointToEdit.Angle);
                Debug.WriteLine("angle: " + pointToEdit.Angle);
            }
            else
            {
                //this._pointToEdit = new Point(x*_mainWindow.DisplayedImageScale, y*_mainWindow.DisplayedImageScale, null, "", "");
                this._pointToEdit = new Point(x, y, null, "", "");
                this._pointToEdit.Color = 'b';
                ConfirmButton.Click += new RoutedEventHandler(AddPointToList);
                DescriptionAlignmentRight.IsChecked = true;
            }

            UpdateDescriptionCanvasProperties();
        }

        private void UpdateDescriptionCanvasProperties()
        {
            double translateX = 0;
            double translateY = 0;
            double rotationCenterX = 0;
            double rotationCenterY = _mainWindow.DescriptionStackPanel.ActualHeight;

            if (_pointToEdit.Alignment == 0)
            {
                translateX = _mainWindow.PointToTextLine.X2;
                translateY = _mainWindow.PointToTextLine.Y2 - (_mainWindow.DescriptionStackPanel.ActualHeight + 5);
                rotationCenterX = 0;
                _pointToEdit.Angle = (-1.0) * Math.Abs(_pointToEdit.Angle);
            }
            if (_pointToEdit.Alignment == 1)
            {
                translateX = _mainWindow.PointToTextLine.X2 - (_mainWindow.DescriptionStackPanel.ActualWidth / 2.0);
                translateY = _mainWindow.PointToTextLine.Y2 - (_mainWindow.DescriptionStackPanel.ActualHeight + 5);
                rotationCenterX = 0.5 * _mainWindow.DescriptionStackPanel.ActualWidth;
                _pointToEdit.Angle = (-1.0) * Math.Abs(_pointToEdit.Angle);
            }
            if (_pointToEdit.Alignment == 2)
            {
                translateX = _mainWindow.PointToTextLine.X2 - (_mainWindow.DescriptionStackPanel.ActualWidth);
                translateY = _mainWindow.PointToTextLine.Y2 - (_mainWindow.DescriptionStackPanel.ActualHeight + 5);
                rotationCenterX = _mainWindow.DescriptionStackPanel.ActualWidth;
            }

            TranslateTransform translateTransform = _mainWindow.DescriptionCanvas.RenderTransform as TranslateTransform;
            translateTransform.X = translateX;
            translateTransform.Y = translateY;

            RotateTransform roateTransform = _mainWindow.DescriptionStackPanel.RenderTransform as RotateTransform;
            _rotateTransform.CenterX = rotationCenterX;
            _rotateTransform.CenterY = rotationCenterY;
            _rotateTransform.Angle = _pointToEdit.Angle;

            _mainWindow.DescriptionStackPanel.RenderTransform = _rotateTransform;

            _mainWindow.UpdateLayout();
            _mainWindow.DescriptionCanvas.UpdateLayout();
        }



        private void AddPointToList(object sender, RoutedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem == null || PrimaryDescription.Text.Equals(""))
            {
                return;
            }
            _pointToEdit.Category = (Category)CategoryComboBox.SelectedItem;
            _metadata.addPoint(_pointToEdit);
            _closeWindowWithButton = true;
            Close();
        }

        private void EditPoint(object sender, RoutedEventArgs e)
        {
            if (CategoryComboBox.SelectedItem == null || PrimaryDescription.Text.Equals(""))
            {
                return;
            }
            Debug.WriteLine(_mainWindow.DisplayedImageScale);

            //_pointToEdit.X = _x * _mainWindow.DisplayedImageScale;
            //_pointToEdit.Y = y * _mainWindow.DisplayedImageScale;
            _pointToEdit.X = _x ;
            _pointToEdit.Y = y ;
            _pointToEdit.Category = (Category)CategoryComboBox.SelectedItem;
            Debug.WriteLine(" Length from slider "+PointToTextLineLengthSlider.Value);
            _closeWindowWithButton = true;
            Close();
        }

        private void PointToTextLineLengthSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _mainWindow.PointToTextLine.Y2 = _mainWindow.PointToTextLine.Y1 - PointToTextLineLengthSlider.Value;
            //_pointToEdit.LineLength = PointToTextLineLengthSlider.Value*_mainWindow.DisplayedImageScale;
            _pointToEdit.LineLength = PointToTextLineLengthSlider.Value;
            UpdateDescriptionCanvasProperties();
        }

        private void PrimaryDescriptionTextChanged(object sender, TextChangedEventArgs e)
        {
            _mainWindow.PrimaryDescriptionTextBlock.Text = PrimaryDescription.Text;
            _pointToEdit.PrimaryDescription = PrimaryDescription.Text;
            UpdateDescriptionCanvasProperties();
        }

        private void SecondaryDescriptionTextChanged(object sender, TextChangedEventArgs e)
        {
            _mainWindow.SecondaryDescriptionTextBlock.Text = SecondaryDescription.Text;
            _pointToEdit.SecondaryDescription = SecondaryDescription.Text;
            UpdateDescriptionCanvasProperties();
        }

        private void DescriptionAlignmentLeftChecked(object sender, RoutedEventArgs e)
        {
            _pointToEdit.Alignment = 0;
            UpdateDescriptionCanvasProperties();
        }

        private void DescriptionAlignmentCenterChecked(object sender, RoutedEventArgs e)
        {
            _pointToEdit.Alignment = 1;
            UpdateDescriptionCanvasProperties();
        }

        private void DescriptionAlignmentRightChecked(object sender, RoutedEventArgs e)
        {
            _pointToEdit.Alignment = 2;
            UpdateDescriptionCanvasProperties();
        }

        private void AngleTextSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _pointToEdit.Angle = AngleTextSlider.Value;
            UpdateDescriptionCanvasProperties();
        }

        private void ColorComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String colorName = _colors.ElementAt(ColorComboBox.SelectedIndex);
            Color color = Colors.Black;
            Char colorSymbol = 'b';
            if ("Biały".Equals(colorName))
            {
                colorSymbol = 'w';
                color = Colors.White;
            }
            else if ("Czarny".Equals(colorName))
            {
                colorSymbol = 'b';
                color = Colors.Black;
            }
            else if ("Żółty".Equals(colorName))
            {
                colorSymbol = 'y';
                color = Colors.Yellow;
            }

            if (_mainWindow != null)
            {
                _mainWindow.PrimaryDescriptionTextBlock.Foreground = new SolidColorBrush(color);
                _mainWindow.SecondaryDescriptionTextBlock.Foreground = new SolidColorBrush(color);
                _mainWindow.PointToTextLine.Stroke = new SolidColorBrush(color);
                _pointToEdit.Color = colorSymbol;
                UpdateDescriptionCanvasProperties();
            }
        }

        void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!_closeWindowWithButton)
            {
                _pointToEdit.SetValues(_initialPoint);
                _mainWindow.ResetPoint = true;
            }
        }

    }
}
