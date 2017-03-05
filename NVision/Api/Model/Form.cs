using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NVision.Api.Model
{
    public class Form
    {
        public Form(int size)
        {
            Mask = new double[size, size];
            Height = size;
            Width = size;
            Center = new Point(size/2,size/2);
            WhitePixelCount = 0;
        }
        public double[,] Mask { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public Point Center { get; set; }
        public int WhitePixelCount { get; set; }

        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    str += " " + Mask[i, j].ToString("N1")+" ";
                }
                str += "\n";
            }

            return str;
        }
    }
}
