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
using System.Threading.Tasks;

namespace Splitio.Services.SplitFetcher.Classes
{
    public class SelfRefreshingSplitFetcher
    {
        protected ISplitCache splitCache;
        private static readonly ILog Log = LogManager.GetLogger(typeof(SelfRefreshingSplitFetcher));
        private readonly ISplitChangeFetcher splitChangeFetcher;
        private readonly SplitParser splitParser;
        private readonly int interval;
        private readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        private readonly IReadinessGatesCache gates;

        public SelfRefreshingSplitFetcher(ISplitChangeFetcher splitChangeFetcher,
            SplitParser splitParser, IReadinessGatesCache gates, int interval, ISplitCache splitCache = null)
        {
            this.splitChangeFetcher = splitChangeFetcher;
            this.splitParser = splitParser;
            this.gates = gates;
            this.interval = interval;
            this.splitCache = splitCache;
        }

        public void Start()
        {
            Task periodicTask = PeriodicTaskFactory.Start(() =>
                                {
                                    RefreshSplits();
                                },
                                intervalInMilliseconds: interval * 1000,
                                cancelToken: cancelTokenSource.Token);
        }

        public void Stop()
        {
            cancelTokenSource.Cancel();
            splitCache.Clear();
        }

        private void UpdateSplitsFromChangeFetcherResponse(List<Split> splitChanges)
        {
            List<Split> addedSplits = new List<Split>();
            List<Split> removedSplits = new List<Split>();

            foreach (Split split in splitChanges)
            {
                //If not active --> Remove Split
                StatusEnum result;
                var isValidStatus = Enum.TryParse(split.status, out result);
                if (!isValidStatus || result != StatusEnum.ACTIVE)
                {
                    splitCache.RemoveSplit(split.name);
                    removedSplits.Add(split);
                }
                else
                {
                    var isUpdated = splitCache.AddOrUpdate(split.name, splitParser.Parse(split));

                    if (!isUpdated)
                    {
                        //If not existing in _splits, its a new split
                        addedSplits.Add(split);
                    }
                }
            }

            if (addedSplits.Count() > 0)
            {
                var addedFeatureNames = addedSplits.Select(x => x.name).ToList();
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(string.Format("Added features: {0}", string.Join(" - ", addedFeatureNames)));
                }
            }
            if (removedSplits.Count() > 0)
            {
                var removedFeatureNames = removedSplits.Select(x => x.name).ToList();
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(string.Format("Deleted features: {0}", string.Join(" - ", removedFeatureNames)));
                }
            }
        }

        private async void RefreshSplits()
        {
            while (true)
            {
                var changeNumber = splitCache.GetChangeNumber();
                try
                {
                    var result = await splitChangeFetcher.Fetch(changeNumber);
                    if (result == null)
                    {
                        break;
                    }
                    if (changeNumber >= result.till)
                    {
                        gates.SplitsAreReady();
                        //There are no new split changes
                        break;
                    }
                    if (result.splits != null && result.splits.Count > 0)
                    {
                        UpdateSplitsFromChangeFetcherResponse(result.splits);
                        splitCache.SetChangeNumber(result.till);
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
                        Log.Debug(string.Format("split fetch before: {0}, after: {1}", changeNumber, splitCache.GetChangeNumber()));
                    }
                }
            }
        }
    }
}
