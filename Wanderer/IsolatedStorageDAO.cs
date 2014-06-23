using Microsoft.Phone;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Wanderer
{
    static class IsolatedStorageDAO
    {

        private const string filename = "places_data.txt";
        private static List<string> cachedPhotos;
        private static List<string> cachedThumbnails;

        public static void InitIsolatedStorageDAO()
        {
            if (cachedPhotos == null)
                cachedPhotos = new List<string>();
            if (cachedThumbnails == null)
                cachedThumbnails = new List<string>();

            LoadPlacesData();
            LoadNewFiles();
        }

        public static void LoadPlacesData()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (storage.FileExists(filename))
                {
                    // tutaj zaladowanie places oraz cachedFiles z pliku
                    // czyli informacji o cahcowanych mmiejscach oraz o plikach ktore sa chachowane
                    // takze dodawanie do dictonary
                }

                if (!storage.DirectoryExists("thumbnails"))
                    storage.CreateDirectory("thumbnails");

                if (!storage.DirectoryExists("photos"))
                    storage.CreateDirectory("photos");

            }

        }

        public static void LoadNewFiles()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string[] photos = storage.GetFileNames("photos\\*");
                foreach (string photo in photos)
                {
                    if (!cachedPhotos.Contains(photo))
                        cachedPhotos.Add(photo.Remove(64, 4));
                    //ProcessFile(file);
                }

                string[] thumbnails = storage.GetFileNames("thumbnails\\*");
                foreach (string thumbnail in thumbnails)
                {
                    Debug.WriteLine(thumbnail);
                    if (!cachedThumbnails.Contains(thumbnail))
                        cachedThumbnails.Add(thumbnail.Remove(64, 4));
                    //ProcessFile(file);
                }

            }
        }

        public static void ProcessFile(string filename)
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var stream = storage.OpenFile(filename, System.IO.FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    string hash = GenerateHash(buffer);
                    cachedPhotos.Add(hash);
                    Debug.WriteLine(hash + " " + filename);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private static String GenerateHash(byte[] data)
        {
            byte[] result;

            using (SHA256Managed sha256 = new SHA256Managed())
            {
                result = sha256.ComputeHash(data);
            }

            string hex = BitConverter.ToString(result);
            hex = hex.Replace("-", "");
            hex = hex.ToLower();
            return hex;
        }

        public static bool IsPhotoCached(string hash)
        {
            foreach (string photo in cachedPhotos)
            {
                Debug.WriteLine(photo);
            }
            Debug.WriteLine(" arg " + hash);

            if (cachedPhotos.Contains(hash))
                return true;
            else
                return false;
        }

        public static bool IsThumbnailCached(string hash)
        {
            if (cachedThumbnails.Contains(hash))
                return true;
            else
                return false;
        }

        public static void CachePhoto(Stream image, int width, int height)
        {


            byte[] bytes = new byte[image.Length];
            image.Read(bytes, 0, bytes.Length);
            string hash = GenerateHash(bytes);
            image.Position = 0;

            Debug.WriteLine(" starting creating cache " + hash);


            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IsolatedStorageFileStream fileStream = storage.CreateFile("/photos/" + hash + ".jpg");
                byte[] buffer = new byte[image.Length];
                image.Read(buffer, 0, buffer.Length);
                fileStream.Write(buffer, 0, buffer.Length);
                Debug.WriteLine("saving image");
                fileStream.Close();
                image.Position = 0;
            }

            cachedPhotos.Add(hash);
            Debug.WriteLine(" size of list " + cachedPhotos.Count);
        }

        public static void CacheThumbnail(Stream image, int width, int height, string hash)
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IsolatedStorageFileStream fileStream = storage.CreateFile("/thumbnails/" + hash + ".jpg");
                byte[] buffer = new byte[image.Length];
                image.Read(buffer, 0, buffer.Length);
                fileStream.Write(buffer, 0, buffer.Length);
                Debug.WriteLine("saving thumbnail");
                fileStream.Close();
                image.Position = 0;
            }

            Debug.WriteLine(" thumbnail cached ");
            cachedThumbnails.Add(hash);
        }

        public static WriteableBitmap loadThumbnail(string hash)
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IsolatedStorageFileStream fileStream = storage.OpenFile("thumbnails\\" + hash + ".jpg", System.IO.FileMode.Open, FileAccess.Read);

                WriteableBitmap bitmapImage = PictureDecoder.DecodeJpeg(fileStream);
                return bitmapImage;

            }
        }

        public static Stream loadPhoto(string hash)
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IsolatedStorageFileStream fileStream = storage.OpenFile("photos\\" + hash + ".jpg", System.IO.FileMode.Open, FileAccess.Read);
                return fileStream;

            }
        }

        public static List<ImageMetadata> getCachedPlaces(List<ImageMetadata> places)
        {
            List<ImageMetadata> cachedPlaces = new List<ImageMetadata>();

            foreach (ImageMetadata place in places)
            {
                if (cachedThumbnails.Contains(place.PictureSHA256))
                    cachedPlaces.Add(place);
            }

            return cachedPlaces;
        }

        public static List<ImageMetadata> getNotCachedPlaces(List<ImageMetadata> places)
        {
            List<ImageMetadata> cachedPlaces = new List<ImageMetadata>();

            foreach (ImageMetadata place in places)
            {
                if (!cachedThumbnails.Contains(place.PictureSHA256))
                    cachedPlaces.Add(place);
            }

            return cachedPlaces;
        }
    }
}
