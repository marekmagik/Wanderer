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

namespace Wanderer
{
    public partial class PanoramaView : PhoneApplicationPage
    {
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

            StorageFolder localRoot = ApplicationData.Current.LocalFolder;
            LoadImage("/PołoninaWetlińska.jpg", PanoramaImage);
        }

        private async void LoadImage(string filename, Image panoramaImage)
        {
            BitmapImage bitmapImage = null;
            using (var imageStream = await LoadImageAsync(filename))
            {
                if (imageStream != null)
                {
                    bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(imageStream);
                }
            }

            ImageSource = bitmapImage;
            panoramaImage.DataContext = imageSource;

            panoramaImage.Width = bitmapImage.PixelWidth;

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
            PanoramaTransform.X += e.DeltaManipulation.Translation.X;
        }

        private void manipulationCompletedHandler(object sender, ManipulationCompletedEventArgs e)
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
                 new System.Threading.Thread(new System.Threading.ThreadStart(delegate { ComputeInertia(dx2); }));
                startupThread.Start();

            }
        }

        /* Metoda wyliczająca kolejne (coraz mniejsze) "kroki" bezwładności i odświeża PanoramaImage */
        private void ComputeInertia(double dx2)
        {
            while (Math.Abs(dx2) > 1.0)
            {
                dx2 = dx2 / 2.2;

                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    PanoramaTransform.X += dx2;
                }
                );

                System.Threading.Thread.Sleep(30);

            }

        }


    }
}