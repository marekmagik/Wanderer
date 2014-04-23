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

namespace Wanderer
{
    public partial class PanoramaView : PhoneApplicationPage
    {
        private double MIN_SCALE;
        private readonly double MAX_SCALE = 1.0;

        private double currentScale;

        private int height;
        private int width;
        private int screenWidth;
        private int screenHeight;

        private ImageSource imageSource;

        public ImageSource ImageSource
        {
            get { return imageSource; }
            set
            {
                imageSource = value;
            }
        }


        public PanoramaView()
        {
            InitializeComponent();

            PanoramaImageLeft.DataContext = null;
            PanoramaImageRight.DataContext = null;
            imageSource = null;
            GC.Collect();

            StorageFolder localRoot = ApplicationData.Current.LocalFolder;
            //LoadImage("/PołoninaWetlińska.jpg");
            LoadImage("/PołoninaWetlińska.jpg", 800, 480, true);

            //LoadImage("/foto4.jpg", 800, 480, true);
            //LoadImage("/foto4.jpg");
        }

        private async void LoadImage(string filename, int screenResolutionWidth, int screenResolutionHeight, bool isImageFullyPanoramic)
        {
            screenWidth = screenResolutionWidth;
            screenHeight = screenResolutionHeight;

            using (var stream = await LoadImageAsync(filename))
            {
                WriteableBitmap bitmapImage = PictureDecoder.DecodeJpeg(await LoadImageAsync(filename));

                ImageSource = bitmapImage;
                PanoramaImageLeft.DataContext = ImageSource;
                PanoramaImageRight.DataContext = ImageSource;

                CompositeTransform transformLeft = (CompositeTransform)PanoramaImageLeft.RenderTransform;
                CompositeTransform transformRight = (CompositeTransform)PanoramaImageRight.RenderTransform;
                //  double scale = 
                MIN_SCALE = (double)screenHeight / (double)bitmapImage.PixelHeight;
                currentScale = MIN_SCALE;

                transformLeft.ScaleX = currentScale;
                transformLeft.ScaleY = currentScale;
                transformRight.ScaleX = currentScale;
                transformRight.ScaleY = currentScale;

                width = bitmapImage.PixelWidth;
                height = bitmapImage.PixelHeight;

                Debug.WriteLine("Size: " + bitmapImage.PixelWidth + " x " + bitmapImage.PixelHeight);
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



        private void manipulationDeltaHandlerLeft(object sender, ManipulationDeltaEventArgs e)
        {

            if (e.DeltaManipulation.Scale.X > 0.0 && e.DeltaManipulation.Scale.Y > 0.0)
            {

                CompositeTransform scaleTransformLeft = (CompositeTransform)PanoramaImageLeft.RenderTransform;
                CompositeTransform scaleTransformRight = (CompositeTransform)PanoramaImageRight.RenderTransform;

                double xScaleValue = currentScale * e.DeltaManipulation.Scale.X;
                double yScaleValue = currentScale * e.DeltaManipulation.Scale.Y;
                double scaleValue = Math.Max(xScaleValue, yScaleValue);

                if (scaleValue < MIN_SCALE)
                {
                    scaleValue = MIN_SCALE;
                }
                else if (scaleValue > MAX_SCALE)
                {
                    scaleValue = MAX_SCALE;
                }

                scaleTransformLeft.ScaleX = scaleValue;
                scaleTransformLeft.ScaleY = scaleValue;
                scaleTransformRight.ScaleX = scaleValue;
                scaleTransformRight.ScaleY = scaleValue;

                currentScale = scaleValue;

                PanoramaTransformRight.TranslateX = PanoramaTransformLeft.TranslateX + (width * currentScale);
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

                    });

                double yTransform = PanoramaTransformLeft.TranslateY + (e.DeltaManipulation.Translation.Y * currentScale);

                if (yTransform < 0 && (yTransform > (((-1.0) * height * currentScale) + screenHeight)))
                {
                    PanoramaTransformLeft.TranslateY += e.DeltaManipulation.Translation.Y * currentScale;
                    PanoramaTransformRight.TranslateY += e.DeltaManipulation.Translation.Y * currentScale;
                }
                else
                {
                    if (yTransform > (((-1.0) * height * currentScale) + screenHeight))
                    {
                        PanoramaTransformLeft.TranslateY = 0;
                        PanoramaTransformRight.TranslateY = 0;
                    }
                    else
                    {
                        PanoramaTransformLeft.TranslateY = (-1.0) * height * currentScale + screenHeight;// -880;
                        PanoramaTransformRight.TranslateY = (-1.0) * height * currentScale + screenHeight;// -880;
                    }
                }

            }

        }


