using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NVision.Api.Model
{
    public class Area
    {
        public Area(int xFrom, int yFrom, int xTo, int yTo)
        {
            From = new Point(xFrom, yFrom);
            To = new Point(xTo, yTo);
        }

        public Point From { get; set; }
        public Point To { get; set; }
    }
}
