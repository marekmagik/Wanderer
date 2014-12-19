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

        private const string Filename = "places_data.wdf";
        private static List<String> _cachedPhotos;
        private static List<String> _cachedThumbnails;
        private static List<String> _cachedCategories;

        public static void InitIsolatedStorageDAO()
        {
            if (_cachedPhotos == null)
                _cachedPhotos = new List<string>();
            if (_cachedThumbnails == null)
                _cachedThumbnails = new List<string>();
            if (_cachedCategories == null)
                _cachedCategories = new List<string>();

            LoadPlacesData();
            LoadNewFiles();
            LoadCategories();
        }

        private static void LoadCategories()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string[] categories = storage.GetFileNames("/categories/*");
                foreach (string category in categories)
                {
                    Debug.WriteLine(category.Remove(category.Length - 4));
                    if (!_cachedPhotos.Contains(category))
                        _cachedPhotos.Add(category.Remove(category.Length-4));
                }

            }
        }

        public static ImageMetadata getCachedMetadata(string hash)
        {
            JSONParser parser = new JSONParser();

            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (StreamReader reader = new StreamReader(storage.OpenFile("/metadata/" + hash + ".meta", FileMode.Open, FileAccess.Read))) {
                    return parser.ParsePhotoMetadataJSON(reader.ReadToEnd());
                }
            }
        }

        public static void CacheMetadata(string json)
        {
            JSONParser parser = new JSONParser();

            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                foreach (KeyValuePair<String, String> metadataAndHash in parser.GetSeparatedMetadataInJSONFormatAndHashes(json))
                {
                    if (!storage.FileExists("/metadata/" + metadataAndHash.Key + ".meta"))
                    {
                        using (StreamWriter writer = new StreamWriter(storage.CreateFile("/metadata/" + metadataAndHash.Key + ".meta")))
                        {
                            writer.Write("[" + metadataAndHash.Value + "]");
                        }
                    }
                    else
                    {
                        // plik już istnieje, dodać metodę rozwiązywania konfliktów (wersjonowanie).
                    }
                }
            }
        }

        public static void LoadPlacesData()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {

                if (!storage.DirectoryExists("thumbnails"))
                    storage.CreateDirectory("thumbnails");

                if (!storage.DirectoryExists("photos"))
                    storage.CreateDirectory("photos");

                if (!storage.DirectoryExists("metadata"))
                    storage.CreateDirectory("metadata");

                if (!storage.DirectoryExists("categories"))
                    storage.CreateDirectory("categories");

                if (storage.FileExists(Filename))
                {
                    // tutaj zaladowanie places oraz cachedFiles z pliku
                    // czyli informacji o cahcowanych mmiejscach oraz o plikach ktore sa chachowane
                    // takze dodawanie do dictonary
                    // ^ DEPRECATED
                }
                else
                {
                    storage.CreateFile(Filename);
                }
            }
        }

        public static void LoadNewFiles()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string[] photos = storage.GetFileNames("/photos/*");
                foreach (string photo in photos)
                {
                    if (!_cachedPhotos.Contains(photo))
                        _cachedPhotos.Add(photo.Remove(64, 4));
                }

                string[] thumbnails = storage.GetFileNames("/thumbnails/*");
                foreach (string thumbnail in thumbnails)
                {
                    Debug.WriteLine("WCZYTUJE MINIATURE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + thumbnail);
                    if (!_cachedThumbnails.Contains(thumbnail))
                        _cachedThumbnails.Add(thumbnail.Remove(64, 4));
                }

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
            foreach (string photo in _cachedPhotos)
            {
                Debug.WriteLine(photo);
            }
            Debug.WriteLine(" arg " + hash);

            if (_cachedPhotos.Contains(hash))
                return true;
            else
                return false;
        }

        public static bool IsThumbnailCached(string hash)
        {
            if (_cachedThumbnails.Contains(hash))
                return true;
            else
                return false;
        }

        public static void CachePhoto(Stream image, int width, int height, ImageMetadata metadata)
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

            _cachedPhotos.Add(hash);
            Debug.WriteLine(" size of list " + _cachedPhotos.Count);
        }

        public static void CacheCategory(String category, int count)
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                IsolatedStorageFileStream fileStream = storage.CreateFile("/categories/" + category + ".wmf");
                byte[] buffer = BitConverter.GetBytes(count);
                fileStream.Write(buffer, 0, buffer.Length);
                Debug.WriteLine("saving category");
                fileStream.Close();
            }
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
            _cachedThumbnails.Add(hash);
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

    }
}
