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
        private double MAX_SCALE = 3.0;

        private int width;
        private String position = "right";
        private int actualPositionLeft = 0;
        private int actualPositionRight = 800;

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
            //LoadImage("/PołoninaWetlińska.jpg");

            LoadImage("/foto4.jpg");
            //LoadImage("/foto4.jpg");
        }

        private async void LoadImage(string filename)
        {
            //BitmapImage bitmapImage2;
            //bitmapImage2.

            using (var stream = await LoadImageAsync(filename))
            {
                WriteableBitmap bitmapImage = PictureDecoder.DecodeJpeg(await LoadImageAsync(filename));

                ImageSource = bitmapImage;
                PanoramaImageLeft.DataContext = ImageSource;
                PanoramaImageRight.DataContext = ImageSource;

                //panoramaImage.Width = bitmapImage.PixelWidth;
                //panoramaImage.Height = bitmapImage.PixelHeight;
                
         //       PanoramaImageRight.Height = 480;
         //      PanoramaImageLeft.Height = 480;

                //width = (int) PanoramaImageLeft.ActualWidth;

                CompositeTransform transformLeft = (CompositeTransform)PanoramaImageLeft.RenderTransform;
                CompositeTransform transformRight = (CompositeTransform)PanoramaImageRight.RenderTransform;
                double scale = 0.5;
                transformLeft.ScaleX = scale;
                transformLeft.ScaleY = scale;
                transformRight.ScaleX = scale;
                transformRight.ScaleY = scale;

                width = (int) (bitmapImage.PixelWidth * scale);

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

                double xScaleValue = scaleTransformLeft.ScaleX * e.DeltaManipulation.Scale.X;
                double yScaleValue = scaleTransformLeft.ScaleY * e.DeltaManipulation.Scale.Y;
                double scaleValue = Math.Max(xScaleValue, yScaleValue);

                if (scaleValue < 1.0)
                {
                    scaleValue = 1.0;
                }
                else if (scaleValue > MAX_SCALE)
                {
                    scaleValue = MAX_SCALE;
                }

                scaleTransformLeft.ScaleX = scaleValue;
                scaleTransformLeft.ScaleY = scaleValue;
                scaleTransformRight.ScaleX = scaleValue;
                scaleTransformRight.ScaleY = scaleValue;
            }
            else
            {
                MoveHorizontial(PanoramaTransformLeft, PanoramaTransformRight, e.DeltaManipulation.Translation.X);

                //  PanoramaTransformLeft.TranslateX += e.DeltaManipulation.Translation.X;

                //       if (PanoramaTransformLeft.ScaleX > 1.0)
                //       {

                PanoramaTransformLeft.TranslateY += e.DeltaManipulation.Translation.Y;
                PanoramaTransformRight.TranslateY += e.DeltaManipulation.Translation.Y;

                //        }

            }

        }


        private void manipulationDeltaHandlerRight(object sender, ManipulationDeltaEventArgs e)
        {

            if (e.DeltaManipulation.Scale.X > 0.0 && e.DeltaManipulation.Scale.Y > 0.0)
            {


                CompositeTransform scaleTransform = (CompositeTransform)PanoramaImageLeft.RenderTransform;

                double xScaleValue = scaleTransform.ScaleX * e.DeltaManipulation.Scale.X;
                double yScaleValue = scaleTransform.ScaleY * e.DeltaManipulation.Scale.Y;
                double scaleValue = Math.Max(xScaleValue, yScaleValue);

                if (scaleValue < 1.0)
                {
                    scaleValue = 1.0;
                }
                else if (scaleValue > MAX_SCALE)
                {
                    scaleValue = MAX_SCALE;
                }

                scaleTransform.ScaleX = scaleValue;
                scaleTransform.ScaleY = scaleValue;

            }
            else
            {
                MoveHorizontial(PanoramaTransformRight, PanoramaTransformLeft, e.DeltaManipulation.Translation.X);

                //  PanoramaTransformLeft.TranslateX += e.DeltaManipulation.Translation.X;

                //       if (PanoramaTransformLeft.ScaleX > 1.0)
                //       {

                PanoramaTransformLeft.TranslateY += e.DeltaManipulation.Translation.Y;
                PanoramaTransformRight.TranslateY += e.DeltaManipulation.Translation.Y;

                //       }

            }

        }


        private void manipulationCompletedHandlerLeft(object sender, ManipulationCompletedEventArgs e)
        {
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
                 new System.Threading.Thread(new System.Threading.ThreadStart(delegate { ComputeInertia(dx2, PanoramaTransformLeft, PanoramaTransformRight); }));
                startupThread.Start();

            }
        }

        private void manipulationCompletedHandlerRight(object sender, ManipulationCompletedEventArgs e)
        {
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
                 new System.Threading.Thread(new System.Threading.ThreadStart(delegate { ComputeInertia(dx2, PanoramaTransformRight, PanoramaTransformLeft); }));
                startupThread.Start();

            }
        }




        /* Metoda wyliczająca kolejne (coraz mniejsze) "kroki" bezwładności i odświeża PanoramaImage */
        private void ComputeInertia(double dx2, CompositeTransform transform, CompositeTransform transform2)
        {
            dx2 /= 1.3;

            while (Math.Abs(dx2) > 1.0)
            {
                dx2 = dx2 / 1.45;

                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    //PanoramaTransform.TranslateX += dx2;
                    MoveHorizontial(transform, transform2, dx2);
                }
                );

                System.Threading.Thread.Sleep(30);

            }

        }

        private void MoveHorizontial(CompositeTransform transform, CompositeTransform transform2, double dx2)
        {
            if (transform2.TranslateX == transform.TranslateX)
                transform2.TranslateX += width - 1;

            transform.TranslateX += dx2;
            transform2.TranslateX += dx2;
            actualPositionRight -= (int)dx2;
            actualPositionLeft -= (int)dx2;

            if (actualPositionLeft > 0 && actualPositionRight < width)
            {
                if (position == "both_l")
                    position = "left";
                else if (position == "both_r")
                    position = "right";
            }

            if (actualPositionRight >= width + 800)
            {
                position = "left";
                actualPositionLeft = 0 - (int)dx2;
                actualPositionRight = 800 - (int)dx2;
            }
            else if (actualPositionRight >= width)
            {
                if (position == "left")
                {
                    transform2.TranslateX += width + 1;
                    transform2.TranslateX += width + 1;
                    position = "both_r";
                }
            }
            else if (actualPositionLeft <= -800)
            {
                position = "right";
                actualPositionLeft = width - 800 - (int)dx2;
                actualPositionRight = width - (int)dx2;
            }
            else if (actualPositionLeft <= 0)
            {
                if (position == "right")
                {
                    transform2.TranslateX -= width - 1;
                    transform2.TranslateX -= width - 1;
                    position = "both_l";
                }
            }
        }

        private void doubleTapHandler(object sender, System.Windows.Input.GestureEventArgs e)
        {

            if (PanoramaTransformLeft.ScaleX == 1.0)
            {
                PanoramaTransformLeft.ScaleX = MAX_SCALE;
                PanoramaTransformLeft.ScaleY = MAX_SCALE;
                PanoramaTransformRight.ScaleX = MAX_SCALE;
                PanoramaTransformRight.ScaleY = MAX_SCALE;

                PanoramaTransformLeft.TranslateX *= MAX_SCALE;
                PanoramaTransformLeft.TranslateX -= 265.0 * MAX_SCALE;
                PanoramaTransformRight.TranslateX *= MAX_SCALE;
                PanoramaTransformRight.TranslateX -= 265.0 * MAX_SCALE;


                PanoramaTransformLeft.TranslateY = -(PanoramaImageRight.ActualHeight);

            }
            else
            {
                double scaleX = PanoramaTransformLeft.ScaleX;
                PanoramaTransformLeft.ScaleX = 1.0;
                PanoramaTransformLeft.ScaleY = 1.0;
                PanoramaTransformRight.ScaleX = 1.0;
                PanoramaTransformRight.ScaleY = 1.0;

                PanoramaTransformLeft.TranslateX *= (1 / scaleX);
                PanoramaTransformLeft.TranslateX += 88.25 * scaleX;
                PanoramaTransformRight.TranslateX *= (1 / scaleX);
                PanoramaTransformRight.TranslateX += 88.25 * scaleX;


                PanoramaTransformLeft.TranslateY = 0.0;
                PanoramaTransformRight.TranslateY = 0.0;
            }
        }

    }
    /*
     * Rozważyć powiększenie przez odczytanie aktualnych współrzędnych (centrum) potem zmiana 
     * wysokości / szerokości (może wystąpić problem granulacji) i ustawienie punktu centrum.
     * 
    */

}