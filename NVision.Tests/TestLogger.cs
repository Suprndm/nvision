using System;
using Helper;

namespace NVision.Tests
{
    public class TestLogger : ILogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
