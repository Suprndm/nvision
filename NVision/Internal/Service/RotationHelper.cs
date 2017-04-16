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
    }
}
