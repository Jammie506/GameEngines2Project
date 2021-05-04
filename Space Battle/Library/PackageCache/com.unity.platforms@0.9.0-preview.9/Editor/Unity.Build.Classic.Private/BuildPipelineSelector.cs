using Bee.Core;

#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using Unity.Build.Classic.Private.IncrementalClassicPipeline;
#endif

namespace Unity.Build.Classic.Private
{
    class BuildPipelineSelector : BuildPipelineSelectorBase
    {
        internal static bool IsBuildPipelineValid(ClassicPipelineBase pipeline, Platform platform)
        {
            var namezpace = pipeline.GetType().Namespace;

            if (string.IsNullOrEmpty(namezpace))
                return false;

            return pipeline.Platform.GetType() == platform.GetType() &&
                   namezpace.StartsWith("Unity.Build.") &&
                   (namezpace.EndsWith(".Classic") || namezpace.EndsWith(".Classic.Private.MissingPipelines")) &&
                   !namezpace.Contains("Test");
        }

        public override BuildPipelineBase SelectFor(Platform platform, bool incremental)
        {
            if (platform == null)
            {
                return null;
            }
#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
            if (incremental)
            {
                return TypeCacheHelper.ConstructTypeDerivedFrom<ClassicIncrementalPipelineBase>(p => IsBuildPipelineValid(p, platform));
            }
            else
#endif
            {
                return TypeCacheHelper.ConstructTypeDerivedFrom<ClassicNonIncrementalPipelineBase>(p => IsBuildPipelineValid(p, platform));
            }
        }
    }
}
