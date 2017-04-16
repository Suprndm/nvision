using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NUnit.Framework;
using NVision.Report;
using Newtonsoft.Json;
using NVision.Internal.Service;
using NVision.Internal.Formatting;

namespace NVision.Tests
{
    public class ImageExtractionTest
    {
        private IList<Bitmap> _testCases;
        private Stopwatch _stopwatch;
        private DocumentPreparationService _documentPreparationService;
        private DocumentCornersDetectionService _documentCornersDetectionService;
        private DocumentStraightenerService _documentStraightenerService;

        [SetUp]
        public void Setup()
        {
            _documentPreparationService = new DocumentPreparationService();
            _documentCornersDetectionService = new DocumentCornersDetectionService();
            _documentStraightenerService = new DocumentStraightenerService();

            _stopwatch = new Stopwatch();
            _testCases = new List<Bitmap>
            {
                Resources.Resources.TestCase1,
                Resources.Resources.TestCase2,
                Resources.Resources.TestCase3,
                Resources.Resources.TestCase4,
                Resources.Resources.TestCase5,
            };
        }

        [Test]
        public void Test()
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(directory, $"Reports/Algorithms tests/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}/");
            Directory.CreateDirectory(path);

            var report = new OperationTestReport
            {
                Date = DateTime.UtcNow,
                ImagesCount = _testCases.Count,
                Results = new Dictionary<Operation, OperationTestResult>() 
            };

            foreach (Operation operation in Enum.GetValues(typeof(Operation)))
            {
                report.Results.Add(operation, new OperationTestResult
                {
                    Accuracy = 100,
                    ExecutionTimePerImageMs = 0,
                    SuccessRatio = 100
                });
            }

            for (int i = 0; i < _testCases.Count; i++)
            {
                // Step Conversion
                var step = Operation.ImageConversion;
                _stopwatch.Start();
                var bitmap = _testCases[i];
                var reducedBitmap = bitmap.ReduceSize((double)500 / Math.Max(bitmap.Width, bitmap.Height));
                var targetBitmap = bitmap.ReduceSize((double)1000 / Math.Max(bitmap.Width, bitmap.Height));
                var targetStandardImage = targetBitmap.ConvertToStandardImage();
                var standardImage = reducedBitmap.ConvertToStandardImage();
                _stopwatch.Stop();
                report.Results[step].ExecutionTimePerImageMs += _stopwatch.ElapsedMilliseconds;
                _stopwatch.Reset();

                // Step preparation
                step = Operation.ImagePreparation;
                _stopwatch.Start();
                var grayImage = _documentPreparationService.IsolateDocument(standardImage);
                _stopwatch.Stop();
                report.Results[step].ExecutionTimePerImageMs += _stopwatch.ElapsedMilliseconds;
                _stopwatch.Reset();

                // Step Corner Detection
                step = Operation.ImageCornerDetection;
                _stopwatch.Start();
                var corners = _documentCornersDetectionService.GetCorners(grayImage);
                _stopwatch.Stop();
                report.Results[step].ExecutionTimePerImageMs += _stopwatch.ElapsedMilliseconds;
                _stopwatch.Reset();

                var originalRatio = targetStandardImage.Height/grayImage.Height;
                IList<Point> originalCornersCoordinates = new List<Point>();
                foreach (var corner in corners)
                {
                    originalCornersCoordinates.Add(new Point(corner.X* originalRatio, corner.Y * originalRatio ));
                }

                    // Step Straightening
                step = Operation.ImageStraightening;
                _stopwatch.Start();
                var straightenImage  = _documentStraightenerService.StraightenDocument(targetStandardImage, originalCornersCoordinates);
                _stopwatch.Stop();
                report.Results[step].ExecutionTimePerImageMs += _stopwatch.ElapsedMilliseconds;
                _stopwatch.Reset();

                _stopwatch.Stop();
                grayImage.ConvertToBitmap().Save(path + $"PreparationResult_{i+1}.jpg", ImageFormat.Jpeg);
               _documentCornersDetectionService.GetCornersImageResult(grayImage, corners).ConvertToBitmap().Save(path + $"CornersResult_{i+1}.jpg", ImageFormat.Jpeg);
                straightenImage.ConvertToBitmap().Save(path + $"StraightenedResult{i+1}.jpg", ImageFormat.Jpeg);
            }

            long totalExecutionTime = 0;
            foreach (var op in report.Results.Keys.ToList())
            {
                report.Results[op].ExecutionTimePerImageMs = report.Results[op].ExecutionTimePerImageMs/_testCases.Count;
                totalExecutionTime += report.Results[op].ExecutionTimePerImageMs;
            }

            report.TotalExecutionTimeMs = totalExecutionTime;

            string json = JsonConvert.SerializeObject(report);
            File.WriteAllText(path + "report.txt", json);
        }
    }
}
