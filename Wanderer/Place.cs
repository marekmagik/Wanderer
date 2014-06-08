using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanderer
{
    class Place
    {
        public int PlaceId { get; set; }
        public double Lon { get; set; }
        public double Lat { get; set; }
        public string Desc { get; set; }
        public double Distance { get; set; }

        public void Print()
        {
            Console.WriteLine(PlaceId + " " + Lon + " " + Lat + " " + Desc+ " " + Distance);
        }

    }
}
