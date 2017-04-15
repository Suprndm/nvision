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

     

            Console.WriteLine("Done");
            Console.ReadKey(true);

        
        }
    }
}
