#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using NiceIO;
using System.Collections.Generic;
using Bee.NativeProgramSupport;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    public static class LinuxMonoSourceFileList
    {
        public static NPath[] GetMetadataSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            var files = new List<NPath>();

            files.AddRange(PosixMonoSourceFileList.GetMetadataSourceFiles(npc, MonoSourceDir));
            files.Add(MonoSourceDir.Combine("mono/metadata/w32process-unix-default.c"));
            files.Add(MonoSourceDir.Combine("support/libm/complex.c"));

            return files.ToArray();
        }

        public static NPath[] GetUtilsSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return PosixMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir);
        }
    }
}
#endif
