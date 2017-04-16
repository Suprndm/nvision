using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NVision.Internal.Formatting;
using NVision.Internal.Model;

namespace NVision.Internal.Service
{
    internal class DocumentStraightenerService
    {
        public StandardImage StraightenDocument(StandardImage image, IList<Point> points)
        {
            double[] system = RotationHelper.GetSystem(points.ToArray());
            int W = 375, H = 500;
            StandardImage target = ImageStandardizer.CreateStandardImage(W, H);

            // pour chaque pixel (x,y) de l'image corrigée
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {

                    // conversion dans le repère orthonormé (u,v) [0,1]x[0,1]
                    double u = (double)x / W;
                    double v = (double)y / H;

                    // passage dans le repère perspective
                    double[] P = RotationHelper.Invert(u, v, system);


                    // copie du pixel (px,py) correspondant de l'image source 
                    // TODO: faire une interpolation
                    int px = (int)Math.Round(P[0]);
                    int py = (int)Math.Round(P[1]);
                    int colorR = 0;
                    int colorG = 0;
                    int colorB = 0;

                    if (px < 0 || px >= W || py < 0 || py >= H)
                    {
                        colorR = 0;
                        colorG = 0;
                        colorB = 0;
                    }
                    else
                    {
                        colorR = image.R[px, py];
                        colorG = image.G[px, py];
                        colorB = image.B[px, py];
                    }

                    target.R[x, y] = colorR;
                    target.G[x, y] = colorG;
                    target.B[x, y] = colorB;

                }
            }
            return target;
        }
    }
}
