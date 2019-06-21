using Common.Logging;
using Splitio.Domain;
using Splitio.Services.Cache.Classes;
using Splitio.Services.EngineEvaluator;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Splitio.Services.Client.Classes
{
    public class LocalhostClient : SplitClient
    {
        private const string DefaultSplitFileName = ".split";
        private const string SplitFileYml = ".yml";
        private const string SplitFileYaml = ".yaml";
        private static readonly ILog Log = LogManager.GetLogger(typeof(LocalhostClient));

        private ILocalhostFileService _localhostFileService;

        private readonly FileSystemWatcher _watcher;
        private readonly string FullPath;

        public LocalhostClient(string filePath, ILog log, Splitter splitter = null) : base(log)
        {
            FullPath = LookupFilePath(filePath);

            if (FullPath.ToLower().EndsWith(SplitFileYaml) || FullPath.ToLower().EndsWith(SplitFileYml))
            {
                _localhostFileService = new YamlLocalhostFileService(Log);
            }
            else
            {
                Log.Warn("Localhost mode: .split/.splits mocks will be deprecated soon in favor of YAML files, which provide more targeting power. Take a look in our documentation.");

                _localhostFileService = new LocalhostFileService(Log);
            }

            var directoryPath = Path.GetDirectoryName(FullPath);

            _watcher = new FileSystemWatcher(directoryPath != string.Empty ? directoryPath : Directory.GetCurrentDirectory(), Path.GetFileName(FullPath));
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Changed += new FileSystemEventHandler(OnFileChanged);
            _watcher.EnableRaisingEvents = true;

            var splits = ParseSplitFile(FullPath);
            splitCache = new InMemorySplitCache(splits);
            BuildSplitter(splitter);
            manager = new SplitManager(splitCache);

            Destroyed = false;

            _trafficTypeValidator = new TrafficTypeValidator(_log, splitCache);
        }

        public override bool Track(string key, string trafficType, string eventType, double? value = default(double?), Dictionary<string, object> properties = null)
        {
            return true;
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            var splits = ParseSplitFile(FullPath);
            splitCache = new InMemorySplitCache(splits);
        }

        private string LookupFilePath(string filePath)
        {
            filePath = filePath ?? DefaultSplitFileName;

            var filePathLowerCase = filePath.ToLower();

            if (filePathLowerCase.Equals(DefaultSplitFileName))
            {
                var home = Environment.GetEnvironmentVariable("USERPROFILE");
                filePath = Path.Combine(home, filePath);

                filePathLowerCase = filePath.ToLower();
            }

            if (!(filePathLowerCase.EndsWith(SplitFileYml) || filePathLowerCase.EndsWith(SplitFileYaml) || filePathLowerCase.EndsWith(DefaultSplitFileName) || filePathLowerCase.EndsWith(".splits")))
                throw new Exception($"Invalid extension specified for Splits mock file. Accepted extensions are \".yml\" and \".yaml\". Your specified file is {filePath}");

            if (!File.Exists(filePath))
                throw new DirectoryNotFoundException($"Split configuration not found in ${filePath} - Please review your Split file location.");

            return filePath;
        }

        private ConcurrentDictionary<string, ParsedSplit> ParseSplitFile(string filePath)
        {
            return _localhostFileService.ParseSplitFile(filePath);
        }

        private void BuildSplitter(Splitter splitter)
        {
            this.splitter = splitter ?? new Splitter();
        }

        public override void Destroy()
        {
            _watcher.Dispose();
            splitCache.Clear();
            Destroyed = true;
        }
    }
}
