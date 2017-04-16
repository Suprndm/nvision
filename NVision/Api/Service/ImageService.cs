using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Helper;
using NVision.Api.Model;
using NVision.Internal.Formatting;
using NVision.Internal.Model;
using NVision.Internal.Service;

namespace NVision.Api.Service
{
    public class ImageService : IImageService
    {
        private readonly ILogger _logger;
        private readonly DocumentPreparationService _documentPreparationService;
        private readonly DocumentCornersDetectionService _documentCornersDetectionService;
        private readonly DocumentStraightenerService _documentStraightenerService;

        private ImageService(
            ILogger logger, 
            DocumentPreparationService documentPreparationService, 
            DocumentCornersDetectionService documentCornersDetectionService, 
            DocumentStraightenerService documentStraightenerService)
        {
            _logger = logger;
            _documentPreparationService = documentPreparationService;
            _documentCornersDetectionService = documentCornersDetectionService;
            _documentStraightenerService = documentStraightenerService;
            _logger.Log(FormSimilarityHelper.Instance.ToString());
        }

        public ImageService(ILogger logger)
            : this( logger,
                  new DocumentPreparationService(),
                  new DocumentCornersDetectionService(),
                  new DocumentStraightenerService())
        {
        }

        public Bitmap PrepareImageToOcr(Bitmap bitmap)
        {
            bitmap = bitmap.ReduceSize((double)500 / Math.Max(bitmap.Width, bitmap.Height));
            var standardImage = bitmap.ConvertToStandardImage();
            var grayImage = _documentPreparationService.IsolateDocument(standardImage);


            grayImage = ImageHelper.Laplacien(grayImage);

            var corners = _documentCornersDetectionService.GetCorners(grayImage);

            var coloredStandardImage = grayImage.ConvertToStandardImage();

            foreach (var point in corners)
            {
                coloredStandardImage = DrawIndicator(coloredStandardImage, point.X, point.Y, 2);
            }

            var rotatedImage = _documentStraightenerService.StraightenDocument(standardImage, corners);

            var result = rotatedImage.ConvertToBitmap();

            return result;
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

       
    }
}