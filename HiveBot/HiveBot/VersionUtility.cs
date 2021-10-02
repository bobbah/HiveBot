using System;
using System.Reflection;

namespace HiveBot
{
    public static class VersionUtility
    {
        private static Version ExecutingVersion => Assembly.GetExecutingAssembly().GetName().Version;
        private static string VersionNumber => ExecutingVersion.ToString(ExecutingVersion.Revision == 0 ? 3 : 4);
        public static string Version => $"HiveBot v{VersionNumber}";
    }
}