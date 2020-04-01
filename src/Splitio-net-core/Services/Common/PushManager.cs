using Splitio.CommonLibraries;
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
        private readonly int _authRetryBackOffBase;

        public PushManager(string url,
            string apiKey,
            long connectionTimeOut,
            int authRetryBackOffBase,
            IAuthApiClient authApiClient = null,
            ISplitLogger log = null,
            IWrapperAdapter wrapperAdapter = null)
        {
            _authRetryBackOffBase = authRetryBackOffBase;

            _authApiClient = authApiClient ?? new AuthApiClient(url, apiKey, connectionTimeOut);
            _log = log ?? WrapperAdapter.GetLogger(typeof(PushManager));
            _wrapperAdapter = wrapperAdapter ?? new WrapperAdapter();
        }

        #region Public Methods
        public async void StartSse()
        {
            try
            {
                var response = await _authApiClient.AuthenticateAsync();

                if (response.PushEnabled.Value)
                {
                    // sseHandler.Start(response.Token, response.Channels);
                    ScheduleNextTokenRefresh(response.Exp.Value);
                }
                else
                {
                    // sseHandler.Stop()
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
