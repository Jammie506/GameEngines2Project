#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using Bee.NativeProgramSupport;
using NiceIO;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    public static class LinuxDebuggerMonoSourceFileList
    {
        public static NPath[] GetMetadataDebuggerSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return UnityMonoSourceFileList.GetMetadataDebuggerSourceFiles(npc, MonoSourceDir);
        }

        public static NPath[] GetUtilsSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return UnityMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir);
        }
    }
}
#endif
