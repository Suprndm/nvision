using System.Drawing;

namespace NVision.Internal.Model
{
    public class SimilarityResult
    {
        public SimilarityResult(Point position, double similarity)
        {
            Position = position;
            Similarity = similarity;
        }

        public Point Position { get; set; }
        public double Similarity { get; set; }
    }
}
