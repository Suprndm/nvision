using System;
using System.Drawing;

namespace NVision.Api.Model
{
    public class LineIntersection
    {
        public Line Line1 { get; set; }
        public Line Line2 { get; set; }
        public double WhiteRatio { get; set; }
        public Point IntersectionPoint { get; set; }

        public LineIntersection(Line line1, Line line2)
        {
            var a = line1.A;
            var b = line1.B;
            var c = line2.A;
            var d = line2.B;

            Line1 = line1;
            Line2 = line2;

            int x = (int)((d - b)/(a - c));
            int y = (int) ((a*d - b*c)/(a - c));
            IntersectionPoint = new Point(x, y);

            WhiteRatio = (Line1.WhiteRatio + line2.WhiteRatio)/2;
        }
    }
}
