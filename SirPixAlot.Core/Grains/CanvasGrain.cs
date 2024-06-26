using SirPixAlot.Core.GrainInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SirPixAlot.Core.GrainInterfaces.ICanvasGrain;

namespace SirPixAlot.Core.Grains
{
    public class CanvasGrain : Grain, ICanvasGrain
    {
        public Task<IEnumerable<Pixel>> GetPixels()
        {
            throw new NotImplementedException();
        }
    }
}
