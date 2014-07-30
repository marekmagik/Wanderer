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

namespace WandererPanoramasEditor
{

    public partial class GenerateJSONWindow : Window
    {
        private readonly ImageMetadata _metadata;
        private readonly string _imageFileName;

        public GenerateJSONWindow(ImageMetadata metadata, string imageFileName)
        {
            InitializeComponent();
            this._metadata = metadata;
            this._imageFileName = imageFileName;
            ImageDescriptionTextBox.Text = metadata.PictureDescription;
            ImageAdditionalDescriptionTextBox.Text = metadata.PictureAdditionalDescription;
            ImageOrientationTextBox.Text = Convert.ToString(metadata.OrientationOfLeftBorder);
            PanoramaCoverageTextBox.Text = Convert.ToString(metadata.CoverageInPercent);
            LongitudeTextBox.Text = Convert.ToString(metadata.Longitude);
            LatitudeTextBox.Text = Convert.ToString(metadata.Latitude);
        }

        private void GenerateButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string description = ImageDescriptionTextBox.Text;
                string additionalDescription = ImageAdditionalDescriptionTextBox.Text;
                double orientation = Convert.ToDouble(ImageOrientationTextBox.Text);
                double coverage = Convert.ToDouble(PanoramaCoverageTextBox.Text);
                double longitude = Convert.ToDouble(LongitudeTextBox.Text);
                double latitude = Convert.ToDouble(LatitudeTextBox.Text);

                _metadata.PictureDescription = description;
                _metadata.PictureAdditionalDescription = additionalDescription;
                _metadata.OrientationOfLeftBorder = orientation;
                _metadata.CoverageInPercent = coverage;
                _metadata.Longitude = longitude;
                _metadata.Latitude = latitude;
                _metadata.Version += 0.001;
                using (FileStream stream = File.Open(_imageFileName, System.IO.FileMode.Open, FileAccess.Read))
                {
                    SHA256Managed sha256 = new SHA256Managed();
                    string hex = BitConverter.ToString(sha256.ComputeHash(stream));
                    Debug.WriteLine(hex);
                    hex = hex.Replace("-", "");
                    hex = hex.ToLower();
                    _metadata.PictureSHA256 = hex;
                }

                string result = JsonConvert.SerializeObject(_metadata);

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
