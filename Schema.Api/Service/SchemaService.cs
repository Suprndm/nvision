using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helper;
using NVision.Api.Service;
using Schema.Api.Model;

namespace Schema.Api.Service
{
    public class SchemaService : ISchemaService
    {
        private readonly IImageService _imageService;
        private readonly ISchemaGenerator _schemaGenerator;
        private readonly ILogger _logger;

        public SchemaService(ILogger logger)
        {
            _logger = logger;
            _schemaGenerator = new SchemaGenerator();
            _imageService = new ImageService(_logger);
        }

        public SchemaPreview ExtractSchemaFromImage(Bitmap bitmap)
        {
            var standardSchema = _imageService.ExtractSchemaFromImage(bitmap);
            var schemaPreview = _schemaGenerator.GeneratePreview(standardSchema);

            return schemaPreview;
        }

        public Bitmap PrepareImage(Bitmap bitmap)
        {
            var result = _imageService.PrepareImage(bitmap);
            return result;
        }
    }
}
