using System.Reflection;

namespace Splitio
{
    public static class Version
    {
#if net40
        public static string SplitSdkVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
#endif

#if NETSTANDARD
        public static string SplitSdkVersion = typeof(Version).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
#endif
        public static string SplitSpecVersion = "1.0";
    }
}
