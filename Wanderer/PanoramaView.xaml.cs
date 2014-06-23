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

            StorageFolder localRoot = ApplicationData.Current.LocalFolder;
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

            StorageFolder localRoot = ApplicationData.Current.LocalFolder;
            //LoadImage("/PołoninaWetlińska.jpg", 800, 480, true);

            //LoadImage("/foto4.jpg", 800, 480, true, 80);
            //LoadImage("/Panorama_z_Barańca_a2.jpg", 800, 480, true, 70);

            //dao = new DAO();

            LoadImageFromServer(800, 480, true, 100);

            //LoadImage("/foto4.jpg");         

        }

        private void setPixelsPerDegree(){
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
            base.OnNavigatingFrom(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
/*
            if (compass != null && compass.IsDataValid)
            {
                compass.Stop();
                timer.Stop();

                Debug.WriteLine("---------------------Compass stopped.");
            }
*/
            base.OnLostFocus(e);

        }


        private void compassCurrentValueChanged(object sender, SensorReadingEventArgs<CompassReading> e)
        {
            isDataValid = compass.IsDataValid;

 //           trueHeading = e.SensorReading.TrueHeading;
            trueHeading = e.SensorReading.MagneticHeading;
            headingAccuracy = Math.Abs(e.SensorReading.HeadingAccuracy);

        }

        private void timerTick(object sender, EventArgs e)
        {
            if (UseCompass)
            {
                if (!isCalibrationInProgress)
                {
                    int newConvertedHeading = Convert.ToInt32(trueHeading + 90.0);

                    if (newConvertedHeading >= 360)
                    {
                        newConvertedHeading -= 360;
                    }
                    if (Math.Abs(convertedHeading - newConvertedHeading) > 1)
                    {
                        Debug.WriteLine("------Magnetic:------" + convertedHeading);

                        double newShift = ((-1.0) * newConvertedHeading * PIXELS_PER_DEGREE) * currentScale;


                        Debug.WriteLine("LEFT BORDER: " + metadata.OrientationOfLeftBorder);

                        double constantShift;
                        if (currentPageOrientationFactor == -1)
                        {
                            constantShift = (metadata.OrientationOfLeftBorder * PIXELS_PER_DEGREE) * currentScale;
                        }
                        else {
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

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
//            Debug.WriteLine("--------------------------------------------------------orientation changed !");
            if(e.Orientation.Equals(PageOrientation.LandscapeLeft)){
                currentPageOrientationFactor = -1.0;
//                Debug.WriteLine("LANDSCAPE LEFT");
            }else{
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

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            Debug.WriteLine("On navigated TO");

//            UseCompass = true;


            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += new EventHandler(timerTick);


            compass = new Compass();
            compass.TimeBetweenUpdates = TimeSpan.FromMilliseconds(100);
            Debug.WriteLine(compass.TimeBetweenUpdates.TotalMilliseconds + " ms");
            compass.CurrentValueChanged +=
                new EventHandler<SensorReadingEventArgs<CompassReading>>(compassCurrentValueChanged);
            compass.Calibrate +=
                new EventHandler<CalibrationEventArgs>(compass_Calibrate);

            useCompassCheckBox.IsChecked = true;
            isCalibrationInProgress = false;

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

/*
        private int GetPhotoId()
        {
            return photoID;
        }

        private void LoadLabels()
        {

        }
*/

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

                CompositeTransform transformLeft = (CompositeTransform)PanoramaImageLeft.RenderTransform;
                CompositeTransform transformRight = (CompositeTransform)PanoramaImageRight.RenderTransform;

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
            //Debug.WriteLine("updateBounds, right.X = " + currentShift);
        }

        private double computeShift() {
            double shift;
            if (PanoramaTransformRight.TranslateX > 0)
            {
                shift = ((PanoramaTransformRight.TranslateX / currentScale) - width);// *(-1.0);
            }
            else
            {
                shift = (PanoramaTransformRight.TranslateX / currentScale);// *(-1.0);
            }
            return shift;
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

        private void MediaElement_Loaded(object sender, RoutedEventArgs e)
        {

        }

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
                        //stream.Position = 0;
                        bitmapImage.LoadJpeg(stream);

                        ImageSource = bitmapImage;
                        PanoramaImageLeft.DataContext = ImageSource;
                        PanoramaImageRight.DataContext = ImageSource;

                        CompositeTransform transformLeft = (CompositeTransform)PanoramaImageLeft.RenderTransform;
                        CompositeTransform transformRight = (CompositeTransform)PanoramaImageRight.RenderTransform;

                        MIN_SCALE = (double)screenHeight / (double)bitmapImage.PixelHeight;
                        currentScale = MIN_SCALE;

                        PanoramaImageLeft.Width = width * currentScale;
                        PanoramaImageLeft.Height = height * currentScale;
                        PanoramaImageRight.Width = width * currentScale;
                        PanoramaImageRight.Height = height * currentScale;

                        LoadingAnimation.Visibility = Visibility.Collapsed;

                        Debug.WriteLine("Size: " + bitmapImage.PixelWidth + " x " + bitmapImage.PixelHeight);
                        ReloadContent();
                        try
                        {
                            setPixelsPerDegree();
                            compass.Start();
                            timer.Start();
                        }
                        catch (InvalidOperationException){}
                    }
                    catch (WebException)
                    {
                        LoadingAnimation.Visibility = Visibility.Collapsed;
                        ConnectionErrorMessage.Visibility = Visibility.Visible;
                    }

                });

            }


        }

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

        private void useCompassCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            UseCompass = true;
            compass.Start();
        }

        private void useCompassCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            UseCompass = false;
            compass.Stop();
        }

        private void FinishCalibrationButtonClick(object sender, RoutedEventArgs e)
        {
            CalibrationStackPanel.Visibility = Visibility.Collapsed;
            isCalibrationInProgress = false;
        }

        private void PanoramaHold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ContextMenu.Visibility = Visibility.Visible;
            useCompassCheckBox.Focus();
         //   Debug.WriteLine("panorama holded!");
        }

        private void ContextMenuLostFocus(object sender, RoutedEventArgs e)
        {
            ContextMenu.Visibility = Visibility.Collapsed;
        }

    }
}