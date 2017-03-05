using System.Drawing;
using NVision.Api.Model;

namespace NVision.Api.Service
{
    public interface IImageService
    {
        StandardSchema ExtractSchemaFromImage(Bitmap bitmap);

        Bitmap PrepareImage(Bitmap bitmap);
    }
}