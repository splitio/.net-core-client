using Splitio.Services.EventSource;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System;

namespace Splitio.Services.Common
{
    public class PushManager : IPushManager
    {
        private readonly IAuthApiClient _authApiClient;
        private readonly ISplitLogger _log;
        private readonly IWrapperAdapter _wrapperAdapter;
        private readonly ISSEHandler _sseHandler;
        private readonly IBackOff _backOff;

        public PushManager(int authRetryBackOffBase,
            ISSEHandler sseHandler,
            IAuthApiClient authApiClient,
            IWrapperAdapter wrapperAdapter = null,
            ISplitLogger log = null,
            IBackOff backOff = null)
        {
            _sseHandler = sseHandler;
            _authApiClient = authApiClient;
            _log = log ?? WrapperAdapter.GetLogger(typeof(PushManager));
            _wrapperAdapter = wrapperAdapter ?? new WrapperAdapter();
            _backOff = backOff ?? new BackOff(authRetryBackOffBase, attempt: 1);
        }

        #region Public Methods
        public void StartSse()
        {
            try
            {
                var response = _authApiClient.AuthenticateAsync().Result;

                _log.Debug($"Auth service response pushEnabled: {response.PushEnabled}.");

                if (response.PushEnabled.Value)
                {
                    _sseHandler.Start(response.Token, response.Channels);
                    ScheduleNextTokenRefresh(response.Expiration.Value);
                }
                else
                {
                    StopSse();
                }

                if (response.Retry.Value)
                {
                    ScheduleNextTokenRefresh(_backOff.GetInterval());
                }
            }
            catch (Exception ex)
            {
                _log.Error($"StartSse: {ex.Message}");
            }
        }

        public void StopSse()
        {
            try
            {
                _sseHandler.Stop();
            }
            catch (Exception ex)
            {
                _log.Error($"StopSse: {ex.Message}");
            }
        }
        #endregion

        #region Private Methods
        private void ScheduleNextTokenRefresh(double time)
        {
            try
            {
                var sleepTime = Convert.ToInt32(time) * 1000;
                _log.Debug($"ScheduleNextTokenRefresh sleep time : {sleepTime} miliseconds.");

                _wrapperAdapter
                    .TaskDelay(sleepTime)
                    .ContinueWith((t) =>
                    {
                        _log.Debug("Starting ScheduleNextTokenRefresh ...");
                        StopSse();
                        StartSse();
                    });
            }
            catch (Exception ex)
            {
                _log.Error($"ScheduleNextTokenRefresh: {ex.Message}");
            }
        }
        #endregion
    }
}
