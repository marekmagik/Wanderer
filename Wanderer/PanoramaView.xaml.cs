using System;
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



namespace Wanderer
{
    public partial class PanoramaView : PhoneApplicationPage
    {
        private readonly List<TextBlock> labels = new List<TextBlock>();

        private Compass compass;
        private DispatcherTimer timer;

        private double trueHeading;
        private double headingAccuracy;
        private int convertedHeading;

        private bool isDataValid;
        private bool isCalibrationInProgress;

        private readonly int HEADING_DIFF_TO_UPDATE_PANORAMA = 5;
        private readonly int HEADING_DIFF_TO_UPDATE_PANORAMA_INCREASED = 30;
        private int headingDiffToUpdate = 5;

        private double PIXELS_PER_DEGREE;
        private double currentShift;
        private double currentPageOrientationFactor;

        private double MIN_SCALE;
        private readonly double MAX_SCALE = 1.0;

        private double currentScale;

        private int height;
        private int width;
        private int screenWidth;
        private int screenHeight;

        private ImageMetadata metadata;
        //  private DAO dao;
        private int photoID;

        private ImageSource imageSource;

        public bool UseCompass { get; set; }

        public ImageSource ImageSource
        {
            get
            {
                return imageSource;
            }
            set
            {
                imageSource = value;
            }
        }

        public PanoramaView()
        {
            InitializeComponent();
        }

        public void InitializePanoramaLocal(string path)
        {
            Debug.WriteLine("InitializePanoramaLocal");
            PanoramaImageLeft.DataContext = null;
            PanoramaImageRight.DataContext = null;
            imageSource = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            MaxSizeReachedMessage.Visibility = Visibility.Collapsed;
            LoadingAnimation.Visibility = Visibility.Visible;

            //LoadImage("/PołoninaWetlińska.jpg", 800, 480, true);
            //LoadImage("/foto4.jpg", 800, 480, true, 80);
            LoadImage(path, 800, 480, true, 70);

        }

        public void InitializePanoramaRemote()
        {
            PanoramaImageLeft.DataContext = null;
            PanoramaImageRight.DataContext = null;
            imageSource = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            MaxSizeReachedMessage.Visibility = Visibility.Collapsed;
            LoadingAnimation.Visibility = Visibility.Visible;

            //LoadImage("/PołoninaWetlińska.jpg", 800, 480, true);
            //LoadImage("/foto4.jpg", 800, 480, true, 80);
            //LoadImage("/Panorama_z_Barańca_a2.jpg", 800, 480, true, 70);
            //dao = new DAO();
            LoadImageFromServer(800, 480, true, 100);
            //LoadImage("/foto4.jpg");         
        }

        private void setPixelsPerDegree()
        {
            PIXELS_PER_DEGREE = ((width) / (360.0));
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (compass != null && compass.IsDataValid)
            {
                compass.Stop();
                timer.Stop();

                Debug.WriteLine("---------------------Compass stopped.");
            }
            /*
             * Usuń naniesione punkty przy powrocie do poprzedniego widoku.
             */
            foreach (Point p in metadata.Points)
            {
                LayoutRoot.Children.Remove(p.LeftCanvas);
                LayoutRoot.Children.Remove(p.LeftPanoramaLine);
                LayoutRoot.Children.Remove(p.RightCanvas);
                LayoutRoot.Children.Remove(p.RightPanoramaLine);
            }
            LoadingAnimation.Visibility = Visibility.Visible;
            base.OnNavigatingFrom(e);
        }


        private void compassCurrentValueChanged(object sender, SensorReadingEventArgs<CompassReading> e)
        {
            isDataValid = compass.IsDataValid;

            trueHeading = e.SensorReading.TrueHeading;
            //            trueHeading = e.SensorReading.MagneticHeading;
            headingAccuracy = Math.Abs(e.SensorReading.HeadingAccuracy);

        }

