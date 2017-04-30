using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NVision.Api.Model
{
    // A x + B
    public class Line
    {
        public double A { get; set; }
        public double B { get; set; }

        public double Theta { get; set; }
        public double R { get; set; }



        public Line(Point p1, Point p2)
        {
            A = (double)(p1.Y - p2.Y)/(p2.X - p1.X);
            B = p1.Y-(A*p1.X);
        }
    }
}
