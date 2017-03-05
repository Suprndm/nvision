using System.Drawing;
using Schema.Api.Model;

namespace Schema.Api.Service
{
    public interface ISchemaService
    {
        SchemaPreview ExtractSchemaFromImage(Bitmap bitmap);

        Bitmap PrepareImage(Bitmap bitmap);
    }
}