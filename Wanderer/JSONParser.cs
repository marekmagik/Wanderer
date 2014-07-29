using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;

namespace Wanderer
{
    class JSONParser
    {

        private int _fontSize = 20;
        private int _maxWidth;
        private int _twoLinesWidth = 750;

        public Dictionary<String, String> GetSeparatedMetadataInJSONFormatAndHashes(string json) {
            Dictionary<String, String> placesInJSON = new Dictionary<String, String>();
            JArray jsonArray = JArray.Parse(json);

            foreach (JObject obj in jsonArray.Children<JObject>())
            {
                string hash = null;
                foreach (JProperty property in obj.Properties())
                {
                    if (property.Name.Equals("picture_hash"))
                        hash = property.Value.ToString();
                }
                placesInJSON.Add(hash, obj.ToString());
            }
            return placesInJSON;
        }


        public List<ImageMetadata> ParsePlacesJSON(string json)
        {
            List<ImageMetadata> places = new List<ImageMetadata>();

            JArray jsonArray = JArray.Parse(json);

            foreach (JObject obj in jsonArray.Children<JObject>())
            {
                ImageMetadata place = new ImageMetadata();

                foreach (JProperty property in obj.Properties())
                {
                    SetMetadata(property, place);
                }

                places.Add(place);
            }

            return places;
        }

        public ImageMetadata ParsePhotoMetadataJSON(string json)
        {
            return ParsePlacesJSON(json).ElementAt(0);
        }

        private void SetMetadata(JProperty property, ImageMetadata metadata)
        {
            if (property.Name.Equals("longitude"))
                metadata.Longitude = Convert.ToDouble(property.Value.ToString());
            else if (property.Name.Equals("latitude"))
                metadata.Latitude = Convert.ToDouble(property.Value.ToString());
            else if (property.Name.Equals("width"))
                metadata.Width = Convert.ToInt32(property.Value.ToString());
            else if (property.Name.Equals("height"))
                metadata.Height = Convert.ToInt32(property.Value.ToString());
            else if (property.Name.Equals("primary_description"))
                metadata.PictureDescription = property.Value.ToString();
            else if (property.Name.Equals("secondary_description"))
            {
                ProcessSecondaryDescripion(metadata, property.Value.ToString());
            }
            else if (property.Name.Equals("coverage"))
                metadata.CoverageInPercent = Convert.ToDouble(property.Value.ToString());
            else if (property.Name.Equals("orientation"))
                metadata.OrientationOfLeftBorder = Convert.ToDouble(property.Value.ToString());
            else if (property.Name.Equals("version"))
                metadata.Version = Convert.ToDouble(property.Value.ToString());
            else if (property.Name.Equals("picture_hash"))
                metadata.PictureSHA256 = property.Value.ToString();
            else if (property.Name.Equals("points"))
            {
                double x = 0, y = 0, linelength = 0, angle = 0;
                Category category = null;
                String primaryDescription = null, secondaryDescription = null, color = null;
                byte alignment = 0;

                JArray jsonArray = JArray.Parse(JArray.Parse(property.Value.ToString()).ToString());

                foreach (JObject obj in jsonArray.Children<JObject>())
                {
                    foreach (JProperty pointProperty in obj.Properties())
                    {
                        if (pointProperty.Name.Equals("primary_description"))
                            primaryDescription = pointProperty.Value.ToString();
                        else if (pointProperty.Name.Equals("secondary_description"))
                            secondaryDescription = pointProperty.Value.ToString();
                        else if (pointProperty.Name.Equals("category"))
                            category = new Category(pointProperty.Value.ToString());
                        else if (pointProperty.Name.Equals("x"))
                            x = Convert.ToDouble(pointProperty.Value.ToString());
                        else if (pointProperty.Name.Equals("y"))
                            y = Convert.ToDouble(pointProperty.Value.ToString());
                        else if (pointProperty.Name.Equals("alignment"))
                            alignment = Convert.ToByte(pointProperty.Value.ToString());
                        else if (pointProperty.Name.Equals("color"))
                            color = pointProperty.Value.ToString();
                        else if (pointProperty.Name.Equals("line_length"))
                            linelength = Convert.ToDouble(pointProperty.Value.ToString());
                        else if (pointProperty.Name.Equals("angle"))
                            angle = Convert.ToDouble(pointProperty.Value.ToString());
                    }
                    Point point = new Point(x, y, category, primaryDescription, secondaryDescription);
                    point.Alignment = alignment;
                    point.Color = getColor(color);
                    point.Angle = angle;
                    point.LineLength = linelength;
                    metadata.Points.Add(point);
                }
            }
        }

