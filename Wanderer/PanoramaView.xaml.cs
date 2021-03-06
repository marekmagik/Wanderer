﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Storage;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Diagnostics;
using Microsoft.Phone;
using System.ComponentModel;
using ImageTools.IO.Gif;
using ImageTools;
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework;
using System.Windows.Threading;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Resources;



namespace Wanderer
{
    public partial class PanoramaView : PhoneApplicationPage, IImageCallbackReceiver
    {
        private TimerCallback _normalPriorityTaskCallback;
        private DispatcherTimer _descRotationTaskTimer;
        private Timer _normalPriorityTaskTimer;

        private Compass _compass;
        private DispatcherTimer _timer;

        private double _trueHeading;
        private double _headingAccuracy;
        private int _convertedHeading;

        private bool _isDataValid;
        private bool _isCalibrationInProgress;

        private readonly int HaedingDiffToUpdatePanorama = 5;
        private readonly int HaedingDiffToUpdatePanoramaIncreased = 30;
        private int _headingDiffToUpdate = 5;

        private double _pixelsPerDegree;
        private double _currentShift;
        private double _currentPageOrientationFactor;

        private double _minScale;
        private double _currentScale;
        public static readonly double MaxScale = 1.0;
        public static readonly double HidingDescriptionsMaxScale = 0.9;

        private int _height;
        private int _width;
        private int _screenWidth;
        private int _screenHeight;

        private ImageMetadata _metadata;

        private int contextMenuInitialized;
        private Boolean isCollisionAlgorithmFinished = false;
        private Boolean isUIThreadReady;
        public bool UseCompass { get; set; }
        public ImageSource ImageSource { get; set; }

        private List<Category> _categories = new List<Category>();
        //private List<Point> _activePoints = new List<Point>();

        public PanoramaView()
        {
            InitializeComponent();
        }

        public void InitializePanorama(string hash)
        {
            CollectPreviousImage();
            MaxSizeReachedMessage.Visibility = Visibility.Collapsed;
            LoadingAnimation.Visibility = Visibility.Visible;

            if (IsolatedStorageDAO.IsPhotoCached(hash))
            {
                LoadImage("/photos/" + hash + ".jpg", 800, 480);
            }
            else
            {
                LoadImageFromServer(800, 480);
            }
        }

        private void CollectPreviousImage()
        {
            PanoramaImageLeft.DataContext = null;
            PanoramaImageRight.DataContext = null;

            /*
            BitmapImage bitmapImage = ImageSource as BitmapImage;
            if (bitmapImage != null)
            {
                bitmapImage.UriSource = null;
            }
            */
              
            ImageSource = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }


        private void SetPixelsPerDegree()
        {
            _pixelsPerDegree = ((_width) / (360.0));
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (_compass != null && _compass.IsDataValid)
            {
                _compass.Stop();
                _timer.Stop();

                Debug.WriteLine("---------------------Compass stopped.");
            }
            /* Usuń naniesione punkty przy powrocie do poprzedniego widoku.
             */
            //foreach (Point p in _activePoints)
            foreach (Point p in _metadata.Points)
            {
                GridLayoutPoints.Children.Remove(p.LeftCanvas);
                GridLayoutPoints.Children.Remove(p.LeftPanoramaLine);
                GridLayoutPoints.Children.Remove(p.RightCanvas);
                GridLayoutPoints.Children.Remove(p.RightPanoramaLine);
            }

            PanoramaImageLeft.DataContext = null;
            PanoramaImageLeft.Source = null;
            PanoramaImageRight.DataContext = null;
            PanoramaImageRight.Source = null;

            WriteableBitmap wb = (WriteableBitmap)ImageSource;
            if (wb != null)
            {
                Uri uri = new Uri("Resources/onePixel.png", UriKind.Relative);
                StreamResourceInfo sr = Application.GetResourceStream(uri);
                using (Stream stream = sr.Stream)
                {
                    wb.SetSource(stream);
                }
            }

            CollectPreviousImage();

            LoadingAnimation.Visibility = Visibility.Visible;
            base.OnNavigatingFrom(e);
        }


        private void CompassCurrentValueChanged(object sender, SensorReadingEventArgs<CompassReading> e)
        {
            _isDataValid = _compass.IsDataValid;

            _trueHeading = e.SensorReading.TrueHeading;
            //            trueHeading = e.SensorReading.MagneticHeading;
            _headingAccuracy = Math.Abs(e.SensorReading.HeadingAccuracy);

        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (UseCompass)
            {
                if (!_isCalibrationInProgress)
                {
                    int newConvertedHeading = Convert.ToInt32(_trueHeading + 90.0);

                    if (newConvertedHeading >= 360)
                    {
                        newConvertedHeading -= 360;
                    }
                    if (Math.Abs(_convertedHeading - newConvertedHeading) > _headingDiffToUpdate)
                    {
                        _headingDiffToUpdate = HaedingDiffToUpdatePanorama;

                        double newShift = ((-1.0) * newConvertedHeading * _pixelsPerDegree) * _currentScale;

                        double constantShift;
                        if (_currentPageOrientationFactor == -1)
                        {
                            constantShift = (_metadata.OrientationOfLeftBorder * _pixelsPerDegree) * _currentScale;
                        }
                        else
                        {
                            constantShift = ((180.0 + _metadata.OrientationOfLeftBorder) * _pixelsPerDegree) * _currentScale;
                        }

                        Debug.WriteLine(newConvertedHeading);

                        newShift += constantShift;

                        PanoramaTransformLeft.TranslateX = newShift;
                        PanoramaTransformRight.TranslateX = newShift + (_width * _currentScale);

                        UpdateImagesBounds();

                        _convertedHeading = newConvertedHeading;
                    }
                }
                else
                {
                    Debug.WriteLine("heading accuracy: " + _headingAccuracy);
                    if (_headingAccuracy <= 10)
                    {
                        Debug.WriteLine("dokładność osiągnięta");
                        calibrationTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                        calibrationTextBlock.Text = _headingAccuracy.ToString("0.0");
                        FinishCalibrationButton.IsEnabled = true;
                    }
                    else
                    {
                        calibrationTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                        calibrationTextBlock.Text = _headingAccuracy.ToString("0.0");
                    }
                }
            }
        }

