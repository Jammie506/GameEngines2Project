using UnityEngine;

namespace Unity.Build.Editor
{
    static class Resources
    {
        // UI Templates
        public static UITemplate BuildConfiguration = new UITemplate("build-configuration");
        public static UITemplate BuildConfigurationDependency = new UITemplate("build-configuration-dependency");
        public static UITemplate BuildComponent = new UITemplate("build-component");
        public static UITemplate ClassicBuildProfile = new UITemplate("classic-build-profile");
        public static UITemplate TypeInspector = new UITemplate("type-inspector");

        // UI Icons
        public static Texture2D BuildComponentIcon = UIIcon.LoadPackageIcon("Component");
        public static Texture2D PlatformStandloneIcon = UIIcon.LoadIcon("Icons", "BuildSettings.Standalone");
        public static Texture2D PlatformAndroidIcon = UIIcon.LoadIcon("Icons", "BuildSettings.Android");
        public static Texture2D PlatformIOSIcon = UIIcon.LoadIcon("Icons", "BuildSettings.iPhone");
        public static Texture2D PlatformPS4Icon = UIIcon.LoadIcon("Icons", "BuildSettings.PS4");
        public static Texture2D PlatformSwitchIcon = UIIcon.LoadIcon("Icons", "BuildSettings.Switch");
        public static Texture2D PlatformTVOSIcon = UIIcon.LoadIcon("Icons", "BuildSettings.tvOS");
        public static Texture2D PlatformUWPIcon = UIIcon.LoadIcon("Icons", "BuildSettings.Metro");
        public static Texture2D PlatformWebGLIcon = UIIcon.LoadIcon("Icons", "BuildSettings.WebGL");
        public static Texture2D PlatformXBoxOneIcon = UIIcon.LoadIcon("Icons", "BuildSettings.XboxOne");
        public static Texture2D PlatformStadiaIcon = UIIcon.LoadIcon("Icons", "BuildSettings.Stadia");
    }
}