        private void timerTick(object sender, EventArgs e)
        {
            if (UseCompass)
            {
                ///Debug.WriteLine("timer tick");
                if (!isCalibrationInProgress)
                {
                    int newConvertedHeading = Convert.ToInt32(trueHeading + 90.0);

                    if (newConvertedHeading >= 360)
                    {
                        newConvertedHeading -= 360;
                    }
                    if (Math.Abs(convertedHeading - newConvertedHeading) > headingDiffToUpdate)
                    {
                        headingDiffToUpdate = HEADING_DIFF_TO_UPDATE_PANORAMA;
//                        Debug.WriteLine("------Magnetic:------" + convertedHeading);

                        double newShift = ((-1.0) * newConvertedHeading * PIXELS_PER_DEGREE) * currentScale;

                        //return;
                        //Debug.WriteLine("LEFT BORDER: " + metadata.OrientationOfLeftBorder);

                        double constantShift;
                        if (currentPageOrientationFactor == -1)
                        {
                            constantShift = (metadata.OrientationOfLeftBorder * PIXELS_PER_DEGREE) * currentScale;
                        }
                        else
                        {
                            constantShift = ((180.0 + metadata.OrientationOfLeftBorder) * PIXELS_PER_DEGREE) * currentScale;
                        }


                        newShift += constantShift;

                        PanoramaTransformLeft.TranslateX = newShift;
                        PanoramaTransformRight.TranslateX = newShift + (width * currentScale);

                        updateImagesBounds();

                        convertedHeading = newConvertedHeading;
                    }
                }
                else
                {
                    Debug.WriteLine("heading accuracy: " + headingAccuracy);
                    if (headingAccuracy <= 10)
                    {
                        Debug.WriteLine("dokładność osiągnięta");
                        calibrationTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                        calibrationTextBlock.Text = headingAccuracy.ToString("0.0");
                        FinishCalibrationButton.IsEnabled = true;
                    }
                    else
                    {
                        calibrationTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                        calibrationTextBlock.Text = headingAccuracy.ToString("0.0");
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
                currentPageOrientationFactor = -1.0;
            }
            else
            {
                currentPageOrientationFactor = 1.0;
            }
            base.OnOrientationChanged(e);
        }

        private void compass_Calibrate(object sender, CalibrationEventArgs e)
        {
            isCalibrationInProgress = true;
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                CalibrationStackPanel.Visibility = Visibility.Visible;
                FinishCalibrationButton.IsEnabled = false;
            });
        }

        private double computeShift()
        {
            double shift;
            if (PanoramaTransformRight.TranslateX > 0)
            {
                shift = ((PanoramaTransformRight.TranslateX / currentScale) - width);
            }
            else
            {
                shift = (PanoramaTransformRight.TranslateX / currentScale);
            }
            return shift;
        }


        private void startCompass()
        {
            isCalibrationInProgress = false;

            compass = new Compass();
            compass.Start();
            compass.TimeBetweenUpdates = TimeSpan.FromMilliseconds(100);
            //            Debug.WriteLine(compass.TimeBetweenUpdates.TotalMilliseconds + " ms");
            compass.CurrentValueChanged +=
                new EventHandler<SensorReadingEventArgs<CompassReading>>(compassCurrentValueChanged);
            compass.Calibrate +=
                new EventHandler<CalibrationEventArgs>(compass_Calibrate);

            useCompassCheckBox.IsChecked = true;
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            Debug.WriteLine("On navigated TO");

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += new EventHandler(timerTick);
            UseCompass = false;
            timer.Start();
            //compass.Start();



            base.OnNavigatedTo(e);

            //bool getFromLocalDatabase = Convert.ToBoolean(NavigationContext.QueryString["useLocalDatabase"]);
            string hash = NavigationContext.QueryString["hash"];
            photoID = Convert.ToInt32(NavigationContext.QueryString["photoID"]);
            if (IsolatedStorageDAO.IsPhotoCached(hash))
            {
                InitializePanoramaLocal("/photos/" + hash + ".jpg");
            }
            else
            {
                InitializePanoramaRemote();
            }
        }

