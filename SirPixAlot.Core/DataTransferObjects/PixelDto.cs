using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirPixAlot.Core.DataTransferObjects
{
    public class PixelDto
    {
        public long X { get; set; }
        public long Y { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}
