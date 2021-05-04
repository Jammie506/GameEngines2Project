#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using Bee.Core;
using NiceIO;
using System.Collections.Generic;
using System.IO;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    static class IncrementalPipelinePrepareAdditionallyProvidedFiles
    {
        public static void Prepare(BuildContext context)
        {
            var customizers = context.GetValue<ClassicSharedData>().Customizers;

            var pairs = new AdditionallyProvidedFiles() { files = new List<(NPath sourceFile, NPath targetFile)>() };
            foreach (var customizer in customizers)
                customizer.RegisterAdditionalFilesToDeploy((from, to) => pairs.files.Add((new NPath(Path.GetFullPath(from)).MakeAbsolute(), new NPath(Path.GetFullPath(to)).MakeAbsolute())));
            context.SetValue(pairs);
        }
    }

    class AdditionallyProvidedFiles
    {
        public List<(NPath sourceFile, NPath targetFile)> files;
    }

    public class SetupAdditionallyProvidedFiles : BuildStepBase
    {
        public override BuildResult Run(BuildContext context)
        {
            var files = context.GetValue<AdditionallyProvidedFiles>().files;
            foreach (var entry in files)
                CopyTool.Instance().Setup(entry.targetFile, entry.sourceFile);
            return context.Success();
        }
    }
}
#endif
