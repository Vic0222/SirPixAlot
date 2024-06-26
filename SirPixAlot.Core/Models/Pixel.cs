using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirPixAlot.Core.Models
{
    [GenerateSerializer]
    public class Pixel
    {
        public string Color { get; set; } = string.Empty;
        public long X { get; set; }

        public long Y { get; set; }

        public Pixel(long x, long y, string color)
        {
            X = x;
            Y = y;
            Color = color;
        }

        public Pixel()
        {

        }
    }
}
