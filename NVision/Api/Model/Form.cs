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
        public Form(bool[,] mask, int size, string name = null)
        {
            Name = name;
            Mask = mask;
            Height = size;
            Width = size;
            Center = new Point(size/2,size/2);
            Whites = new List<Point>();
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (Mask[i, j])
                    {
                        Whites.Add(new Point(i, j));
                    }
                }
            }
        }

        public bool[,] Mask { get; set; }

        public int Height { get; set; }
        public int Width { get; set; }
        public Point Center { get; set; }
        public string Name { get; set; }
        public IList<Point> Whites { get; set; }

        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    str += " " + Mask[i, j].ToString()[0]+" ";
                }
                str += "\n";
            }

            return str;
        }
    }
}
