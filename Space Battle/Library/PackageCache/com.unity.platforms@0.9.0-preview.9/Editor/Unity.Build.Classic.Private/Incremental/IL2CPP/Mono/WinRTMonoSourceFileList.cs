#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using NiceIO;
using System.Collections.Generic;
using Bee.NativeProgramSupport;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    public static class WinRTMonoSourceFileList
    {
        public static NPath[] GetEGLibSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return new[] { MonoSourceDir.Combine("mono/eglib/gunicode-win32-uwp.c") };
        }

        public static NPath[] GetMetadataSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return WindowsSharedMonoSourceFileList.GetMetadataSourceFiles(npc, MonoSourceDir);
        }

        public static NPath[] GetUtilsSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            var files = new List<NPath>();

            files.AddRange(WindowsSharedMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir));
            files.Add(MonoSourceDir.Combine("mono/utils/mono-dl-windows-uwp.c"));

            return files.ToArray();
        }
    }
}
#endif
