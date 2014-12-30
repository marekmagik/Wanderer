using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;

namespace WandererPanoramasEditor
{
    class JSONParser
    {
        #region Methods
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
                metadata.PictureAdditionalDescription = property.Value.ToString();
            else if (property.Name.Equals("coverage"))
                metadata.CoverageInPercent = Convert.ToDouble(property.Value.ToString());
            else if (property.Name.Equals("orientation"))
                metadata.OrientationOfLeftBorder = Convert.ToDouble(property.Value.ToString());
            else if (property.Name.Equals("version"))
                metadata.Version = Convert.ToDouble(property.Value.ToString());
            else if (property.Name.Equals("picture_hash"))
                metadata.PictureSHA256 = property.Value.ToString();
            else if (property.Name.Equals("category"))
                metadata.Category = property.Value.ToString();
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
                    point.Color = color[0];
                    point.Angle = angle;
                    point.LineLength = linelength;
                    metadata.Points.Add(point);
                }
            }
        }
        #endregion
    }
}