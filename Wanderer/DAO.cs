using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanderer
{
    class DAO
    {

        public List<LabeledPoint> getPointsInRange(double longitude, double latitude, double range)
        {
            List<LabeledPoint> list = new List<LabeledPoint>();

            list.Add(new LabeledPoint(100, 100, LabeledPointType.MOUNTAIN_PEAK, "Napis1 BOLD", "napis1 normal"));
            list.Add(new LabeledPoint(300, 100, LabeledPointType.MOUNTAIN_PEAK, "Napis2 BOLD", "napis2 normal"));


            return list;
        }

    }
}
