#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using Bee.NativeProgramSupport;
using NiceIO;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    public static class WindowsSharedMonoSourceFileList
    {
        public static NPath[] GetMetadataSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return new[]
            {
                MonoSourceDir.Combine("mono/metadata/console-win32.c"),
                MonoSourceDir.Combine("mono/metadata/w32error-win32.c"),
                MonoSourceDir.Combine("mono/metadata/w32event-win32.c"),
                MonoSourceDir.Combine("mono/metadata/w32file-win32.c"),
                MonoSourceDir.Combine("mono/metadata/w32mutex-win32.c"),
                MonoSourceDir.Combine("mono/metadata/w32process-win32.c"),
                MonoSourceDir.Combine("mono/metadata/w32semaphore-win32.c"),
                MonoSourceDir.Combine("mono/metadata/w32socket-win32.c")
            };
        }

        public static NPath[] GetUtilsSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return new[]
            {
                MonoSourceDir.Combine("mono/utils/mono-dl-windows.c"),
                MonoSourceDir.Combine("mono/utils/mono-log-windows.c"),
                MonoSourceDir.Combine("mono/utils/mono-os-wait-win32.c"),
                MonoSourceDir.Combine("mono/utils/mono-threads-windows.c"),
                MonoSourceDir.Combine("mono/utils/os-event-win32.c"),
                MonoSourceDir.Combine("mono/utils/networking-posix.c"),
                MonoSourceDir.Combine("mono/utils/networking-windows.c")
            };
        }
    }
}
#endif
