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

        private const int FontSize = 20;
        private int _maxWidth;
        private const int OneLineWidth = 380;
        private const String WordTextBlock = "word";
        private const String WhitesignsTextBlock = "whitesigns";

        public Dictionary<String, String> GetSeparatedMetadataInJSONFormatAndHashes(string json)
        {
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
                List<TextBlock> textBlocks = SplitDescriptionIntoBlocks(fullDescription, fullDescription.ElementAt(0));

                int actualWidth = 0;
                String shortDescription = "";
                Boolean secondLineConstructed = false;
                Boolean firstLineConstructed = false;

                foreach (TextBlock textBlock in textBlocks)
                {
                    actualWidth+= (int)textBlock.ActualWidth;
                    if (!firstLineConstructed && actualWidth  >= OneLineWidth)
                    {
                        if (textBlock.Name.Equals(WordTextBlock))
                            actualWidth = OneLineWidth + (int)textBlock.ActualWidth;
                        else if (textBlock.Name.Equals(WhitesignsTextBlock))
                            actualWidth = OneLineWidth;
                        firstLineConstructed = true;
                    }

                    if (!secondLineConstructed)
                    {
                        if (actualWidth >= _maxWidth)
                        {
                            secondLineConstructed = true;
                            shortDescription += " ...";
                        }
                        else
                            shortDescription += textBlock.Text;
                    }
                }

                metadata.PictureAdditionalDescription = shortDescription;
                metadata.PictureDescriptionToChange = fullDescription;
            });
        }

        private List<TextBlock> SplitDescriptionIntoBlocks(String fullDescription, Char firstSign)
        {
            Boolean isWordTextBlock;
            if(firstSign.Equals(' '))
                isWordTextBlock=false;
            else
                isWordTextBlock=true;

            String textForActualBlock = "";
            List<TextBlock> textBlocks = new List<TextBlock>();


            for (int i = 0; i < fullDescription.Length; i++)
            {
                textForActualBlock += fullDescription.ElementAt(i);
                if(i==fullDescription.Length-1 ||  CheckIfEndOfTextBlock(isWordTextBlock,fullDescription.ElementAt(i+1))){
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = textForActualBlock;
                    textBlock.FontSize = FontSize;
                    textBlock.Name = GetTypeOfTextBlock(isWordTextBlock);
                    textBlocks.Add(textBlock);
                    textForActualBlock = "";
                    isWordTextBlock = !isWordTextBlock;
                }
            }

            return textBlocks;
        }

        private string GetTypeOfTextBlock(bool isWordTextBlock)
        {
            if (isWordTextBlock)
                return WordTextBlock;
            else
                return WhitesignsTextBlock;
        }

        private bool CheckIfEndOfTextBlock(bool isWordTextBlock, char sign)
        {
            if (isWordTextBlock && sign.Equals(' '))
                return true;
            else if (!isWordTextBlock && !sign.Equals(' '))
                return true;
            else
                return false;
        }

        private void SetMaxWidth()
        {
            _maxWidth = OneLineWidth * 2 - GetTextWidth(" ...");
        }

        private Color getColor(string color)
        {
            if (color == null)
                return Colors.Black;
            if (color.Equals("b"))
                return Colors.Black;
            else if (color.Equals("y"))
                return Colors.Yellow;
            else
                return Colors.White;
        }

        private int GetTextWidth(String text)
        {
            int width = 0;
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.FontSize = FontSize;
            width = (int)textBlock.ActualWidth;
            return width;
        }
    }
}
