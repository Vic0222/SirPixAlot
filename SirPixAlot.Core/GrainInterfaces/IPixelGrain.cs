using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirPixAlot.Core.GrainInterfaces
{
    public interface IPixelGrain : IGrainWithIntegerCompoundKey
    {
        Task<bool> ChangeColor(string color);
        Task<string> GetColor();
    }
}
