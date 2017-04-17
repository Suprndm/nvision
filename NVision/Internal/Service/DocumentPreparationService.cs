using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ColorMine.ColorSpaces;
using NVision.Internal.Formatting;
using NVision.Internal.Model;

namespace NVision.Internal.Service
{
    internal class DocumentPreparationService
    {
        public GrayscaleStandardImage IsolateDocument(StandardImage image, IDictionary<Point, int> svPikes )
        {
            //var documentColor = GetDocumentColor(image);
            var grayImage = SvPikeHat(image, svPikes);
          //  grayImage = ImageHelper.Hat(grayImage, 20);
          // grayImage = UniformizeDocument(grayImage);
            return grayImage;
        }

        public GrayscaleStandardImage SvPikeHat(StandardImage image, IDictionary<Point, int> svPikes)
        {
            var grayImage = image.ConvertToGrayScaleStandardImage();
            var saturationCutoff = 0;
            var brightnessCutoff = 0;
            if (svPikes.Count == 1)
            {
                saturationCutoff = svPikes.Single().Key.X+30;
                brightnessCutoff = svPikes.Single().Key.Y-30;
            } else if (svPikes.Count == 2)
            {
                saturationCutoff = (svPikes.First().Key.X + svPikes.Last().Key.X)/2;
                brightnessCutoff = (svPikes.First().Key.Y + svPikes.Last().Key.Y)/2;
            }
            else
            {
                var pikesList = svPikes.ToList();
                pikesList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
                var pike1 = pikesList[0];
                var pike2 = pikesList[1];
                saturationCutoff = (pike1.Key.X + pike2.Key.X) / 2;
                brightnessCutoff = (pike1.Key.Y + pike2.Key.Y) / 2;
            }

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    var myRgb = new Rgb {R = image.R[i,j], G = image.G[i, j], B = image.B[i, j] };
                    var hsl = myRgb.To<Hsv>();

                    var saturation = hsl.S*100;
                    var brightness = hsl.V*100;
                    if (saturation < saturationCutoff && brightness  > brightnessCutoff)
                        grayImage.C[i, j] = 255;
                    else
                        grayImage.C[i, j] = 0;
                }
            }

            return grayImage;
        }

        public GrayscaleStandardImage DocumentEligibilityMap(StandardImage image)
        {
            var grayImage = image.ConvertToGrayScaleStandardImage();
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    bool stop = false;
                    if (i == 114 && j == 50)
                        stop = true;
                    Color pixelColor = Color.FromArgb(1, image.R[i, j], image.G[i, j], image.B[i, j]);

                    grayImage.C[i, j] = pixelColor.CouldBeDocumentColor() == true ? 255 : 0;
                }
            }

            return grayImage;
        }

        public Color GetDocumentColor(StandardImage image)
        {
            var colors = new Dictionary<Color, int>();
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = Color.FromArgb(1, image.R[x, y], image.G[x, y], image.B[x, y]);

                    if (colors.ContainsKey(pixelColor))
                        colors[pixelColor]++;
                    else
                    {
                        colors.Add(pixelColor, 1);
                    }
                }
            }
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

        public GrayscaleStandardImage UniformizeDocument(GrayscaleStandardImage image)
        {

             image = ImageHelper.Erosion(image, 0);
            image = ImageHelper.Dilatation(image, 0);
             image = ImageHelper.Erosion(image, 0);
             image = ImageHelper.Dilatation(image, 0);
             image = ImageHelper.Erosion(image, 0);
             image = ImageHelper.Dilatation(image, 0);

            //image = ImageHelper.Dilatation(image, 1);
            //image = ImageHelper.Dilatation(image, 1);
            //image = ImageHelper.Erosion(image, 0.5);

            //var maskSize = 50;

            //for (int x = maskSize; x < image.Width - maskSize; x++)
            //{
            //    for (int y = maskSize; y < image.Height - maskSize; y++)
            //    {
            //        int sum = 0;
            //        for (int i = 0; i < maskSize; i++)
            //        {
            //            for (int j = 0; j < maskSize; j++)
            //            {
            //                if (i == 0 || j == 0 || i == maskSize - 1 || j == maskSize - 1)
            //                {
            //                    sum += image.C[x - (i - maskSize / 2), y - (j - maskSize / 2)];

            //                }
            //            }
            //        }

            //        if (image.C[x, y] == 0 && (maskSize - 1) * 4 * 255 == sum)
            //        {
            //            image.C[x, y] = 255;
            //        }
            //        else if (image.C[x, y] == 255 && sum == 0)
            //        {
            //            image.C[x, y] = 0;
            //        }
            //    }
            //}

            return image;
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
    }
}
