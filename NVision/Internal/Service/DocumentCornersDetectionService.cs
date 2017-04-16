using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NVision.Api.Model;
using NVision.Api.Service;
using NVision.Internal.Model;

namespace NVision.Internal.Service
{
    internal class DocumentCornersDetectionService
    {
        internal IList<Point> GetCorners(GrayscaleStandardImage image)
        {
            var points = new List<Point>();
            var pointsDictionnary = new Dictionary<string, Point>();
            var corners = new Dictionary<Form, Area>();

            corners.Add(CornersBuilder.BuildTopLeftCornerForm(), new Area(0, 0, image.Width / 2, image.Height / 2));
            corners.Add(CornersBuilder.BuildTopRightCornerForm(), new Area(image.Width / 2, 0, image.Width, image.Height / 2));
            corners.Add(CornersBuilder.BuildBottomRightCornerForm(), new Area(image.Width / 2, image.Height / 2, image.Width, image.Height));
            corners.Add(CornersBuilder.BuildBottomLeftCornerForm(), new Area(0, image.Height / 2, image.Width / 2, image.Height));


            Parallel.ForEach(corners.Keys, (form) => pointsDictionnary.Add(form.Name, FormSimilarityHelper.Instance.SearchForForm(form, image, corners[form]).Position));

            points.Add(pointsDictionnary["TopLeftCorner"]);
            points.Add(pointsDictionnary["TopRightCorner"]);
            points.Add(pointsDictionnary["BottomRightCorner"]);
            points.Add(pointsDictionnary["BottomLeftCorner"]);

            return points;
        }
    }
}