        private void manipulationDeltaHandlerRight(object sender, ManipulationDeltaEventArgs e)
        {

            if (e.DeltaManipulation.Scale.X > 0.0 && e.DeltaManipulation.Scale.Y > 0.0)
            {

                CompositeTransform scaleTransformLeft = (CompositeTransform)PanoramaImageLeft.RenderTransform;
                CompositeTransform scaleTransformRight = (CompositeTransform)PanoramaImageRight.RenderTransform;

                double xScaleValue = currentScale * e.DeltaManipulation.Scale.X;
                double yScaleValue = currentScale * e.DeltaManipulation.Scale.Y;
                double scaleValue = Math.Max(xScaleValue, yScaleValue);

                if (scaleValue < MIN_SCALE)
                {
                    scaleValue = MIN_SCALE;
                }
                else if (scaleValue > MAX_SCALE)
                {
                    scaleValue = MAX_SCALE;
                }

                scaleTransformLeft.ScaleX = scaleValue;
                scaleTransformLeft.ScaleY = scaleValue;
                scaleTransformRight.ScaleX = scaleValue;
                scaleTransformRight.ScaleY = scaleValue;

                currentScale = scaleValue;

                PanoramaTransformRight.TranslateX = PanoramaTransformLeft.TranslateX + (width * currentScale);
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
                     });

                double yTransform = PanoramaTransformRight.TranslateY + (e.DeltaManipulation.Translation.Y * currentScale);

