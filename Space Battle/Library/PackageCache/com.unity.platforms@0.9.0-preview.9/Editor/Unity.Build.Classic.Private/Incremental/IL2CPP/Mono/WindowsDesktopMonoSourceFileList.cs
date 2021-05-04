#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using Bee.NativeProgramSupport;
using NiceIO;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    public static class WindowsDesktopMonoSourceFileList
    {
        public static NPath[] GetEGLibSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return new[] { MonoSourceDir.Combine("mono/eglib/gunicode-win32.c") };
        }

        public static NPath[] GetMetadataSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return WindowsSharedMonoSourceFileList.GetMetadataSourceFiles(npc, MonoSourceDir);
        }

        public static NPath[] GetUtilsSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return WindowsSharedMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir);
        }
    }
}
#endif
