using System;
using System.Linq;

namespace Unity.Build
{
    /// <summary>
    /// Can stores a set of hierarchical build components per type, which can be inherited or overridden using dependencies.
    /// </summary>
    public sealed class BuildConfiguration : HierarchicalComponentContainer<BuildConfiguration, IBuildComponent>
    {
        /// <summary>
        /// File extension for build configuration assets.
        /// </summary>
        public const string AssetExtension = ".buildconfiguration";

        /// <summary>
        /// Retrieve the build pipeline of this build configuration.
        /// </summary>
        /// <returns>The build pipeline if found, otherwise <see langword="null"/>.</returns>
        public BuildPipelineBase GetBuildPipeline() => TryGetComponent<IBuildPipelineComponent>(out var component) ? component.Pipeline : null;

        /// <summary>
        /// Determine if component is used by the build pipeline of this build configuration.
        /// Returns <see langword="false"/> if this build configuration does not have a build pipeline.
        /// </summary>
        /// <param name="type">The component type.</param>
        /// <returns><see langword="true"/> if the component is used by the build pipeline, <see langword="false"/> otherwise.</returns>
        public bool IsComponentUsed(Type type) => GetBuildPipeline()?.IsComponentUsed(type) ?? false;

        /// <summary>
        /// Determine if component is used by the build pipeline of this build configuration.
        /// Returns <see langword="false"/> if this build configuration does not have a build pipeline.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns><see langword="true"/> if the component is used by the build pipeline, <see langword="false"/> otherwise.</returns>
        public bool IsComponentUsed<T>() where T : IBuildComponent => IsComponentUsed(typeof(T));

        /// <summary>
        /// Determine if the build pipeline of this build configuration can build.
        /// </summary>
        /// <returns>A result describing if the pipeline can build or not.</returns>
        public BoolResult CanBuild()
        {
            var pipeline = GetBuildPipeline();
            var canUse = CanUsePipeline(pipeline);
            return canUse.Result ? pipeline.CanBuild(this) : canUse;
        }

        /// <summary>
        /// Run the build pipeline of this build configuration to build the target.
        /// </summary>
        /// <returns>The result of the build pipeline build.</returns>
        public BuildResult Build()
        {
            var pipeline = GetBuildPipeline();
            var canUse = CanUsePipeline(pipeline);
            if (!canUse.Result)
            {
                return BuildResult.Failure(pipeline, this, canUse.Reason);
            }

            var what = !string.IsNullOrEmpty(name) ? $" {name}" : string.Empty;
            using (var progress = new BuildProgress($"Building{what}", "Please wait..."))
            {
                return pipeline.Build(this, progress);
            }
        }

        /// <summary>
        /// Determine if the build pipeline of this build configuration can run.
        /// </summary>
        /// <param name="runTargets">List of run targets to deploy and run on.</param>
        /// <returns>A result describing if the pipeline can run or not.</returns>
        public BoolResult CanRun(params RunTargetBase[] runTargets)
        {
            var pipeline = GetBuildPipeline();
            var canUse = CanUsePipeline(pipeline);
            return canUse.Result ? pipeline.CanRun(this, runTargets) : canUse;
        }

        /// <summary>
        /// Run the resulting target from building the build pipeline of this build configuration.
        /// </summary>
        /// <param name="runTargets">List of run targets to deploy and run on.</param>
        /// <returns></returns>
        public RunResult Run(params RunTargetBase[] runTargets)
        {
            var pipeline = GetBuildPipeline();
            var canUse = CanUsePipeline(pipeline);
            return canUse.Result ? pipeline.Run(this, runTargets) : RunResult.Failure(pipeline, this, canUse.Reason);
        }

        /// <summary>
        /// Clean the build result from building the build pipeline of this build configuration.
        /// </summary>
        public CleanResult Clean()
        {
            var pipeline = GetBuildPipeline();
            var canUse = CanUsePipeline(pipeline);
            return canUse.Result ? pipeline.Clean(this) : CleanResult.Failure(pipeline, this, canUse.Reason);
        }

        /// <summary>
        /// Get the value of the first build artifact that is assignable to type <see cref="Type"/>.
        /// </summary>
        /// <param name="config">The build configuration that was used to store the build artifact.</param>
        /// <param name="type">The type of the build artifact.</param>
        /// <returns>The build artifact if found, otherwise <see langword="null"/>.</returns>
        public IBuildArtifact GetLastBuildArtifact(Type type) => BuildArtifacts.GetBuildArtifact(this, type);

        /// <summary>
        /// Get the value of the first build artifact that is assignable to type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the build artifact.</typeparam>
        /// <param name="config">The build configuration that was used to store the build artifact.</param>
        /// <returns>The build artifact if found, otherwise <see langword="null"/>.</returns>
        public T GetLastBuildArtifact<T>() where T : class, IBuildArtifact => BuildArtifacts.GetBuildArtifact<T>(this);

        /// <summary>
        /// Get the last build result for this build configuration.
        /// </summary>
        /// <param name="config">The build configuration that was used to store the build artifact.</param>
        /// <returns>The build result if found, otherwise <see langword="null"/>.</returns>
        public BuildResult GetLastBuildResult() => BuildArtifacts.GetBuildResult(this);

        /// <summary>
        /// Get the output build directory override for this build configuration.
        /// The output build directory can be overridden using a <see cref="OutputBuildDirectory"/> component.
        /// </summary>
        /// <returns>The output build directory.</returns>
        public string GetOutputBuildDirectory()
        {
            var pipeline = GetBuildPipeline();
            if (pipeline == null)
                throw new NullReferenceException("The BuildConfiguration must have a BuildPipline in order to retrieve the OutputBuildDirectory");

            return pipeline.GetOutputBuildDirectory(this).ToString();
        }

        protected override void OnComponentConstruct(ref IBuildComponent component)
        {
            if (component is ICustomBuildComponentConstructor constructible)
            {
                constructible.Construct(AsReadOnly());
            }
        }

        BoolResult CanUsePipeline(BuildPipelineBase pipeline)
        {
            if (pipeline == null)
            {
                return BoolResult.False($"No valid build pipeline found for {this.ToHyperLink()}. At least one component that derives from {nameof(IBuildPipelineComponent)} must be present.");
            }
            return BoolResult.True();
        }
    }

    public static class BuildConfigurationReadOnlyExtensions
    {
        /// <summary>
        /// Retrieve the build pipeline of this build configuration.
        /// </summary>
        /// <returns>The build pipeline if found, otherwise <see langword="null"/>.</returns>
        static public BuildPipelineBase GetBuildPipeline(this BuildConfiguration.ReadOnly config) => config.TryGetComponent<IBuildPipelineComponent>(out var component) ? component.Pipeline : null;
    }
}
