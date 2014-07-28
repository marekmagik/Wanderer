using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Wanderer
{
    public class ImageMetadata
    {
        public List<Point> Points { get; set; }
        public List<Category> Categories { get; set; }
        public double OrientationOfLeftBorder { get; set; }
        public double CoverageInPercent { get; set; }
        public String PictureDescription { get; set; }
        public String PictureAdditionalDescription { get; set; }
        public String PictureDescriptionToChange { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Version { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public ImageSource Image { get; set; }
        public String PictureSHA256 { get; set; }
        public bool IsPanoramaCached
        {
            get
            {
                return IsolatedStorageDAO.IsPhotoCached(PictureSHA256);
            }
        }

        public ImageMetadata()
        {
            Points = new List<Point>();
            Categories = new List<Category>();
        }

        public void ToggleDescriptions()
        {
            String temp = PictureAdditionalDescription;
            PictureAdditionalDescription = PictureDescriptionToChange;
            PictureDescriptionToChange = temp;
        }
    }
}