        private void LoadImageFromServer(int screenResolutionWidth, int screenResolutionHeight, bool isImageFullyPanoramic, int panoramaPercentage)
        {
            MaxSizeReachedMessage.Visibility = Visibility.Collapsed;
            screenWidth = screenResolutionWidth;
            screenHeight = screenResolutionHeight;
            DAO.GetPhotoDescById(this, photoID);
        }

        private async void LoadImage(string filename, int screenResolutionWidth, int screenResolutionHeight, bool isImageFullyPanoramic, int panoramaPercentage)
        {
            MaxSizeReachedMessage.Visibility = Visibility.Collapsed;
            screenWidth = screenResolutionWidth;
            screenHeight = screenResolutionHeight;

            using (var stream = await LoadImageAsync(filename))
            {
                try
                {
                    SetImageResolution(filename, panoramaPercentage);
                }
                catch (PossibleMemoryAccessViolationException)
                {
                    Debug.WriteLine("Żądanie zostało odrzucone ze względu na rozmiar panoramy.");
                    MaxSizeReachedMessage.Visibility = Visibility.Visible;
                    LoadingAnimation.Visibility = Visibility.Collapsed;
                    return;
                }

                WriteableBitmap bitmapImage = new WriteableBitmap(width, height);

                bitmapImage.LoadJpeg(stream);

                ImageSource = bitmapImage;
                PanoramaImageLeft.DataContext = ImageSource;
                PanoramaImageRight.DataContext = ImageSource;

                MIN_SCALE = (double)screenHeight / (double)bitmapImage.PixelHeight;
                currentScale = MIN_SCALE;

                PanoramaImageLeft.Width = width * currentScale;
                PanoramaImageLeft.Height = height * currentScale;
                PanoramaImageRight.Width = width * currentScale;
                PanoramaImageRight.Height = height * currentScale;

                LoadingAnimation.Visibility = Visibility.Collapsed;

                Debug.WriteLine("Size: " + bitmapImage.PixelWidth + " x " + bitmapImage.PixelHeight);
            }
        }

        /* Metoda obliczająca atrybuty width i height, jakie musi przyjąć kostruktor klasy WriteableBitmap.
         * Jest to konieczne, do ustawienia odpowiedniego dopełnienia (gdy panorama nie jest dookolna).
         * Nie można tego zadania zrealizować za pomocą przepisania WritableBitmap do WritableBitmap, ponieważ 
         * dwa obiekty tej klasy, nie mogą jednocześnie egzystować w pamięci (zajmują dużo miejsca -> możliwy OutOfMemoryException).
         * 
         * @throws PossibleMemoryAccessViolationException - jeśli stworzenie zdjęcia w danym rozmiarze, może naruszyć pamięć.
         *          Wartość 34.500.000 została przetestowana i jest bezpieczna, każda powyżej jest ryzykowna i należy zakończyć operację.
         *
         * NOTE: plik JPG musi zostać zdekodowany, wielkość nie jest w nim przechowywana excplicite.
         */
        private void SetImageResolution(string filename, int panoramaPercentage)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isoStore.FileExists(filename))
                {
                    using (var stream = isoStore.OpenFile(filename, System.IO.FileMode.Open, FileAccess.Read))
                    {
                        WriteableBitmap bitmapImage = PictureDecoder.DecodeJpeg(stream);

                        width = bitmapImage.PixelWidth;
                        height = bitmapImage.PixelHeight;

                        if (panoramaPercentage < 100)
                        {
                            width += (width * (100 - panoramaPercentage) / panoramaPercentage);
                        }

                        /* Rzuć wyjątek, jeśli wyświetlenie zdjęcia może skutkować zamknięciem aplikacji. */
                        if (width * height > 34500000)
                        {
                            throw new PossibleMemoryAccessViolationException();
                        }

                        /* Usuń z pamięci obiekt WriteableBitmap, aby móc utworzyć kolejny w metodzie wywołującej tą metodę. */
                        bitmapImage = null;
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
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



        private void manipulationDeltaHandler(object sender, ManipulationDeltaEventArgs e)
        {
            headingDiffToUpdate = HEADING_DIFF_TO_UPDATE_PANORAMA_INCREASED;
            if (e.DeltaManipulation.Scale.X > 0.0 && e.DeltaManipulation.Scale.Y > 0.0)
            {

                CompositeTransform scaleTransformLeft = (CompositeTransform)PanoramaImageLeft.RenderTransform;
                CompositeTransform scaleTransformRight = (CompositeTransform)PanoramaImageRight.RenderTransform;

                double xScaleValue = currentScale * e.DeltaManipulation.Scale.X;
                double yScaleValue = currentScale * e.DeltaManipulation.Scale.Y;
                double scaleValue = Math.Min(xScaleValue, yScaleValue);

                /* Jeśli nie można zmienić skali zakończ funkcję. */
                if ((currentScale == MIN_SCALE && scaleValue <= MIN_SCALE) || (currentScale == MAX_SCALE && scaleValue >= MAX_SCALE))
                {
                    return;
                }

                if (scaleValue <= MIN_SCALE)
                {
                    scaleValue = MIN_SCALE;
                }
                else if (scaleValue >= MAX_SCALE)
                {
                    scaleValue = MAX_SCALE;
                }

                scaleImages(scaleValue, 0.8);
                updateImagesBounds();

            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        if (e.DeltaManipulation.Translation.X < width)
                        {
                            MoveHorizontial(e.DeltaManipulation.Translation.X * currentScale);
                        }
                        else
                        {
                            MoveHorizontial(width * currentScale);
                        }

                        MoveVertical(e.DeltaManipulation.Translation.Y * currentScale);
                    });
            }
        }

