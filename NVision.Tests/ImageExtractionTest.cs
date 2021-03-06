﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using NVision.Report;
using Newtonsoft.Json;
using NVision.Api.Model;
using NVision.Internal.Service;
using NVision.Internal.Formatting;
using NVision.Internal.Model;

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
        public void ShouldGetLine()
        {
            var grayImage = new GrayscaleStandardImage() { Height = 500, Width = 350 };
            var point = new Point(175, 250);
            var line = _documentCornersDetectionService.GetLineFromAngleAndPoint(grayImage, point, 45);
        }

        [Test]
        public void SpectralAnalysis()
        {
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(directory, $"Reports/Analysis tests/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}/");
            Directory.CreateDirectory(path);
            var pikes = "";
            for (int i = 0; i < _testCases.Count; i++)
            {
                _stopwatch.Start();
                var bitmap = _testCases[i];
                var reducedBitmap = bitmap.ReduceSize((double)500 / Math.Max(bitmap.Width, bitmap.Height));
                var standardImage = reducedBitmap.ConvertToStandardImage();
                var saturationMap = ImageHelper.GetSaturationMap(standardImage);
                var brightnessMap = ImageHelper.GetBrightnessMap(standardImage);
                var brightnessPikes = ImageHelper.GetPikes(brightnessMap);
                var saturationPikes = ImageHelper.GetPikes(saturationMap);

                var svMap = ImageHelper.GetSVMap(standardImage);
                var svPikes = ImageHelper.GetSvPikes(svMap);

                var hueMap = ImageHelper.GetHueMAp(standardImage);
                pikes += $"Case {i + 1}: \n\r svPikes:{svPikes.Count} \n\r ";
                //  string saturationReport = "Level;SaturationFrequence;SaturationPikes;BrightnessFrequence;BrightnessPikes\n";
                //for (int j = 0; j <= 99; j++)
                //{
                //    var saturationFrequency = 0;
                //    var brightnessFrequency = 0;
                //    var saturationPikesElements = 0;
                //    var brightnessPikesElements = 0;

                //    if (brightnessMap.ContainsKey(j))
                //        brightnessFrequency = brightnessMap[j] + 1;

                //    if (saturationMap.ContainsKey(j))
                //        saturationFrequency = saturationMap[j];

                //    if (brightnessPikes.ContainsKey(j))
                //        brightnessPikesElements = brightnessPikes[j] + 1;

                //    if (saturationPikes.ContainsKey(j))
                //        saturationPikesElements = saturationPikes[j] + 1;

                //    saturationReport += $"{j};{saturationFrequency};{saturationPikesElements};{brightnessFrequency};{brightnessPikesElements}\n";
                //}

                var svReport = "Saturation;Brightness;Frequency\n";
                for (int j = 0; j < 100; j++)
                {
                    for (int k = 0; k < 100; k++)
                    {
                        var point = new Point(j, k);
                        var svFrequency = 0;
                        if (svMap.ContainsKey(point))
                            svFrequency = svMap[point];

                        svReport += $"{j};{k};{svFrequency}\n";
                    }
                }

                File.WriteAllText(path + $"SpectralAnalysis{i + 1}.csv", svReport);
            }
            File.WriteAllText(path + $"pikes.txt", pikes);
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
                var targetStandardImage = targetBitmap.ConvertToStandardImage(100);
                var image = reducedBitmap.ConvertToStandardImage(100);
                _stopwatch.Stop();
                report.Results[step].ExecutionTimePerImageMs += _stopwatch.ElapsedMilliseconds;
                _stopwatch.Reset();
                image.ConvertToBitmap().Save(path + $"Initial Image{i + 1}.jpg", ImageFormat.Jpeg);

                // Step preparation
                step = Operation.ImagePreparation;
                _stopwatch.Start();
                var svMap = ImageHelper.GetSVMap(image);
                var svPikes = ImageHelper.GetSvPikes(svMap);
                var grayImage = _documentPreparationService.IsolateDocument(image, svPikes);
                _stopwatch.Stop();
                report.Results[step].ExecutionTimePerImageMs += _stopwatch.ElapsedMilliseconds;
                _stopwatch.Reset();
                image = grayImage.ConvertToStandardImage();
                image.ConvertToBitmap().Save(path + $"PreparationResult_{i + 1}.jpg", ImageFormat.Jpeg);

                // Step Corner Detection
                step = Operation.ImageCornerDetection;
                _stopwatch.Start();
                var masks = new Dictionary<FormType, Area>();
                var widthOverlay = image.Width / 10;
                var heightOverlay = image.Height / 10;

                masks.Add(FormType.TopLeft, new Area(0, 0, image.Width / 2 + widthOverlay, image.Height / 2 + heightOverlay));
                masks.Add(FormType.TopRight, new Area(image.Width / 2-widthOverlay, 0, image.Width, image.Height / 2 + heightOverlay));
                masks.Add(FormType.BottomLeft, new Area(0, image.Height / 2 - heightOverlay, image.Width / 2+widthOverlay, image.Height));
                masks.Add(FormType.BottomRight, new Area(image.Width / 2 - widthOverlay, image.Height / 2 - heightOverlay, image.Width, image.Height));
                var allForms = CornersBuilder.GetCornerForms();
                var copiedImage = grayImage.ConvertToStandardImage();
                var potentialCorners = new ConcurrentDictionary<FormType, IList<Point>>();
                var interestColor = Color.FromArgb(200, 255, 200);
                var lineColor = Color.FromArgb(255, 0, 0);
                var cornerColor = Color.FromArgb(255, 200, 255);
                var lines = new ConcurrentBag<Line>();
                Parallel.ForEach(masks.Keys, (form) =>
                {
                    var interests = _documentCornersDetectionService.GetPointsOfInterest(grayImage, form, masks[form]);


                    foreach (var interest in interests)
                    {
                        copiedImage = ImageHelper.DrawIndicator(copiedImage, interest.X, interest.Y, 5, interestColor);
                    }
                    var partialLines = _documentCornersDetectionService.GetLines(interests, grayImage);

                    foreach (var partialLine in partialLines)
                    {
                            lines.Add(partialLine);
                    }
                  });

                foreach (var line in lines)
                {
                    var pixels = ImageHelper.GetLinePixels(line.P1.X, line.P1.Y, line.P2.X, line.P2.Y);
                    copiedImage = copiedImage.DrawPixels(pixels, lineColor);
                }

                var corners = _documentCornersDetectionService.GetCorners(lines.ToList(), grayImage);

                //foreach (var corner in corners)
                //{
                //    copiedImage = ImageHelper.DrawIndicator(copiedImage, corner.X, corner.Y, 5, cornerColor);
                //    potentialCorners.TryAdd(form, corners);
                //}

                var finalCorners = _documentCornersDetectionService.GetFinalCorners(grayImage,
                    potentialCorners.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

                foreach (var finalCorner in finalCorners)
                {
                    copiedImage = ImageHelper.DrawIndicator(copiedImage, finalCorner.X, finalCorner.Y, 5, lineColor);
                }

                _stopwatch.Stop();
                report.Results[step].ExecutionTimePerImageMs += _stopwatch.ElapsedMilliseconds;
                _stopwatch.Reset();
                copiedImage.ConvertToBitmap().Save(path + $"CornersResult_{i + 1}.jpg", ImageFormat.Jpeg);

                var originalRatio = targetStandardImage.Height / grayImage.Height;
                IList<Point> originalCornersCoordinates = new List<Point>();
                foreach (var corner in finalCorners)
                {
                    originalCornersCoordinates.Add(new Point(corner.X * originalRatio, corner.Y * originalRatio));
                }

                // Step Straightening
                step = Operation.ImageStraightening;
                _stopwatch.Start();
                var straightenImage = _documentStraightenerService.StraightenDocument(targetStandardImage, originalCornersCoordinates);
                _stopwatch.Stop();
                report.Results[step].ExecutionTimePerImageMs += _stopwatch.ElapsedMilliseconds;
                _stopwatch.Reset();

                _stopwatch.Stop();
                straightenImage.ConvertToBitmap().Save(path + $"StraightenedResult{i + 1}.jpg", ImageFormat.Jpeg);
            }

            long totalExecutionTime = 0;
            foreach (var op in report.Results.Keys.ToList())
            {
                report.Results[op].ExecutionTimePerImageMs = report.Results[op].ExecutionTimePerImageMs / _testCases.Count;
                totalExecutionTime += report.Results[op].ExecutionTimePerImageMs;
            }

            report.TotalExecutionTimeMs = totalExecutionTime;

            string json = JsonConvert.SerializeObject(report);
            File.WriteAllText(path + "report.txt", json);
        }
    }
}
