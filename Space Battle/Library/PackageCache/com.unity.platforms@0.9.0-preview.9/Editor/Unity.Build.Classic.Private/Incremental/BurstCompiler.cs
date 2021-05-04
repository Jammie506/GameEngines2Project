#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using Bee.Core;
using Bee.DotNet;
using Bee.TundraBackend;
using NiceIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Profiling;
using UnityEngine;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    class BurstCompiler
    {
        public string BurstTarget { get; }
        public string BurstPlatform { get; }
        public NPath OutputDirectory { get; }

        private bool _installedBurstSupportsIncludeRootAssemblyReferencesFeature;
        private bool _installedBurstSupportsCaching;
        private bool _installedBurstIs1_3Preview10OrLater;
        private DotNetRunnableProgram _burstRunnableProgram;
        private NPath[] _burstCompilerInputFiles;

        private DotNetRuntime _dotnetRuntime;

        public BurstCompiler(string burstTarget, string burstPlatform, NPath outputDirectory)
        {
            BurstTarget = burstTarget;
            BurstPlatform = burstPlatform;
            OutputDirectory = outputDirectory;

            // On macOS the PlatformName is set as OSX in bee but it needs to be macOS for bcl.
            if (burstPlatform == "OSX")
                BurstPlatform = "macOS";

            if (burstPlatform == "IOS" && burstTarget == "ARMV8A_AARCH64")
                BurstPlatform = "iOS";

            NPath burstDir = Path.GetFullPath("Packages/com.unity.burst/.Runtime");
            var burstFiles = burstDir.Files(recurse: true);

            bool IsManagedBurstLibrary(NPath f)
            {
                if (!f.HasExtension(".dll"))
                    return false;

                if (f.FileName.StartsWith("burst-llvm-"))
                    return false;

                // These two libraries are not crossgen-compatible.
                if (f.FileName == "Unity.Cecil.Rocks.dll" || f.FileName == "Newtonsoft.Json.dll")
                    return false;

                return true;
            }

            var bclAssembly = new DotNetAssembly(burstDir.Combine("bcl.exe"), Framework.Framework471)
                .WithRuntimeDependencies(burstFiles
                    .Where(IsManagedBurstLibrary)
                    .Select(f => new DotNetAssembly(f, Framework.Framework471))
                    .ToArray());
#if BURST_NETCORE
            //todo: turn this to true.  we cannot right now because NetCoreRuntime.SteveDore is implemented through a static field,
            //which is incompatible with our "create graph in editor, and maybe we will create two graphs during the same domain".  the creation
            //of the steve artifact happens only once,  but it needs to be registered in each backend. the fact that it doesn't means you can get into
            //ugly situations where on second builds the dependencies to netcorerun are not properly setup.
            bool useNetCore = false;

            if (useNetCore)
            {
                var runtime = NetCoreRunRuntime.FromSteve;
                _dotnetRuntime = runtime;

                var useCrossGen = false;
                if (useCrossGen)
                {
                    bclAssembly = runtime.SetupAheadOfTimeCompilation(bclAssembly, "artifacts/bcl-crossgen");

                    bclAssembly = bclAssembly.WithPath(bclAssembly.Path.MakeAbsolute(BeeProjectRoot));
                    foreach (var file in burstFiles.Where(x => !IsManagedBurstLibrary(x)))
                    {
                        var relative = file.RelativeTo(burstDir);

                        var temp = CopyTool.Instance().Setup(bclAssembly.Path.Parent.Combine(relative), file);
                        Backend.Current.AddDependency(temp, bclAssembly.Path);
                    }
                }
            }
            else
#else
            {
                _dotnetRuntime = DotNetRuntime.FindFor(bclAssembly);
            }
#endif

            _burstRunnableProgram = new DotNetRunnableProgram(bclAssembly, _dotnetRuntime);
            _burstCompilerInputFiles = burstFiles;

            var burstPackageInfo =
                UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.burst/somefile");
            var burstPackageRawVersion = burstPackageInfo.version;

            // Remove everything after '-' if this is a preview package.
            var parts = burstPackageRawVersion.Split('-');
            if (parts.Length > 1)
                burstPackageRawVersion = parts[0];

            // Get the preview version number.
            // Default to max int to signal a very high preview number on release.
            var previewNumber = int.MaxValue;
            if (parts.Length > 1)
                previewNumber = int.Parse(parts[1].Split('.')[1]);

            var burstVersion = Version.Parse(burstPackageRawVersion);
            var burstVersionWithIncludeRootAssemblyReferencesFeature = new Version(1, 3);
            _installedBurstSupportsIncludeRootAssemblyReferencesFeature = burstVersion >= burstVersionWithIncludeRootAssemblyReferencesFeature;
            if (!_installedBurstSupportsIncludeRootAssemblyReferencesFeature)
                Debug.Log($"Using burst 1.3 instead of {burstVersion.ToString()} will give much better build times. At the time of this writing it is only available by building manually on your machine.");

            _installedBurstSupportsCaching =
                (burstVersion >= new Version(1, 3) && previewNumber > 7)
                || burstVersion >= new Version(1, 4);
            if (!_installedBurstSupportsCaching)
                Debug.Log($"Using burst 1.3 preview 8 or above instead of {burstVersion.ToString()} will give much better build times. At the time of this writing it is only available by building manually on your machine.");

            _installedBurstIs1_3Preview10OrLater =
                (burstVersion >= new Version(1, 3) && previewNumber >= 10)
                || burstVersion >= new Version(1, 4);
        }

        public NPath[] Setup(Architecture architecture, DotNetAssembly[] inputAssemblies, int assemblyIndex, string burstLibraryName, Dictionary<string, string> environmentVariables, BurstOutputMode outputMode)
        {
            using (new ProfilerMarker("Burst-" + burstLibraryName).Auto())
            {
                NPath intermediateOutputDir = Configuration.RootArtifactsPath.Combine($"bcl/{architecture.Name}/{burstLibraryName}");

                var runtimeDependencies = inputAssemblies.SelectMany(i =>
                    {
                        return i.RecursiveRuntimeDependenciesIncludingSelf;
                    })
                    .Distinct().ToArray();

                var args = new List<string>
                {
                    $"--target={BurstTarget}",
                    $"--platform={BurstPlatform}",
                    //"--log-timings",
                    "--dump=none",
                    $"--output={intermediateOutputDir.Combine(burstLibraryName)}",
                    inputAssemblies.Select(inputAssembly => $"--root-assembly={inputAssembly.Path.InQuotesResolved()}"),
                    runtimeDependencies.Select(r => r.Path.ResolveWithFileSystem().Parent.InQuotes()).Distinct().Select(d => $"--assembly-folder={d}"),
                };

                if (_installedBurstSupportsIncludeRootAssemblyReferencesFeature)
                    args.Add("--include-root-assembly-references=false");

                if (_installedBurstSupportsCaching)
                {
                    args.Add("--always-create-output=false");

                    if (outputMode == BurstOutputMode.LibraryPerJob)
                    {
                        args.Add("--output-mode=LibraryPerJob");
                        args.Add(
                            $"--cache-directory={NPath.CurrentDirectory.Combine("Library/BurstCache/Incremental")}");
                    }
                }

                if (!_installedBurstIs1_3Preview10OrLater)
                    args.Add($"--mintarget={BurstTarget}");

                if (_installedBurstIs1_3Preview10OrLater)
                    args.Add("--debug=Full");
                else
                    args.Add("--debug");

                Backend.Current.AddAction(
                    "Burst"
                    , new NPath[]
                    {
                    }
                    , runtimeDependencies.Select(p => p.Path).Concat(_dotnetRuntime.Inputs)
                        .Concat(_burstCompilerInputFiles).ToArray()
                    , executableStringFor: _burstRunnableProgram.InvocationString
                    , args.ToArray()
                    , targetDirectories: new[] {intermediateOutputDir}
                    , environmentVariables: environmentVariables
                );

                // This file extension thingy doesn't scale at all
                NPath[] producedFiles;

                using (new ProfilerMarker("Globbing produced files").Auto())
                {
                    producedFiles = intermediateOutputDir.FilesIfExists("*.dll")
                        .Concat(intermediateOutputDir.FilesIfExists("*.so"))
                        .Concat(intermediateOutputDir.FilesIfExists("*.a"))
                        .Concat(intermediateOutputDir.FilesIfExists("*.bundle"))
                        .ToArray();
                }

                int jobCounter = 0;
                var targetFiles = new List<NPath>();
                foreach (var producedFile in producedFiles)
                {
                    var targetFile = OutputDirectory.Combine($"lib_burst_{assemblyIndex}_{jobCounter++}").ChangeExtension(producedFile.Extension);
                    targetFiles.Add(CopyTool.Instance().Setup(targetFile, producedFile));

                    if (BurstPlatform == "Windows" && inputAssemblies.Any(x => x.DebugSymbolPath != null))
                    {
                        const string debugExtension = ".pdb"; // TODO: Handle other platforms.

                        var producedDebugSymbols = producedFile.ChangeExtension(debugExtension);

                        var targetDebugSymbols = targetFile.ChangeExtension(debugExtension);
                        targetFiles.Add(CopyTool.Instance().Setup(targetDebugSymbols, producedDebugSymbols));
                    }
                }

                return targetFiles.ToArray();
            }
        }

        public enum BurstOutputMode
        {
            SingleLibrary,
            LibraryPerJob
        }
    }
}
#endif