        private void scaleImages(double newScale, double translateFactor)
        {
            double positionHorizontalBefore = PanoramaTransformLeft.TranslateX;
            double positionVerticalBefore = PanoramaTransformLeft.TranslateY;

            double positionHorizontalnPercentBefore = (positionHorizontalBefore / (width * currentScale));
            double positionVerticalInPercentBefore = (positionVerticalBefore / (height * currentScale));

            double positionHorizontalAfter = positionHorizontalnPercentBefore * width * newScale;
            double positionVerticalAfter = positionVerticalInPercentBefore * height * newScale;


            PanoramaImageLeft.Width = width * newScale;
            PanoramaImageLeft.Height = height * newScale;
            PanoramaImageRight.Width = width * newScale;
            PanoramaImageRight.Height = height * newScale;

            PanoramaTransformLeft.TranslateX = positionHorizontalAfter - (screenWidth * translateFactor * (newScale - currentScale));
            PanoramaTransformLeft.TranslateY = positionVerticalAfter - (screenHeight * translateFactor * (newScale - currentScale));
            PanoramaTransformRight.TranslateY = PanoramaTransformLeft.TranslateY;


            /* Utrzymaj prawe zdjęcie przyklejone do lewego. */
            PanoramaTransformRight.TranslateX = PanoramaTransformLeft.TranslateX + (width * newScale);

            currentScale = newScale;
        }


