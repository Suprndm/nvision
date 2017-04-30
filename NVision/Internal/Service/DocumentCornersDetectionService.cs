using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
            var points = new ConcurrentBag<Point>();
            var pointsDictionnary = new Dictionary<FormType, Point>();
            var corners = new Dictionary<FormType, Area>();

            //      image = ImageHelper.Laplacien(image);

            corners.Add(FormType.TopLeft, new Area(0, 0, image.Width / 2, image.Height / 2));
            corners.Add(FormType.TopRight, new Area(image.Width / 2, 0, image.Width, image.Height / 2));
            corners.Add(FormType.BottomRight, new Area(image.Width / 2, image.Height / 2, image.Width, image.Height));
            corners.Add(FormType.BottomLeft, new Area(0, image.Height / 2, image.Width / 2, image.Height));

            var allForms = CornersBuilder.GetCornerForms();
            Parallel.ForEach(corners.Keys, (form) =>
            {

                var results =
                    FormSimilarityHelper.Instance.SearchForForm(allForms.Where(f => f.Type == form).ToList(), image, corners[form]);
                // SORT TAKE HALF BEST
                results = results.OrderByDescending(s => s.Similarity).ToList();
                results = results.Take(results.Count / 2).ToList();
                var groupedResults = GroupSimilarityResults(results);
                foreach (var result in groupedResults)
                {
                    points.Add(result);
                }
            });

            //points.Add(pointsDictionnary[FormType.TopLeft]);
            //points.Add(pointsDictionnary[FormType.TopRight]);
            //points.Add(pointsDictionnary[FormType.BottomRight]);
            //points.Add(pointsDictionnary[FormType.BottomLeft]);

            return points.ToList();
        }


        //private IDictionary<Point, double> EvalLinesAtPoint(GrayscaleStandardImage image, Point point)
        //{
        //    int step = 5;
        //    for (int i = 0; i < 360 / step; i++) //Supprimer cas i=0 et i=360/step
        //    {
        //    }
        //}

        public Line GetLineFromAngleAndPoint(GrayscaleStandardImage image, Point point, double angle)
        {
            double theta = angle * 2 * Math.PI / 360;
            double alpha = Math.Cos(theta);
            double beta = Math.Sin(theta);

            double bottomIntersect = point.X + (0 - point.Y) * alpha / beta;
            double topIntersect = point.X + (image.Height - point.Y) * alpha / beta;
            double leftIntersect = point.Y + (0 - point.X) * beta / alpha;
            double rightIntersect = point.Y + (image.Width - point.X) * beta / alpha;

            List<Point> intersectionPoints = new List<Point>();
            intersectionPoints.Add(new Point((int)Math.Round(bottomIntersect), 0));
            intersectionPoints.Add(new Point((int)Math.Round(topIntersect), image.Height));
            intersectionPoints.Add(new Point(0, (int)Math.Round(leftIntersect)));
            intersectionPoints.Add(new Point(image.Width, (int)Math.Round(rightIntersect)));

            var innerIntersectionPoints = intersectionPoints.Where(p => p.X >= 0 && p.X < image.Width && p.Y >= 0 && p.Y < image.Height).ToList();

            return new Line(innerIntersectionPoints[0], innerIntersectionPoints[1]);
        }

        private IList<Point> GroupSimilarityResults(IList<SimilarityResult> results)
        {
            var g = 50;
            var points = new Dictionary<Point, int>();
            results = results.OrderByDescending(s => s.Similarity).ToList();

            foreach (var result in results)
            {
                if (points.Count == 0)
                    points.Add(new Point(result.Position.X, result.Position.Y), 1);
                else
                {
                    var forces = new Dictionary<Point, double>();
                    foreach (var point in points)
                    {
                        var distance =
                            Math.Sqrt(Math.Pow(point.Key.X - result.Position.X, 2) + Math.Pow(point.Key.Y - result.Position.Y, 2));

                        forces.Add(point.Key, 1 / (distance) * g);
                    }

                    var strongestForcePike = forces.Aggregate((l, r) => l.Value > r.Value ? l : r);
                    if (strongestForcePike.Value > 1)
                    {
                        points[strongestForcePike.Key] += 1;
                    }
                    else
                        points.Add(new Point(result.Position.X, result.Position.Y), 1);
                }
            }

            return points.Keys.ToList();
        }

        public StandardImage GetCornersImageResult(StandardImage standard, IList<Point> corners)
        {
            foreach (var point in corners)
            {
                standard = standard.DrawIndicator(point.X, point.Y, 2);
            }

            return standard;
        }
    }
}
