using System.Text.RegularExpressions;
using Unity.Properties;
using Unity.Serialization;
using UnityEditor;

namespace Unity.Build.Common
{
    [FormerName("Unity.Platforms.Android.Build.ApplicationIdentifier, Unity.Platforms.Android.Build")]
    [FormerName("Unity.Build.Android.ApplicationIdentifier, Unity.Build.Android")]
    [FormerName("Unity.Build.iOS.BundleIdentifier, Unity.Build.iOS")]
    public sealed partial class ApplicationIdentifier : IBuildComponent, ICustomBuildComponentConstructor
    {
        string SanitizeIdentifier(string id)
        {
            return Regex.Replace(id, "[^A-Za-z0-9]", "");
        }

        void ICustomBuildComponentConstructor.Construct(BuildConfiguration.ReadOnly config)
        {
            var generalSettings = config.GetComponentOrDefault<GeneralSettings>();
            m_PackageName = $"com.{SanitizeIdentifier(generalSettings.CompanyName)}.{SanitizeIdentifier(generalSettings.ProductName)}";
        }
    }
}
