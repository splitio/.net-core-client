using Splitio.Domain;
using Splitio.Services.Client.Classes;
using Splitio.Services.Logger;
using Splitio.Services.Shared.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace Splitio.Services.Shared.Classes
{
    public class WrapperAdapter : IWrapperAdapter
    {
        public ReadConfigData ReadConfig(ConfigurationOptions config, ISplitLogger log)
        {
            var data = new ReadConfigData();

            try
            {
                data.SdkMachineName = config.SdkMachineName ?? Environment.MachineName;
            }
            catch (Exception e)
            {
                data.SdkMachineName = "unknown";
                log.Warn("Exception retrieving machine name.", e);
            }

            data.SdkSpecVersion = ".NET-" + SplitSpecVersion();

            try
            {
#if NETSTANDARD
                data.SdkVersion = ".NET_CORE-" + SplitSdkVersion();                

                var hostAddressesTask = Dns.GetHostAddressesAsync(Environment.MachineName);
                hostAddressesTask.Wait();
                data.SdkMachineIP = config.SdkMachineIP ?? hostAddressesTask.Result.Where(x => x.AddressFamily == AddressFamily.InterNetwork && x.IsIPv6LinkLocal == false).Last().ToString();
#else
                data.SdkVersion = ".NET-" + SplitSdkVersion();

                data.SdkMachineIP = config.SdkMachineIP ?? Dns.GetHostAddresses(Environment.MachineName).Where(x => x.AddressFamily == AddressFamily.InterNetwork && x.IsIPv6LinkLocal == false).Last().ToString();
#endif
            }
            catch (Exception e)
            {
                data.SdkMachineIP = "unknown";
                log.Warn("Exception retrieving machine IP.", e);
            }

            return data;
        }

        public Task TaskDelay(int millisecondsDelay)
        {
#if NETSTANDARD || NET45
            return Task.Delay(millisecondsDelay);
#else
            return TaskEx.Delay(millisecondsDelay);
#endif
        }

        public async Task<T> TaskFromResult<T>(T result)
        {
#if NETSTANDARD || NET45
            return await Task.FromResult(result);
#else
            return await TaskEx.FromResult(result);
#endif
        }

        private string SplitSdkVersion()
        {
#if NETSTANDARD
            return typeof(Split).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
#else
            return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
#endif
        }

        public static ISplitLogger GetLogger(Type type)
        {
#if NETSTANDARD
            return new MicrosoftExtensionsLogging(type);
#else
            return new CommonLogging(type);
#endif
        }

        public static ISplitLogger GetLogger(string type)
        {
#if NETSTANDARD
            return new MicrosoftExtensionsLogging(type);
#else
            return new CommonLogging(type);
#endif
        }

        private string SplitSpecVersion()
        {
            return "1.0";
        }
    }
}
