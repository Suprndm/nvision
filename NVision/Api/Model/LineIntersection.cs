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
            Line1 = line1;
            Line2 = line2;

            int a = (int)((line2.B - line1.B) / (line1.A - Line2.A));
            int b = (int)((line1.A * line2.B - line1.B * line2.A) / (line1.A - Line2.A));
            IntersectionPoint = new Point(a, b);

            WhiteRatio = (Line1.WhiteRatio + line2.WhiteRatio)/2;
        }
    }
}
