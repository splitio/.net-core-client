using Splitio.Domain;
using Splitio.Services.Cache.Classes;
using Splitio.Services.Impressions.Classes;
using Splitio.Services.InputValidation.Classes;
using Splitio.Services.Logger;
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
        
        private ILocalhostFileService _localhostFileService;

        private readonly FileSystemWatcher _watcher;
        private readonly string _fullPath;

        public LocalhostClient(string filePath, 
            ISplitLogger log = null) : base(GetLogger(log))
        {
            _fullPath = LookupFilePath(filePath);

            if (_fullPath.ToLower().EndsWith(SplitFileYaml) || _fullPath.ToLower().EndsWith(SplitFileYml))
            {
                _localhostFileService = new YamlLocalhostFileService();
            }
            else
            {
                _log.Warn("Localhost mode: .split/.splits mocks will be deprecated soon in favor of YAML files, which provide more targeting power. Take a look in our documentation.");

                _localhostFileService = new LocalhostFileService();
            }

            var directoryPath = Path.GetDirectoryName(_fullPath);

            _watcher = new FileSystemWatcher(directoryPath != string.Empty ? directoryPath : Directory.GetCurrentDirectory(), Path.GetFileName(_fullPath))
            {
                NotifyFilter = NotifyFilters.LastWrite
            };

            _watcher.Changed += new FileSystemEventHandler(OnFileChanged);
            _watcher.EnableRaisingEvents = true;

            var splits = ParseSplitFile(_fullPath);
            _splitCache = new InMemorySplitCache(splits);

            _blockUntilReadyService = new NoopBlockUntilReadyService();
            _manager = new SplitManager(_splitCache, _blockUntilReadyService);

            ApiKey = "localhost";
            Destroyed = false;

            _trafficTypeValidator = new TrafficTypeValidator(_splitCache);

            BuildEvaluator();

            _impressionsManager = new ImpressionsManager(null, null, null, false, ImpressionModes.Debug);
        }

        #region Public Methods
        public override bool Track(string key, string trafficType, string eventType, double? value = default(double?), Dictionary<string, object> properties = null)
        {
            return true;
        }

        public override void Destroy()
        {
            if (!Destroyed)
            {
                _watcher.Dispose();
                _splitCache.Clear();
                base.Destroy();
            }
        }
        #endregion

        #region Private Methods
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            var splits = ParseSplitFile(_fullPath);

            _splitCache.Clear();

            foreach (var split in splits)
            {
                if (split.Value != null)
                {
                    _splitCache.AddSplit(split.Key, split.Value);
                }
            }
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

        private static ISplitLogger GetLogger(ISplitLogger splitLogger = null)
        {
            return splitLogger ?? WrapperAdapter.GetLogger(typeof(LocalhostClient));
        }
        #endregion
    }
}