                if (yTransform < 0 && (yTransform > (((-1.0) * height * currentScale) + screenHeight)))
                {
                    PanoramaTransformLeft.TranslateY += e.DeltaManipulation.Translation.Y * currentScale;
                    PanoramaTransformRight.TranslateY += e.DeltaManipulation.Translation.Y * currentScale;
                }
                else
                {
                    if (yTransform > (((-1.0) * height * currentScale) + screenHeight))
                    {
                        PanoramaTransformLeft.TranslateY = 0;
                        PanoramaTransformRight.TranslateY = 0;
                    }
                    else
                    {
                        PanoramaTransformLeft.TranslateY = (-1.0) * height * currentScale + screenHeight;
                        PanoramaTransformRight.TranslateY = (-1.0) * height * currentScale + screenHeight;
                    }
                }

            }

        }


        private void manipulationCompletedHandler(object sender, ManipulationCompletedEventArgs e)
        {
            //return;
            /* jeśli podczas oderwania palca od ekranu powinna być widoczna bezwładność obrazka */
            if (e.IsInertial)
            {

                /* wyliczenie współczynnika bezwładności na podstawie prędkości przesuwania palca */
                double dx2 = e.FinalVelocities.LinearVelocity.X / 4.0;

                /* utworzenie nowego wątku - ma to na celu opuszczenie Handlera (zwolnienie Dispatchera), aby móc odświeżać ekran. 
                   Odświeżanie okranu odbywa się po ZAKOŃCZENIU metody, którą obsługuje wątek UI, czyli Dispatcher.
                   Handler jest metodą Dispatchera, dlatego nie można wewnątrz niej odświeżać ekranu.
                */
                System.Threading.Thread startupThread =
                 new System.Threading.Thread(new System.Threading.ThreadStart(delegate { ComputeInertia(dx2); }));
                startupThread.Start();

            }
        }


        /* Metoda wyliczająca kolejne (coraz mniejsze) "kroki" bezwładności i odświeża PanoramaImage */
        private void ComputeInertia(double dx2)
        {
            double singleStep;
            dx2 *= currentScale;
            dx2 /= 1.3;
            while (Math.Abs(dx2) > 1.0)
            {
                dx2 = dx2 / 1.45;
                singleStep = dx2;
                //Debug.WriteLine(singleStep);
                /* wywołanie metody odpowiedzialnej za przesuwanie musi odbywać się wewnątrz wątku Dispatchera, 
                 * aby po jej zakończeniu wygenerowany został Event aktualizujący View.
                 */
                while (Math.Abs(singleStep) > 0)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(delegate
                    {
                        singleStep = MoveHorizontial(singleStep);
                    }
                    );

                    /* Cykliczne wstrzymanie aktualizacji jest konieczne, aby efekt zmniejszania inercjii był widoczny.
                     */
                    System.Threading.Thread.Sleep(30);


                }


            }

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

            return distanceToCompleteSingleIntertiaStep;
        }

        private void doubleTapHandler(object sender, System.Windows.Input.GestureEventArgs e)
        {

            if (PanoramaTransformLeft.ScaleX == MAX_SCALE)
            {
                PanoramaTransformLeft.ScaleX = MIN_SCALE;
                PanoramaTransformLeft.ScaleY = MIN_SCALE;
                PanoramaTransformRight.ScaleX = MIN_SCALE;
                PanoramaTransformRight.ScaleY = MIN_SCALE;


                //    PanoramaTransformLeft.TranslateX *= MIN_SCALE;
                //     PanoramaTransformLeft.TranslateX -= 265.0 * MIN_SCALE;
                //      PanoramaTransformRight.TranslateX *= MIN_SCALE;
                //      PanoramaTransformRight.TranslateX -= 265.0 * MIN_SCALE;

                currentScale = MIN_SCALE;
                PanoramaTransformLeft.TranslateY = 0.0;
                PanoramaTransformRight.TranslateY = 0.0;
            }
            else
            {
                double scaleX = currentScale;

                PanoramaTransformLeft.ScaleX = MAX_SCALE;
                PanoramaTransformLeft.ScaleY = MAX_SCALE;
                PanoramaTransformRight.ScaleX = MAX_SCALE;
                PanoramaTransformRight.ScaleY = MAX_SCALE;


                //    PanoramaTransformLeft.TranslateX *= (1 / scaleX);
                // PanoramaTransformLeft.TranslateX += 88.25 * scaleX;
                //    PanoramaTransformRight.TranslateX *= (1 / scaleX);
                //  PanoramaTransformRight.TranslateX += 88.25 * scaleX;

                currentScale = MAX_SCALE;

                PanoramaTransformLeft.TranslateY = -(PanoramaImageRight.ActualHeight);

            }
        }

    }
    /* 
     * BRUDNOPIS:
     * 
     * Rozważyć powiększenie przez odczytanie aktualnych współrzędnych (centrum) potem zmiana 
     * wysokości / szerokości (może wystąpić problem granulacji) i ustawienie punktu centrum.
     * 
    */

    /* Update:
     * Problem granulacji jest już rozwiązany, przez zastąpienie Bitmap przez WriteableBitmap, 
     * TO DO:
     * 
     * - zapytać promotora o powiększanie ponad oryginalny rozmiar zdjęcia,
     * - sparametryzować szerokość drugiego zdjęcia (możliwość ustawienia czarnego tła)
     * - czarny piksel, rozciągnięcie, itd.
     * - sprawdzić cykl życia strony (ponowne ładowanie strony, obrazka, [jeśli taki sam = brak przeładowania])
     * 
     */

}