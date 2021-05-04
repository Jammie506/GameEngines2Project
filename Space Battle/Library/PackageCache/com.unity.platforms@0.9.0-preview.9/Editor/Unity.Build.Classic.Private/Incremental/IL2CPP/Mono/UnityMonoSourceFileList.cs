#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using Bee.NativeProgramSupport;
using NiceIO;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    public static class UnityMonoSourceFileList
    {
        public static NPath[] GetUtilsSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return new[]
            {
                MonoSourceDir.Combine("mono/utils/mono-dl-unity.c"),
                MonoSourceDir.Combine("mono/utils/mono-log-unity.c"),
                MonoSourceDir.Combine("mono/utils/mono-threads-unity.c"),
                MonoSourceDir.Combine("mono/utils/networking-unity.c"),
                MonoSourceDir.Combine("mono/utils/os-event-unity.c")
            };
        }

        public static NPath[] GetMetadataDebuggerSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return new[]
            {
                MonoSourceDir.Combine("mono/metadata/console-unity.c"),
                MonoSourceDir.Combine("mono/metadata/file-mmap-unity.c"),
                MonoSourceDir.Combine("mono/metadata/w32error-unity.c"),
                MonoSourceDir.Combine("mono/metadata/w32event-unity.c"),
                MonoSourceDir.Combine("mono/metadata/w32file-unity.c"),
                MonoSourceDir.Combine("mono/metadata/w32mutex-unity.c"),
                MonoSourceDir.Combine("mono/metadata/w32process-unity.c"),
                MonoSourceDir.Combine("mono/metadata/w32semaphore-unity.c"),
                MonoSourceDir.Combine("mono/metadata/w32socket-unity.c")
            };
        }
    }
}
#endif
