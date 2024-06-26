using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirPixAlot.Core.GrainInterfaces
{
    /// <summary>
    /// A grain use for querying a block of pixels
    /// </summary>
    public interface ICanvasGrain : IGrainWithStringKey
    {
        Task<IEnumerable<Pixel>> GetPixels();

        public class Pixel
        {
            
        }
    }
}
