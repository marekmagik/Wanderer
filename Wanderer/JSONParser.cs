using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;

namespace Wanderer
{
    class JSONParser
    {
        
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
        
/*
        private void SetProperty(JProperty property, Place place)
        {
            if (property.Name.Equals("place_id"))
                place.PlaceId = Convert.ToInt32(property.Value.ToString());
            else if (property.Name.Equals("lon"))
                place.Lon = Convert.ToDouble(property.Value.ToString());
            else if (property.Name.Equals("lat"))
                place.Lat = Convert.ToDouble(property.Value.ToString());
            else if (property.Name.Equals("desc"))
                place.Desc = property.Value.ToString();
            else if (property.Name.Equals("distance"))
                place.Distance = Convert.ToDouble(property.Value.ToString());
            place.Distance /= 1000;
            place.Distance = Math.Round(place.Distance, 2);
        }
*/  
        public ImageMetadata ParsePhotoMetadataJSON(string json)
        {
            ImageMetadata metadata = new ImageMetadata();

            Debug.WriteLine(json);

            JArray jsonArray = JArray.Parse(json);

            foreach (JObject obj in jsonArray.Children<JObject>())
            {

                foreach (JProperty property in obj.Properties())
                {
                    SetMetadata(property, metadata);
                }
            }

            return metadata;
        }

        private void SetMetadata(JProperty property, ImageMetadata metadata)
        {
/*
            if (property.Name.Equals("perc"))
                metadata.CoverageInPercent = Convert.ToDouble(property.Value.ToString());
            else if (property.Name.Equals("width"))
                metadata.Width = Convert.ToInt32(property.Value.ToString());
            else if (property.Name.Equals("height"))
                metadata.Height = Convert.ToInt32(property.Value.ToString());
*/



            if (property.Name.Equals("metadata_id"))
                metadata.IdInDatabase = Convert.ToInt32(property.Value.ToString());
            else if (property.Name.Equals("longitude"))
                metadata.Longitude = Convert.ToDouble(property.Value.ToString());
            else if (property.Name.Equals("latitude"))
                metadata.Latitude = Convert.ToDouble(property.Value.ToString());
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




        }


        private byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

    }
}
