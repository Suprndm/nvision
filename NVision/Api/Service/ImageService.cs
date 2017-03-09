﻿using System;
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
        private readonly FormSimilarityService _formSimilarityService;

        private ImageService(ImageStandardizer imageStandardizer, CornersBuilder cornersBuilder, ILogger logger,
            FormSimilarityService formSimilarityService)
        {
            _imageStandardizer = imageStandardizer;
            _logger = logger;
            _formSimilarityService = formSimilarityService;
            _cornersBuilder = cornersBuilder;
            _logger.Log(_formSimilarityService.ToString());
        }

        public ImageService(ILogger logger)
            : this(new ImageStandardizer(), new CornersBuilder(), logger, new FormSimilarityService())
        {
        }

        public StandardSchema ExtractSchemaFromImage(Bitmap bitmap)
        {
            var standardImage = _imageStandardizer.ConvertToStandardImage(bitmap);

            return new StandardSchema();
        }

        private StandardImage Round(StandardImage image, int precision)
        {
            for (int x = 1; x < image.Width - 1; x++)
            {
                for (int y = 1; y < image.Height - 1; y++)
                {
                    image.B[x, y] = (image.B[x, y] / precision) * precision;
                    image.G[x, y] = (image.G[x, y] / precision) * precision;
                    image.B[x, y] = (image.B[x, y] / precision) * precision;
                }
            }

            return image;
        }

        private IDictionary<Color, int> GetColors(Bitmap image)
        {
            var colors = new Dictionary<Color, int>();
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    if (colors.ContainsKey(pixelColor))
                        colors[pixelColor]++;
                    else
                    {
                        colors.Add(pixelColor, 1);
                    }
                }
            }

            return colors;
        }


        private Color GetMostPopularLightColor(IDictionary<Color, int> colors)
        {
            List<KeyValuePair<Color, int>> lightPopularity = new List<KeyValuePair<Color, int>>();

            foreach (var kv in colors)
            {
                var color = kv.Key;
                lightPopularity.Add(new KeyValuePair<Color, int>(kv.Key, kv.Value * (color.R + color.G + color.B)));
            }

            lightPopularity.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
            var lightPopuplarColor = lightPopularity.First().Key;
            int roundness = 20;
            int pixelCount = 0;
            int r = 0;
            int g = 0;
            int b = 0;

            foreach (var kv in lightPopularity)
            {
                var color = kv.Key;

                if (color.R >= lightPopuplarColor.R - roundness
                    && color.G >= lightPopuplarColor.G - roundness
                    && color.B >= lightPopuplarColor.B - roundness
                    && color.R <= lightPopuplarColor.R + roundness
                    && color.G <= lightPopuplarColor.G + roundness
                    && color.B <= lightPopuplarColor.B + roundness)
                {
                    var colorOccurence = colors[color];
                    r += kv.Key.R * colorOccurence;
                    g += kv.Key.G * colorOccurence;
                    b += kv.Key.B * colorOccurence;

                    pixelCount += colorOccurence;
                }
            }

            return Color.FromArgb(255, r / pixelCount, g / pixelCount, b / pixelCount);
        }

        private Color GetAverageColor(IList<KeyValuePair<Color, int>> colors)
        {
            int r = 0;
            int g = 0;
            int b = 0;

            int pixelCount = 0;

            foreach (var kv in colors)
            {
                r += kv.Key.R * kv.Value;
                g += kv.Key.G * kv.Value;
                b += kv.Key.B * kv.Value;

                pixelCount += kv.Value;
            }

            return Color.FromArgb(255, r / pixelCount, g / pixelCount, b / pixelCount);
        }

        public Bitmap PrepareImage(Bitmap bitmap)
        {
            bitmap = ReduceSize(bitmap, (double) 500 / Math.Max(bitmap.Width, bitmap.Height));
            var colors = GetColors(bitmap);
            var averageColor = GetAverageColor(colors.ToList());
            var popularLightColor = GetMostPopularLightColor(colors);
            var standardImage = _imageStandardizer.ConvertToStandardImage(bitmap);
            var grayImage = ExactHat(standardImage, popularLightColor, 50);
            //var standardImage = _imageStandardizer.ConvertToGrayScaleStandardImage(bitmap);
            //_logger.Log(bitmap.Width + " x " + bitmap.Height);
            //standardImage = ChangeConstrast(standardImage, 250);
            //standardImage = ChangeLuminosity(standardImage, 50);
            //standardImage = Hat(standardImage, 200);

            grayImage = Erosion(grayImage, 0);
            grayImage = Dilatation(grayImage, 0);

            grayImage = Erosion(grayImage, 0);
            grayImage = Dilatation(grayImage, 0);

            grayImage = Erosion(grayImage, 0);
            grayImage = Dilatation(grayImage, 0);

            grayImage = Erosion(grayImage, 0);
            grayImage = Dilatation(grayImage, 0);

            grayImage = Erosion(grayImage, 0);
            grayImage = Dilatation(grayImage, 0);

            grayImage = Laplacien(grayImage);

            var corners = GetCorners(grayImage);
            var coloredStandardImage = _imageStandardizer.ConvertToStandardImage(grayImage);

            foreach (var point in corners)
            {
                coloredStandardImage = DrawIndicator(coloredStandardImage, point.X, point.Y, 2);
            }

            Bitmap result = null;
            result = _imageStandardizer.ConvertToBitmap(coloredStandardImage);

            return result;
        }

        private GrayscaleStandardImage UniformizeImage(GrayscaleStandardImage image, int maskSize)
        {
            for (int x = maskSize; x < image.Width - maskSize; x++)
            {
                for (int y = maskSize; y < image.Height - maskSize; y++)
                {
                    int sum = 0;
                    for (int i = 0; i < maskSize; i++)
                    {
                        for (int j = 0; j < maskSize; j++)
                        {
                            if (i == 0 || j == 0 || i == maskSize - 1 || j == maskSize - 1)
                            {
                                sum += image.C[x - (i - maskSize / 2), y - (j - maskSize / 2)];
                                
                            }
                        }
                    }

                    if (image.C[x, y] == 0 && (maskSize-1)*4*255==sum)
                    {
                        image.C[x, y] = 255;
                    } else if(image.C[x, y] == 255 && sum==0)
                    {
                        image.C[x, y] = 0;
                    }
                }
            }

            return image;
        }

        private SimilarityResult SearchForForm(Form form, GrayscaleStandardImage image, Area area)
        {
            var scores = new List<SimilarityResult>();
            for (int i = area.From.X; i < area.To.X; i++)
            {
                for (int j = area.From.Y; j < area.To.Y; j++)
                {
                    var position = new Point(i, j);
                    scores.Add(new SimilarityResult(position,
                        _formSimilarityService.EvalFormSimilarity(form, image, position)));
                }
            }

            var orderedScores = scores.OrderByDescending(x => x.Similarity);

            var bestResult = orderedScores.First();

            return bestResult;
        }

        private IList<Point> GetCorners(GrayscaleStandardImage image)
        {
            var points = new List<Point>();
            var topLeftCornerForm = _cornersBuilder.BuildTopLeftCornerForm();
            var bestResult = SearchForForm(topLeftCornerForm, image, new Area(0, 0, image.Width / 2, image.Height / 2));
            points.Add(bestResult.Position);
            _logger.Log("Top Left best position : (" + bestResult.Position.X + "," + bestResult.Position.Y + ") with " +
                        bestResult.Similarity);

            var topRightCornerForm = _cornersBuilder.BuildTopRightCornerForm();
            bestResult = SearchForForm(topRightCornerForm, image,
                new Area(image.Width / 2, 0, image.Width, image.Height / 2));
            points.Add(bestResult.Position);
            _logger.Log("Top Right best position : (" + bestResult.Position.X + "," + bestResult.Position.Y + ") with " +
                        bestResult.Similarity);

            var bottomRightCornerForm = _cornersBuilder.BuildBottomRightCornerForm();
            bestResult = SearchForForm(bottomRightCornerForm, image,
                new Area(image.Width / 2, image.Height / 2, image.Width, image.Height));
            points.Add(bestResult.Position);
            _logger.Log("Bottom Right best position : (" + bestResult.Position.X + "," + bestResult.Position.Y +
                        ") with " + bestResult.Similarity);

            var bottomLeftCornerForm = _cornersBuilder.BuildBottomLeftCornerForm();
            bestResult = SearchForForm(bottomLeftCornerForm, image,
                new Area(0, image.Height / 2, image.Width / 2, image.Height));
            points.Add(bestResult.Position);
            _logger.Log("Bottom Left best position : (" + bestResult.Position.X + "," + bestResult.Position.Y +
                        ") with " + bestResult.Similarity);


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
            double luminosityCoeff = 1 + (double) intensity / 100;
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    image.C[x, y] = SafeAdd(0, (int) (image.C[x, y] * luminosityCoeff));
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
                            (int) ((double) intensity / 100 * (image.C[x, y] - 127)));
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

        private GrayscaleStandardImage ExactHat(StandardImage image, Color colorHat, int roundness)
        {
            var resultImage = _imageStandardizer.ConvertToGrayScaleStandardImage(image);
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if (image.R[x, y] >= colorHat.R - roundness
                        && image.G[x, y] >= colorHat.G - roundness
                        && image.B[x, y] >= colorHat.B - roundness
                        && image.R[x, y] <= colorHat.R + roundness
                        && image.G[x, y] <= colorHat.G + roundness
                        && image.B[x, y] <= colorHat.B + roundness)
                    {
                        resultImage.C[x, y] = 255;
                    }
                    else
                    {
                        resultImage.C[x, y] = 0;
                    }
                }
            }

            return resultImage;
        }

        private GrayscaleStandardImage Hat(StandardImage image, Color colorHat)
        {
            var resultImage = _imageStandardizer.ConvertToGrayScaleStandardImage(image);
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if (image.R[x, y] > colorHat.R
                        && image.G[x, y] > colorHat.G
                        && image.B[x, y] > colorHat.B)
                    {
                        resultImage.C[x, y] = 255;
                    }
                    else
                    {
                        resultImage.C[x, y] = 0;
                    }
                }
            }

            return resultImage;
        }

        private GrayscaleStandardImage Laplacien(GrayscaleStandardImage image)
        {
            var matrice = new int[3, 3]
            {
                {0, -1, 0},
                {-1, 4, -1},
                {0, -1, 0}
            };


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
            Bitmap newBitmap = new Bitmap(bitmap, (int) (bitmap.Width * reduceRatio),
                (int) (bitmap.Height * reduceRatio));
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