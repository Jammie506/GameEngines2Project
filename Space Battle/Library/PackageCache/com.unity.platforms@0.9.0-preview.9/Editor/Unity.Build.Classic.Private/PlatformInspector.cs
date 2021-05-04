using Bee.Core;
using System;
using System.Linq;
using Unity.Build.Editor;
using Unity.Properties.Editor;
using UnityEngine;
using Resources = Unity.Build.Editor.Resources;

namespace Unity.Build.Classic.Private
{
    sealed class PlatformInspector : TypeInspector<Platform>
    {
        public override string Title => "Platform";
        public override Func<Type, bool> TypeFilter => type =>
        {
            // If there is a pipeline that supports this platform, we want the platform to appear in the list.
            if (TypeCacheHelper.ConstructTypesDerivedFrom<ClassicPipelineBase>().Any(pipeline => pipeline.Platform.GetType() == type))
                return true;

            // If there is not, but it is a known common platform, we also want it in the list, so we have a way
            // to inform users that they have to install a package to build for that platform.
            return KnownPlatforms.All.ContainsKey(type);
        };
        public override Func<Type, string> TypeName => type => TypeConstruction.Construct<Platform>(type).DisplayName;
        public override Func<Type, string> TypeCategory => type => string.Empty;
        public override Func<Type, Texture2D> TypeIcon => type =>
        {
            if (type == typeof(WindowsPlatform) || type == typeof(MacOSXPlatform) || type == typeof(LinuxPlatform))
            {
                return Resources.PlatformStandloneIcon;
            }
            else if (type == typeof(AndroidPlatform))
            {
                return Resources.PlatformAndroidIcon;
            }
            else if (type == typeof(IosPlatform))
            {
                return Resources.PlatformIOSIcon;
            }
            else if (type == typeof(PS4Platform))
            {
                return Resources.PlatformPS4Icon;
            }
            else if (type == typeof(SwitchPlatform))
            {
                return Resources.PlatformSwitchIcon;
            }
            else if (type == typeof(TvosPlatform))
            {
                return Resources.PlatformTVOSIcon;
            }
            else if (type == typeof(UniversalWindowsPlatform))
            {
                return Resources.PlatformUWPIcon;
            }
            else if (type == typeof(WebGLPlatform))
            {
                return Resources.PlatformWebGLIcon;
            }
            else if (type == typeof(XboxOnePlatform))
            {
                return Resources.PlatformXBoxOneIcon;
            }
            else if (type == typeof(StadiaPlatform))
            {
                return Resources.PlatformStadiaIcon;
            }
            return null;
        };
    }
}
