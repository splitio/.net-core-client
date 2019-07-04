using System.Reflection;
using System.Diagnostics;

namespace Splitio
{
    public static class Version
    {
#if NETSTANDARD
        public static string SplitSdkVersion = typeof(Version).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
#else
        public static string SplitSdkVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
#endif
        public static string SplitSpecVersion = "1.0";
    }
}