        private void ProcessSecondaryDescripion(ImageMetadata metadata, String fullDescription)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                SetMaxWidth();
                int fullDescriptionWidth = fullDescription.Length;
                String shortDescription = "";
                Boolean shouldEnd = false;
                Boolean check = true;
                Boolean ignoreWhiteSigns = false;
                int actualIndex = 0;
                int actualWidth = 0;

                while (!shouldEnd)
                {
                    if (actualIndex == fullDescriptionWidth - 1)
                        shouldEnd = true;

                    Char actualChar = fullDescription.ElementAt(actualIndex);

                    if (!actualChar.Equals(' ') && ignoreWhiteSigns)
                        ignoreWhiteSigns = false;

                    int widthToEndOfNextWord = 0;
                    if (!shouldEnd && fullDescription.ElementAt(actualIndex + 1).Equals(' '))
                    {
                        widthToEndOfNextWord = GetWidthToEndOfNextWord(fullDescription,actualIndex + 1);
                    }
                    int widthOfActualChar = 0;
                    if (actualChar.Equals(' ') && ignoreWhiteSigns)
                        widthOfActualChar = 0;
                    else
                    {
                        TextBlock block = new TextBlock();
                        block.Text = actualChar.ToString();
                        block.FontSize = _fontSize;
                        widthOfActualChar = (int)block.ActualWidth;

                    }

                    if (actualWidth + widthOfActualChar > _maxWidth)
                    {
                        shouldEnd = true;
                        shortDescription += "...";
                    }
                    else
                    {
                        actualIndex++;
                        actualWidth += widthOfActualChar;
                        shortDescription += actualChar;
                    }

                    if (check && widthToEndOfNextWord!=0 && widthToEndOfNextWord + actualWidth > _twoLinesWidth / 2)
                    {
                        check = false;
                        actualWidth = _twoLinesWidth / 2 + 13;
                        ignoreWhiteSigns = true;
                    }
                }

                metadata.PictureDescriptionToChange = fullDescription;
                metadata.PictureAdditionalDescription = shortDescription;
            });
        }

        private int GetWidthToEndOfNextWord(String fullDescription, int index)
        {
            Boolean shouldEnd = false;
            Boolean searchForEnd = false;
            Boolean searchForBegining = true;
            int actualWidth = 0;
            int actualIndex = index;

            while (!shouldEnd)
            {
                Char actualChar = ' ';
                if (actualIndex == fullDescription.Length)
                    shouldEnd = true;
                else
                {
                    actualChar = fullDescription.ElementAt(actualIndex);
                    if (!actualChar.Equals(' ') && searchForBegining)
                    {
                        searchForBegining = false;
                        searchForEnd = true;
                    }

                    if (actualChar.Equals(' ') && searchForEnd)
                    {
                        shouldEnd = true;
                    }
                }

                if (!shouldEnd)
                {
                    TextBlock block = new TextBlock();
                    block.Text = actualChar.ToString();
                    block.FontSize = _fontSize;
                    int widthOfActualChar = (int)block.ActualWidth;
                    actualWidth += widthOfActualChar;
                    actualIndex++;
                }
            }

            return actualWidth;
        }

        private void SetMaxWidth()
        {
            TextBlock text = new TextBlock();
            text.Text = "...";
            text.FontSize = _fontSize;
            _maxWidth = _twoLinesWidth - (int)text.ActualWidth;
        }

        private Color getColor(string color)
        {
            if (color.Equals("b"))
                return Colors.Black;
            else if (color.Equals("y"))
                return Colors.Yellow;
            else
                return Colors.White;
        }
    }
}
