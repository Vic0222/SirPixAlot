using Orleans;
using SirPixAlot.Core.Grains;
using SirPixAlot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirPixAlot.Core.GrainInterfaces
{
    public interface ICanvasGrain : IGrainWithStringKey
    {
        Task<bool> ChangePixelColor(long x, long y, string color);

        Task<IEnumerable<Pixel>> GetPixels();

        Task<Pixel?> GetPixel(long x, long y);
    }
}
