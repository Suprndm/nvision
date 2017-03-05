using System.Drawing;
using NVision.Internal.Model;

namespace NVision.Internal.Formatting
{
    public class ImageStandardizer
    {
        internal StandardImage ConvertToStandardImage(Bitmap bitmap)
        {
            var imageData = new StandardImage
            {
                Height = bitmap.Height,
                Width = bitmap.Width,
                R = new int[bitmap.Width, bitmap.Height],
                G = new int[bitmap.Width, bitmap.Height],
                B = new int[bitmap.Width, bitmap.Height],
            };

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    imageData.R[x, y] = pixelColor.R;
                    imageData.G[x, y] = pixelColor.G;
                    imageData.B[x, y] = pixelColor.B;
                }
            }

            return imageData;
        }

        internal GrayscaleStandardImage ConvertToGrayScaleStandardImage(Bitmap bitmap)
        {
            var grayscaleImageData = new GrayscaleStandardImage
            {
                Height = bitmap.Height,
                Width = bitmap.Width,
                C = new int[bitmap.Width, bitmap.Height],
            };

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);
                    grayscaleImageData.C[x, y] = (pixelColor.R+ pixelColor.G + pixelColor.B)/3;
                }
            }

            return grayscaleImageData;
        }

        internal Bitmap ConvertToBitmap(StandardImage standardImage)
        {
            var bitmap = new Bitmap(standardImage.Width, standardImage.Height);
         

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    bitmap.SetPixel(x,y, Color.FromArgb(standardImage.R[x, y], standardImage.G[x, y], standardImage.B[x, y]));
                }
            }

            return bitmap;
        }

        internal Bitmap ConvertToBitmap(GrayscaleStandardImage grayScaleStandardImage)
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

        internal StandardImage ConvertToStandardImage(GrayscaleStandardImage image)
        {
            var standardImage = new StandardImage
            {
                Height = image.Height,
                Width = image.Width,
                R = new int[image.Width, image.Height],
                G = new int[image.Width, image.Height],
                B = new int[image.Width, image.Height],
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
    }
}
