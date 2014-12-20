using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.ComponentModel;
using System.Net;

namespace WandererPanoramasEditor
{

    public partial class GenerateJSONWindow : Window
    {
        private readonly ImageMetadata _metadata;
        private readonly string _imageFileName;
        public static String GENERATE_JSON_MODE = "GenerateJSONMode";
        public static String SEND_TO_SERVER_MODE = "SendToServerMode";
        private String _actualMode;
        private byte[] _image;
        private Boolean _shouldDeletePlaceFromWaitingList;
        private BindingList<ImageMetadata> _waitingRoomList;

        public GenerateJSONWindow(ImageMetadata metadata, string imageFileName, String mode , byte [] image, bool ifDeletePlaceFromWaitingRoom, BindingList<ImageMetadata> bindingList)
        {
            InitializeComponent();
            this._metadata = metadata;
            this._imageFileName = imageFileName;
            ImageDescriptionTextBox.Text = metadata.PictureDescription;
            ImageAdditionalDescriptionTextBox.Text = metadata.PictureAdditionalDescription;
            ImageCategoryTextBox.Text = metadata.Category;
            ImageOrientationTextBox.Text = Convert.ToString(metadata.OrientationOfLeftBorder);
            PanoramaCoverageTextBox.Text = Convert.ToString(metadata.CoverageInPercent);
            LongitudeTextBox.Text = Convert.ToString(metadata.Longitude);
            LatitudeTextBox.Text = Convert.ToString(metadata.Latitude);
            this._actualMode=mode;
            this._image = image;
            _shouldDeletePlaceFromWaitingList = ifDeletePlaceFromWaitingRoom;
            this._waitingRoomList = bindingList;

            if(mode.Equals(GENERATE_JSON_MODE))
                GenerateButtonTextBlock.Text = "Generuj plik metadanych";
            else if(mode.Equals(SEND_TO_SERVER_MODE))
                GenerateButtonTextBlock.Text = "Wyślij na serwer";
        }

        private void GenerateButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string description = ImageDescriptionTextBox.Text;
                string additionalDescription = ImageAdditionalDescriptionTextBox.Text;
                string category = ImageCategoryTextBox.Text;
                double orientation = Convert.ToDouble(ImageOrientationTextBox.Text);
                double coverage = Convert.ToDouble(PanoramaCoverageTextBox.Text);
                double longitude = Convert.ToDouble(LongitudeTextBox.Text);
                double latitude = Convert.ToDouble(LatitudeTextBox.Text);

                _metadata.PictureDescription = description;
                _metadata.PictureAdditionalDescription = additionalDescription;
                _metadata.Category = category;
                _metadata.OrientationOfLeftBorder = orientation;
                _metadata.CoverageInPercent = coverage;
                _metadata.Longitude = longitude;
                _metadata.Latitude = latitude;
                _metadata.Version += 0.001;
                if ("".Equals(_metadata.PictureSHA256))
                {
                    using (FileStream stream = File.Open(_imageFileName, System.IO.FileMode.Open, FileAccess.Read))
                    {
                        SHA256Managed sha256 = new SHA256Managed();
                        string hex = BitConverter.ToString(sha256.ComputeHash(stream));
                        Debug.WriteLine(hex);
                        hex = hex.Replace("-", "");
                        hex = hex.ToLower();
                        _metadata.PictureSHA256 = hex;
                    }
                }

                string result = JsonConvert.SerializeObject(_metadata);

                if (_actualMode.Equals(GENERATE_JSON_MODE))
                {
                    Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
                    dialog.Filter = "Metadane programu Wanderer|*.wan";
                    dialog.ShowDialog();
                    Console.WriteLine(dialog.FileName);
                    if (!dialog.FileName.Equals(""))
                    {
                        using (FileStream fileStream = new FileStream(dialog.FileName, FileMode.CreateNew))
                        {
                            using (StreamWriter writer = new StreamWriter(fileStream))
                            {
                                writer.Write(result);
                            }
                        }
                    }
                }
                else if (_actualMode.Equals(SEND_TO_SERVER_MODE))
                {


                    Boolean resultOfRequest = ServerDAO.SendDataToServer(_image, _metadata);
                    if (_shouldDeletePlaceFromWaitingList)
                        resultOfRequest = ServerDAO.DeletePlaceFromWaitingRoom(_metadata.PictureSHA256);

                    this.Close();
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                    {
                        _waitingRoomList.Remove(_metadata);
                    }));

                    if(resultOfRequest)
                        MessageBox.Show("Zdjęcie pomyślnie wysłane na serwer", "Wanderer");
                    else
                        MessageBox.Show("Błąd podczas wysyłania zdjęcia na serwer", "Wanderer");
                }

            }
            catch (WebException)
            {
                MessageBox.Show("Nieudana próba połączenia z serwerem", "Wanderer");
            }
            catch (Exception)
            {
                showToolTipOnError();
            }

        }

        private void showToolTipOnError()
        {
            ToolTip toolTip = new ToolTip() { Content = "Podane wartości są niepoprawne!" };
            GenerateButton.ToolTip = toolTip;
            toolTip.IsOpen = true;
        }
    }
}
