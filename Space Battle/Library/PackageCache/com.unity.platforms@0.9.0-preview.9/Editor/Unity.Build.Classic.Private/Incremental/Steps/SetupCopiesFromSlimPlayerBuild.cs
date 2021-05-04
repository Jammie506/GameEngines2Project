#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using Bee.Core;
using NiceIO;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    public class SetupCopiesFromSlimPlayerBuild : BuildStepBase
    {
        public override BuildResult Run(BuildContext context)
        {
            CopyAllFilesFrom(SlimBuildPipeline.TempDataPath, context.GetValue<IncrementalClassicSharedData>().DataDeployDirectory);
            CopyAllFilesFrom(SlimBuildPipeline.TempStreamingAssetsPath, context.GetValue<ClassicSharedData>().StreamingAssetsDirectory);

            return context.Success();
        }

        private static void CopyAllFilesFrom(NPath fromPath, NPath toPath)
        {
            foreach (var file in fromPath.Files(true))
                CopyTool.Instance().Setup(
                    toPath.Combine(file.RelativeTo(fromPath)).MakeAbsolute(),
                    file.MakeAbsolute());
        }
    }
}
#endif
