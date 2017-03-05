using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Helper;
using NVision.Api.Model;
using NVision.Internal.Formatting;
using NVision.Internal.Model;

namespace NVision.Api.Service
{
    public class ImageService : IImageService
    {
        private readonly ImageStandardizer _imageStandardizer;
        private readonly ILogger _logger;
        private readonly CornersBuilder _cornersBuilder;

        private ImageService(ImageStandardizer imageStandardizer, CornersBuilder cornersBuilder, ILogger logger)
        {
            _imageStandardizer = imageStandardizer;
            _logger = logger;
            _cornersBuilder = cornersBuilder;
        }

        public ImageService(ILogger logger) : this(new ImageStandardizer(), new CornersBuilder(), logger)
        {

        }

        public StandardSchema ExtractSchemaFromImage(Bitmap bitmap)
        {
            var standardImage = _imageStandardizer.ConvertToStandardImage(bitmap);

            return new StandardSchema();
        }


        public Bitmap PrepareImage(Bitmap bitmap)
        {
            bitmap = ReduceSize(bitmap, (double)500 / Math.Max(bitmap.Width, bitmap.Height));
            var standardImage = _imageStandardizer.ConvertToGrayScaleStandardImage(bitmap);
            _logger.Log(bitmap.Width + " x " + bitmap.Height);
            standardImage = ChangeConstrast(standardImage, 250);
            standardImage = ChangeLuminosity(standardImage, 50);

            standardImage = Erosion(standardImage, 0);
            standardImage = Dilatation(standardImage, 0);

            standardImage = Erosion(standardImage, 0);
            standardImage = Dilatation(standardImage, 0);

            standardImage = Erosion(standardImage, 0);
            standardImage = Dilatation(standardImage, 0);

            standardImage = Laplacien(standardImage);
            standardImage = Hat(standardImage, 100);

            var corners = GetCorners(standardImage);
            var coloredStandardImage = _imageStandardizer.ConvertToStandardImage(standardImage);

            foreach (var point in corners)
            {
                coloredStandardImage = DrawIndicator(coloredStandardImage, point.X, point.Y, 2);
            }

            var result = _imageStandardizer.ConvertToBitmap(coloredStandardImage);

            return result;
        }

        private SimilarityResult SearchForForm(Form form, GrayscaleStandardImage image, Area area)
        {
            var scores = new List<SimilarityResult>();
            for (int i = area.From.X+ form.Center.X; i < area.To.X - form.Center.X; i++)
            {
                for (int j = area.From.Y + form.Center.Y; j < area.To.Y - form.Center.Y; j++)
                {
                    var position = new Point(i, j);
                    scores.Add(new SimilarityResult(position,
                        EvalFormSimilarity(form, image, position)));
                }
            }

            var bestResult = scores.OrderByDescending(x => x.Similarity).First();

            return bestResult;
        }

        private IList<Point> GetCorners(GrayscaleStandardImage image)
        {
            var points = new List<Point>();
            var topLeftCornerForm = _cornersBuilder.BuildTopLeftCornerForm();
            var bestResult = SearchForForm(topLeftCornerForm, image, new Area(0, 0, image.Width/2, image.Height/2));
            points.Add(bestResult.Position);
            _logger.Log("Top Left best position : ("+bestResult.Position.X+","+bestResult.Position.Y+") with "+bestResult.Similarity);

            var topRightCornerForm = _cornersBuilder.BuildTopRightCornerForm();
             bestResult = SearchForForm(topRightCornerForm, image, new Area(image.Width / 2, 0, image.Width, image.Height / 2));
            points.Add(bestResult.Position);
            _logger.Log("Top Right best position : (" + bestResult.Position.X + "," + bestResult.Position.Y + ") with " + bestResult.Similarity);

            var bottomRightCornerForm = _cornersBuilder.BuildBottomRightCornerForm();
            bestResult = SearchForForm(bottomRightCornerForm, image, new Area(image.Width / 2, image.Height/2, image.Width, image.Height));
            points.Add(bestResult.Position);
            _logger.Log("Bottom Right best position : (" + bestResult.Position.X + "," + bestResult.Position.Y + ") with " + bestResult.Similarity);

            var bottomLeftCornerForm = _cornersBuilder.BuildBottomLeftCornerForm();
            bestResult = SearchForForm(bottomLeftCornerForm, image, new Area(0, image.Height / 2, image.Width/2, image.Height));
            points.Add(bestResult.Position);
            _logger.Log("Bottom Left best position : (" + bestResult.Position.X + "," + bestResult.Position.Y + ") with " + bestResult.Similarity);


            return points;
        }

        private double EvalFormSimilarity(Form form, GrayscaleStandardImage image, Point position)
        {
            double score = 0;
            for (int i = 0; i < form.Width; i++)
            {
                for (int j = 0; j < form.Height; j++)
                {
                    score += (form.Mask[i, j] * image.C[position.X + i - form.Center.X, position.Y + j - form.Center.Y]/255);
                }
            }

            score = score / form.WhitePixelCount;

            return score;
        }

