using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanderer
{
    
    public class DAO
    {

        public static List<Point> getPointsInRange(double longitude, double latitude, double range)
        {
            List<Point> list = new List<Point>();

            list.Add(new Point(100.0, 100.0, new Category("Szczyty"), "Wielka góra", "napis1 normal"));
            list.Add(new Point(300.0, 100.0, new Category("Szczyty"), "Też duża", "napis2 normal"));


            return list;
        }

    }
}
