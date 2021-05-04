#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using NiceIO;
using System.Collections.Generic;
using Bee.Core;
using Bee.NativeProgramSupport;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    public static class MonoSourceFileList
    {
        public static NPath[] GetEGLibSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir, bool managedDebuggingEnabled)
        {
            var files = new List<NPath>
            {
                MonoSourceDir.Combine("mono/eglib/garray.c"),
                MonoSourceDir.Combine("mono/eglib/gbytearray.c"),
                MonoSourceDir.Combine("mono/eglib/gdate-unity.c"),
                MonoSourceDir.Combine("mono/eglib/gdir-unity.c"),
                MonoSourceDir.Combine("mono/eglib/gerror.c"),
                MonoSourceDir.Combine("mono/eglib/gfile-unity.c"),
                MonoSourceDir.Combine("mono/eglib/gfile.c"),
                MonoSourceDir.Combine("mono/eglib/ghashtable.c"),
                MonoSourceDir.Combine("mono/eglib/giconv.c"),
                MonoSourceDir.Combine("mono/eglib/glist.c"),
                MonoSourceDir.Combine("mono/eglib/gmarkup.c"),
                MonoSourceDir.Combine("mono/eglib/gmem.c"),
                MonoSourceDir.Combine("mono/eglib/gmisc-unity.c"),
                MonoSourceDir.Combine("mono/eglib/goutput.c"),
                MonoSourceDir.Combine("mono/eglib/gpath.c"),
                MonoSourceDir.Combine("mono/eglib/gpattern.c"),
                MonoSourceDir.Combine("mono/eglib/gptrarray.c"),
                MonoSourceDir.Combine("mono/eglib/gqsort.c"),
                MonoSourceDir.Combine("mono/eglib/gqueue.c"),
                MonoSourceDir.Combine("mono/eglib/gshell.c"),
                MonoSourceDir.Combine("mono/eglib/gslist.c"),
                MonoSourceDir.Combine("mono/eglib/gspawn.c"),
                MonoSourceDir.Combine("mono/eglib/gstr.c"),
                MonoSourceDir.Combine("mono/eglib/gstring.c"),
                MonoSourceDir.Combine("mono/eglib/gunicode.c"),
                MonoSourceDir.Combine("mono/eglib/gutf8.c")
            };

            if (managedDebuggingEnabled)
            {
                if (npc.ToolChain.Platform is WindowsPlatform)
                    files.AddRange(WindowsDebuggerMonoSourceFileList.GetEGLibSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is UniversalWindowsPlatform)
                    files.AddRange(WinRTDebuggerMonoSourceFileList.GetEGLibSourceFiles(npc, MonoSourceDir));
            }
            else
            {
                if (npc.ToolChain.Platform is WindowsPlatform)
                    files.AddRange(WindowsDesktopMonoSourceFileList.GetEGLibSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is WindowsGamesPlatform)
                    files.AddRange(WindowsGamesMonoSourceFileList.GetEGLibSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is UniversalWindowsPlatform)
                    files.AddRange(WinRTMonoSourceFileList.GetEGLibSourceFiles(npc, MonoSourceDir));
            }

            return files.ToArray();
        }

        public static NPath[] GetMetadataSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir, bool managedDebuggingEnabled)
        {
            var files = new List<NPath>();

            if (managedDebuggingEnabled)
            {
                files.Add(MonoSourceDir.Combine("mono/metadata/mono-hash.c"));
                files.Add(MonoSourceDir.Combine("mono/metadata/profiler.c"));

                if (npc.ToolChain.Platform is WindowsPlatform)
                    files.AddRange(WindowsDebuggerMonoSourceFileList.GetMetadataDebuggerSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is UniversalWindowsPlatform)
                    files.AddRange(WinRTDebuggerMonoSourceFileList.GetMetadataDebuggerSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is AndroidPlatform)
                    files.AddRange(AndroidDebuggerMonoSourceFileList.GetMetadataDebuggerSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is IosPlatform)
                    files.AddRange(iOSDebuggerMonoSourceFileList.GetMetadataDebuggerSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is LinuxPlatform)
                    files.AddRange(LinuxDebuggerMonoSourceFileList.GetMetadataDebuggerSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is MacOSXPlatform)
                    files.AddRange(OSXDebuggerMonoSourceFileList.GetMetadataDebuggerSourceFiles(npc, MonoSourceDir));
            }
            else
            {
                files.AddRange(new[]
                {
                    MonoSourceDir.Combine("mono/metadata/appdomain.c"),
                    MonoSourceDir.Combine("mono/metadata/assembly.c"),
                    MonoSourceDir.Combine("mono/metadata/attach.c"),
                    MonoSourceDir.Combine("mono/metadata/boehm-gc.c"),
                    MonoSourceDir.Combine("mono/metadata/class-accessors.c"),
                    MonoSourceDir.Combine("mono/metadata/class.c"),
                    MonoSourceDir.Combine("mono/metadata/cominterop.c"),
                    MonoSourceDir.Combine("mono/metadata/coree.c"),
                    MonoSourceDir.Combine("mono/metadata/custom-attrs.c"),
                    MonoSourceDir.Combine("mono/metadata/debug-helpers.c"),
                    MonoSourceDir.Combine("mono/metadata/debug-mono-ppdb.c"),
                    MonoSourceDir.Combine("mono/metadata/debug-mono-symfile.c"),
                    MonoSourceDir.Combine("mono/metadata/decimal-ms.c"),
                    MonoSourceDir.Combine("mono/metadata/domain.c"),
                    MonoSourceDir.Combine("mono/metadata/dynamic-image.c"),
                    MonoSourceDir.Combine("mono/metadata/dynamic-stream.c"),
                    MonoSourceDir.Combine("mono/metadata/environment.c"),
                    MonoSourceDir.Combine("mono/metadata/exception.c"),
                    MonoSourceDir.Combine("mono/metadata/fdhandle.c"),
                    MonoSourceDir.Combine("mono/metadata/file-mmap-posix.c"),
                    MonoSourceDir.Combine("mono/metadata/file-mmap-windows.c"),
                    MonoSourceDir.Combine("mono/metadata/filewatcher.c"),
                    MonoSourceDir.Combine("mono/metadata/gc-stats.c"),
                    MonoSourceDir.Combine("mono/metadata/gc.c"),
                    MonoSourceDir.Combine("mono/metadata/handle.c"),
                    MonoSourceDir.Combine("mono/metadata/icall-windows.c"),
                    MonoSourceDir.Combine("mono/metadata/icall.c"),
                    MonoSourceDir.Combine("mono/metadata/image.c"),
                    MonoSourceDir.Combine("mono/metadata/jit-info.c"),
                    MonoSourceDir.Combine("mono/metadata/loader.c"),
                    MonoSourceDir.Combine("mono/metadata/locales.c"),
                    MonoSourceDir.Combine("mono/metadata/lock-tracer.c"),
                    MonoSourceDir.Combine("mono/metadata/marshal-windows.c"),
                    MonoSourceDir.Combine("mono/metadata/marshal.c"),
                    MonoSourceDir.Combine("mono/metadata/mempool.c"),
                    MonoSourceDir.Combine("mono/metadata/metadata-cross-helpers.c"),
                    MonoSourceDir.Combine("mono/metadata/metadata-verify.c"),
                    MonoSourceDir.Combine("mono/metadata/metadata.c"),
                    MonoSourceDir.Combine("mono/metadata/method-builder.c"),
                    MonoSourceDir.Combine("mono/metadata/monitor.c"),
                    MonoSourceDir.Combine("mono/metadata/mono-basic-block.c"),
                    MonoSourceDir.Combine("mono/metadata/mono-conc-hash.c"),
                    MonoSourceDir.Combine("mono/metadata/mono-config-dirs.c"),
                    MonoSourceDir.Combine("mono/metadata/mono-config.c"),
                    MonoSourceDir.Combine("mono/metadata/mono-debug.c"),
                    MonoSourceDir.Combine("mono/metadata/mono-endian.c"),
                    MonoSourceDir.Combine("mono/metadata/mono-hash.c"),
                    MonoSourceDir.Combine("mono/metadata/mono-mlist.c"),
                    MonoSourceDir.Combine("mono/metadata/mono-perfcounters.c"),
                    MonoSourceDir.Combine("mono/metadata/mono-security-windows.c"),
                    MonoSourceDir.Combine("mono/metadata/mono-security.c"),
                    MonoSourceDir.Combine("mono/metadata/null-gc.c"),
                    MonoSourceDir.Combine("mono/metadata/number-ms.c"),
                    MonoSourceDir.Combine("mono/metadata/object.c"),
                    MonoSourceDir.Combine("mono/metadata/opcodes.c"),
                    MonoSourceDir.Combine("mono/metadata/profiler.c"),
                    MonoSourceDir.Combine("mono/metadata/property-bag.c"),
                    MonoSourceDir.Combine("mono/metadata/rand.c"),
                    MonoSourceDir.Combine("mono/metadata/reflection.c"),
                    MonoSourceDir.Combine("mono/metadata/remoting.c"),
                    MonoSourceDir.Combine("mono/metadata/runtime.c"),
                    MonoSourceDir.Combine("mono/metadata/security-core-clr.c"),
                    MonoSourceDir.Combine("mono/metadata/security-manager.c"),
                    MonoSourceDir.Combine("mono/metadata/seq-points-data.c"),
                    MonoSourceDir.Combine("mono/metadata/sgen-bridge.c"),
                    MonoSourceDir.Combine("mono/metadata/sgen-mono.c"),
                    MonoSourceDir.Combine("mono/metadata/sgen-new-bridge.c"),
                    MonoSourceDir.Combine("mono/metadata/sgen-old-bridge.c"),
                    MonoSourceDir.Combine("mono/metadata/sgen-stw.c"),
                    MonoSourceDir.Combine("mono/metadata/sgen-tarjan-bridge.c"),
                    MonoSourceDir.Combine("mono/metadata/sgen-toggleref.c"),
                    MonoSourceDir.Combine("mono/metadata/sre-encode.c"),
                    MonoSourceDir.Combine("mono/metadata/sre-save.c"),
                    MonoSourceDir.Combine("mono/metadata/sre.c"),
                    MonoSourceDir.Combine("mono/metadata/string-icalls.c"),
                    MonoSourceDir.Combine("mono/metadata/sysmath.c"),
                    MonoSourceDir.Combine("mono/metadata/threadpool-io.c"),
                    MonoSourceDir.Combine("mono/metadata/threadpool-worker-default.c"),
                    MonoSourceDir.Combine("mono/metadata/threadpool.c"),
                    MonoSourceDir.Combine("mono/metadata/threads.c"),
                    MonoSourceDir.Combine("mono/metadata/unity-icall.c"),
                    MonoSourceDir.Combine("mono/metadata/unity-liveness.c"),
                    MonoSourceDir.Combine("mono/metadata/unity-utils.c"),
                    MonoSourceDir.Combine("mono/metadata/verify.c"),
                    MonoSourceDir.Combine("mono/metadata/w32file.c"),
                    MonoSourceDir.Combine("mono/metadata/w32handle-namespace.c"),
                    MonoSourceDir.Combine("mono/metadata/w32handle.c"),
                    MonoSourceDir.Combine("mono/metadata/w32process.c"),
                    MonoSourceDir.Combine("mono/metadata/w32socket.c")
                });

                if (npc.ToolChain.Platform is WindowsPlatform)
                    files.AddRange(WindowsDesktopMonoSourceFileList.GetMetadataSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is WindowsGamesPlatform)
                    files.AddRange(WindowsGamesMonoSourceFileList.GetMetadataSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is UniversalWindowsPlatform)
                    files.AddRange(WinRTMonoSourceFileList.GetMetadataSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is AndroidPlatform)
                    files.AddRange(AndroidMonoSourceFileList.GetMetadataSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is LinuxPlatform)
                    files.AddRange(LinuxMonoSourceFileList.GetMetadataSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is MacOSXPlatform)
                    files.AddRange(OSXMonoSourceFileList.GetMetadataSourceFiles(npc, MonoSourceDir));
            }

            return files.ToArray();
        }

        public static NPath[] GetMiniSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir)
        {
            return new[] { MonoSourceDir.Combine("mono/mini/debugger-agent.c") };
        }

        public static NPath[] GetUtilsSourceFiles(NativeProgramConfiguration npc, NPath MonoSourceDir, bool managedDebuggingEnabled)
        {
            var files = new List<NPath>
            {
                MonoSourceDir.Combine("mono/utils/atomic.c"),
                MonoSourceDir.Combine("mono/utils/bsearch.c"),
                MonoSourceDir.Combine("mono/utils/dlmalloc.c"),
                MonoSourceDir.Combine("mono/utils/hazard-pointer.c"),
                MonoSourceDir.Combine("mono/utils/json.c"),
                MonoSourceDir.Combine("mono/utils/lock-free-alloc.c"),
                MonoSourceDir.Combine("mono/utils/lock-free-array-queue.c"),
                MonoSourceDir.Combine("mono/utils/lock-free-queue.c"),
                MonoSourceDir.Combine("mono/utils/memfuncs.c"),
                MonoSourceDir.Combine("mono/utils/mono-codeman.c"),
                MonoSourceDir.Combine("mono/utils/mono-conc-hashtable.c"),
                MonoSourceDir.Combine("mono/utils/mono-context.c"),
                MonoSourceDir.Combine("mono/utils/mono-counters.c"),
                MonoSourceDir.Combine("mono/utils/mono-dl.c"),
                MonoSourceDir.Combine("mono/utils/mono-error.c"),
                MonoSourceDir.Combine("mono/utils/mono-filemap.c"),
                MonoSourceDir.Combine("mono/utils/mono-hwcap.c"),
                MonoSourceDir.Combine("mono/utils/mono-internal-hash.c"),
                MonoSourceDir.Combine("mono/utils/mono-io-portability.c"),
                MonoSourceDir.Combine("mono/utils/mono-linked-list-set.c"),
                MonoSourceDir.Combine("mono/utils/mono-log-common.c"),
                MonoSourceDir.Combine("mono/utils/mono-logger.c"),
                MonoSourceDir.Combine("mono/utils/mono-math.c"),
                MonoSourceDir.Combine("mono/utils/mono-md5.c"),
                MonoSourceDir.Combine("mono/utils/mono-mmap-windows.c"),
                MonoSourceDir.Combine("mono/utils/mono-mmap.c"),
                MonoSourceDir.Combine("mono/utils/mono-networkinterfaces.c"),
                MonoSourceDir.Combine("mono/utils/mono-os-mutex.c"),
                MonoSourceDir.Combine("mono/utils/mono-path.c"),
                MonoSourceDir.Combine("mono/utils/mono-poll.c"),
                MonoSourceDir.Combine("mono/utils/mono-proclib-windows.c"),
                MonoSourceDir.Combine("mono/utils/mono-proclib.c"),
                MonoSourceDir.Combine("mono/utils/mono-property-hash.c"),
                MonoSourceDir.Combine("mono/utils/mono-publib.c"),
                MonoSourceDir.Combine("mono/utils/mono-sha1.c"),
                MonoSourceDir.Combine("mono/utils/mono-stdlib.c"),
                MonoSourceDir.Combine("mono/utils/mono-threads-coop.c"),
                MonoSourceDir.Combine("mono/utils/mono-threads-state-machine.c"),
                MonoSourceDir.Combine("mono/utils/mono-threads.c"),
                MonoSourceDir.Combine("mono/utils/mono-tls.c"),
                MonoSourceDir.Combine("mono/utils/mono-uri.c"),
                MonoSourceDir.Combine("mono/utils/mono-value-hash.c"),
                MonoSourceDir.Combine("mono/utils/monobitset.c"),
                MonoSourceDir.Combine("mono/utils/networking-missing.c"),
                MonoSourceDir.Combine("mono/utils/networking.c"),
                MonoSourceDir.Combine("mono/utils/parse.c"),
                MonoSourceDir.Combine("mono/utils/strenc.c"),
                MonoSourceDir.Combine("mono/utils/unity-rand.c"),
                MonoSourceDir.Combine("mono/utils/unity-time.c")
            };

            if (npc.ToolChain.Architecture is EmscriptenArchitecture)
                files.Add(MonoSourceDir.Combine("mono/utils/mono-hwcap-web.c"));
            else if (npc.ToolChain.Architecture is x86Architecture || npc.ToolChain.Architecture is x64Architecture)
                files.Add(MonoSourceDir.Combine("mono/utils/mono-hwcap-x86.c"));

            if (npc.ToolChain.Architecture is ARMv7Architecture)
                files.Add(MonoSourceDir.Combine("mono/utils/mono-hwcap-arm.c"));

            if (npc.ToolChain.Architecture is Arm64Architecture)
                files.Add(MonoSourceDir.Combine("mono/utils/mono-hwcap-arm64.c"));

            if (managedDebuggingEnabled)
            {
                if (npc.ToolChain.Platform is WindowsPlatform)
                    files.AddRange(WindowsDebuggerMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is UniversalWindowsPlatform)
                    files.AddRange(WinRTDebuggerMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is AndroidPlatform)
                    files.AddRange(AndroidDebuggerMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is IosPlatform)
                    files.AddRange(iOSDebuggerMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is LinuxPlatform)
                    files.AddRange(LinuxDebuggerMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is MacOSXPlatform)
                    files.AddRange(OSXDebuggerMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir));
            }
            else
            {
                if (npc.ToolChain.Platform is WindowsPlatform)
                    files.AddRange(WindowsDesktopMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is WindowsGamesPlatform)
                    files.AddRange(WindowsGamesMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is UniversalWindowsPlatform)
                    files.AddRange(WinRTMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is AndroidPlatform)
                    files.AddRange(AndroidMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is LinuxPlatform)
                    files.AddRange(LinuxMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir));

                if (npc.ToolChain.Platform is MacOSXPlatform)
                    files.AddRange(OSXMonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir));
            }

            return files.ToArray();
        }
    }
}
#endif
