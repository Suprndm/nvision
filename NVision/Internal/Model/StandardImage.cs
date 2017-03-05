namespace NVision.Internal.Model
{
    internal class StandardImage
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public int[,] R { get; set; }
        public int[,] G { get; set; }
        public int[,] B { get; set; }
    }
}
