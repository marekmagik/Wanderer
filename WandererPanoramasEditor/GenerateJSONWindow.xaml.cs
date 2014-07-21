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
        private readonly ImageMetadata metadata;
        private readonly string imageFileName;

        public GenerateJSONWindow(ImageMetadata metadata, string imageFileName)
        {
            InitializeComponent();
            this.metadata = metadata;
            this.imageFileName = imageFileName;
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

                metadata.PictureDescription = description;
                metadata.PictureAdditionalDescription = additionalDescription;
                metadata.OrientationOfLeftBorder = orientation;
                metadata.CoverageInPercent = coverage;
                metadata.Longitude = longitude;
                metadata.Latitude = latitude;
                metadata.Version += 0.001;
                using (FileStream stream = File.Open(imageFileName, System.IO.FileMode.Open, FileAccess.Read))
                {
                    SHA256Managed sha256 = new SHA256Managed();
                    string hex = BitConverter.ToString(sha256.ComputeHash(stream));
                    Debug.WriteLine(hex);
                    hex = hex.Replace("-", "");
                    hex = hex.ToLower();
                    metadata.PictureSHA256 = hex;
                }

                string result = JsonConvert.SerializeObject(metadata);

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
