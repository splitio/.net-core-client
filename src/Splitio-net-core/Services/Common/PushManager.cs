using Splitio.Services.EventSource;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Classes;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Threading.Tasks;

namespace Splitio.Services.Common
{
    public class PushManager : IPushManager
    {
        private readonly IAuthApiClient _authApiClient;
        private readonly ISplitLogger _log;
        private readonly IWrapperAdapter _wrapperAdapter;
        private readonly ISSEHandler _sseHandler;
        private readonly int _authRetryBackOffBase;

        public PushManager(int authRetryBackOffBase,
            ISSEHandler sseHandler,
            IAuthApiClient authApiClient,
            ISplitLogger log = null,
            IWrapperAdapter wrapperAdapter = null)
        {
            _sseHandler = sseHandler;
            _authApiClient = authApiClient;
            _log = log ?? WrapperAdapter.GetLogger(typeof(PushManager));
            _wrapperAdapter = wrapperAdapter ?? new WrapperAdapter();
            _authRetryBackOffBase = authRetryBackOffBase;
        }

        #region Public Methods
        public async void StartSse()
        {
            try
            {
                var response = await _authApiClient.AuthenticateAsync();

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
                    ScheduleNextTokenRefresh(_authRetryBackOffBase);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
        }

        public void StopSse()
        {
            _sseHandler.Stop();
        }
        #endregion

        #region Private Methods
        private void ScheduleNextTokenRefresh(double time)
        {
            Task
                .Factory
                .StartNew(() => 
                {
                    _wrapperAdapter.TaskDelay(Convert.ToInt32(time)).Wait();

                    StartSse();
                });
        }
        #endregion
    }
}
