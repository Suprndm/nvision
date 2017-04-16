using NVision.Api.Model;

namespace NVision.Internal.Model
{
    internal class GrayscaleStandardImage
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public int[,] C { get; set; }
        public Area Area { get; set; }
    }
}
