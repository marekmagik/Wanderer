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
            panoramaImage.Width = 7000;

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
            PanoramaImageMove.X += e.DeltaManipulation.Translation.X;

        }

    }
}