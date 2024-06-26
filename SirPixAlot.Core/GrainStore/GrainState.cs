using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirPixAlot.Core.StateStore
{
    public class GrainState
    {
        public string GrainId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Version { get; set; }

        public byte[] Data { get; set; } = new byte[0];
    }
}
