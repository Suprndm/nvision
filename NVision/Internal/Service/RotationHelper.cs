using System;
using System.Collections.Generic;
using System.Drawing;

namespace NVision.Internal.Service
{
    public static class RotationHelper
    {
        public static double[] GetSystem(Point[] P)
        {
            double[] sYstem = new double[8];
            double sX = (P[0].X - P[1].X) + (P[2].X - P[3].X);
            double sY = (P[0].Y - P[1].Y) + (P[2].Y - P[3].Y);
            double dX1 = P[1].X - P[2].X;
            double dX2 = P[3].X - P[2].X;
            double dY1 = P[1].Y - P[2].Y;
            double dY2 = P[3].Y - P[2].Y;

            double z = (dX1 * dY2) - (dY1 * dX2);
            double g = ((sX * dY2) - (sY * dX2)) / z;
            double h = ((sY * dX1) - (sX * dY1)) / z;

            sYstem[0] = P[1].X - P[0].X + g * P[1].X;
            sYstem[1] = P[3].X - P[0].X + h * P[3].X;
            sYstem[2] = P[0].X;
            sYstem[3] = P[1].Y - P[0].Y + g * P[1].Y;
            sYstem[4] = P[3].Y - P[0].Y + h * P[3].Y;
            sYstem[5] = P[0].Y;
            sYstem[6] = g;
            sYstem[7] = h;

            return sYstem;
        }

        public static double[] Invert(double u, double v, double[] system)
        {
            double X = (system[0] * u + system[1] * v + system[2]) / (system[6] * u + system[7] * v + 1);
            double Y = (system[3] * u + system[4] * v + system[5]) / (system[6] * u + system[7] * v + 1);
            return new double[] { X, Y };
        }

        public static Size GetOriginalDimensions(IList<Point> points)
        {
            double w1 = GetDistanceBetweenTwoPoints(points[0], points[1]);
            double h1 = GetDistanceBetweenTwoPoints(points[1], points[2]);
            double w2 = GetDistanceBetweenTwoPoints(points[2], points[3]);
            double h2 = GetDistanceBetweenTwoPoints(points[3], points[0]);

            double originalW = 0;
            double originalH= 0;
            double widthRatio;
            double heightRatio;

            if (w1 >= w2)
            {
                widthRatio = w1/w2;
                originalW = w1;
            }
            else
            {
                widthRatio = w2 / w1;
                originalW = w2;
            }

            h1 = h1*widthRatio;
            h2 = h2*widthRatio;

            if (h1 >= h2)
            {
               heightRatio = h1 / h2;
                originalH = h1;
            }
            else
            {
                heightRatio = h2 / h1;
                originalH = h2;
            }

            originalW = originalW* heightRatio;

            return new Size((int)originalW, (int)originalH);
        }

        public static int GetDistanceBetweenTwoPoints(Point a, Point b)
        {
            return (int)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }
    }
}
