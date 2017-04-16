using System.Drawing;

namespace NVision.Api.Service
{
    public interface IImageService
    {
        Bitmap PrepareImageToOcr(Bitmap bitmap);
    }
}