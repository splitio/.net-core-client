using App.Metrics;
using App.Metrics.Abstractions.ReservoirSampling;
using App.Metrics.Core.Options;
using App.Metrics.ReservoirSampling.ExponentialDecay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerformanceWebApp
{
    public static class AppMetricsRegistery
    {
        public static TimerOptions SampleTimer { get; } = new TimerOptions
        {
            Name = "Sample Timer",
            MeasurementUnit = Unit.Items,
            DurationUnit = TimeUnit.Milliseconds,
            RateUnit = TimeUnit.Milliseconds//,
            //Reservoir = new Lazy<IReservoir>(() => new DefaultForwardDecayingReservoir(sampleSize: 1028, alpha: 0.015))
        };
    }
}
