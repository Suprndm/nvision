using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NVision.Api.Model;
using Schema.Api.Model;

namespace Schema.Api.Service
{
    public class SchemaGenerator:ISchemaGenerator
    {
        public SchemaPreview GeneratePreview(StandardSchema schema)
        {
            return new SchemaPreview {StandardSchema = schema};
        }
    }
}
