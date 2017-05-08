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

        internal IList<Point> GetPointsOfInterest(GrayscaleStandardImage image, FormType formType, Area area)
        {
            var allForms = CornersBuilder.GetCornerForms();

            var results =
                 FormSimilarityHelper.Instance.SearchForForm(allForms.Where(f => f.Type == formType).ToList(), image,
                     area);

            // SORT TAKE HALF BEST
            results = results.OrderByDescending(s => s.Similarity).ToList();
            results = results.Take(results.Count / 2).ToList();
            return GroupSimilarityResults(results);
        }

        internal IList<Line> GetLines(IList<Point> interests, GrayscaleStandardImage image)
        {
            Dictionary<Line, double> linesResults = new Dictionary<Line, double>();
            foreach (var result in interests)
            {
                var lines = EvalLinesAtPoint(image, result);
                foreach (var line in lines)
                {
                    linesResults.Add(line.Key, line.Value);
                }
            }

            // GROUP LINES HERE
            var groupedLines = GroupLines(linesResults.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

            return groupedLines;
        }

        internal IList<Point> GetFinalCorners(GrayscaleStandardImage image, IDictionary<FormType, IList<Point>> potentialCorners)
        {
            var potentialDocumentCorners = new Dictionary<IList<Point>, double>();

            foreach (var topLeftCorner in potentialCorners[FormType.TopLeft])
            {
                foreach (var topRightCorner in potentialCorners[FormType.TopRight])
                {
                    foreach (var bottomRightCorner in potentialCorners[FormType.BottomRight])
                    {
                        foreach (var bottomLeftCorner in potentialCorners[FormType.BottomLeft])
                        {
                            var pixels = new List<Point>();
                            pixels.AddRange(ImageHelper.GetLinePixels(topLeftCorner.X, topLeftCorner.Y, topRightCorner.X, topRightCorner.Y));
                            pixels.AddRange(ImageHelper.GetLinePixels(topRightCorner.X, topRightCorner.Y, bottomRightCorner.X, bottomRightCorner.Y));
                            pixels.AddRange(ImageHelper.GetLinePixels(bottomRightCorner.X, bottomRightCorner.Y, bottomLeftCorner.X, bottomLeftCorner.Y));
                            pixels.AddRange(ImageHelper.GetLinePixels(bottomLeftCorner.X, bottomLeftCorner.Y, topLeftCorner.X, topLeftCorner.Y));

                            int whiteCount = 0;
                            foreach (var pixel in pixels)
                            {
                                if (image.C[pixel.X, pixel.Y] == 255)
                                    whiteCount++;
                            }

                            potentialDocumentCorners.Add(new List<Point> { topLeftCorner, topRightCorner, bottomRightCorner, bottomLeftCorner }, (double)whiteCount / pixels.Count);
                        }
                    }
                }
            }

           return potentialDocumentCorners.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
        }

        internal IList<Point> GetCorners(IList<Line> lines, GrayscaleStandardImage image)
        {
            ConcurrentBag<Point> bestCorners = new ConcurrentBag<Point>();
            var potentialCorners = new List<Point>();
            // GET BEST PAIR OF LINE
            for (int i = 0; i < lines.Count - 1; i++)
            {
                for (int j = i + 1; j < lines.Count; j++)
                {
                    var intersection = new LineIntersection(lines[i], lines[j]);

                    if (intersection.IntersectionPoint.X > 0 && intersection.IntersectionPoint.X < image.Width
                        && intersection.IntersectionPoint.Y > 0 &&
                        intersection.IntersectionPoint.Y < image.Height)
                    {
                        bestCorners.Add(intersection.IntersectionPoint);
                    }
                }
            }

            return bestCorners.ToList();
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
                line.WhiteRatio = score;
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
            double averageScore = 0;
            foreach (var line in lines)
            {
                averageScore += line.Value;
            }
            averageScore = averageScore/lines.Count;
            var maxScore = lines.Max(kv => kv.Value);
            var cutoff = (averageScore + maxScore)/2;
            var orderedLines = lines.Where(kv => kv.Value > cutoff).OrderByDescending(s => s.Value).ToList();
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

                        var distance = (distanceP1 + distanceP2) / 2;

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
    }
}
