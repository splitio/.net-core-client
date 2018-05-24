using Common.Logging;
using Splitio.Domain;
using Splitio.Services.Cache.Classes;
using Splitio.Services.EngineEvaluator;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;

namespace Splitio.Services.Client.Classes
{
    public class LocalhostClient : SplitClient
    {
        private const string DefaultSplitFileName = ".split";
        private static readonly ILog Log = LogManager.GetLogger(typeof(LocalhostClient));

        private FileSystemWatcher watcher;
        private string fullPath;

        public LocalhostClient(string filePath, ILog log, Splitter splitter = null) : base(log)
        {
            fullPath = LookupFilePath(filePath);
            var directoryPath = Path.GetDirectoryName(fullPath);

            watcher = new FileSystemWatcher(directoryPath != string.Empty ? directoryPath : Directory.GetCurrentDirectory(), Path.GetFileName(fullPath));
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += new FileSystemEventHandler(OnFileChanged);
            watcher.EnableRaisingEvents = true;

            var splits = ParseSplitFile(fullPath);
            splitCache = new InMemorySplitCache(splits);
            BuildSplitter(splitter);
            manager = new SplitManager(splitCache);
        }

        public override bool Track(string key, string trafficType, string eventType, double? value = default(double?))
        {
            return true;
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            var splits = ParseSplitFile(fullPath);
            splitCache = new InMemorySplitCache(splits);
        }

        private string LookupFilePath(string filePath)
        {
            if (File.Exists(filePath))
            {
                return filePath;
            }
            var home = Environment.GetEnvironmentVariable("USERPROFILE");

            var fullPath = Path.Combine(home, filePath ?? DefaultSplitFileName);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }

            throw new DirectoryNotFoundException("Splits file could not be found");
        }

        private ConcurrentDictionary<string, ParsedSplit> ParseSplitFile(string filePath)
        {
            ConcurrentDictionary<string, ParsedSplit> splits = new ConcurrentDictionary<string, ParsedSplit>();

            string line;

            StreamReader file = new StreamReader(File.OpenText(filePath).BaseStream);
            
            while ((line = file.ReadLine()) != null)
            {
                line = line.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                {
                    continue;
                }

                string[] feature_treatment = Regex.Split(line, @"\s+");

                if (feature_treatment.Length != 2)
                {
                    Log.Info("Ignoring line since it does not have exactly two columns: " + line);
                    continue;
                }

                splits.TryAdd(feature_treatment[0], CreateParsedSplit(feature_treatment[0], feature_treatment[1]));
                Log.Info("100% of keys will see " + feature_treatment[1] + " for " + feature_treatment[0]);

            }

            file.Dispose();

            return splits;
        }

        /// <summary>
        /// Creates a ParsedSplit instance that always returns 
        /// treatment specified in input file. It is implemented this way
        /// for simplification. When a split is killed, the client 
        /// returns default treatment for that feature.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="treatment"></param>
        /// <returns></returns>
        private ParsedSplit CreateParsedSplit(string name, string treatment)
        {
            var split = new ParsedSplit()
            {
                name = name,
                seed = 0,
                killed = true,
                defaultTreatment = treatment,
                conditions = null
            };

            return split;
        }

        private void BuildSplitter(Splitter splitter)
        {
            this.splitter = splitter ?? new Splitter();
        }

        public override void Destroy()
        {
            watcher.Dispose();
            splitCache.Clear();
        }
    }
}
