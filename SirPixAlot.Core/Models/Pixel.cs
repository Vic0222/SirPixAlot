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
        [Id(0)]
        public string Color { get; set; } = string.Empty;
        [Id(1)]
        public long X { get; set; }

        [Id(2)]
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
