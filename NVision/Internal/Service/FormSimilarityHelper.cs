using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NVision.Api.Model;
using NVision.Internal.Model;

namespace NVision.Internal.Service
{
    internal class FormSimilarityHelper
    {
        private const double FuzzinessSpread = 1;
        private const double FuzzinessStrenght = 6;
        private static double[,] _detectionMask;
        private const int Md = 3;
        private const int Size = 7;

        private static FormSimilarityHelper _instance;

        private FormSimilarityHelper() { }

        public static FormSimilarityHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FormSimilarityHelper();
                    _detectionMask = new double[Size, Size];
                    _detectionMask[Md, Md] = 1;
                    _detectionMask = Fuzzify(_detectionMask, Size);
                }
                return _instance;
            }
        }
        public SimilarityResult SearchForCorner(Form form, StandardImage image, GrayscaleStandardImage edges, Area area)
        {
            var scores = new List<SimilarityResult>();
            for (int i = area.From.X; i < area.To.X; i++)
            {
                for (int j = area.From.Y; j < area.To.Y; j++)
                {
                    var position = new Point(i, j);
                    if (edges.C[i,j]==255)
                        scores.Add(new SimilarityResult(position, EvalFormSimilarity(form, image, position)));
                }
            }

            var orderedScores = scores.OrderByDescending(x => x.Similarity);

            var bestResult = orderedScores.First();

            return bestResult;
        }

        public SimilarityResult SearchForForm(Form form, GrayscaleStandardImage image, Area area)
        {
            var scores = new List<SimilarityResult>();
            for (int i = area.From.X; i < area.To.X; i++)
            {
                for (int j = area.From.Y; j < area.To.Y; j++)
                {
                    var position = new Point(i, j);
                    if (image.C[i, j] == 255)
                        scores.Add(new SimilarityResult(position, EvalFormSimilarity(form, image, position)));
                }
            }

            var orderedScores = scores.OrderByDescending(x => x.Similarity);

            var bestResult = orderedScores.First();

            return bestResult;
        }

        public double EvalFormSimilarity(Form form, StandardImage image, Point position)
        {
            Color positionPixelColor = Color.FromArgb(1,
                image.R[position.X, position.Y],
                image.G[position.X, position.Y],
                image.B[position.X, position.Y]);

            double score = 0;
            int beginX = Math.Max(0, position.X - form.Width / 2);
            int endX = Math.Min(image.Width, position.X + form.Width / 2);

            int beginY = Math.Max(0, position.Y - form.Height / 2);
            int endY = Math.Min(image.Height, position.Y + form.Height / 2);

            var test = 0;
            if (position.X == 161 && position.Y == 58)
                test = 1;

            //double topLeftAverageDistance = 0;
            //double topRightAverageDistance = 0;
            //double topAverageDistance = 0;
            //double topLeftAverageDistance = 0;
            var pixelAnalysed = 0;
            for (int i = beginX; i < endX; i++)
            {
                for (int j = beginY; j < endY; j++)
                {
                    pixelAnalysed++;
                    var xMask = i - position.X + form.Width / 2;
                    var yMask = j - position.Y + form.Height / 2;
                    var evaluatedPixelColor = Color.FromArgb(1,
                         image.R[i, j],
                         image.G[i, j],
                         image.B[i, j]);

                    var distance = ImageHelper.GetDistanceBetweenColors(positionPixelColor, evaluatedPixelColor);
                    if ((form.Mask[xMask, yMask] && distance < 75) || (!form.Mask[xMask, yMask] && distance > 75))
                        score++;

                }
            }



            score = score/pixelAnalysed;

            return score;
        }

        public double EvalFormSimilarity(Form form, GrayscaleStandardImage image, Point position)
        {
            double score = 0;
            int whiteCount = 0;

            foreach (var white in form.Whites)
            {

                var whiteX = position.X + (white.X - form.Center.X);
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
                            pixelScores.Add((image.C[i, j] * _detectionMask[i - whiteX + Md, j - whiteY + Md] / 255));
                        }
                    }

                    score += pixelScores.Max();
                }

            }


            score = score / form.Whites.Count;

            return score;
        }


        private static double[,] Fuzzify(double[,] mask, int size)
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
                    str += " " + _detectionMask[i, j].ToString("N1") + " ";
                }
                str += "\n";
            }

            return str;
        }
    }
}
