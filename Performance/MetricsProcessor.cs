using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Splitio.Services.Client.Interfaces;
using Splitio.Services.Client.Classes;
using System.Diagnostics;


namespace Performance
{
    public class MetricsProcessor
    {
        ISplitClient client;


       /* public MetricsProcessor()
        {

        }*/

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
            client = GetInstance("89d5uvpc1ktg9gdj1nrhk0coh6k5vsqj1uu4");
            Console.WriteLine("Key: " + "89d5uvpc1ktg9gdj1nrhk0coh6k5vsqj1uu4");
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
                            client.GetTreatment("abcdefghijklmnopqrxyz123456789ABCDEF", "benchmark_jw_1", atributes);
                    }
                });
            }
            while (true)
            {
                if (sw.Elapsed > TimeSpan.FromMinutes(minutesRunning))
                {
                    Console.WriteLine("Finished.");
                    //Console.Read();
                    return;
                }
            }

            
        }
    }
}