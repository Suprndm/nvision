using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using NVision.Api.Model;
using NVision.Internal.Formatting;
using NVision.Internal.Model;

namespace NVision.Internal.Service
{
    internal class DocumentCornersDetectionService
    {
        internal IList<Point> GetCorners(GrayscaleStandardImage image)
        {
            var points = new List<Point>();
            var pointsDictionnary = new Dictionary<FormType, Point>();
            var corners = new Dictionary<Form, Area>();

            image = ImageHelper.Laplacien(image);

            corners.Add(CornersBuilder.BuildTopLeftCornerForm(), new Area(0, 0, image.Width / 2, image.Height / 2));
            corners.Add(CornersBuilder.BuildTopRightCornerForm(), new Area(image.Width / 2, 0, image.Width, image.Height / 2));
            corners.Add(CornersBuilder.BuildBottomRightCornerForm(), new Area(image.Width / 2, image.Height / 2, image.Width, image.Height));
            corners.Add(CornersBuilder.BuildBottomLeftCornerForm(), new Area(0, image.Height / 2, image.Width / 2, image.Height));


            Parallel.ForEach(corners.Keys, (form) => pointsDictionnary.Add(form.Type, FormSimilarityHelper.Instance.SearchForForm(form, image, corners[form]).Position));

            points.Add(pointsDictionnary[FormType.TopLeft]);
            points.Add(pointsDictionnary[FormType.TopRight]);
            points.Add(pointsDictionnary[FormType.BottomRight]);
            points.Add(pointsDictionnary[FormType.BottomLeft]);

            return points;
        }

        public StandardImage GetCornersImageResult(GrayscaleStandardImage grayImage, IList<Point> corners)
        {
            grayImage = ImageHelper.Laplacien(grayImage);
            var coloredStandardImage = grayImage.ConvertToStandardImage();

            foreach (var point in corners)
            {
                coloredStandardImage = coloredStandardImage.DrawIndicator(point.X, point.Y, 2);
            }

            return coloredStandardImage;
        }
    }
}