        private IList<Point> GetPointsOfPage(GrayscaleStandardImage image)
        {
            var points = new List<Point>();

            for (int y = 1; y < image.Height - 1; y++)
            {
                if ((image.C[image.Width / 3, y] == 255)) points.Add(new Point(image.Width / 3, y));
                if ((image.C[(2 * image.Width) / 3, y] == 255)) points.Add(new Point(2 * image.Width / 3, y));
            }


            for (int x = 1; x < image.Width - 1; x++)
            {
                if ((image.C[x, image.Height / 3] == 255)) points.Add(new Point(x, image.Height / 3));
                if ((image.C[x, 2 * image.Height / 3] == 255)) points.Add(new Point(x, 2 * image.Height / 3));
            }



            return points;
        }

        private
            GrayscaleStandardImage Erosion(GrayscaleStandardImage image, int erosionCoeff)
        {
            var outputData = new GrayscaleStandardImage()
            {
                Height = image.Height,
                Width = image.Width,
                C = new int[image.Width, image.Height],
            };

            for (int x = 1; x < image.Width - 1; x++)
            {
                for (int y = 1; y < image.Height - 1; y++)
                {
                    int newValue = 0;
                    if (image.C[x, y] == 255)
                    {
                        int sum = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                sum += image.C[x + i - 1, y + j - 1] * (1 - erosionCoeff);
                            }
                        }

                        if (sum > 4 * 255)
                            newValue = 255;
                    }

                    outputData.C[x, y] = SafeAdd(0, newValue);
                }
            }

            return outputData;
        }


        private GrayscaleStandardImage Dilatation(GrayscaleStandardImage image, int dilatationCoeff)
        {
            var outputData = new GrayscaleStandardImage()
            {
                Height = image.Height,
                Width = image.Width,
                C = new int[image.Width, image.Height],
            };

            for (int x = 1; x < image.Width - 1; x++)
            {
                for (int y = 1; y < image.Height - 1; y++)
                {
                    int newValue = 0;
                    if (image.C[x, y] == 0)
                    {
                        int sum = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                sum += image.C[x + i - 1, y + j - 1] * (1 - dilatationCoeff);
                            }
                        }

                        if (sum > 4 * 255)
                            newValue = 255;

                    }
                    else newValue = 255;

                    outputData.C[x, y] = SafeAdd(0, newValue);
                }
            }

            return outputData;
        }

        private GrayscaleStandardImage ChangeLuminosity(GrayscaleStandardImage image, int intensity)
        {
            double luminosityCoeff = 1 + (double)intensity / 100;
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    image.C[x, y] = SafeAdd(0, (int)(image.C[x, y] * luminosityCoeff));
                }
            }

            return image;
        }

        private GrayscaleStandardImage ChangeConstrast(GrayscaleStandardImage image, int intensity)
        {
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    image.C[x, y] =
                        SafeAdd(image.C[x, y],
                        (int)((double)intensity / 100 * (image.C[x, y] - 127)));
                }
            }

            return image;
        }

        private StandardImage DrawIndicator(StandardImage image, int x, int y, int size)
        {
            for (int i = -size; i < size; i++)
            {
                for (int j = -size; j < size; j++)
                {
                    var posX = x + i;
                    var posY = y + j;
                    if (posX > 0 && posX < image.Width && posY > 0 && posY < image.Height)
                    {
                        image.R[posX, posY] = 0;
                        image.G[posX, posY] = 255;
                        image.B[posX, posY] = 0;
                    }
                }
            }

            return image;
        }

        private GrayscaleStandardImage Hat(GrayscaleStandardImage image, int hat)
        {
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if (image.C[x, y] >= hat)
                        image.C[x, y] = 255;
                    else image.C[x, y] = 0;
                }
            }

            return image;
        }

        private GrayscaleStandardImage Laplacien(GrayscaleStandardImage image)
        {
            var matrice = new int[3, 3]
                {{0, -1, 0},
                 {-1, 4, -1},
                 {0, -1, 0}};


            var outputData = new GrayscaleStandardImage()
            {
                Height = image.Height,
                Width = image.Width,
                C = new int[image.Width, image.Height],
            };

            for (int x = 1; x < image.Width - 1; x++)
            {
                for (int y = 1; y < image.Height - 1; y++)
                {
                    int valueC = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            valueC += image.C[x + i - 1, y + j - 1] * matrice[i, j];
                        }
                    }

                    outputData.C[x, y] = SafeAdd(0, valueC);
                }
            }

            return outputData;
        }


        public Bitmap ReduceSize(Bitmap bitmap, double reduceRatio)
        {
            Bitmap newBitmap = new Bitmap(bitmap, (int)(bitmap.Width * reduceRatio), (int)(bitmap.Height * reduceRatio));
            return newBitmap;
        }

        public int SafeAdd(int number, int addition)
        {
            var sum = number + addition;
            if (sum < 0) return 0;
            if (sum > 255) return 255;
            return sum;
        }
    }
}
