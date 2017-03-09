using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NVision.Api.Model;
using NVision.Internal.Model;

namespace NVision.Api.Service
{
    internal class FormSimilarityService
    {
        private const double FuzzinessSpread = 1;
        private const double FuzzinessStrenght = 6;
        private readonly double[,] _detectionMask;
        private const int Md = 3;
        private const int Size = 7;
        public FormSimilarityService()
        {
            _detectionMask = new double[Size, Size];
            _detectionMask[Md, Md] = 1;
            _detectionMask = Fuzzify(_detectionMask, Size);


        }
        public double EvalFormSimilarity(Form form, GrayscaleStandardImage image, Point position)
        {
            double score = 0;
            int whiteCount = 0;

            foreach(var white in form.Whites) { 

                var whiteX = position.X +(white.X - form.Center.X);
                var whiteY = position.Y + (white.Y - form.Center.Y);

                if (whiteX >= 0 && whiteX < image.Width && whiteY >= 0 && whiteY < image.Height)
                {
                    int beginX = Math.Max(0, whiteX - Md);
                    int endX = Math.Min(image.Width, whiteX + Md);

                    int beginY = Math.Max(0, whiteY - Md);
                    int endY = Math.Min(image.Height, whiteY + Md);

                    var pixelScores = new List<double>();
                    for (int i = beginX; i < endX; i++)
                    {
                        for (int j = beginY; j < endY; j++)
                        {
                            pixelScores.Add((image.C[i,j]* _detectionMask[i- whiteX + Md, j- whiteY + Md] /255));
                        }
                    }

                    score += pixelScores.Max();
                }

            }


            score = score / form.Whites.Count;

            return score;
        }


        private double[,] Fuzzify(double[,] mask, int size)
        {
            var fuzinessSize = (int)(size * FuzzinessSpread);

            // Fuzzifie
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (mask[i, j] == 1)
                    {
                        for (int x = 0; x < fuzinessSize; x++)
                        {
                            for (int y = 0; y < fuzinessSize; y++)
                            {
                                var posX = i - (x - fuzinessSize / 2);
                                var posY = j - (y - fuzinessSize / 2);
                                if (posX < size && posX >= 0 && posY < size && posY >= 0 && !(posX == i && posY == j) && mask[posX, posY] != 1)
                                {
                                    var distanceX = (double)(1 + fuzinessSize / 2 - Math.Abs(x - fuzinessSize / 2)) / fuzinessSize / 2;
                                    var distanceY = (double)(1 + fuzinessSize / 2 - Math.Abs(y - fuzinessSize / 2)) / fuzinessSize / 2;
                                    mask[posX, posY] += FuzzinessStrenght * ((Math.Min(distanceX, distanceY)) / 2);
                                }
                            }
                        }
                    }
                }
            }

            return mask;
        }

        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    str += " " + _detectionMask[i, j].ToString("N1")+ " ";
                }
                str += "\n";
            }

            return str;
        }
    }
}
