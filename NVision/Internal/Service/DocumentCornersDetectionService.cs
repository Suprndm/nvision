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
        internal IList<Line> GetCorners(GrayscaleStandardImage image)
        {
            var points = new ConcurrentBag<Point>();
            var pointsDictionnary = new Dictionary<FormType, Point>();
            var corners = new Dictionary<FormType, Area>();

            //      image = ImageHelper.Laplacien(image);

            corners.Add(FormType.TopLeft, new Area(0, 0, image.Width / 2, image.Height / 2));
             corners.Add(FormType.TopRight, new Area(image.Width / 2, 0, image.Width, image.Height / 2));
             corners.Add(FormType.BottomLeft, new Area(0, image.Height / 2, image.Width / 2, image.Height));
             corners.Add(FormType.BottomRight, new Area(image.Width / 2, image.Height / 2, image.Width, image.Height));

            var allForms = CornersBuilder.GetCornerForms();
            ConcurrentDictionary<Line, double> linesResults = new ConcurrentDictionary<Line, double>();
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
                    var lines = EvalLinesAtPoint(image, result);
                    foreach (var line in lines)
                    {
                        linesResults.TryAdd(line.Key, line.Value);
                    }
                }


            });

            var groupedLines = GroupLines(linesResults.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

            //points.Add(pointsDictionnary[FormType.TopLeft]);
            //points.Add(pointsDictionnary[FormType.TopRight]);
            //points.Add(pointsDictionnary[FormType.BottomRight]);
            //points.Add(pointsDictionnary[FormType.BottomLeft]);

            return groupedLines;
        }


        private IDictionary<Line, double> EvalLinesAtPoint(GrayscaleStandardImage image, Point point)
        {
            int step = 1;
            var results = new Dictionary<Line, double>();
            for (int angle = 0; angle < 180 / step; angle++)
            {
                var line = GetLineFromAngleAndPoint(image, point, angle);
                var linePixels = ImageHelper.GetLinePixels(line.P1.X, line.P1.Y, line.P2.X, line.P2.Y);
                double score = 0;
                foreach (var pixel in linePixels)
                {
                    if (image.C[pixel.X, pixel.Y] == 255)
                        score++;
                }

                score = score / linePixels.Count;
                results.Add(line, score);
            }

            return results;
        }

        public Line GetLineFromAngleAndPoint(GrayscaleStandardImage image, Point point, double angle)
        {
            double theta = angle * 2 * Math.PI / 360;
            double alpha = Math.Cos(theta);
            double beta = Math.Sin(theta);
            //point = new Point(point.X, image.Height - point.Y);

            double bottomIntersect = point.X + (0 - point.Y) * alpha / beta;
            double topIntersect = point.X + (image.Height - 1 - point.Y) * alpha / beta;
            double leftIntersect = point.Y + (0 - point.X) * beta / alpha;
            double rightIntersect = point.Y + (image.Width - 1 - point.X) * beta / alpha;

            List<Point> intersectionPoints = new List<Point>();
            intersectionPoints.Add(new Point((int)Math.Round(bottomIntersect), 0));
            intersectionPoints.Add(new Point((int)Math.Round(topIntersect), image.Height - 1));
            intersectionPoints.Add(new Point(0, (int)Math.Round(leftIntersect)));
            intersectionPoints.Add(new Point(image.Width - 1, (int)Math.Round(rightIntersect)));

            var innerIntersectionPoints = intersectionPoints.Where(p => p.X >= 0 && p.X < image.Width && p.Y >= 0 && p.Y < image.Height).ToList();

            return new Line(innerIntersectionPoints[0], innerIntersectionPoints[1]);
        }

        private IList<Line> GroupLines(IDictionary<Line, double> lines)
        {
            var g = 50;
            var groupedLines = new Dictionary<Line, double>();
            var orderedLines = lines.Where(kv=>kv.Value>0.4).OrderByDescending(s => s.Value).ToList();
            foreach (var result in orderedLines)
            {
                if (groupedLines.Count == 0)
                    groupedLines.Add(new Line(result.Key.P1, result.Key.P2), 1);
                else
                {
                    var forces = new Dictionary<Line, double>();
                    foreach (var groupedLine in groupedLines)
                    {
                        var distanceP1 =
                            Math.Sqrt(Math.Pow(groupedLine.Key.P1.X - result.Key.P1.X, 2) + Math.Pow(groupedLine.Key.P1.Y - result.Key.P1.Y, 2));

                        var distanceP2 =
                             Math.Sqrt(Math.Pow(groupedLine.Key.P2.X - result.Key.P2.X, 2) + Math.Pow(groupedLine.Key.P2.Y - result.Key.P2.Y, 2));

                        var distance = (distanceP1 + distanceP2)/2;

                        forces.Add(groupedLine.Key, 1 / (distance) * g);
                    }

                    var strongestForcePike = forces.Aggregate((l, r) => l.Value > r.Value ? l : r);
                    if (strongestForcePike.Value > 1)
                    {
                        groupedLines[strongestForcePike.Key] += 1;
                    }
                    else
                        groupedLines.Add(new Line(result.Key.P1, result.Key.P2), 1);
                }
            }

            return groupedLines.Keys.ToList();
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
