using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanderer
{
    
    public class DAO
    {

        public static List<ImageMetadata> getPointsInRange(double longitude, double latitude, double range)
        {
            List<Point> listOfPoints = new List<Point>();
            Category category = new Category("Szczyty");

            listOfPoints.Add(new Point(100.0, 100.0, category, "Wielka góra", "napis1 normal"));
            listOfPoints.Add(new Point(300.0, 100.0, category, "Też duża", "napis2 normal"));

            ImageMetadata image1 = new ImageMetadata();
            image1.addCategory(category);
            image1.Points = listOfPoints;
            image1.PictureDescription = "Stacja kosmiczna Mir";
            List<ImageMetadata> listOfMetadata = new List<ImageMetadata>();
            listOfMetadata.Add(image1);

            return listOfMetadata;
        }

    }
}
