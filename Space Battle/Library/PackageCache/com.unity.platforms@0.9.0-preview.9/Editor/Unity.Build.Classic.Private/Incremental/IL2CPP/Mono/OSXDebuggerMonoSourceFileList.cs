#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using NiceIO;
using System.Collections.Generic;
using Bee.NativeProgramSupport;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    public static class OSXDebuggerMonoSourceFileList
    {
        public static NPath[] GetMetadataDebuggerSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return UnityMonoSourceFileList.GetMetadataDebuggerSourceFiles(npc, MonoSourceDir);
        }

        public static NPath[] GetUtilsSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            var files = new List<NPath>();

            files.AddRange(UnityMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir));
            files.Add(MonoSourceDir.Combine("mono/utils/mach-support-amd64.c"));

            return files.ToArray();
        }
    }
}
#endif
