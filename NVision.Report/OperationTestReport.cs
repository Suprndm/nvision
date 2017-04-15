using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NVision.Report
{
    public class OperationTestReport
    {
        public Operation Operation { get; set; }
        public DateTime Date { get; set; }
        public int ImagesCount { get; set; }
        public OperationTestResult Result { get; set; }
    }
}
