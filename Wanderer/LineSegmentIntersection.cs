using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace LineSegmentIntersection
{
    // Na podstawie:
    // http://www.dcs.gla.ac.uk/~pat/52233/slides/Geometry1x1.pdf
    class LineSegmentIntersection
    {
        private static bool onSegment(Point p, Point q, Point r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y))
                return true;

            return false;
        }

        // 0 --> Punkty współliniowe
        // 1 --> Orientacja zgodna z ruchem wskazówek zegara
        // 2 --> w p.p.
        private static int orientation(Point p, Point q, Point r)
        {
            double val = (q.Y - p.Y) * (r.X - q.X) -
                      (q.X - p.X) * (r.Y - q.Y);

            if (val == 0)
            {
                return 0;
            }
            return (val > 0) ? 1 : 2;
        }

        private static bool doIntersect(Point pX, Point pY, Point qX, Point qY)
        {
            int o1 = orientation(pX, pY, qX);
            int o2 = orientation(pX, pY, qY);
            int o3 = orientation(qX, qY, pX);
            int o4 = orientation(qX, qY, pY);

            if (o1 != o2 && o3 != o4)
            {
                return true;
            }

            /* Jeśli trzy punkty są współliniowe, należy sprawdzić czy trzeci punkt leży na odcinku
             * łączącym pozostałe dwa.
             */
            if (o1 == 0 && onSegment(pX, qX, pY))
            {
                return true;
            }

            if (o2 == 0 && onSegment(pX, qY, pY))
            {
                return true;
            }

            if (o3 == 0 && onSegment(qX, pX, qY))
            {
                return true;
            }

            if (o4 == 0 && onSegment(qX, pY, qY))
            {
                return true;
            }
            return false;
        }

        public static bool doQuadrilateralsIntersect(Line firstTopLine, Line firstBottomLine, Line secondTopLine, Line secondBottomLine)
        {
            Point firstLeftUpperCorner = new Point(firstTopLine.X1, firstTopLine.Y1);
            Point firstLeftLowerCorner = new Point(firstBottomLine.X1, firstBottomLine.Y1);
            Point firstRightUpperCorner = new Point(firstTopLine.X2, firstTopLine.Y2);
            Point firstRightLowerCorner = new Point(firstBottomLine.X2, firstBottomLine.Y2);

            Point secondLeftUpperCorner = new Point(secondTopLine.X1, secondTopLine.Y1);
            Point secondLeftLowerCorner = new Point(secondBottomLine.X1, secondBottomLine.Y1);
            Point secondRightUpperCorner = new Point(secondTopLine.X2, secondTopLine.Y2);
            Point secondRightLowerCorner = new Point(secondBottomLine.X2, secondBottomLine.Y2);


            if (doIntersect(firstLeftLowerCorner, firstLeftUpperCorner, secondLeftLowerCorner, secondLeftUpperCorner) ||
               doIntersect(firstLeftLowerCorner, firstLeftUpperCorner, secondLeftUpperCorner, secondRightUpperCorner) ||
               doIntersect(firstLeftLowerCorner, firstLeftUpperCorner, secondRightUpperCorner, secondRightLowerCorner) ||
               doIntersect(firstLeftLowerCorner, firstLeftUpperCorner, secondRightLowerCorner, secondLeftLowerCorner)
                )
            {
                return true;
            }

            if (doIntersect(firstLeftUpperCorner, firstRightUpperCorner, secondLeftLowerCorner, secondLeftUpperCorner) ||
               doIntersect(firstLeftUpperCorner, firstRightUpperCorner, secondLeftUpperCorner, secondRightUpperCorner) ||
               doIntersect(firstLeftUpperCorner, firstRightUpperCorner, secondRightUpperCorner, secondRightLowerCorner) ||
               doIntersect(firstLeftUpperCorner, firstRightUpperCorner, secondRightLowerCorner, secondLeftLowerCorner)
                )
            {
                return true;
            }

            if (doIntersect(firstRightUpperCorner, firstRightLowerCorner, secondLeftLowerCorner, secondLeftUpperCorner) ||
               doIntersect(firstRightUpperCorner, firstRightLowerCorner, secondLeftUpperCorner, secondRightUpperCorner) ||
               doIntersect(firstRightUpperCorner, firstRightLowerCorner, secondRightUpperCorner, secondRightLowerCorner) ||
               doIntersect(firstRightUpperCorner, firstRightLowerCorner, secondRightLowerCorner, secondLeftLowerCorner)
                )
            {
                return true;
            }

            if (doIntersect(firstRightLowerCorner, firstLeftLowerCorner, secondLeftLowerCorner, secondLeftUpperCorner) ||
               doIntersect(firstRightLowerCorner, firstLeftLowerCorner, secondLeftUpperCorner, secondRightUpperCorner) ||
               doIntersect(firstRightLowerCorner, firstLeftLowerCorner, secondRightUpperCorner, secondRightLowerCorner) ||
               doIntersect(firstRightLowerCorner, firstLeftLowerCorner, secondRightLowerCorner, secondLeftLowerCorner)
                )
            {
                return true;
            }

            return false;
        }

    }
}
