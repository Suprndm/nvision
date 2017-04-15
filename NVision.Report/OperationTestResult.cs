using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NVision.Report
{
    public class OperationTestResult
    {
        public long ExecutionTimePerImageMs { get; set; }
        public double Accuracy { get; set; }
        public double SuccessRatio { get; set; } 
    }
}