        private void manipulationCompletedHandler(object sender, ManipulationCompletedEventArgs e)
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
            updateImagesBounds();
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
                PanoramaTransformLeft.TranslateX -= (width * currentScale) - 1;
            }

            /* Warunek jest spełniony jeśli przesunięcie palcem było na tyle silne, że w czasie przesuwania zdjęć 
             * wystąpi conajmniej dwukrotnie konieczność przesunięcia obu do środka, aby uzyskać efekt nieskończonego przesuwania. 
             */
            if (Math.Abs(dx2) > (width * currentScale))
            {
                if (dx2 > 0)
                {
                    distanceToCompleteSingleIntertiaStep = dx2 - (width * currentScale);
                    dx2 = width * currentScale;
                }
                else
                {
                    distanceToCompleteSingleIntertiaStep = dx2 + (width * currentScale);
                    dx2 = (-1.0) * width * currentScale;
                }
            }

            /* Przesunięcie zdjęcia. */
            PanoramaTransformLeft.TranslateX += dx2;
            PanoramaTransformRight.TranslateX += dx2;

            updateImagesBounds();

            return distanceToCompleteSingleIntertiaStep;
        }

        private void updateImagesBounds()
        {
            /* Warunki są spełnione jeśli odpowiednio: 
           *   - nastąpiło zbyt duże przesunięcie w lewo,
           *   - nastąpiło zbyt duże przesunięcie w prawo, 
           *  wtedy oba zdjęcia przesuwamy do środka, o długość zdjęcia
           */
            if (PanoramaTransformLeft.TranslateX >= 0)
            {
                PanoramaTransformLeft.TranslateX -= width * currentScale;
                PanoramaTransformRight.TranslateX -= width * currentScale;
            }
            if ((PanoramaTransformRight.TranslateX + (width * currentScale)) < screenWidth)
            {
                PanoramaTransformLeft.TranslateX += width * currentScale;
                PanoramaTransformRight.TranslateX += width * currentScale;
            }
            if (PanoramaTransformLeft.TranslateY >= 0)
            {
                PanoramaTransformLeft.TranslateY = 0;
                PanoramaTransformRight.TranslateY = 0;
            }
            if (PanoramaTransformLeft.TranslateY < (((-1.0) * height * currentScale) + screenHeight))
            {
                PanoramaTransformLeft.TranslateY = (-1.0) * height * currentScale + screenHeight;
                PanoramaTransformRight.TranslateY = (-1.0) * height * currentScale + screenHeight;
            }

            currentShift = computeShift();
            /*
             * Zaktualizuj współrzędne opisów i linii na zdjęciu.
             */
            foreach (Point point in metadata.Points)
            {
                updateDescriptionCanvasProperties(point);
            }

        }



        private double computeRelativeX(double realX)
        {

            CompositeTransform translateTransform = PanoramaImageRight.RenderTransform as CompositeTransform;
            return translateTransform.TranslateX + realX * currentScale;
        }

        private double computeRelativeY(double realY)
        {
            CompositeTransform translateTransform = PanoramaImageRight.RenderTransform as CompositeTransform;
            return translateTransform.TranslateY + realY * currentScale;
        }


        private void updateDescriptionCanvasProperties(Point point)
        {
            System.Threading.Thread.Sleep(1);
            /* test */
            point.LeftPrimaryDescriptionTextBlock.FontSize = Configuration.PrimaryDescriptionFontSize * currentScale;
            point.LeftSecondaryDescriptionTextBlock.FontSize = Configuration.SecondaryDescriptionFontSize * currentScale;
            point.RightPrimaryDescriptionTextBlock.FontSize = Configuration.PrimaryDescriptionFontSize * currentScale;
            point.RightSecondaryDescriptionTextBlock.FontSize = Configuration.SecondaryDescriptionFontSize * currentScale;

            /* test */
            point.RightPanoramaLine.X1 = computeRelativeX(point.X);
            point.RightPanoramaLine.X2 = point.RightPanoramaLine.X1;

            point.RightPanoramaLine.Y1 = computeRelativeY(point.Y);
            point.RightPanoramaLine.Y2 = point.RightPanoramaLine.Y1 - currentScale * point.LineLength;

            point.LeftPanoramaLine.X1 = point.RightPanoramaLine.X1 - width * currentScale;
            point.LeftPanoramaLine.X2 = point.LeftPanoramaLine.X1;
            point.LeftPanoramaLine.Y1 = point.RightPanoramaLine.Y1;
            point.LeftPanoramaLine.Y2 = point.RightPanoramaLine.Y2;

            double translateX = 0;
            double translateY = 0;

            if (point.Alignment == 0)
            {
                translateX = point.RightPanoramaLine.X2;
                translateY = point.RightPanoramaLine.Y2 - (point.RightStackPanel.ActualHeight + 5);
            }
            else if (point.Alignment == 1)
            {
                translateX = point.RightPanoramaLine.X2 - (point.RightStackPanel.ActualWidth / 2.0);
                translateY = point.RightPanoramaLine.Y2 - (point.RightStackPanel.ActualHeight + 5);
            }
            else if (point.Alignment == 2)
            {
                translateX = point.RightPanoramaLine.X2 - (point.RightStackPanel.ActualWidth);
                translateY = point.RightPanoramaLine.Y2 - (point.RightStackPanel.ActualHeight + 5);
            }

            TranslateTransform LeftCanvasTranslateTransform = point.LeftCanvas.RenderTransform as TranslateTransform;
            TranslateTransform RightCanvasTranslateTransform = point.RightCanvas.RenderTransform as TranslateTransform;
            RightCanvasTranslateTransform.X = translateX;
            RightCanvasTranslateTransform.Y = translateY;

            LeftCanvasTranslateTransform.X = translateX - (width * currentScale);
            LeftCanvasTranslateTransform.Y = translateY;
        }


        private void doubleTapHandler(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (currentScale == MAX_SCALE)
            {
                scaleImages(MIN_SCALE, 0.8);
                PanoramaTransformLeft.TranslateX -= (screenWidth * 0.15);
                PanoramaTransformRight.TranslateX = PanoramaTransformLeft.TranslateX + (width * MIN_SCALE);
            }
            else
            {
                double scale = currentScale;
                scaleImages(MAX_SCALE, 0.8);
                PanoramaTransformLeft.TranslateX -= (screenWidth * 0.3) * ((MAX_SCALE - scale));
                PanoramaTransformRight.TranslateX = PanoramaTransformLeft.TranslateX + (width * MAX_SCALE);
            }
            updateImagesBounds();
        }

        #region DAOCallbacksMethods

        public void DescRequestCallback(IAsyncResult result)
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
                    JSONParser parser = new JSONParser();
                    metadata = parser.ParsePhotoMetadataJSON(json);

                    width = metadata.Width;
                    height = metadata.Height;

                    if (metadata.CoverageInPercent < 100)
                    {
                        width += (int)(width * (100 - metadata.CoverageInPercent) / metadata.CoverageInPercent);
                    }

                    /* Rzuć wyjątek, jeśli wyświetlenie zdjęcia może skutkować zamknięciem aplikacji. */
                    if (width * height > 34500000)
                    {
                        throw new PossibleMemoryAccessViolationException();
                    }

                    Point point1 = new Point(961.0, 420.0, null, "Zamek", "Baszta stracenców");
                    point1.Alignment = 0;
                    point1.LineLength = 122.0;
                    point1.Angle = -45.0;
                    point1.Color = Colors.Black;

                    Point point2 = new Point(1314.0, 622.0, null, "Nabrzeże portowe", "");
                    point2.Alignment = 1;
                    point2.LineLength = 339.2;
                    point2.Angle = 0;
                    point2.Color = Colors.Yellow;

                    Point point3 = new Point(1292.0, 430.7, null, "Zadar", "Wybrzeże zachodnie");
                    point3.Alignment = 2;
                    point3.LineLength = 59.3;
                    point3.Angle = 0;
                    point3.Color = Colors.White;

                    metadata.Points.Add(point1);
                    metadata.Points.Add(point2);
                    metadata.Points.Add(point3);


                    generateUIElementsForPoints(metadata.Points);

                    Debug.WriteLine("getPhotoById");
                    DAO.GetPhotoById(this, photoID);

                }
                catch (WebException)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        LoadingAnimation.Visibility = Visibility.Collapsed;
                        ConnectionErrorMessage.Visibility = Visibility.Visible;
                    });
                }
                catch (PossibleMemoryAccessViolationException)
                {
                    LoadingAnimation.Visibility = Visibility.Collapsed;
                    MaxSizeReachedMessage.Visibility = Visibility.Visible;
                }
            }
        }

        private void generateUIElementsForPoints(List<Point> points)
        {
            foreach (Point point in points)
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    /*
                     * Tworzenie komponenów linii i granic opisów.
                     */
                    point.LeftPanoramaLine = new Line();
                    point.RightPanoramaLine = new Line();
                    point.LeftCanvas = new Canvas();
                    point.RightCanvas = new Canvas();

 
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

                    /*
                     * Tworzenie komponentu TranslateTransform, używanego do przesuwania opisu.
                     */
                    point.LeftCanvas.RenderTransform = new TranslateTransform();
                    point.RightCanvas.RenderTransform = new TranslateTransform();


                    /*
                     * Tworzenie odpowiedniej rotacji i zastosowywanie jej do obu komponentów StackPanel.
                     */
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

                    /*
                     * Ustawienie tekstu podpisów.
                     */
                    point.LeftPrimaryDescriptionTextBlock.Text = point.PrimaryDescription;
                    point.RightPrimaryDescriptionTextBlock.Text = point.PrimaryDescription;
                    point.LeftSecondaryDescriptionTextBlock.Text = point.SecondaryDescription;
                    point.RightSecondaryDescriptionTextBlock.Text = point.SecondaryDescription;

                    /*
                     * Ustawienie grubości linii oraz kolory linii oraz tekstu.
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

                    /*
                     * Umieść przygotowane elementy na ekranie.
                     */
                    LayoutRoot.Children.Add(point.LeftCanvas);
                    LayoutRoot.Children.Add(point.RightCanvas);
                    LayoutRoot.Children.Add(point.LeftPanoramaLine);
                    LayoutRoot.Children.Add(point.RightPanoramaLine);
                });

            }
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

                        WebResponse response = request.EndGetResponse(result);
                        Stream stream = response.GetResponseStream();

                        IsolatedStorageDAO.CachePhoto(stream, width, height);

                        WriteableBitmap bitmapImage = new WriteableBitmap(width, height);
                        Debug.WriteLine(width + " x " + height);
                        bitmapImage.LoadJpeg(stream);

                        ImageSource = bitmapImage;
                        PanoramaImageLeft.DataContext = ImageSource;
                        PanoramaImageRight.DataContext = ImageSource;

                        MIN_SCALE = (double)screenHeight / (double)bitmapImage.PixelHeight;
                        currentScale = MIN_SCALE;

                        PanoramaImageLeft.Width = width * currentScale;
                        PanoramaImageLeft.Height = height * currentScale;
                        PanoramaImageRight.Width = width * currentScale;
                        PanoramaImageRight.Height = height * currentScale;

                        LoadingAnimation.Visibility = Visibility.Collapsed;

                        Debug.WriteLine("Size: " + bitmapImage.PixelWidth + " x " + bitmapImage.PixelHeight);
                        ReloadContent();

                        setPixelsPerDegree();
                        startCompass();
                    }
                    catch (WebException)
                    {
                        LoadingAnimation.Visibility = Visibility.Collapsed;
                        ConnectionErrorMessage.Visibility = Visibility.Visible;
                    }

                });

            }
        }

        #endregion


        public void ReloadContent()
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                PanoramaImageLeft.Source = null;
                PanoramaImageLeft.Source = imageSource;
                PanoramaImageRight.Source = null;
                PanoramaImageRight.Source = imageSource;
            });
        }



        #region CompassMethods


        /* Metoda blokująca przycisk "Wstecz" na czas trwania kalibracji kompasu.
         * Wymusza to na użytkowniku dokończenie procesu kalibracji.
         */
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (!isCalibrationInProgress)
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
            compass.Start();
        }

        /* Metoda deaktywująca kompas (odznaczono checkbox w menu kontekstowym).
         */
        private void useCompassCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            UseCompass = false;
            compass.Stop();
        }

        /* Metoda zamykająca okno kalibracji kompasu. 
         * Przywraca normalne działanie kompasu przez usunięcie flagi isCalibrationInProgress.
         */
        private void FinishCalibrationButtonClick(object sender, RoutedEventArgs e)
        {
            CalibrationStackPanel.Visibility = Visibility.Collapsed;
            isCalibrationInProgress = false;
        }


        #endregion

        #region ContextMenuMethods


        /* Metoda otwiera menu kontekstowe (długie przytrzymanie zdjęcia).
         */
        private void PanoramaHold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ContextMenu.Visibility = Visibility.Visible;
            useCompassCheckBox.Focus();
        }

        /* Metoda zamyka menu kontekstowe.
         */
        private void ContextMenuLostFocus(object sender, RoutedEventArgs e)
        {
            ContextMenu.Visibility = Visibility.Collapsed;
        }


        #endregion

    }
}