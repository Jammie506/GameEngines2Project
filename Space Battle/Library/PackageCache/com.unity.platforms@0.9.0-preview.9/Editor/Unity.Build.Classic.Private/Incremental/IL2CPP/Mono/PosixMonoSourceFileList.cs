#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using Bee.NativeProgramSupport;
using NiceIO;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    public class PosixMonoSourceFileList
    {
        public static NPath[] GetMetadataSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return new[]
            {
                MonoSourceDir.Combine("mono/metadata/console-unix.c"),
                MonoSourceDir.Combine("mono/metadata/mono-route.c"),
                MonoSourceDir.Combine("mono/metadata/w32error-unix.c"),
                MonoSourceDir.Combine("mono/metadata/w32event-unix.c"),
                MonoSourceDir.Combine("mono/metadata/w32file-unix-glob.c"),
                MonoSourceDir.Combine("mono/metadata/w32file-unix.c"),
                MonoSourceDir.Combine("mono/metadata/w32mutex-unix.c"),
                MonoSourceDir.Combine("mono/metadata/w32process-unix.c"),
                MonoSourceDir.Combine("mono/metadata/w32semaphore-unix.c"),
                MonoSourceDir.Combine("mono/metadata/w32socket-unix.c")
            };
        }

        public static NPath[] GetUtilsSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return new[]
            {
                MonoSourceDir.Combine("mono/utils/mono-dl-posix.c"),
                MonoSourceDir.Combine("mono/utils/mono-log-posix.c"),
                MonoSourceDir.Combine("mono/utils/mono-threads-posix.c"),
                MonoSourceDir.Combine("mono/utils/mono-threads-posix-signals.c"),
                MonoSourceDir.Combine("mono/utils/os-event-unix.c"),
                MonoSourceDir.Combine("mono/utils/networking-posix.c")
            };
        }
    }
}
#endif
