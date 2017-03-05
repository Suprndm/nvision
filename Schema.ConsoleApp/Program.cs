using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Schema.Api.Service;

namespace Schema.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var schemaService = new SchemaService(new ConsoleLogger());
            //var schemaPreview = schemaService.ExtractSchemaFromImage(Resources.Resources.Schema_Test);
            var preparedImage = schemaService.PrepareImage(Resources.Resources.Schema_Test);


            preparedImage.Save("output.jpg", ImageFormat.Jpeg);
            Console.WriteLine("Done");
            Console.ReadKey(true);
        }
    }
}
