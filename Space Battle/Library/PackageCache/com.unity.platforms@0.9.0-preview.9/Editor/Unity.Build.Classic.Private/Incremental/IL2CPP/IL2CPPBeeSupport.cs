#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using Bee.Core;
using Bee.DotNet;
using Bee.Toolchain.VisualStudio;
using Bee.Toolchain.Xcode;
using NiceIO;
using System;
using System.Collections.Generic;
using System.Linq;
using Bee.NativeProgramSupport;
using Bee.Tools;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Player;
using UnityEditor.UnityLinker;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    abstract class IL2CPPPlatformBeeSupport
    {
        public abstract void ProvideNativeProgramSettings(NativeProgram program);
        public abstract void ProvideLibIl2CppProgramSettings(NativeProgram program);
    }

    class IL2CPPBeeSupport
    {
        private static IncrementalClassicSharedData _incrementalClassicSharedData;

        public IL2CPPBeeSupport(IncrementalClassicSharedData sharedData)
        {
            _incrementalClassicSharedData = sharedData;
        }

        public NPath[] SetupIl2CppOutputFiles(BuildTarget buildTarget,
            List<NPath> copiedPrebuiltAssemblies, List<DotNetAssembly> postProcessedPlayerAssemblies,
            NPath il2cpp_data_dir, string workingDirectory)
        {
            var linkerOutputFiles = SetupLinker(copiedPrebuiltAssemblies, postProcessedPlayerAssemblies, buildTarget, workingDirectory);
            var il2cppOutputFiles = SetupIL2CPPConversion(linkerOutputFiles, il2cpp_data_dir);
            return il2cppOutputFiles;
        }

        public NativeProgram NativeProgramForIL2CPPOutputFor(string nativeProgramName, IL2CPPPlatformBeeSupport platformSupport, NPath[] il2cppOutputFiles)
        {
            var nativeProgramForIl2CppOutput = NativeProgramForIL2CPPOutput(nativeProgramName, il2cppOutputFiles, platformSupport);
            return nativeProgramForIl2CppOutput;
        }

        private NativeProgram NativeProgramForIL2CPPOutput(string nativeProgramName, NPath[] il2cppOutputFiles, IL2CPPPlatformBeeSupport platformSupport, bool managedDebuggingEnabled = true, bool libil2cpptiny = false)
        {
            var il2cppPlayerPackageDirectory = _incrementalClassicSharedData.PlayerPackageDirectory.Combine("il2cpp");
            var nativeProgram = new NativeProgram(nativeProgramName)
            {
                Sources = { il2cppOutputFiles },
                Exceptions = { true },
                IncludeDirectories =
                {
                    il2cppPlayerPackageDirectory.Exists() ? il2cppPlayerPackageDirectory.Combine("libil2cpp/include") : Distribution.Path.Combine("libil2cpp"),
                    il2cppPlayerPackageDirectory.Exists() ? il2cppPlayerPackageDirectory.Combine("external/baselib/include") : Distribution.Path.Combine("external/baselib/include"),
                    c => il2cppPlayerPackageDirectory.Exists() ? il2cppPlayerPackageDirectory.Combine($"external/baselib/Platforms/{c.Platform.Name}/Include") : Distribution.Path.Combine($"external/baselib/Platforms/{c.Platform.Name}/Include")
                },
                Defines = { "BASELIB_INLINE_NAMESPACE=il2cpp_baselib", "RUNTIME_IL2CPP=1" }
            };
            nativeProgram.Libraries.Add(c => CreateLibIl2CppProgram(!libil2cpptiny, platformSupport, managedDebuggingEnabled, libil2cpptiny ? "libil2cpptiny" : "libil2cpp"));

            nativeProgram.Libraries.Add(c => c.ToolChain.Platform is WindowsPlatform &&
                                             _incrementalClassicSharedData.VariationDirectory.Combine("baselib.dll.lib").Exists(),
                new StaticLibrary(_incrementalClassicSharedData.VariationDirectory.Combine("baselib.dll.lib")));
            nativeProgram.Libraries.Add(c => c.ToolChain.Platform is AndroidPlatform && c.ToolChain.Architecture is Arm64Architecture &&
                                             _incrementalClassicSharedData.VariationDirectory.Combine("StaticLibs/arm64-v8a/baselib.a").Exists(),
                new StaticLibrary(_incrementalClassicSharedData.VariationDirectory.Combine("StaticLibs/arm64-v8a/baselib.a")));
            nativeProgram.Libraries.Add(c => c.ToolChain.Platform is AndroidPlatform && c.ToolChain.Architecture is ARMv7Architecture &&
                                             _incrementalClassicSharedData.VariationDirectory.Combine("StaticLibs/armeabi-v7a/baselib.a").Exists(),
                new StaticLibrary(_incrementalClassicSharedData.VariationDirectory.Combine("StaticLibs/armeabi-v7a/baselib.a")));
            nativeProgram.Libraries.Add(c => c.ToolChain.Platform is MacOSXPlatform &&
                                             _incrementalClassicSharedData.VariationDirectory.Combine("baselib.a").Exists(),
                new StaticLibrary(_incrementalClassicSharedData.VariationDirectory.Combine("baselib.a")));
            nativeProgram.Libraries.Add(c => c.ToolChain.Platform is IosPlatform &&
                                             _incrementalClassicSharedData.PlayerPackageDirectory.Combine("Trampoline/Libraries/baselib-dev.a").Exists(),
                new StaticLibrary(_incrementalClassicSharedData.PlayerPackageDirectory.Combine("Trampoline", "Libraries", "baselib-dev.a")));

            if (il2cppPlayerPackageDirectory.Combine("libil2cpp/include/pch").Exists() || Distribution.Path.Combine("libil2cpp/pch").Exists())
            {
                var libil2cppDir = il2cppPlayerPackageDirectory.Combine("libil2cpp/include").Exists() ? il2cppPlayerPackageDirectory.Combine("libil2cpp/include") : Distribution.Path.Combine("libil2cpp");
                var libil2cpptinyDir = il2cppPlayerPackageDirectory.Combine("libil2cpptiny/include").Exists() ? il2cppPlayerPackageDirectory.Combine("libil2cpptiny/include") : Distribution.Path.Combine("libil2cpptiny");

                if (libil2cpptiny)
                {
                    nativeProgram.Defines.Add("IL2CPP_TINY");
                    if (managedDebuggingEnabled)
                    {
                        nativeProgram.IncludeDirectories.Add(libil2cppDir.Combine("pch"));

                        nativeProgram.PerFilePchs.Add(libil2cppDir.Combine("pch/pch-cpp.hpp"),
                            nativeProgram.Sources.ForAny().Where(f => f.HasExtension(".cpp")));
                        nativeProgram.PerFilePchs.Add(libil2cppDir.Combine("pch/pch-c.h"),
                            nativeProgram.Sources.ForAny().Where(f => f.HasExtension(".c")));
                    }
                    else
                    {
                        // Tiny needs to be able to find its include directory before libil2cpp
                        nativeProgram.IncludeDirectories.AddFront(libil2cpptinyDir);
                        nativeProgram.IncludeDirectories.AddFront(libil2cpptinyDir.Combine("pch"));

                        nativeProgram.PerFilePchs.Add(libil2cpptinyDir.Combine("pch/pch-cpp.hpp"),
                            nativeProgram.Sources.ForAny().Where(f => f.HasExtension(".cpp")));
                    }
                }
                else
                {
                    nativeProgram.IncludeDirectories.Add(libil2cppDir.Combine("pch"));

                    nativeProgram.PerFilePchs.Add(libil2cppDir.Combine("pch/pch-cpp.hpp"),
                        nativeProgram.Sources.ForAny().Where(f => f.HasExtension(".cpp")));
                    nativeProgram.PerFilePchs.Add(libil2cppDir.Combine("pch/pch-c.h"),
                        nativeProgram.Sources.ForAny().Where(f => f.HasExtension(".c")));
                }
            }

            nativeProgram.CompilerSettings().Add(s => s.WithCppLanguageVersion(CppLanguageVersion.Cpp11));
            nativeProgram.CompilerSettingsForMsvc()
                .Add(c => c.WithCustomFlags(new[] { "/EHs" }));
            nativeProgram.CompilerSettingsForMsvc()
                .Add(c => c.WithWarningPolicies(new[] { new WarningAndPolicy("4102", WarningPolicy.Silent) }));
            // nativeProgram.CompilerSettingsForMsvc()
            //     .Add(c => c.WithPDB(_incrementalClassicSharedData.BeeProjectRoot.Combine("libil2cpp.pdb")));
            nativeProgram.CompilerSettingsForClang().Add(c => c.WithWarningPolicies(new[] { new WarningAndPolicy("pragma-once-outside-header", WarningPolicy.Silent) }));
            if (platformSupport != null)
                platformSupport.ProvideNativeProgramSettings(nativeProgram);

            nativeProgram.NonLumpableFiles.Add(nativeProgram.Sources.ForAny());

            return nativeProgram;
        }

        private NPath[] SetupIL2CPPConversion(NPath[] linkerOutputFiles, NPath il2cppdata_destinationdir)
        {

            var il2cpp =
                new DotNetAssembly(
                    string.IsNullOrEmpty(CustomIL2CPPLocation) ?
                    $"{EditorApplication.applicationContentsPath}/il2cpp/build/deploy/net471/il2cpp.exe" :
                    $"{CustomIL2CPPLocation}/build/deploy/net471/il2cpp.exe",
                    Framework.Framework471);
            var netCoreRunRuntime = DotNetRuntime.FindFor(il2cpp); //NetCoreRunRuntime.FromSteve;
            var il2cppProgram = new DotNetRunnableProgram(il2cpp, netCoreRunRuntime);

            var extraTypes = new HashSet<string>();
            foreach (var extraType in PlayerBuildInterface.ExtraTypesProvider?.Invoke() ?? Array.Empty<string>())
            {
                extraTypes.Add(extraType);
            }

            NPath extraTypesFile = Configuration.RootArtifactsPath.Combine("extra-types.txt").MakeAbsolute().WriteAllLines(extraTypes.ToArray());

            NPath il2cppOutputDir = Configuration.RootArtifactsPath.Combine("il2cpp");

            Backend.Current.AddAction("IL2CPP", Array.Empty<NPath>(),
                linkerOutputFiles
                    .Concat(il2cpp.Path.Parent.Files()).Concat(netCoreRunRuntime.Inputs)
                    .Concat(new[] { extraTypesFile })
                    .ToArray(),
                il2cppProgram.InvocationString,
                new[]
                {
                    "--convert-to-cpp",
                    "--emit-null-checks",
                    "--enable-array-bounds-check",
                    "--dotnetprofile=\"unityaot\"",
                    "--libil2cpp-static",
                    $"--extra-types-file={extraTypesFile.InQuotes()}",
                    "--profiler-report",
                    $"--generatedcppdir={il2cppOutputDir.InQuotes(SlashMode.Native)}",
                    $"--directory={linkerOutputFiles.First().Parent.InQuotes(SlashMode.Native)}",
                }, targetDirectories: new[] { il2cppOutputDir }, allowUnwrittenOutputFiles: true);

            var il2cppOutputFiles = GuessTargetDirectoryContentsFor(il2cppOutputDir, "dummy.cpp");

            var dataDir = il2cppOutputDir.Combine("Data");
            foreach (var il2cppdatafile in dataDir.FilesIfExists(recurse: true))
            {
                CopyTool.Instance().Setup(il2cppdata_destinationdir.Combine(il2cppdatafile.RelativeTo(dataDir)),
                    il2cppdatafile);
            }

            return il2cppOutputFiles;
        }

        private static NPath[] SetupLinker(List<NPath> copiedAssemblies, List<DotNetAssembly> postProcessedPlayerAssemblies, BuildTarget buildTarget, NPath workingDirectory)
        {
            /*
     * Invoking UnityLinker with arguments: -out=C:/dots/Samples/Temp/StagingArea/Data/Managed/tempStrip -x=C:/Users/Lucas/AppData/Local/Temp/tmp5eb9e11d.tmp
     * -x=C:/dots/Samples/Temp/StagingArea/Data/Managed/TypesInScenes.xml -x=C:/dots/Samples/Temp/StagingArea/Data/Managed/DotsStripping.xml
     * -d=C:/dots/Samples/Temp/StagingArea/Data/Managed --include-unity-root-assembly=C:/dots/Samples/Temp/StagingArea/Data/Managed/Assembly-CSharp.dll
     * --include-unity-root-assembly=C:/dots/Samples/Temp/StagingArea/Data/Managed/RotateMe.dll --include-unity-root-assembly=C:/dots/Samples/Temp/StagingArea/Data/Managed/Unity.Scenes.Hybrid.dll
     * --include-unity-root-assembly=C:/dots/Samples/Temp/StagingArea/Data/Managed/Samples.GridPath.dll --include-unity-root-assembly=C:/dots/Samples/Temp/StagingArea/Data/Managed/Unity.Entities.Hybrid.dll
     * --include-unity-root-assembly=C:/dots/Samples/Temp/StagingArea/Data/Managed/SubsceneWithBuildSettings.dll --include-unity-root-assembly=C:/dots/Samples/Temp/StagingArea/Data/Managed/HelloCube.dll
     * --include-unity-root-assembly=C:/dots/Samples/Temp/StagingArea/Data/Managed/Samples.Boids.dll --dotnetruntime=il2cpp --dotnetprofile=unityaot --use-editor-options --include-directory=C:/dots/Samples/Temp/StagingArea/Data/Managed
     * --editor-settings-flag=Development --rule-set=Conservative --editor-data-file=C:/dots/Samples/Temp/StagingArea/Data/Managed/EditorToUnityLinkerData.json --platform=WindowsDesktop
     * --engine-modules-asset-file=C:/unity/build/WindowsStandaloneSupport/Whitelists/../modules.asset
    C:\unity\build\WindowsEditor\Data\il2cpp\build/deploy/net471/UnityLinker.exe exited after 4100 ms.

    "C:\\code\\unity-src-git\\build\\WindowsEditor\\Data\\il2cpp\\build\\deploy\\net471\\UnityLinker.exe --dotnetruntime=mono --dotnetprofile=unityaot
    --use-editor-options --include-directory=\"artifacts\\managedassemblies\" --editor-settings-flag=Development --rule-set=conservative --out=\"artifacts\\linkeroutput\"
    --include-unity-root-assembly=\"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/UnityEngine.UI.dll/Release/UnityEngine.UI.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Unity.Mathematics.dll/Release/Unity.Mathematics.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Unity.Entities.StaticTypeRegistry.dll/Release/Unity.Entities.StaticTypeRegistry.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Unity.Burst.dll/Release/Unity.Burst.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Unity.ScriptableBuildPipeline.dll/Release/Unity.ScriptableBuildPipeline.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Unity.Collections.dll/Release/Unity.Collections.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Unity.Properties.dll/Release/Unity.Properties.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Unity.Jobs.dll/Release/Unity.Jobs.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Unity.Mathematics.Extensions.dll/Release/Unity.Mathematics.Extensions.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Unity.Entities.dll/Release/Unity.Entities.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Unity.Transforms.dll/Release/Unity.Transforms.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Unity.Entities.Determinism.dll/Release/Unity.Entities.Determinism.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Unity.Mathematics.Extensions.Hybrid.dll/Release/Unity.Mathematics.Extensions.Hybrid.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Unity.Entities.Hybrid.dll/Release/Unity.Entities.Hybrid.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Unity.Transforms.Hybrid.dll/Release/Unity.Transforms.Hybrid.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Unity.Scenes.Hybrid.dll/Release/Unity.Scenes.Hybrid.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Unity.Rendering.Hybrid.dll/Release/Unity.Rendering.Hybrid.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Samples.Boids.dll/Release/Samples.Boids.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/HelloCube.dll/Release/HelloCube.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Samples.GridPath.dll/Release/Samples.GridPath.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/SubsceneWithBuildSettings.dll/Release/SubsceneWithBuildSettings.dll\",
    \"C:/code/dots/Samples/Library/IncrementalClassicBuildPipeline/Win64-LiveLink/artifacts/Assembly-CSharp.dll/Release/Assembly-CSharp.dll\"
    --include-unity-root-assembly=\"C:/code/unity-src-git/build/WindowsStandaloneSupport/Variations/win64_development_mono/Data/Managed/UnityEngine.dll\"",

     */

            DotNetAssembly linker =
                new DotNetAssembly(
                    string.IsNullOrEmpty(CustomIL2CPPLocation) ?
                    $"{EditorApplication.applicationContentsPath}/il2cpp/build/deploy/net471/UnityLinker.exe" :
                    $"{CustomIL2CPPLocation}/build/deploy/net471/UnityLinker.exe",
                    Framework.Framework471);

            var linkerProgram = new DotNetRunnableProgram(linker);

            var linkXmlFiles = NPath.CurrentDirectory.Combine("Assets").Files("link.xml", true);

            var unityLinkerProcessors = TypeCacheHelper.ConstructTypesDerivedFrom<IUnityLinkerProcessor>();

            var linkerData = new UnityLinkerBuildPipelineData(buildTarget, workingDirectory.ToString());
            linkXmlFiles = linkXmlFiles.Concat(unityLinkerProcessors.Select(p => (NPath)p.GenerateAdditionalLinkXmlFile(null, linkerData))).ToArray();

            var inputFiles = copiedAssemblies
                .Concat(postProcessedPlayerAssemblies.Select(p => p.Path))
                .Concat(linker.Path.Parent.Files())
                .Concat(linkXmlFiles)
            //    .Concat(dotNetRuntime.Inputs)
                .ToArray();

            var searchDirectories = new HashSet<NPath>();
            foreach (var file in copiedAssemblies)
                searchDirectories.Add(file.Parent);
            foreach (var file in postProcessedPlayerAssemblies)
                searchDirectories.Add(file.Path.Parent);
            searchDirectories.Add(_incrementalClassicSharedData.UnityEngineAssembliesDirectory.ToString());

            // Pass along the correct platform names for macOS and Windows desktop targets
            var platformName = _incrementalClassicSharedData.PlatformName == "Windows" ?
                "WindowsDesktop" :
                _incrementalClassicSharedData.PlatformName == "UniversalWindows" ?
                    "WinRT" :
                    _incrementalClassicSharedData.PlatformName;

            platformName = _incrementalClassicSharedData.PlatformName == "OSX" ? "MacOSX" : platformName;

            NPath linkerOutputDir = Configuration.RootArtifactsPath.Combine("linkeroutput");
            Backend.Current.AddAction("UnityLinker", Array.Empty<NPath>(), inputFiles,
                linkerProgram.InvocationString, new[]
                {
                    $"--platform={platformName}",
                    "--dotnetruntime=il2cpp",
                    "--dotnetprofile=unityaot",
                    "--use-editor-options",
                    $"--include-directory={copiedAssemblies.First().Parent.InQuotes(SlashMode.Native)}",
                    "--editor-settings-flag=Development",

                    //if you want to boost up from conservative to aggressive, the linker will start to remove MonoBehaviours. We'd need to add a step that analyzes all assets
                    //in the databuild, and build a list of actually used monobehaviours. this goes very much against the concept of incrementalness. Let's stick to conservative for now.t
                    "--rule-set=Conservative",
                    $"--out={linkerOutputDir.InQuotes(SlashMode.Native)}",
                    $"--search-directory={searchDirectories.Select(d=>d.MakeAbsolute().ToString()).SeparateWithComma()}",
                    $"--include-unity-root-assembly={postProcessedPlayerAssemblies.Where(a => a.Path.FileName != "Unity.Serialization.dll").Select(a => a.Path.MakeAbsolute()).InQuotes().SeparateWithComma()}",
                    $"--engine-modules-asset-file={_incrementalClassicSharedData.PlayerPackageDirectory.Combine("modules.asset").InQuotes(SlashMode.Native)}",
                    linkXmlFiles.Select(f => $"--include-link-xml={f}").SeparateWithSpace(),
                }, allowUnwrittenOutputFiles: true,
                supportResponseFile: true,
                targetDirectories: new[]
                {
                    linkerOutputDir
                }
            );

            NPath[] linkerOutputFiles = GuessTargetDirectoryContentsFor(linkerOutputDir, "dummy.dll");

            return linkerOutputFiles;
        }

        private static string CustomIL2CPPLocation => Environment.GetEnvironmentVariable("UNITY_IL2CPP_PATH");
        private static LocalFileBundle Distribution { get; } =
            new LocalFileBundle(!string.IsNullOrEmpty(CustomIL2CPPLocation) ?
                CustomIL2CPPLocation :
                EditorApplication.applicationContentsPath + "/il2cpp"
            );

        private NativeProgramAsLibrary CreateLibIl2CppProgram(bool useExceptions, IL2CPPPlatformBeeSupport platformSupport, bool managedDebuggingEnabled = true, string libil2cppname = "libil2cpp")
        {
            NPath[] fileList;
            if (libil2cppname == "libil2cpptiny" && managedDebuggingEnabled)
                fileList = Distribution.GetFileList("libil2cpp").ToArray();
            else
                fileList = Distribution.GetFileList(libil2cppname).ToArray();
            var nPaths = fileList.Where(f => f.Parent.FileName != "pch" && f.HasExtension("cpp")).ToArray();

            var il2cppPlayerPackageDirectory = _incrementalClassicSharedData.PlayerPackageDirectory.Combine("il2cpp");

            var program = new NativeProgram(libil2cppname)
            {
                Sources = { nPaths },
                Exceptions = { useExceptions },
                PublicIncludeDirectories =
                {
                    Distribution.Path.Combine("libil2cpp")
                },
                IncludeDirectories =
                {
                    il2cppPlayerPackageDirectory.Exists() ? il2cppPlayerPackageDirectory.Combine("external/baselib/include") : Distribution.Path.Combine("external/baselib/include"),
                    c => il2cppPlayerPackageDirectory.Exists() ? il2cppPlayerPackageDirectory.Combine($"external/baselib/Platforms/{c.Platform.Name}/Include") : Distribution.Path.Combine($"external/baselib/Platforms/{c.Platform.Name}/Include"),
                    Distribution.Path.Combine("external/mono/mono/eglib"),
                    Distribution.Path.Combine("external/mono/mono"),
                    Distribution.Path.Combine("external/mono/"),
                    Distribution.Path.Combine("external/mono/mono/sgen"),
                    Distribution.Path.Combine("external/mono/mono/utils"),
                    Distribution.Path.Combine("external/mono/mono/metadata"),
                    Distribution.Path.Combine("external/mono/mono/metadata/private"),
                    Distribution.Path.Combine("libmono/config"),
                    Distribution.Path.Combine("libil2cpp/os/c-api"),
                    Distribution.Path.Combine("libil2cpp/debugger")
                },
                PublicDefines =
                {
                    "NET_4_0",
                    "GC_NOT_DLL",
                    "RUNTIME_IL2CPP",
                    "LIBIL2CPP_IS_IN_EXECUTABLE=0",
                    {c => c.ToolChain is VisualStudioToolchain, "NOMINMAX", "WIN32_THREADS", "IL2CPP_TARGET_WINDOWS=1"},
                    {c => c.CodeGen == CodeGen.Debug, "DEBUG", "IL2CPP_DEBUG=1"},
                    {c => !(c.Platform is WebGLPlatform), "GC_THREADS=1", "USE_MMAP=1", "USE_MUNMAP=1"},
                },
                Defines =
                {
                    "BASELIB_INLINE_NAMESPACE=il2cpp_baselib"
                },
                Libraries =
                {
                    {
                        c => c.Platform is WindowsPlatform,
                        new[]
                        {
                            "user32.lib", "advapi32.lib", "ole32.lib", "oleaut32.lib", "Shell32.lib", "Crypt32.lib",
                            "psapi.lib", "version.lib", "MsWSock.lib", "ws2_32.lib", "Iphlpapi.lib", "Dbghelp.lib"
                        }.Select(s => new SystemLibrary(s))
                    },
                    {c => c.Platform is MacOSXPlatform || c.Platform is IosPlatform, new ILibrary[] {new SystemFramework("CoreFoundation")}},
                    {c => c.Platform is LinuxPlatform, new SystemLibrary("dl")},
                    {c => c.Platform is AndroidPlatform, new ILibrary[] { new SystemLibrary("log")}},
                }
            };

            program.RTTI.Set(c => useExceptions && c.ToolChain.EnablingExceptionsRequiresRTTI);

            if (libil2cppname == "libil2cpptiny")
            {
                program.Defines.Add("IL2CPP_TINY");

                if (!managedDebuggingEnabled)
                {
                    // Tiny needs to be able to find its include directory before libil2cpp
                    program.IncludeDirectories.AddFront(Distribution.Path.Combine("libil2cpptiny"));

                    program.Sources.Add(Distribution.GetFileList("libil2cpp/os"));
                    program.Sources.Add(Distribution.GetFileList("libil2cpp/gc"));
                    program.Sources.Add(Distribution.GetFileList("libil2cpp/utils"));
                    program.Sources.Add(Distribution.GetFileList("libil2cpp/vm-utils"));
                }
            }

            if (managedDebuggingEnabled)
            {
                program.Defines.Add("IL2CPP_MONO_DEBUGGER=1",
                    "IL2CPP_DEBUGGER_PORT=56000",
                    "PLATFORM_UNITY",
                    "UNITY_USE_PLATFORM_STUBS",
                    "ENABLE_OVERRIDABLE_ALLOCATORS",
                    "IL2CPP_ON_MONO=1",
                    "DISABLE_JIT=1",
                    "DISABLE_REMOTING=1",
                    "HAVE_CONFIG_H",
                    "MONO_DLL_EXPORT=1");
                program.Defines.Add(c => c.ToolChain.Platform is WebGLPlatform, "HOST_WASM=1");

                program.CompilerSettingsForMsvc().Add(c => c.WithCustomFlags(new[] { "/EHcs" }));
                program.CompilerSettingsForClang().Add(c => c.WithRTTI(true));
                program.CompilerSettingsForClang().Add(c => c.WithExceptions(true));
            }
            else
            {
                program.Defines.Add("IL2CPP_MONO_DEBUGGER_DISABLED");
            }

            program.PublicDefines.Add(c => c.ToolChain.Platform is AndroidPlatform,
                "LINUX",
                "ANDROID",
                "PLATFORM_ANDROID",
                "__linux__",
                "__STDC_FORMAT_MACROS"
            );
            program.PublicDefines.Add(c => c.ToolChain.Platform is AndroidPlatform && c.ToolChain.Architecture is Arm64Architecture, "TARGET_ARM64");

            var MonoSourceDir = Distribution.Path.Combine("external/mono");
            program.Sources.Add(c => MonoSourcesFor(c, MonoSourceDir, managedDebuggingEnabled));
            program.NonLumpableFiles.Add(c => MonoSourcesFor(c, MonoSourceDir, managedDebuggingEnabled));

            if (libil2cppname != "libil2cpptiny")
            {
                program.PublicIncludeDirectories.Add(Distribution.Path.Combine("external/zlib"));
                var zlibSources = Distribution.GetFileList("external/zlib").Where(f => f.Extension.Equals("c")).ToArray();
                program.NonLumpableFiles.Add(zlibSources);
                program.Sources.Add(zlibSources);
            }
            else
            {
                program.PublicIncludeDirectories.Add(Distribution.Path.Combine("external/xxHash"));
                program.Sources.Add(Distribution.Path.Combine("external/xxHash/xxhash.c"));
            }

            var boehmGcRoot = Distribution.Path.Combine("external/bdwgc");
            program.Sources.Add(boehmGcRoot.Combine("extra/gc.c"));
            program.PublicIncludeDirectories.Add(boehmGcRoot.Combine("include"));
            program.IncludeDirectories.Add(boehmGcRoot.Combine("libatomic_ops/src"));
            program.Defines.Add(
                "ALL_INTERIOR_POINTERS=1",
                "GC_GCJ_SUPPORT=1",
                "JAVA_FINALIZATION=1",
                "NO_EXECUTE_PERMISSION=1",
                "GC_NO_THREADS_DISCOVERY=1",
                "IGNORE_DYNAMIC_LOADING=1",
                "GC_DONT_REGISTER_MAIN_STATIC_DATA=1",
                "NO_DEBUGGING=1",
                "GC_VERSION_MAJOR=7",
                "GC_VERSION_MINOR=7",
                "GC_VERSION_MICRO=0",
                "HAVE_BDWGC_GC",
                "HAVE_BOEHM_GC",
                "DEFAULT_GC_NAME=\"BDWGC\"",
                "NO_CRT=1",
                "DONT_USE_ATEXIT=1",
                "NO_GETENV=1");

            program.Defines.Add(c => !(c.Platform is WebGLPlatform), "GC_THREADS=1", "USE_MMAP=1", "USE_MUNMAP=1");
            program.Defines.Add(c => c.ToolChain is VisualStudioToolchain, "NOMINMAX", "WIN32_THREADS");
            program.CompilerSettings().Add(s => s.WithCppLanguageVersion(CppLanguageVersion.Cpp11));
            if (platformSupport != null)
                platformSupport.ProvideLibIl2CppProgramSettings(program);

            return program;
        }

        private static NPath[] GuessTargetDirectoryContentsFor(NPath path, NPath ifEmptyUse = null)
        {
            var result = path.FilesIfExists();
            return result.Length == 0 ? new[] { path.Combine(ifEmptyUse) } : result;
        }

        private NPath[] MonoSourcesFor(NativeProgramConfiguration npc, NPath MonoSourceDir, bool managedDebuggingEnabled)
        {
            var monoSources = new List<NPath>();

            if (managedDebuggingEnabled)
                monoSources.Add(MonoSourceFileList.GetMiniSourceFiles(npc, MonoSourceDir));

            monoSources.Add(MonoSourceFileList.GetEGLibSourceFiles(npc, MonoSourceDir, managedDebuggingEnabled));
            monoSources.Add(MonoSourceFileList.GetMetadataSourceFiles(npc, MonoSourceDir, managedDebuggingEnabled));
            monoSources.Add(MonoSourceFileList.GetUtilsSourceFiles(npc, MonoSourceDir, managedDebuggingEnabled));

            return monoSources.ToArray();
        }
    }
}
#endif
