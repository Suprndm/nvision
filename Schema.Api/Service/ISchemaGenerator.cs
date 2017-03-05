using NVision.Api.Model;
using Schema.Api.Model;

namespace Schema.Api.Service
{
    public interface ISchemaGenerator
    {
        SchemaPreview GeneratePreview(StandardSchema schema);
    }
}