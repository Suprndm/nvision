using System.Drawing;
using NVision.Api.Model;
using NVision.Internal.Model;

namespace NVision.Internal.Formatting
{
    public static class ImageStandardizer
    {
        internal static StandardImage ConvertToStandardImage(this Bitmap bitmap, int overlay = 0)
        {
            var imageData = new StandardImage
            {
                Height = bitmap.Height + overlay,
                Width = bitmap.Width + overlay,
                R = new int[bitmap.Width + overlay, bitmap.Height + overlay],
                G = new int[bitmap.Width + overlay, bitmap.Height + overlay],
                B = new int[bitmap.Width + overlay, bitmap.Height + overlay],
                Area = new Area(0, 0, bitmap.Width + overlay, bitmap.Height + overlay)
            };

            for (int x = -overlay / 2; x < bitmap.Width + overlay / 2; x++)
            {
                for (int y = -overlay / 2; y < bitmap.Height + overlay / 2; y++)
                {
                    Color pixelColor = Color.DarkBlue;
                    if (x < 0 && (y >= 0 && y < bitmap.Height))
                    {
                        pixelColor = bitmap.GetPixel(0, y);
                    }
                    else if (y < 0 / 2 && (x >= 0 && x < bitmap.Width))
                    {
                        pixelColor = bitmap.GetPixel(x, 0);
                    }
                    else if (x >= bitmap.Width && (y >= 0 && y < bitmap.Height))
                    {
                        pixelColor = bitmap.GetPixel(bitmap.Width - 1, y);
                    }
                    else if (y >= bitmap.Height && (x >= 0 && x < bitmap.Width))
                    {
                        pixelColor = bitmap.GetPixel(x, bitmap.Height - 1);
                    }
                    else if (x < 0 && y < 0)
                    {
                        pixelColor = bitmap.GetPixel(0, 0);
                    }
                    else if (x < 0 && y >= bitmap.Height)
                    {
                        pixelColor = bitmap.GetPixel(0, bitmap.Height - 1);
                    }

                    else if (x >= bitmap.Width && y < 0)
                    {
                        pixelColor = bitmap.GetPixel(bitmap.Width - 1, 0);
                    }
                    else if (x >= bitmap.Width && y >= bitmap.Height)
                    {
                        pixelColor = bitmap.GetPixel(bitmap.Width - 1, bitmap.Height - 1);
                    }
                    else
                    {
                        pixelColor = bitmap.GetPixel(x, y);
                    }

                    imageData.R[x + overlay / 2, y + overlay / 2] = pixelColor.R;
                    imageData.G[x + overlay / 2, y + overlay / 2] = pixelColor.G;
                    imageData.B[x + overlay / 2, y + overlay / 2] = pixelColor.B;
                }
            }

            return imageData;
        }

        internal static GrayscaleStandardImage ConvertToGrayScaleStandardImage(this Bitmap bitmap)
        {
            var grayscaleImageData = new GrayscaleStandardImage
            {
                Height = bitmap.Height,
                Width = bitmap.Width,
                C = new int[bitmap.Width, bitmap.Height],
                Area = new Area(0, 0, bitmap.Width, bitmap.Height)
            };

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    grayscaleImageData.C[x, y] = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                }
            }

            return grayscaleImageData;
        }

        internal static Bitmap ConvertToBitmap(this StandardImage standardImage)
        {
            var bitmap = new Bitmap(standardImage.Width, standardImage.Height);


            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    bitmap.SetPixel(x, y, Color.FromArgb(standardImage.R[x, y], standardImage.G[x, y], standardImage.B[x, y]));
                }
            }

            return bitmap;
        }

        internal static GrayscaleStandardImage CreateGrayscaleStandardImage(int width, int height)
        {
            var standardImage = new GrayscaleStandardImage
            {
                Height = height,
                Width = width,
                C = new int[width, height],
                Area = new Area(0, 0, width, height)
            };

            return standardImage;
        }


        internal static StandardImage CreateStandardImage(int width, int height)
        {
            var standardImage = new StandardImage()
            {
                Height = height,
                Width = width,
                R = new int[width, height],
                G = new int[width, height],
                B = new int[width, height],
                Area = new Area(0, 0, width, height)
            };

            return standardImage;
        }

        internal static Bitmap ConvertToBitmap(this GrayscaleStandardImage grayScaleStandardImage)
        {
            var bitmap = new Bitmap(grayScaleStandardImage.Width, grayScaleStandardImage.Height);


            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    bitmap.SetPixel(x, y, Color.FromArgb(grayScaleStandardImage.C[x, y], grayScaleStandardImage.C[x, y], grayScaleStandardImage.C[x, y]));
                }
            }

            return bitmap;
        }

        internal static StandardImage ConvertToStandardImage(this GrayscaleStandardImage image)
        {
            var standardImage = new StandardImage
            {
                Height = image.Height,
                Width = image.Width,
                R = new int[image.Width, image.Height],
                G = new int[image.Width, image.Height],
                B = new int[image.Width, image.Height],
                Area = image.Area
            };

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    standardImage.R[x, y] = image.C[x, y];
                    standardImage.G[x, y] = image.C[x, y];
                    standardImage.B[x, y] = image.C[x, y];
                }
            }

            return standardImage;
        }

        internal static GrayscaleStandardImage ConvertToGrayScaleStandardImage(this StandardImage image)
        {
            var standardImage = new GrayscaleStandardImage
            {
                Height = image.Height,
                Width = image.Width,
                C = new int[image.Width, image.Height],
                Area = image.Area
            };

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    standardImage.C[x, y] = (image.R[x, y] + image.G[x, y] + image.B[x, y]) / 3;
                }
            }

            return standardImage;
        }
    }
}
