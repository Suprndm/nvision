using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NUnit.Framework;
using NVision.Api.Service;
using NVision.Report;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace NVision.Tests
{
    public class ImageExtractionTest
    {
        private ImageService _imageService;
        private IList<Bitmap> _testCases;
        private Stopwatch _stopwatch;

        [SetUp]
        public void Setup()
        {
            _imageService = new ImageService(new TestLogger());
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
            var path = Path.Combine(directory, $"Reports/ImageExtractionTest/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}/");
            Directory.CreateDirectory(path);


            var result = new OperationTestResult
            {
                Accuracy = 100,
                ExecutionTimePerImageMs = 0,
                SuccessRatio = 100
            };

            for (int i = 0; i < _testCases.Count; i++)
            {
                _stopwatch.Start();
                var image = _imageService.PrepareImage(_testCases[i]);
                _stopwatch.Stop();
                image.Save(path + $"CaseResult_{i+1}.jpg", ImageFormat.Jpeg);
            }

            result.ExecutionTimePerImageMs += _stopwatch.ElapsedMilliseconds/_testCases.Count;
            var report = new OperationTestReport
            {
                Date = DateTime.UtcNow,
                ImagesCount = _testCases.Count,
                Result = result,
            };

            string json = JsonConvert.SerializeObject(report);
            File.WriteAllText(path + "report.txt", json);

        }
    }
}
