using hq.metrics.Core;
using System;

namespace Performance
{
    class Program
    {
        //private IMetric _resultsMeter;

        static void Main(string[] args)
        {
            
            /*Metric.Config
            .WithHttpEndpoint("http://localhost:12345");
            ;*/
            
            

            var metricsProcessor = new MetricsProcessor();
            metricsProcessor.ProcessRequest(Int32.Parse(args[0]), Int32.Parse(args[1]));
        }
    }
}