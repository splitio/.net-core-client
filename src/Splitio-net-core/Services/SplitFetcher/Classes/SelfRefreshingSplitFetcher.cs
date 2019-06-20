using Common.Logging;
using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Parsing;
using Splitio.Services.SplitFetcher.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Splitio.Services.SplitFetcher.Classes
{
    public class SelfRefreshingSplitFetcher
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SelfRefreshingSplitFetcher));

        private readonly ISplitChangeFetcher _splitChangeFetcher;
        private readonly IReadinessGatesCache _gates;
        private readonly ITrafficTypesCache _trafficTypesCache;
        private readonly ISplitCache _splitCache;
        private readonly SplitParser _splitParser;
        private readonly CancellationTokenSource _cancelTokenSource;
        private readonly int interval;
        
        public SelfRefreshingSplitFetcher(ISplitChangeFetcher splitChangeFetcher,
            SplitParser splitParser, 
            IReadinessGatesCache gates, 
            int interval,
            ITrafficTypesCache trafficTypesCache,
            ISplitCache splitCache = null)
        {
            _cancelTokenSource = new CancellationTokenSource();
            _splitChangeFetcher = splitChangeFetcher;
            _splitParser = splitParser;
            _gates = gates;
            _trafficTypesCache = trafficTypesCache;
            _splitCache = splitCache;

            this.interval = interval;
        }

        public void Start()
        {
            var periodicTask = PeriodicTaskFactory
                .Start(() =>
                {
                    RefreshSplits();
                },
                intervalInMilliseconds: interval * 1000,
                cancelToken: _cancelTokenSource.Token);
        }

        public void Stop()
        {
            _cancelTokenSource.Cancel();
            _splitCache.Clear();
        }

        private void UpdateSplitsFromChangeFetcherResponse(List<Split> splitChanges)
        {
            var addedSplits = new List<Split>();
            var removedSplits = new List<Split>();

            foreach (var split in splitChanges)
            {
                //If not active --> Remove Split
                var isValidStatus = Enum.TryParse(split.status, out StatusEnum result);

                if (!isValidStatus || result != StatusEnum.ACTIVE)
                {
                    _splitCache.RemoveSplit(split.name);
                    removedSplits.Add(split);
                }
                else
                {
                    //Test if its a new Split, remove if existing
                    var isRemoved = _splitCache.RemoveSplit(split.name);

                    if (!isRemoved)
                    {
                        //If not existing in _splits, its a new split
                        addedSplits.Add(split);
                    }

                    var parsedSplit = _splitParser.Parse(split);

                    _splitCache.AddSplit(parsedSplit.name, parsedSplit);
                }
            }



            if (addedSplits.Count() > 0 && Log.IsDebugEnabled)
            {
                var addedFeatureNames = addedSplits.Select(x => x.name).ToList();

                Log.Debug(string.Format("Added features: {0}", string.Join(" - ", addedFeatureNames)));
            }

            if (removedSplits.Count() > 0 && Log.IsDebugEnabled)
            {
                var removedFeatureNames = removedSplits.Select(x => x.name).ToList();

                Log.Debug(string.Format("Deleted features: {0}", string.Join(" - ", removedFeatureNames)));
            }
        }

        private async void RefreshSplits()
        {
            while (true)
            {
                var changeNumber = _splitCache.GetChangeNumber();

                try
                {
                    var result = await _splitChangeFetcher.Fetch(changeNumber);

                    if (result == null)
                    {
                        break;
                    }

                    if (changeNumber >= result.till)
                    {
                        //There are no new split changes
                        _gates.SplitsAreReady();
                        break;
                    }

                    if (result.splits != null && result.splits.Count > 0)
                    {
                        UpdateSplitsFromChangeFetcherResponse(result.splits);
                        _splitCache.SetChangeNumber(result.till);

                        var splits = _splitCache.GetAllSplits();

                        _trafficTypesCache.Clear();
                        _trafficTypesCache.Load(splits.Select(tt => ((ParsedSplit)tt).trafficTypeName).ToList());
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Exception caught refreshing splits", e);
                    Stop();
                }
                finally
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug(string.Format("split fetch before: {0}, after: {1}", changeNumber, _splitCache.GetChangeNumber()));
                    }
                }
            }
        }
    }
}
