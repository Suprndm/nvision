using System;
using System.Collections.Generic;
using System.Drawing;
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

            Bitmap preparedImage = null;

            preparedImage = schemaService.PrepareImage(Resources.Resources.Schema_Test);
            preparedImage.Save("output1.jpg", ImageFormat.Jpeg);

            preparedImage = schemaService.PrepareImage(Resources.Resources.Schema_2);
            preparedImage.Save("output2.jpg", ImageFormat.Jpeg);

            preparedImage = schemaService.PrepareImage(Resources.Resources.Schema_3);
            preparedImage.Save("output3.jpg", ImageFormat.Jpeg);
            Console.WriteLine("Done");
            Console.ReadKey(true);
        }
    }
}
