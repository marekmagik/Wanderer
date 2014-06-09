using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanderer
{
    class JSONParser
    {
        public List<Place> ParsePlacesJSON(string json)
        {
            List<Place> places = new List<Place>();

            JArray jsonArray = JArray.Parse(json);

            foreach (JObject obj in jsonArray.Children<JObject>())
            {
                Place place = new Place();

                foreach (JProperty property in obj.Properties())
                {
                    SetProperty(property, place);
                }

                places.Add(place);
            }

            return places;
        }

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

        }

        public ImageMetadata ParsePhotoMetadataJSON(string json)
        {
            ImageMetadata metadata = new ImageMetadata();

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
            if (property.Name.Equals("perc"))
                metadata.CoverageInPercent = Convert.ToDouble(property.Value.ToString());
            else if (property.Name.Equals("width"))
                metadata.Width = Convert.ToInt32(property.Value.ToString());
            else if (property.Name.Equals("height"))
                metadata.Height = Convert.ToInt32(property.Value.ToString());
        }
    }
}
