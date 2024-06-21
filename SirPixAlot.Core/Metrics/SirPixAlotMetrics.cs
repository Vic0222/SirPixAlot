using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirPixAlot.Core.Metrics
{
    public class SirPixAlotMetrics
    {
        private readonly Histogram<long> _grainReadEventsDuration;

        public SirPixAlotMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create("SirPixAlot");
            _grainReadEventsDuration = meter.CreateHistogram<long>("sirpixalot.grain.read_events_duration");
        }
        
        public void GrainReadEventsDuration(long duration)
        {
            _grainReadEventsDuration.Record(duration);
        }
    }
}
