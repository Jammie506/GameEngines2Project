namespace Unity.Build
{
    /// <summary>
    /// Base interface for custom build component constructor.
    /// </summary>
    internal interface ICustomBuildComponentConstructor
    {
        void Construct(BuildConfiguration.ReadOnly config);
    }
}
