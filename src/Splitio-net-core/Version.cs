using System.Reflection;

namespace Splitio
{
    public static class Version
    {
        public static string SplitSdkVersion = typeof(Version).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        public static string SplitSpecVersion = "1.0";
    }
}
