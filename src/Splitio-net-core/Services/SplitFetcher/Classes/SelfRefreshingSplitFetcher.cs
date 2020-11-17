using Splitio.CommonLibraries;
using Splitio.Domain;
using Splitio.Services.Cache.Interfaces;
using Splitio.Services.Logger;
using Splitio.Services.Parsing.Interfaces;
using Splitio.Services.Shared.Classes;
using Splitio.Services.SplitFetcher.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Splitio.Services.SplitFetcher.Classes
{
    public class SelfRefreshingSplitFetcher : ISplitFetcher
    {
        private static readonly ISplitLogger _log = WrapperAdapter.GetLogger(typeof(SelfRefreshingSplitFetcher));

        private readonly ISplitChangeFetcher _splitChangeFetcher;
        private readonly ISplitParser _splitParser;
        private readonly IReadinessGatesCache _gates;
        private readonly ISplitCache _splitCache;
        private readonly CancellationTokenSource _cancelTokenSource;
        private readonly int _interval;

        public SelfRefreshingSplitFetcher(ISplitChangeFetcher splitChangeFetcher,
            ISplitParser splitParser, 
            IReadinessGatesCache gates, 
            int interval, 
            ISplitCache splitCache = null)
        {
            _cancelTokenSource = new CancellationTokenSource();

            _splitChangeFetcher = splitChangeFetcher;
            _splitParser = splitParser;
            _gates = gates;
            _interval = interval;
            _splitCache = splitCache;
        }

        #region Public Methods
        public void Start()
        {
            var periodicTask = PeriodicTaskFactory.Start(async() =>
            {
                await FetchSplits();
            },
            intervalInMilliseconds: _interval * 1000,
            cancelToken: _cancelTokenSource.Token);
        }

        public void Stop()
        {
            _cancelTokenSource.Cancel();
        }

        public void Clear()
        {
            _splitCache.Clear();
        }

        public async Task FetchSplits()
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
                        _gates.SplitsAreReady();
                        //There are no new split changes
                        break;
                    }

                    if (result.splits != null && result.splits.Count > 0)
                    {
                        UpdateSplitsFromChangeFetcherResponse(result.splits);
                        _splitCache.SetChangeNumber(result.till);
                    }
                }
                catch (Exception e)
                {
                    _log.Error("Exception caught refreshing splits", e);
                    Stop();
                }
                finally
                {
                    if (_log.IsDebugEnabled)
                    {
                        _log.Debug(string.Format("split fetch before: {0}, after: {1}", changeNumber, _splitCache.GetChangeNumber()));
                    }
                }
            }
        }
        #endregion

        #region Private Methods
        private void UpdateSplitsFromChangeFetcherResponse(List<Split> splitChanges)
        {
            var addedSplits = new List<Split>();
            var removedSplits = new List<Split>();

            foreach (Split split in splitChanges)
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
                    var isUpdated = _splitCache.AddOrUpdate(split.name, _splitParser.Parse(split));

                    if (!isUpdated)
                    {
                        //If not existing in _splits, its a new split
                        addedSplits.Add(split);
                    }
                }
            }

            if (addedSplits.Count() > 0)
            {
                var addedFeatureNames = addedSplits
                    .Select(x => x.name)
                    .ToList();

                if (_log.IsDebugEnabled)
                {
                    _log.Debug(string.Format("Added features: {0}", string.Join(" - ", addedFeatureNames)));
                }
            }

            if (removedSplits.Count() > 0)
            {
                var removedFeatureNames = removedSplits
                    .Select(x => x.name)
                    .ToList();

                if (_log.IsDebugEnabled)
                {
                    _log.Debug(string.Format("Deleted features: {0}", string.Join(" - ", removedFeatureNames)));
                }
            }
        }
        #endregion
    }
}
