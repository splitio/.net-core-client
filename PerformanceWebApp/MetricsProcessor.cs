using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Splitio.Services.Client.Interfaces;
using Splitio.Services.Client.Classes;
using System.Diagnostics;
using App.Metrics;

namespace PerformanceWebApp
{
    public class MetricsProcessor
    {
        ISplitClient client;
        private readonly IMetrics _metrics;

        public MetricsProcessor(IMetrics metrics)
        {
            _metrics = metrics;
        }

        private ISplitClient GetInstance(string apikey)
        {
            var config = new ConfigurationOptions();
            config.FeaturesRefreshRate = 30;
            config.SegmentsRefreshRate = 30;
            config.Endpoint = "https://sdk-aws-staging.split.io";
            config.EventsEndpoint = "https://events-aws-staging.split.io";
            config.ReadTimeout = 30000;
            config.ConnectionTimeout = 30000;
            config.Ready = 240000;
            config.NumberOfParalellSegmentTasks = 10;
            config.MaxImpressionsLogSize = 500000;
            config.ImpressionsRefreshRate = 300000;
            var instance = new SelfRefreshingClient(apikey, config);
            return instance;
        }

        public void ProcessRequest(int numberOfThreads, int minutesRunning)
        {
            client = GetInstance("mhfvtl3ilt6aeds8i1amn8qjssbdarlr76ju");
            Console.WriteLine("Running with: " + numberOfThreads + " threads " + minutesRunning + " minutes ");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < numberOfThreads; i++)
            {
                Task.Factory.StartNew(() =>
                {
                    var atributes = new Dictionary<string, object>();
                    atributes.Add("atrib", "1475094824");
                    atributes.Add("atrib2", "20");
                    while (true)
                    {
                        using (_metrics.Measure.Timer.Time(AppMetricsRegistery.SampleTimer))
                        {
                            client.GetTreatment("abcdefghijklmnopqrxyz123456789ABCDEF", "benchmark_jw_1", atributes);
                        }
                    }
                });
            }
            while (true)
            {
                if (sw.Elapsed > TimeSpan.FromMinutes(minutesRunning))
                {
                    Console.WriteLine("Finished.");
                    Console.Read();
                    return;
                }
            }

            
        }
    }
}