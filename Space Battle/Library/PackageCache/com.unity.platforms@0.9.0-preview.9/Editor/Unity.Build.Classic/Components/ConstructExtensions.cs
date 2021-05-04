using Bee.Core;
using UnityEditor;

namespace Unity.Build.Classic
{
    internal static class ConstructExtensions
    {
        public static BuildTargetGroup GetBuildTargetGroup(this BuildConfiguration.ReadOnly config)
        {
            if (!config.TryGetComponent<IBuildPipelineComponent>(out var value))
                return BuildTargetGroup.Unknown;

            var profile = value as ClassicBuildProfile;
            if (profile == null)
                return BuildTargetGroup.Unknown;

            if (profile.Platform is WindowsPlatform ||
                profile.Platform is MacOSXPlatform ||
                profile.Platform is LinuxPlatform)
                return BuildTargetGroup.Standalone;
            if (profile.Platform is UniversalWindowsPlatform)
                return BuildTargetGroup.WSA;
            if (profile.Platform is AndroidPlatform)
                return BuildTargetGroup.Android;
            if (profile.Platform is IosPlatform)
                return BuildTargetGroup.iOS;
            if (profile.Platform is TvosPlatform)
                return BuildTargetGroup.tvOS;
            if (profile.Platform is WebGLPlatform)
                return BuildTargetGroup.WebGL;
            if (profile.Platform is PS4Platform)
                return BuildTargetGroup.PS4;
            if (profile.Platform is SwitchPlatform)
                return BuildTargetGroup.Switch;

            return BuildTargetGroup.Unknown;
        }
    }
}
