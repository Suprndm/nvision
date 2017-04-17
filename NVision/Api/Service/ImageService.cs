using System;
using System.Drawing;
using Helper;
using NVision.Internal.Formatting;
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
            : this(logger,
                  new DocumentPreparationService(),
                  new DocumentCornersDetectionService(),
                  new DocumentStraightenerService())
        {
        }

        public Bitmap PrepareImageToOcr(Bitmap bitmap)
        {
            bitmap = bitmap.ReduceSize((double)500 / Math.Max(bitmap.Width, bitmap.Height));
            var standardImage = bitmap.ConvertToStandardImage();
            var grayImage = _documentPreparationService.DocumentEligibilityMap(standardImage);

            var corners = _documentCornersDetectionService.GetCorners(standardImage, grayImage);

            var coloredStandardImage = grayImage.ConvertToStandardImage();

            foreach (var point in corners)
            {
                coloredStandardImage = coloredStandardImage.DrawIndicator(point.X, point.Y, 2);
            }

            var rotatedImage = _documentStraightenerService.StraightenDocument(standardImage, corners);

            var result = rotatedImage.ConvertToBitmap();

            return result;
        }
    }
}