#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using NiceIO;
using System.Collections.Generic;
using Bee.Core;
using Bee.NativeProgramSupport;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    public class OSXMonoSourceFileList
    {
        public static NPath[] GetMetadataSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            var files = new List<NPath>();

            files.AddRange(PosixMonoSourceFileList.GetMetadataSourceFiles(npc, MonoSourceDir));
            files.Add(MonoSourceDir.Combine("mono/metadata/w32process-unix-osx.c"));

            return files.ToArray();
        }

        public static NPath[] GetUtilsSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            var files = new List<NPath>();

            files.AddRange(PosixMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir));
            files.AddRange(new[]
            {
                MonoSourceDir.Combine("mono/utils/mach-support.c"),
                MonoSourceDir.Combine("mono/utils/mono-dl-darwin.c"),
                MonoSourceDir.Combine("mono/utils/mono-log-darwin.c"),
                MonoSourceDir.Combine("mono/utils/mono-threads-mach.c"),
                MonoSourceDir.Combine("mono/utils/mono-threads-mach-helper.c")
            });

            if (npc.ToolChain.Architecture is x64Architecture)
                files.Add(MonoSourceDir.Combine("mono/utils/mach-support-amd64.c"));

            if (npc.ToolChain.Architecture is x86Architecture)
                files.Add(MonoSourceDir.Combine("mono/utils/mach-support-x86.c"));

            return files.ToArray();
        }
    }
}
#endif