        /* Metoda wyznacza współczynnik orientacji używany podczas przesuwania zdjęcia 
         * za pomocą kompasu. Zdjęcie może być wyświetlane tylko w trybie "landscape", 
         * możliwe za to jest skierowanie urządzenia "górą w prawo lub w lewo".
         * Gdy urządzenie jest skierowane "w lewo" przesuwamy przeciwnie niż gdy jest 
         * skierowane "w prawo".
         */
        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (e.Orientation.Equals(PageOrientation.LandscapeLeft))
            {
                _currentPageOrientationFactor = -1.0;
            }
            else
            {
                _currentPageOrientationFactor = 1.0;
            }
            base.OnOrientationChanged(e);
        }

        private void CompassCalibrate(object sender, CalibrationEventArgs e)
        {
            _isCalibrationInProgress = true;
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                CalibrationStackPanel.Visibility = Visibility.Visible;
                FinishCalibrationButton.IsEnabled = false;
            });
        }

        private double ComputeShift()
        {
            double shift;
            if (PanoramaTransformRight.TranslateX > 0)
            {
                shift = ((PanoramaTransformRight.TranslateX / _currentScale) - _width);
            }
            else
            {
                shift = (PanoramaTransformRight.TranslateX / _currentScale);
            }
            return shift;
        }


        private void StartCompass()
        {
            _isCalibrationInProgress = false;

            _compass = new Compass();

            _compass.TimeBetweenUpdates = TimeSpan.FromMilliseconds(100);
            _compass.CurrentValueChanged +=
                new EventHandler<SensorReadingEventArgs<CompassReading>>(CompassCurrentValueChanged);
            _compass.Calibrate +=
                new EventHandler<CalibrationEventArgs>(CompassCalibrate);

            useCompassCheckBox.IsChecked = true;
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += new EventHandler(TimerTick);
            UseCompass = false;
            _timer.Start();

            string hash = NavigationContext.QueryString["hash"];

            JSONParser parser = new JSONParser();
            _metadata = IsolatedStorageDAO.getCachedMetadata(hash);
            InitializeCategoriesList(_metadata);

            InitializePanorama(hash);
        }

        private void InitializeCategoriesList(ImageMetadata _metadata)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                foreach (Point point in _metadata.Points)
                {
                    if (point.Category != null && !_categories.Contains(point.Category))
                    {
                        Category category = point.Category;
                        category.IsActive = true;
                        _categories.Add(category);
                    }
                }
                //_activePoints = _metadata.Points;
                SetActivePoints();
                this.DataContext = _categories;
                CategoriesListBox.ItemsSource = null;
                CategoriesListBox.Items.Clear();
                CategoriesListBox.ItemsSource = _categories;
                contextMenuInitialized = _categories.Count;
            });
        }

        private void SetActivePoints()
        {

            foreach (Point point in _metadata.Points)
            {
                int index = _categories.IndexOf(point.Category);
                if (index >= 0 && _categories.ElementAt(index).IsActive)
                {
                    point.setPointVisibility(Visibility.Visible, _currentScale);
                }
                else
                    point.setPointVisibility(Visibility.Collapsed, _currentScale);
            }
            if (isUIThreadReady)
            {
                useCompassCheckBox.IsChecked = false;
                foreach (Point p in _metadata.Points)
                {
                    p.MinimumScaleDescriptionVisibility = MaxScale;
                }
                ScaleImages(_minScale, 0);

                _normalPriorityTaskCallback = new TimerCallback(this.descCollisionsAlgorithm);
                _normalPriorityTaskTimer = new Timer(_normalPriorityTaskCallback);
                _normalPriorityTaskTimer.Change(0, Timeout.Infinite);
            }
        }


        private void AddCategory(Category category)
        {
            if (!_categories.Contains(category))
                _categories.Add(category);
        }

        private void RemoveCategory(Category category)
        {
            _categories.Remove(category);
        }

        private void LoadImageFromServer(int screenResolutionWidth, int screenResolutionHeight)
        {
            MaxSizeReachedMessage.Visibility = Visibility.Collapsed;
            _screenWidth = screenResolutionWidth;
            _screenHeight = screenResolutionHeight;
            DAO.SendRequestForPanorama(this, _metadata.PictureSHA256);
        }

        private async void LoadImage(string filename, int screenResolutionWidth, int screenResolutionHeight)
        {
            MaxSizeReachedMessage.Visibility = Visibility.Collapsed;
            _screenWidth = screenResolutionWidth;
            _screenHeight = screenResolutionHeight;

            try
            {
                SetImageResolution();
            }
            catch (PossibleMemoryAccessViolationException)
            {
                Debug.WriteLine("Żądanie zostało odrzucone ze względu na rozmiar panoramy.");
                MaxSizeReachedMessage.Visibility = Visibility.Visible;
                LoadingAnimation.Visibility = Visibility.Collapsed;
                return;
            }

            using (var stream = await LoadImageAsync(filename))
            {
                WriteableBitmap bitmapImage = new WriteableBitmap(_width, _height);

                bitmapImage.LoadJpeg(stream);

                ImageSource = bitmapImage;
                PanoramaImageLeft.DataContext = ImageSource;
                PanoramaImageRight.DataContext = ImageSource;

                _minScale = (double)_screenHeight / (double)bitmapImage.PixelHeight;
                _currentScale = _minScale;

                PanoramaImageLeft.Width = _width * _currentScale;
                PanoramaImageLeft.Height = _height * _currentScale;
                PanoramaImageRight.Width = _width * _currentScale;
                PanoramaImageRight.Height = _height * _currentScale;

                SetPixelsPerDegree();

                GenerateUIElementsForPoints();
            }

        }

        private void descRotationTaskTimerTick(object sender, EventArgs e)
        {
            (sender as DispatcherTimer).Stop();
            SetPointsDesctiptionsRotation(_metadata.Points);

            MoveHorizontial(_width * _currentScale);

            isUIThreadReady = true;
        }



        /* Metoda obliczająca atrybuty width i height, jakie musi przyjąć kostruktor klasy WriteableBitmap.
         * Jest to konieczne, do ustawienia odpowiedniego dopełnienia (gdy panorama nie jest dookolna).
         *
         * @throws PossibleMemoryAccessViolationException - jeśli stworzenie zdjęcia w danym rozmiarze, może naruszyć pamięć.
         *          Wartość 34.500.000 została przetestowana i jest bezpieczna, każda powyżej jest ryzykowna i należy zakończyć operację.
         *
         * NOTE: wartości pobierane są z metadanych, wielkość nie jest przechowywana w pliku JPG excplicite.
         */
        private void SetImageResolution()
        {
            _width = _metadata.Width;
            _height = _metadata.Height;

            if (_metadata.CoverageInPercent < 100)
            {
                _width += (int)(_width * (100 - _metadata.CoverageInPercent) / _metadata.CoverageInPercent);
            }

            /* Rzuć wyjątek, jeśli wyświetlenie zdjęcia może skutkować zamknięciem aplikacji. */
            if (_width * _height > 34500000)
            {
                throw new PossibleMemoryAccessViolationException();
            }
        }

        private Task<Stream> LoadImageAsync(string filename)
        {
            return Task.Factory.StartNew<Stream>(() =>
            {
                if (filename == null)
                {
                    throw new ArgumentException("Filename is null.");
                }

                Stream stream = null;

                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (isoStore.FileExists(filename))
                    {
                        stream = isoStore.OpenFile(filename, System.IO.FileMode.Open, FileAccess.Read);
                    }
                }
                return stream;
            });
        }



        private void ManipulationDeltaHandler(object sender, ManipulationDeltaEventArgs e)
        {
            _headingDiffToUpdate = HaedingDiffToUpdatePanoramaIncreased;
            if (e.DeltaManipulation.Scale.X > 0.0 && e.DeltaManipulation.Scale.Y > 0.0)
            {

                CompositeTransform scaleTransformLeft = (CompositeTransform)PanoramaImageLeft.RenderTransform;
                CompositeTransform scaleTransformRight = (CompositeTransform)PanoramaImageRight.RenderTransform;

                double xScaleValue = _currentScale * e.DeltaManipulation.Scale.X;
                double yScaleValue = _currentScale * e.DeltaManipulation.Scale.Y;
                double scaleValue = Math.Min(xScaleValue, yScaleValue);

                /* Jeśli nie można zmienić skali zakończ funkcję. */
                if ((_currentScale == _minScale && scaleValue <= _minScale) || (_currentScale == MaxScale && scaleValue >= MaxScale))
                {
                    return;
                }

                if (scaleValue <= _minScale)
                {
                    scaleValue = _minScale;
                }
                else if (scaleValue >= MaxScale)
                {
                    scaleValue = MaxScale;
                }

                ScaleImages(scaleValue, 0.8);
                UpdateImagesBounds();

            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        if (e.DeltaManipulation.Translation.X < _width)
                        {
                            MoveHorizontial(e.DeltaManipulation.Translation.X * _currentScale);
                        }
                        else
                        {
                            MoveHorizontial(_width * _currentScale);
                        }

                        MoveVertical(e.DeltaManipulation.Translation.Y * _currentScale);
                    });
            }
        }

        private void ScaleImages(double newScale, double translateFactor)
        {
            double positionHorizontalBefore = PanoramaTransformLeft.TranslateX;
            double positionVerticalBefore = PanoramaTransformLeft.TranslateY;

            double positionHorizontalnPercentBefore = (positionHorizontalBefore / (_width * _currentScale));
            double positionVerticalInPercentBefore = (positionVerticalBefore / (_height * _currentScale));

            double positionHorizontalAfter = positionHorizontalnPercentBefore * _width * newScale;
            double positionVerticalAfter = positionVerticalInPercentBefore * _height * newScale;


            PanoramaImageLeft.Width = _width * newScale;
            PanoramaImageLeft.Height = _height * newScale;
            PanoramaImageRight.Width = _width * newScale;
            PanoramaImageRight.Height = _height * newScale;

            PanoramaTransformLeft.TranslateX = positionHorizontalAfter - (_screenWidth * translateFactor * (newScale - _currentScale));
            PanoramaTransformLeft.TranslateY = positionVerticalAfter - (_screenHeight * translateFactor * (newScale - _currentScale));
            PanoramaTransformRight.TranslateY = PanoramaTransformLeft.TranslateY;

            /* Utrzymaj prawe zdjęcie przyklejone do lewego. */
            PanoramaTransformRight.TranslateX = PanoramaTransformLeft.TranslateX + (_width * newScale);

            _currentScale = newScale;
        }


        private void ManipulationCompletedHandler(object sender, ManipulationCompletedEventArgs e)
        {
            /* jeśli podczas oderwania palca od ekranu powinna być widoczna bezwładność obrazka */
            if (e.IsInertial)
            {

                /* wyliczenie współczynnika bezwładności na podstawie prędkości przesuwania palca */
                double dx2 = e.FinalVelocities.LinearVelocity.X / 2.0;
                double dy2 = e.FinalVelocities.LinearVelocity.Y / 2.0;

                /* utworzenie nowego wątku - ma to na celu opuszczenie Handlera (zwolnienie Dispatchera), aby móc odświeżać ekran. 
                   Odświeżanie okranu odbywa się po ZAKOŃCZENIU metody, którą obsługuje wątek UI, czyli Dispatcher.
                   Handler jest metodą Dispatchera, dlatego nie można wewnątrz niej odświeżać ekranu.
                */
                System.Threading.Thread startupThread =
                new System.Threading.Thread(new System.Threading.ThreadStart(delegate { ComputeInertia(dx2, dy2); }));
                startupThread.Start();
            }
        }


        /* Metoda wyliczająca kolejne (coraz mniejsze) "kroki" bezwładności i odświeża PanoramaImage */
        private void ComputeInertia(double dx2, double dy2)
        {
            if (Math.Abs(dx2) < 500.0)
            {
                dx2 /= 10.0;
                dy2 /= 10.0;
            }
            else
            {
                dx2 /= 3.0;
                dy2 /= 3.0;
            }

            double singleStep;
            bool executeAtLeastOnce;

            while ((Math.Abs(dx2) > 1.0) || (Math.Abs(dy2) > 1.0))
            {
                dx2 /= 1.45;

                singleStep = dx2;

                executeAtLeastOnce = true;

                /* wywołanie metody odpowiedzialnej za przesuwanie musi odbywać się wewnątrz wątku Dispatchera, 
                 * aby po jej zakończeniu wygenerowany został Event aktualizujący View.
                 */
                while ((Math.Abs(singleStep) > 0) || executeAtLeastOnce)
                {
                    executeAtLeastOnce = false;
                    dy2 /= 1.45;
                    Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        if (Math.Abs(singleStep) > 0)
                            singleStep = MoveHorizontial(singleStep);

                        if (Math.Abs(dy2) > 1.0)
                        {
                            MoveVertical(dy2);
                        }
                    });

                    /* Cykliczne wstrzymanie aktualizacji jest konieczne, aby efekt zmniejszania inercjii był widoczny. */
                    System.Threading.Thread.Sleep(30);
                }
            }
        }

        private void MoveVertical(double dy2)
        {
            PanoramaTransformLeft.TranslateY += dy2;
            PanoramaTransformRight.TranslateY += dy2;
            UpdateImagesBounds();
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
        private double MoveHorizontial(double dx2)
        {
            double distanceToCompleteSingleIntertiaStep = 0;

            /* Po wczytaniu strony oba zdjęcia znajdują się w tym samym miejscu. 
             * Lewe zdjęcie należy przesunąć o szerokość zdjęcia. 
             * Warunek ten jest spełniony tylko jeden raz w przeciągu istnienia danej instancji tej strony.
             */
            if (PanoramaTransformLeft.TranslateX == PanoramaTransformRight.TranslateX)
            {
                PanoramaTransformLeft.TranslateX -= (_width * _currentScale) - 1;
            }

            /* Warunek jest spełniony jeśli przesunięcie palcem było na tyle silne, że w czasie przesuwania zdjęć 
             * wystąpi conajmniej dwukrotnie konieczność przesunięcia obu do środka, aby uzyskać efekt nieskończonego przesuwania. 
             */
            if (Math.Abs(dx2) > (_width * _currentScale))
            {
                if (dx2 > 0)
                {
                    distanceToCompleteSingleIntertiaStep = dx2 - (_width * _currentScale);
                    dx2 = _width * _currentScale;
                }
                else
                {
                    distanceToCompleteSingleIntertiaStep = dx2 + (_width * _currentScale);
                    dx2 = (-1.0) * _width * _currentScale;
                }
            }

            /* Przesunięcie zdjęcia. */
            PanoramaTransformLeft.TranslateX += dx2;
            PanoramaTransformRight.TranslateX += dx2;

            UpdateImagesBounds();

            return distanceToCompleteSingleIntertiaStep;
        }

        private void UpdateImagesBounds()
        {
            /* Warunki są spełnione jeśli odpowiednio: 
           *   - nastąpiło zbyt duże przesunięcie w lewo,
           *   - nastąpiło zbyt duże przesunięcie w prawo, 
           *  wtedy oba zdjęcia przesuwamy do środka, o długość zdjęcia
           */
            if (PanoramaTransformLeft.TranslateX >= 0)
            {
                PanoramaTransformLeft.TranslateX -= _width * _currentScale;
                PanoramaTransformRight.TranslateX -= _width * _currentScale;
            }
            if ((PanoramaTransformRight.TranslateX + (_width * _currentScale)) < _screenWidth)
            {
                PanoramaTransformLeft.TranslateX += _width * _currentScale;
                PanoramaTransformRight.TranslateX += _width * _currentScale;
            }
            if (PanoramaTransformLeft.TranslateY >= 0)
            {
                PanoramaTransformLeft.TranslateY = 0;
                PanoramaTransformRight.TranslateY = 0;
            }
            if (PanoramaTransformLeft.TranslateY < (((-1.0) * _height * _currentScale) + _screenHeight))
            {
                PanoramaTransformLeft.TranslateY = (-1.0) * _height * _currentScale + _screenHeight;
                PanoramaTransformRight.TranslateY = (-1.0) * _height * _currentScale + _screenHeight;
            }

            _currentShift = ComputeShift();
            /*
             * Zaktualizuj współrzędne opisów i linii na zdjęciu.
             */
            foreach (Point point in _metadata.Points)
            {
                UpdateDescriptionCanvasProperties(point);
            }

        }



        private double ComputeRelativeX(double realX)
        {
            CompositeTransform translateTransform = PanoramaImageRight.RenderTransform as CompositeTransform;
            return translateTransform.TranslateX + realX * _currentScale;
        }

        private double ComputeRelativeY(double realY)
        {
            CompositeTransform translateTransform = PanoramaImageRight.RenderTransform as CompositeTransform;
            return translateTransform.TranslateY + realY * _currentScale;
        }


        private void UpdateDescriptionCanvasProperties(Point point)
        {
            point.LeftPrimaryDescriptionTextBlock.FontSize = Configuration.PrimaryDescriptionFontSize;
            point.LeftSecondaryDescriptionTextBlock.FontSize = Configuration.SecondaryDescriptionFontSize;
            point.RightPrimaryDescriptionTextBlock.FontSize = Configuration.PrimaryDescriptionFontSize;
            point.RightSecondaryDescriptionTextBlock.FontSize = Configuration.SecondaryDescriptionFontSize;

            point.RightPanoramaLine.X1 = ComputeRelativeX(point.X);
            point.RightPanoramaLine.X2 = point.RightPanoramaLine.X1;

            point.RightPanoramaLine.Y1 = ComputeRelativeY(point.Y);
            point.RightPanoramaLine.Y2 = point.RightPanoramaLine.Y1 - _currentScale * point.LineLength;

            point.LeftPanoramaLine.X1 = point.RightPanoramaLine.X1 - _width * _currentScale;
            point.LeftPanoramaLine.X2 = point.LeftPanoramaLine.X1;
            point.LeftPanoramaLine.Y1 = point.RightPanoramaLine.Y1;
            point.LeftPanoramaLine.Y2 = point.RightPanoramaLine.Y2;

            double translateX = 0;
            double translateY = 0;

            double descriptionWidth = point.RightStackPanel.ActualWidth;
            double descriptionHeight = point.RightStackPanel.ActualHeight;

            double descriptionAlignmentWidth = 0;

            if (point.Alignment == 0)
            {
                translateX = point.RightPanoramaLine.X2;
                translateY = point.RightPanoramaLine.Y2 - (point.RightStackPanel.ActualHeight + 5);

                point.LeftBall.Margin = new Thickness(-10.0, point.RightStackPanel.ActualHeight / 2.0, 0.0, 0.0);
                point.RightBall.Margin = new Thickness(-10.0, point.RightStackPanel.ActualHeight / 2.0, 0.0, 0.0);
            }
            else if (point.Alignment == 1)
            {
                translateX = point.RightPanoramaLine.X2 - (point.RightStackPanel.ActualWidth / 2.0);
                translateY = point.RightPanoramaLine.Y2 - (point.RightStackPanel.ActualHeight + 10);

                translateX -= descriptionHeight * 0.5 * Math.Sin((-1.0) * point.Angle * (Math.PI / 180));

                descriptionAlignmentWidth = (point.RightStackPanel.ActualWidth / 2.0);

                point.LeftBall.Margin = new Thickness(point.RightStackPanel.ActualWidth / 2.0 - 10.0 + descriptionHeight * 0.5 * Math.Sin((-1.0) * point.Angle * (Math.PI / 180)), point.RightStackPanel.ActualHeight / 2.0, 0.0, 0.0);
                point.RightBall.Margin = new Thickness(point.RightStackPanel.ActualWidth / 2.0 - 10.0 + descriptionHeight * 0.5 * Math.Sin((-1.0) * point.Angle * (Math.PI / 180)), point.RightStackPanel.ActualHeight / 2.0, 0.0, 0.0);
            }
            else if (point.Alignment == 2)
            {
                translateX = point.RightPanoramaLine.X2 - (point.RightStackPanel.ActualWidth);
                translateY = point.RightPanoramaLine.Y2 - (point.RightStackPanel.ActualHeight + 5);

                descriptionAlignmentWidth = (point.RightStackPanel.ActualWidth);

                point.LeftBall.Margin = new Thickness(point.RightStackPanel.ActualWidth - 10.0, point.RightStackPanel.ActualHeight / 2.0, 0.0, 0.0);
                point.RightBall.Margin = new Thickness(point.RightStackPanel.ActualWidth - 10.0, point.RightStackPanel.ActualHeight / 2.0, 0.0, 0.0);

            }

            //if(point.)
            int index = _categories.IndexOf(point.Category);
            if (index >= 0 && _categories.ElementAt(index).IsActive)
            {
                if (point.MinimumScaleDescriptionVisibility < MaxScale && _currentScale < HidingDescriptionsMaxScale && _currentScale < point.MinimumScaleDescriptionVisibility)
                {
                    point.LeftBall.Visibility = Visibility.Visible;
                    point.RightBall.Visibility = Visibility.Visible;
                    point.LeftStackPanel.Visibility = Visibility.Collapsed;
                    point.RightStackPanel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    point.LeftBall.Visibility = Visibility.Collapsed;
                    point.RightBall.Visibility = Visibility.Collapsed;
                    point.LeftStackPanel.Visibility = Visibility.Visible;
                    point.RightStackPanel.Visibility = Visibility.Visible;
                }
            }
            else
            {
                point.LeftBall.Visibility = Visibility.Collapsed;
                point.RightBall.Visibility = Visibility.Collapsed;
                point.LeftStackPanel.Visibility = Visibility.Collapsed;
                point.RightStackPanel.Visibility = Visibility.Collapsed;
            }
            TranslateTransform LeftCanvasTranslateTransform = point.LeftCanvas.RenderTransform as TranslateTransform;
            TranslateTransform RightCanvasTranslateTransform = point.RightCanvas.RenderTransform as TranslateTransform;
            RightCanvasTranslateTransform.X = translateX;
            RightCanvasTranslateTransform.Y = translateY;

            LeftCanvasTranslateTransform.X = translateX - (_width * _currentScale);
            LeftCanvasTranslateTransform.Y = translateY;

            if (!isCollisionAlgorithmFinished)
            {
                double heightOffset = descriptionWidth * Math.Sin((-1.0) * point.Angle * (Math.PI / 180));

                if (point.Alignment == 0)
                {
                    translateY += 5.0;

                    point.BottomLine.X1 = translateX;// ComputeRelativeX(point.X) /*- translateX */- descriptionAlignmentWidth;

                    point.TopLine.X1 = point.BottomLine.X1;

                    point.BottomLine.X2 = point.BottomLine.X1 + descriptionWidth * Math.Cos((-1.0) * point.Angle * (Math.PI / 180));
                    point.TopLine.X2 = point.BottomLine.X2;

                    point.BottomLine.Y1 = translateY + descriptionHeight;// ComputeRelativeY(point.Y) - translateY - point.LineLength * _currentScale;
                    point.BottomLine.Y2 = point.BottomLine.Y1 - heightOffset;

                    point.TopLine.Y1 = point.BottomLine.Y1 - descriptionHeight;
                    point.TopLine.Y2 = point.TopLine.Y1 - heightOffset;

                }
                else if (point.Alignment == 1)
                {
                    translateY += 5.0 + descriptionWidth * 0.5 * Math.Sin((-1.0) * point.Angle * (Math.PI / 180));
                    translateX += descriptionHeight * Math.Sin((-1.0) * point.Angle * (Math.PI / 180));

                    point.BottomLine.X1 = translateX;// -descriptionAlignmentWidth * Math.Cos((-1.0) * point.Angle * (Math.PI / 180));// (point.X) - translateX - descriptionAlignmentWidth * Math.Cos((-1.0) * point.Angle * (Math.PI / 180));

                    point.TopLine.X1 = point.BottomLine.X1;

                    point.BottomLine.X2 = point.BottomLine.X1 + descriptionWidth * Math.Cos((-1.0) * point.Angle * (Math.PI / 180));
                    point.TopLine.X2 = point.BottomLine.X2;

                    point.BottomLine.Y1 = translateY + descriptionHeight;
                    point.BottomLine.Y2 = point.BottomLine.Y1 - heightOffset;

                    point.TopLine.Y1 = point.BottomLine.Y1 - descriptionHeight;
                    point.TopLine.Y2 = point.TopLine.Y1 - heightOffset;

                }
                else
                {
                    translateY += 5.0;
                    translateX += descriptionHeight * Math.Cos((-1.0) * point.Angle * (Math.PI / 180));

                    point.BottomLine.X1 = translateX;
                    point.TopLine.X1 = point.BottomLine.X1;

                    point.BottomLine.X2 = point.BottomLine.X1 + descriptionWidth * Math.Cos((-1.0) * point.Angle * (Math.PI / 180));
                    point.TopLine.X2 = point.BottomLine.X2;

                    point.BottomLine.Y2 = translateY + descriptionHeight;
                    point.BottomLine.Y1 = point.BottomLine.Y2 + heightOffset;

                    point.TopLine.Y2 = point.BottomLine.Y2 - descriptionHeight;
                    point.TopLine.Y1 = point.TopLine.Y2 + heightOffset;
                }



                point.TopLine.X1 += point.Angle / 90.0 * descriptionHeight / 2.0;
                point.TopLine.X2 += point.Angle / 90.0 * descriptionHeight / 2.0;

                point.BottomLine.X1 -= point.Angle / 90.0 * descriptionHeight / 2.0;
                point.BottomLine.X2 -= point.Angle / 90.0 * descriptionHeight / 2.0;

                point.BottomLine.StrokeThickness = 3;
                point.BottomLine.Stroke = LoadingAnimation.Background;
                point.TopLine.StrokeThickness = point.BottomLine.StrokeThickness;
                point.TopLine.Stroke = LoadingAnimation.Background;
            }
        }


        private void DoubleTapHandler(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (_currentScale == MaxScale)
            {
                ScaleImages(_minScale, 0.8);
                PanoramaTransformLeft.TranslateX -= (_screenWidth * 0.15);
                PanoramaTransformRight.TranslateX = PanoramaTransformLeft.TranslateX + (_width * _minScale);
            }
            else
            {
                double scale = _currentScale;
                ScaleImages(MaxScale, 0.8);
                PanoramaTransformLeft.TranslateX -= (_screenWidth * 0.3) * ((MaxScale - scale));
                PanoramaTransformRight.TranslateX = PanoramaTransformLeft.TranslateX + (_width * MaxScale);
            }
            UpdateImagesBounds();
        }

        #region DAOCallbacksMethods

        
        private void GenerateUIElementsForPoints()
        {
            int index = 0;
            foreach (Point point in _metadata.Points)
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    /* Tworzenie komponenów linii i granic opisów.
                     */
                    point.LeftPanoramaLine = new Line();
                    point.RightPanoramaLine = new Line();
                    point.LeftCanvas = new Canvas();
                    point.RightCanvas = new Canvas();

                    /* Tworzenie odcinków będących górną i dolną granicą opisu punktu.
                     * Odcinki te posłużą do detekcji kolizji opisów.
                     */
                    point.TopLine = new Line();
                    point.BottomLine = new Line();

                    point.LeftStackPanel = new StackPanel();
                    point.RightStackPanel = new StackPanel();

                    point.LeftStackPanel.RenderTransform = new RotateTransform();
                    point.RightStackPanel.RenderTransform = new RotateTransform();

                    TextBlock PrimaryDescriptionTextBlockLeft = new TextBlock() { FontSize = Configuration.PrimaryDescriptionFontSize, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center };
                    TextBlock SecondaryDescriptionTextBlockLeft = new TextBlock() { FontSize = Configuration.SecondaryDescriptionFontSize, Margin = new Thickness(0, 0, 0, 0), TextAlignment = TextAlignment.Center };
                    point.LeftPrimaryDescriptionTextBlock = PrimaryDescriptionTextBlockLeft;
                    point.LeftSecondaryDescriptionTextBlock = SecondaryDescriptionTextBlockLeft;


                    TextBlock PrimaryDescriptionTextBlockRight = new TextBlock() { FontSize = Configuration.PrimaryDescriptionFontSize, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center };
                    TextBlock SecondaryDescriptionTextBlockRight = new TextBlock() { FontSize = Configuration.SecondaryDescriptionFontSize, Margin = new Thickness(0, 0, 0, 0), TextAlignment = TextAlignment.Center };
                    point.RightPrimaryDescriptionTextBlock = PrimaryDescriptionTextBlockRight;
                    point.RightSecondaryDescriptionTextBlock = SecondaryDescriptionTextBlockRight;

                    /* Tworzenie komponentu TranslateTransform, używanego do przesuwania opisu.
                     */
                    point.LeftCanvas.RenderTransform = new TranslateTransform();
                    point.RightCanvas.RenderTransform = new TranslateTransform();

                    /* Generowanie punktu pojawiającego się zamiast podpisu w momencie kolizji.
                     */
                    Ellipse leftBall = new Ellipse();
                    Ellipse rightBall = new Ellipse();
                    leftBall.Width = 20;
                    leftBall.Height = leftBall.Width;
                    rightBall.Width = leftBall.Width;
                    rightBall.Height = leftBall.Width;
                    leftBall.Fill = new SolidColorBrush(Colors.Red);
                    rightBall.Fill = new SolidColorBrush(Colors.Red);
                    point.LeftBall = leftBall;
                    point.RightBall = rightBall;

                    /* Ustawienie tekstu podpisów.
                     */
                    point.LeftPrimaryDescriptionTextBlock.Text = point.PrimaryDescription;
                    point.RightPrimaryDescriptionTextBlock.Text = point.PrimaryDescription;
                    point.LeftSecondaryDescriptionTextBlock.Text = point.SecondaryDescription;
                    point.RightSecondaryDescriptionTextBlock.Text = point.SecondaryDescription;

                    /* Ustawienie grubości linii oraz kolory linii oraz tekstu.
                     */
                    point.LeftPanoramaLine.StrokeThickness = 3;
                    point.LeftPanoramaLine.Stroke = new SolidColorBrush(point.Color);
                    point.RightPanoramaLine.StrokeThickness = point.LeftPanoramaLine.StrokeThickness;
                    point.RightPanoramaLine.Stroke = point.LeftPanoramaLine.Stroke;
                    point.LeftPrimaryDescriptionTextBlock.Foreground = new SolidColorBrush(point.Color);
                    point.RightPrimaryDescriptionTextBlock.Foreground = point.LeftPrimaryDescriptionTextBlock.Foreground;
                    point.LeftSecondaryDescriptionTextBlock.Foreground = point.LeftPrimaryDescriptionTextBlock.Foreground;
                    point.RightSecondaryDescriptionTextBlock.Foreground = point.LeftPrimaryDescriptionTextBlock.Foreground;


                    point.LeftStackPanel.Children.Add(PrimaryDescriptionTextBlockLeft);
                    point.LeftStackPanel.Children.Add(SecondaryDescriptionTextBlockLeft);
                    point.RightStackPanel.Children.Add(PrimaryDescriptionTextBlockRight);
                    point.RightStackPanel.Children.Add(SecondaryDescriptionTextBlockRight);
                    point.LeftCanvas.Children.Add(point.LeftStackPanel);
                    point.RightCanvas.Children.Add(point.RightStackPanel);

                    point.LeftCanvas.Children.Add(point.LeftBall);
                    point.RightCanvas.Children.Add(point.RightBall);

                    /* Umieść przygotowane elementy na ekranie.
                     */
                    GridLayoutPoints.Children.Add(point.LeftCanvas);
                    GridLayoutPoints.Children.Add(point.RightCanvas);
                    GridLayoutPoints.Children.Add(point.LeftPanoramaLine);
                    GridLayoutPoints.Children.Add(point.RightPanoramaLine);

                    LayoutRoot.Children.Add(point.TopLine);
                    LayoutRoot.Children.Add(point.BottomLine);

                    index++;
                    if (index == _metadata.Points.Count)
                    {
                        isUIThreadReady = false;

                        _descRotationTaskTimer = new DispatcherTimer();
                        _descRotationTaskTimer.Interval = TimeSpan.FromMilliseconds(0);
                        _descRotationTaskTimer.Tick += new EventHandler(descRotationTaskTimerTick);
                        _descRotationTaskTimer.Start();

                        /* Resztę ładowania należy wykonać w wątku nie będącym wątkiem Dispatchera UI.
                         */
                        _normalPriorityTaskCallback = new TimerCallback(this.descCollisionsAlgorithm);
                        _normalPriorityTaskTimer = new Timer(_normalPriorityTaskCallback);
                        _normalPriorityTaskTimer.Change(0, Timeout.Infinite);
                    }
                });
            }
        }

        public void descCollisionsAlgorithm(Object state)
        {
            Timer t = (Timer)state;
            t.Dispose();

            isCollisionAlgorithmFinished = false;

            while (!isUIThreadReady)
            {
                System.Threading.Thread.Sleep(1);
            }

            isUIThreadReady = false;
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                LoadingAnimation.Visibility = Visibility.Visible;
                PanoramaTransformLeft.TranslateX = (-1.0) * _width * _currentScale;
                PanoramaTransformRight.TranslateX = 0.0;

                isUIThreadReady = true;
            });

            while (!isUIThreadReady)
            {
                System.Threading.Thread.Sleep(1);
            }


            double scale = 0;
            Line PanoramaTopLine = null;
            Line PanoramaBottomLine = null;

            isUIThreadReady = false;
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                PanoramaTopLine = new Line();
                PanoramaTopLine.X1 = 0.0;
                PanoramaTopLine.Y1 = 0.0;
                PanoramaTopLine.Y2 = 0.0;
                PanoramaBottomLine = new Line();
                PanoramaBottomLine.X1 = 0.0;
                isUIThreadReady = true;
            });

            while (!isUIThreadReady)
            {
                System.Threading.Thread.Sleep(1);
            }

            for (scale = 1.00; scale > _minScale; scale = scale - 0.01)
            {
                isUIThreadReady = false;

                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    ScaleImages(scale, 0);
                    UpdateImagesBounds();
                    isUIThreadReady = true;
                });

                while (!isUIThreadReady)
                {
                    System.Threading.Thread.Sleep(1);
                }

                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    for (int i = 0; i < _metadata.Points.Count; i++)
                    {
                        Point firstPoint = _metadata.Points.ElementAt(i);

                        PanoramaTopLine.X2 = _width * scale;
                        PanoramaBottomLine.Y1 = _height * scale;
                        PanoramaBottomLine.X2 = _width * scale;
                        PanoramaBottomLine.Y2 = _height * scale;

                        if (firstPoint.MinimumScaleDescriptionVisibility == MaxScale)
                        {
                            if (LineSegmentIntersection.LineSegmentIntersection.doQuadrilateralsIntersect(firstPoint.TopLine, firstPoint.BottomLine, PanoramaTopLine, PanoramaBottomLine))
                            {
                                firstPoint.MinimumScaleDescriptionVisibility = scale;
                            }
                            else
                            {
                                if (i < _metadata.Points.Count - 1)
                                {
                                    for (int j = i + 1; j < _metadata.Points.Count; j++)
                                    {
                                        Point secondPoint = _metadata.Points.ElementAt(j);

                                        int firstPointCategoryIndex = _categories.IndexOf(firstPoint.Category);
                                        int secondPointCategoryIndex = _categories.IndexOf(secondPoint.Category);
                                        if (_categories.ElementAt(firstPointCategoryIndex).IsActive && _categories.ElementAt(secondPointCategoryIndex).IsActive)
                                        {
                                            if (LineSegmentIntersection.LineSegmentIntersection.doQuadrilateralsIntersect(firstPoint.TopLine, firstPoint.BottomLine, secondPoint.TopLine, secondPoint.BottomLine))
                                            {
                                                firstPoint.MinimumScaleDescriptionVisibility = scale;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    isUIThreadReady = true;
                });
                while (!isUIThreadReady)
                {
                    System.Threading.Thread.Sleep(1);
                }
            }
            Deployment.Current.Dispatcher.BeginInvoke(delegate
               {

                   foreach (Point p in _metadata.Points)
                   {
                       p.TopLine.Visibility = Visibility.Collapsed;
                       p.BottomLine.Visibility = Visibility.Collapsed;
                   }
                   isCollisionAlgorithmFinished = true;
                   StartCompass();
                   LoadingAnimation.Visibility = Visibility.Collapsed;
               });
        }

        public void ImageRequestCallback(IAsyncResult result)
        {
            HttpWebRequest request = result.AsyncState as HttpWebRequest;
            if (request != null)
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    try
                    {
                        SetImageResolution();
                        SetPixelsPerDegree();

                        WebResponse response = request.EndGetResponse(result);
                        Stream stream = response.GetResponseStream();
                        IsolatedStorageDAO.CachePhoto(stream, _width, _height, _metadata);
                        WriteableBitmap bitmapImage = new WriteableBitmap(_width, _height);
                        bitmapImage.LoadJpeg(stream);

                        ImageSource = bitmapImage;
                        PanoramaImageLeft.DataContext = ImageSource;
                        PanoramaImageRight.DataContext = ImageSource;

                        _minScale = (double)_screenHeight / (double)bitmapImage.PixelHeight;
                        _currentScale = _minScale;

                        PanoramaImageLeft.Width = _width * _currentScale;
                        PanoramaImageLeft.Height = _height * _currentScale;
                        PanoramaImageRight.Width = _width * _currentScale;
                        PanoramaImageRight.Height = _height * _currentScale;

                        Debug.WriteLine("Size: " + bitmapImage.PixelWidth + " x " + bitmapImage.PixelHeight);
                        ReloadContent();

                        GenerateUIElementsForPoints();
                    }
                    catch (WebException)
                    {
                        Configuration.InternetExceptionOccured = true;
                        LoadingAnimation.Visibility = Visibility.Collapsed;
                        ConnectionErrorMessage.Visibility = Visibility.Visible;
                    }
                    catch (PossibleMemoryAccessViolationException)
                    {
                        LoadingAnimation.Visibility = Visibility.Collapsed;
                        MaxSizeReachedMessage.Visibility = Visibility.Visible;
                    }
                });
            }
        }

        private void SetPointsDesctiptionsRotation(List<Point> list)
        {
            /* Tworzenie odpowiedniej rotacji i zastosowywanie jej do obu komponentów StackPanel.
             */
            foreach (Point point in list)
            {
                double rotationCenterX = 0;
                double rotationCenterY = point.RightPrimaryDescriptionTextBlock.ActualHeight;
                if (point.Alignment == 0)
                {
                    rotationCenterX = 0;
                    point.Angle = (-1.0) * Math.Abs(point.Angle);
                }
                if (point.Alignment == 1)
                {
                    rotationCenterX = 0.5 * point.RightStackPanel.ActualWidth;
                    point.Angle = (-1.0) * Math.Abs(point.Angle);
                }
                if (point.Alignment == 2)
                {
                    rotationCenterX = point.RightStackPanel.ActualWidth;
                }
                RotateTransform rotateTransform = point.RightStackPanel.RenderTransform as RotateTransform;
                rotateTransform.CenterX = rotationCenterX;
                rotateTransform.CenterY = rotationCenterY;
                rotateTransform.Angle = point.Angle;
                point.LeftStackPanel.RenderTransform = rotateTransform;
                point.RightStackPanel.RenderTransform = rotateTransform;
            }
        }

        #endregion


        private void ReloadContent()
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                PanoramaImageLeft.Source = null;
                PanoramaImageLeft.Source = ImageSource;
                PanoramaImageRight.Source = null;
                PanoramaImageRight.Source = ImageSource;
            });
        }


        #region CompassMethods


        /* Metoda blokująca przycisk "Wstecz" na czas trwania kalibracji kompasu.
         * Wymusza to na użytkowniku dokończenie procesu kalibracji.
         */
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (!_isCalibrationInProgress)
            {
                base.OnBackKeyPress(e);
            }
            else
            {
                e.Cancel = true;
            }
        }

        /* Metoda uaktywniająca kompas (zaznaczono checkbox w menu kontekstowym).
         */
        private void useCompassCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            UseCompass = true;
            _compass.Start();
        }

        /* Metoda deaktywująca kompas (odznaczono checkbox w menu kontekstowym).
         */
        private void useCompassCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            UseCompass = false;
            _compass.Stop();
        }

        /* Metoda zamykająca okno kalibracji kompasu. 
         * Przywraca normalne działanie kompasu przez usunięcie flagi isCalibrationInProgress.
         */
        private void FinishCalibrationButtonClick(object sender, RoutedEventArgs e)
        {
            CalibrationStackPanel.Visibility = Visibility.Collapsed;
            _isCalibrationInProgress = false;
        }


        #endregion

        #region ContextMenuMethods


        /* Metoda otwiera menu kontekstowe (długie przytrzymanie zdjęcia).
         */
        private void PanoramaHold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ContextMenu.Visibility = Visibility.Visible;
            //useCompassCheckBox.Focus();
        }

        /* Metoda zamyka menu kontekstowe.
         */
        private void HideContextMenu(object sender, RoutedEventArgs e)
        {
            ContextMenu.Visibility = Visibility.Collapsed;
        }

        private void CategoryChecked(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            Category category = (Category)element.DataContext;
            int index = _categories.IndexOf(category);
            if (index >= 0)
                _categories.ElementAt(index).IsActive = true;
            if (contextMenuInitialized == 0)
            {
                SetActivePoints();
            }
            else
            {
                contextMenuInitialized--;
            }
        }

        private void CategoryUnchecked(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            Category category = (Category)element.DataContext;
            int index = _categories.IndexOf(category);
            if (index >= 0)
                _categories.ElementAt(index).IsActive = false;
            if (contextMenuInitialized == 0)
            {
                SetActivePoints();
            }
            else
            {
                contextMenuInitialized--;
            }
        }

        #endregion

    }
}