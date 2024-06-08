using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirPixAlot.Core.EventStore
{
    public class Event
    {
        public string GrainId { get; set; } = string.Empty;

        public int Version { get; set; }

        public long GlobalPosition { get; set; }

        public string EventType { get; set; } = string.Empty;

        public string Data { get; set; } = string.Empty;
    }
}
