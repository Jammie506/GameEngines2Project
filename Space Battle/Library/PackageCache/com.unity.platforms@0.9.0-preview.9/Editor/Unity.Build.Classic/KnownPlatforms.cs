using Bee.Core;
using System;
using System.Collections.Generic;

namespace Unity.Build.Classic
{
    static class KnownPlatforms
    {
        public static Dictionary<Type, string> All { get; } = new Dictionary<Type, string>
        {
            {typeof(WindowsPlatform), "com.platforms.windows"},
            {typeof(MacOSXPlatform), "com.platforms.macos"},
            {typeof(LinuxPlatform), "com.platforms.linux"},
            {typeof(IosPlatform), "com.platforms.ios"},
            {typeof(AndroidPlatform), "com.platforms.android"}
        };
    }
}
