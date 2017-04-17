using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using ColorMine.ColorSpaces;
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

        internal static IDictionary<Point, int> GetSVMap(StandardImage image)
        {
            var map = new Dictionary<Point, int>();

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var myRgb = new Rgb { R = image.R[x, y], G = image.G[x, y], B = image.B[x, y] };
                    var hsl = myRgb.To<Hsv>();
                    var saturation = (int)(hsl.S * 100);
                    var brightness = (int)(hsl.V * 100);
                    var point = new Point(saturation,brightness);

                    if (map.ContainsKey(point))
                        map[point]++;
                    else
                    {
                        map.Add(point, 1);
                    }
                }
            }

            return map;
        }

        internal static IDictionary<int, int> GetSaturationMap(StandardImage image)
        {
            var map = new Dictionary<int, int>();

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var myRgb = new Rgb { R = image.R[x, y], G = image.G[x, y], B = image.B[x, y] };
                    var hsl = myRgb.To<Hsv>();
                    var saturation = (int)(hsl.S * 100);

                    if (map.ContainsKey(saturation))
                        map[saturation]++;
                    else
                    {
                        map.Add(saturation, 1);
                    }
                }
            }

            return map;
        }



        internal static IDictionary<Point, int> GetSvPikes(IDictionary<Point, int> data)
        {
            var g = 30;
            var pikes = new Dictionary<Point, int>();
            var dataList = data.ToList();
            dataList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            foreach (var element in dataList)
            {
                if (pikes.Count == 0)
                    pikes.Add(element.Key, 1);
                else
                {
                    var forces = new Dictionary<Point, double>();
                    foreach (var pike in pikes)
                    {
                        var distance =
                            Math.Sqrt(Math.Pow(pike.Key.X - element.Key.X, 2) + Math.Pow(pike.Key.Y - element.Key.Y, 2));

                        forces.Add(pike.Key, (double)pike.Value / (distance * distance) * g);
                    }

                    var strongestForcePike = forces.Aggregate((l, r) => l.Value > r.Value ? l : r);
                    if (strongestForcePike.Value > 1) pikes[strongestForcePike.Key] += 1;
                    else
                        pikes.Add(element.Key, 1);
                }
            }

            return pikes;
        }

        internal static IDictionary<int, int> GetPikes(IDictionary<int, int> data)
        {
            var g = 20;
            var pikes = new Dictionary<int, int>();
            var dataList = data.ToList();
            dataList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            foreach (var element in dataList)
            {
                if (pikes.Count == 0)
                    pikes.Add(element.Key, 1);
                else
                {
                    var forces = new Dictionary<int, double>();
                    foreach (var pike in pikes)
                    {
                        var distance = Math.Abs(pike.Key - element.Key);
                        forces.Add(pike.Key, (double)pike.Value / (distance* distance) * g);
                    }
                    
                    var strongestForcePike = forces.Aggregate((l, r) => l.Value > r.Value ? l : r);
                    if (strongestForcePike.Value > 1) pikes[strongestForcePike.Key] += 1;
                    else 
                        pikes.Add(element.Key, 1);
                }
            }

            return pikes;
        }


        internal static IDictionary<int, int> GetHueMAp(StandardImage image)
        {
            var map = new Dictionary<int, int>();

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var myRgb = new Rgb { R = image.R[x, y], G = image.G[x, y], B = image.B[x, y] };
                    var hsl = myRgb.To<Hsv>();
                    var hue = (int)(hsl.H / 3.6);

                    if (map.ContainsKey(hue))
                        map[hue]++;
                    else
                    {
                        map.Add(hue, 1);
                    }
                }
            }

            return map;
        }

        internal static IDictionary<int, int> GetBrightnessMap(StandardImage image)
        {
            var map = new Dictionary<int, int>();

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var myRgb = new Rgb { R = image.R[x, y], G = image.G[x, y], B = image.B[x, y] };
                    var hsl = myRgb.To<Hsv>();
                    var brightness = (int)(hsl.V * 100);

                    if (map.ContainsKey(brightness))
                        map[brightness]++;
                    else
                    {
                        map.Add(brightness, 1);
                    }
                }
            }

            return map;
        }


        internal static
            GrayscaleStandardImage GetDocumentEdges(StandardImage image)
        {
            var outputData = new GrayscaleStandardImage()
            {
                Height = image.Height,
                Width = image.Width,
                C = new int[image.Width, image.Height],
            };

            var matrice = new[,]
            {
                {0, -1, 0},
                {-1, 4, -1},
                {0, -1, 0}
            };
            int scanSize = 3;
            int half = (scanSize - 1) / 2;
            for (int x = half; x < image.Width - half; x++)
            {
                for (int y = half; y < image.Height - half; y++)
                {
                    var color1 = Color.FromArgb(255, image.R[x, y], image.R[x, y], image.R[x, y]);
                    int valueC = 0;
                    for (int i = 0; i < scanSize; i++)
                    {
                        for (int j = 0; j < scanSize; j++)
                        {
                            var color2 = Color.FromArgb(255, image.R[x + i - half, y + j - half], image.R[x + i - half, y + j - half], image.R[x + i - half, y + j - half]);

                            valueC += (int)GetDistanceBetweenColors(color1, color2);
                        }
                    }

                    if (CouldBeDocumentColor(color1))
                        outputData.C[x, y] = SafeAdd(0, valueC);
                }
            }

            return outputData;
        }

        internal static double GetDistanceBetweenColors(Color c1, Color c2)
        {

            var hue1 = c1.GetHue();
            var hue2 = c2.GetHue();
            if (hue1 > 180) hue1 = hue1 - 360;
            if (hue2 > 180) hue2 = hue2 - 360;
            var hueDifference = Math.Abs(hue1 - hue2) / 1.8;

            var saturation1 = c1.GetSaturation();
            var saturation2 = c2.GetSaturation();
            var saturationDifference = Math.Abs(saturation1 - saturation2) * 100;
            var brightness1 = c1.GetBrightness();
            var brightness2 = c2.GetBrightness();
            var brightnessDifference = Math.Abs(brightness1 - brightness2) * 100;


            return hueDifference + saturationDifference + brightnessDifference;
        }

        internal static bool CouldBeDocumentColor(this Color c1)
        {
            var myRgb = new Rgb { R = c1.R, G = c1.G, B = c1.B };
            var hsl = myRgb.To<Hsv>();

            var saturation = hsl.S * 100;
            var brightness = hsl.V * 100;
            if (saturation < 37 && brightness > 50)
                return true;

            return false;
        }

        internal static double DistanceToGray(this Color c)
        {
            var average = (c.R + c.G + c.B) / 3;
            var distanceToGray = Math.Abs(c.R - average) + Math.Abs(c.G - average) + Math.Abs(c.B - average);

            return distanceToGray;
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

        internal static StandardImage Round(this StandardImage image, int precision)
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
                        image.R[posX, posY] = 255;
                        image.G[posX, posY] = 0;
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

        private static int SafeAdd(int number, int addition)
        {
            var sum = number + addition;
            if (sum < 0) return 0;
            if (sum > 255) return 255;
            return sum;
        }
    }
}
