using Metrics;
using System;

namespace Performance
{
    class Program
    {

        static void Main(string[] args)
        {
            Metric.Config
                    .WithHttpEndpoint("http://localhost:1345/")
                    .WithAllCounters();

            var metricsProcessor = new MetricsProcessor();
            metricsProcessor.ProcessRequest(Int32.Parse(args[0]), Int32.Parse(args[1]));
        }
    }
}