#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using Bee.NativeProgramSupport;
using NiceIO;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    public static class WindowsGamesMonoSourceFileList
    {
        public static NPath[] GetEGLibSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return WindowsDesktopMonoSourceFileList.GetEGLibSourceFiles(npc, MonoSourceDir);
        }

        public static NPath[] GetMetadataSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return WindowsDesktopMonoSourceFileList.GetMetadataSourceFiles(npc, MonoSourceDir);
        }

        public static NPath[] GetUtilsSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return WindowsDesktopMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir);
        }
    }
}
#endif
