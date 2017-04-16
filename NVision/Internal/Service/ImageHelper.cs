using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using NVision.Api.Model;
using NVision.Internal.Formatting;
using NVision.Internal.Model;

namespace NVision.Internal.Service
{
    internal static class ImageHelper
    {
        internal static GrayscaleStandardImage Hat(GrayscaleStandardImage image, int hat)
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

        internal static GrayscaleStandardImage ExactHat(StandardImage image, Color colorHat, int roundness)
        {
            var resultImage = image.ConvertToGrayScaleStandardImage();
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

        internal static GrayscaleStandardImage Hat(StandardImage image, Color colorHat)
        {
            var resultImage = image.ConvertToGrayScaleStandardImage();
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

        internal static GrayscaleStandardImage Laplacien(GrayscaleStandardImage image)
        {
            var matrice = new[,]
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


        internal static Bitmap ReduceSize(this Bitmap bitmap, double reduceRatio)
        {
            Bitmap newBitmap = new Bitmap(bitmap, (int)(bitmap.Width * reduceRatio),
                (int)(bitmap.Height * reduceRatio));
            return newBitmap;
        }

        internal static GrayscaleStandardImage Erosion(GrayscaleStandardImage image, double erosionCoeff)
        {
            var outputData = new GrayscaleStandardImage()
            {
                Height = image.Height,
                Width = image.Width,
                C = new int[image.Width, image.Height],
            };

            for (int x = 3; x < image.Width - 3; x++)
            {
                for (int y = 3; y < image.Height - 3; y++)
                {
                    int newValue = 0;
                    if (image.C[x, y] == 255)
                    {
                        int sum = 0;
                        for (int i = 0; i < 6; i++)
                        {
                            for (int j = 0; j < 6; j++)
                            {
                                sum += (int)(image.C[x + i - 3, y + j - 3] * (1 - erosionCoeff));
                            }
                        }

                        if (sum > 10 * 255)
                            newValue = 255;
                    }

                    outputData.C[x, y] = SafeAdd(0, newValue);
                }
            }

            return outputData;
        }


        internal static GrayscaleStandardImage Dilatation(GrayscaleStandardImage image, double dilatationCoeff)
        {
            var outputData = new GrayscaleStandardImage()
            {
                Height = image.Height,
                Width = image.Width,
                C = new int[image.Width, image.Height],
            };

            for (int x = 3; x < image.Width - 3; x++)
            {
                for (int y = 3; y < image.Height - 3; y++)
                {
                    int newValue = 0;
                    if (image.C[x, y] == 0)
                    {
                        int sum = 0;
                        for (int i = 0; i < 6; i++)
                        {
                            for (int j = 0; j < 6; j++)
                            {
                                sum += (int)(image.C[x + i - 3, y + j - 3] * (1 - dilatationCoeff));
                            }
                        }

                        if (sum > 15 * 255)
                            newValue = 255;
                    }
                    else newValue = 255;

                    outputData.C[x, y] = SafeAdd(0, newValue);
                }
            }

            return outputData;
        }

        internal static GrayscaleStandardImage ChangeLuminosity(GrayscaleStandardImage image, int intensity)
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

        internal static GrayscaleStandardImage ChangeConstrast(GrayscaleStandardImage image, int intensity)
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

        internal static StandardImage Round( this StandardImage image, int precision)
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

        internal static StandardImage DrawIndicator(this StandardImage image, int x, int y, int size)
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

        internal static IList<Area> SplitIntoFour(this Area area)
        {
            return new List<Area>
            {
                new Area(area.From.X, area.From.Y, area.To.X/2, area.To.Y/2),
                new Area(area.To.X/2, area.From.Y, area.To.X, area.To.Y/2),
                new Area(area.To.X/2, area.To.Y/2, area.To.X, area.To.Y),
                new Area(area.From.X, area.To.Y/2, area.To.X/2, area.To.Y)
            };
        }

        private  static int SafeAdd(int number, int addition)
        {
            var sum = number + addition;
            if (sum < 0) return 0;
            if (sum > 255) return 255;
            return sum;
        }
    }
}
