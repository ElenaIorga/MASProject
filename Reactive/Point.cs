using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reactive
{
    class Point
    {
        public int r;
        public int c;
        public Point cellToWhichIsConnected;

        public Point(int x, int y, Point p)
        {
            r = x;
            c = y;
            cellToWhichIsConnected = p;
        }

        public Point opposite()
        {
            if (r.CompareTo(cellToWhichIsConnected.r) != 0)
                return new Point(r + r.CompareTo(cellToWhichIsConnected.r), c, this);
            if (c.CompareTo(cellToWhichIsConnected.c) != 0)
                return new Point(r, c + c.CompareTo(cellToWhichIsConnected.c), this);
            return null;
        }
    }
}
