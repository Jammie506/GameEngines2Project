#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using Bee.Core;
using Bee.DotNet;
using Bee.Stevedore.Program;
using NiceIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bee.CSharpSupport;
using Bee.Tools;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    class GraphSetupCodeGeneration
    {
        static readonly NPath s_monoBleedingEdgeLib = $"{EditorApplication.applicationContentsPath}/MonoBleedingEdge/lib";
        static readonly NPath s_netStandardDir = $"{EditorApplication.applicationContentsPath}/NetStandard";

        public enum Mode
        {
            Full,
            EnoughToProduceTypeDB
        }

        public void SetupCodegeneration(BuildContext context, Mode mode)
        {
            var classicContext = context.GetValue<IncrementalClassicSharedData>();
            var assemblyGraph =
                new List<Assembly>(CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies));
            var editorAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor);

            //codegen assemblies are not part of the CompilationPipeline.GetAssemblies(Player) graph. we need to grab them from the editor one.
            //it would be kind of a waste to compile all the many assemblies they reference from the editorgraph, as they are all also in the player one.
            var codeGenAssemblies = editorAssemblies.Where(e => e.IsCodeGenAssembly())
                .Select(assembly => TransplantFromEditorGraphIntoPlayerGraph(assembly, assemblyGraph)).ToList();
            var assembliesToCompile = assemblyGraph.Where(a => !Path.GetFileName(a.outputPath).Contains("Test"))
                .OrderByDependencies().ToList();

            //We should probably at some point switch back to using the roslyn that ships in the editor, however that has a bug where it crashes
            //with /refout on our postprocessing code. To work around that for now, we're going to compile with the latest roslyn from stevedore.
            var csharpCompiler = Csc.Latest; //new UnityEditorCsc
            CSharpProgram.DefaultConfig = new CSharpProgramConfiguration(CSharpCodeGen.Release, csharpCompiler, DebugFormat.PortablePdb);

            Dictionary<Assembly, (CSharpProgram program, DotNetAssembly assembly)> unityAssemblyToCSharpProgramAndBuiltAssembly = SetupCSharpProgramsFor(context, assembliesToCompile);

            ScriptAssembliesJsonGenerator.Setup(unityAssemblyToCSharpProgramAndBuiltAssembly, classicContext.DataDeployDirectory);

            var postProcessedPlayerAssemblies = SetupPostProcessPlayerAssembliesFor(assembliesToCompile, classicContext,
                codeGenAssemblies, unityAssemblyToCSharpProgramAndBuiltAssembly, context.UsesIL2CPP());

            var playerAssemblies = postProcessedPlayerAssemblies.Where(kvp => !IsEditorAssembly(kvp.unityAssembly)).Select(w => w.dotNetAssembly).ToArray();
            SetupTypeResourcesFor(playerAssemblies, classicContext.DataDeployDirectory, classicContext.TypeDBOutputDirectory);
            if (mode == Mode.EnoughToProduceTypeDB)
                return;

            postProcessedPlayerAssemblies.FilterByBool(p => IsExternal(p.unityAssembly), out var external, out var locals);

            var localAssemblies = locals.Select(l => l.dotNetAssembly).ToArray();
            var externalAssemblies = external.Select(l => l.dotNetAssembly).ToArray();
            var burstSettings = context.GetOrCreateValue<BurstSettings>();

            using (new ProfilerMarker(nameof(BurstCompiler)).Auto())
            {
                foreach (var arch in classicContext.Architectures)
                {
                    var a = arch.Value;
                    var burstCompiler = new BurstCompiler(a.BurstTarget, classicContext.PlatformName, a.DynamicLibraryDeployDirectory);

                    switch (burstSettings.BurstGranularity)
                    {
                        case BurstGranularity.One:
                            burstCompiler.Setup(arch.Key, localAssemblies.Concat(externalAssemblies).ToArray(), 0, "wholeprogram",
                                burstSettings.EnvironmentVariables, BurstCompiler.BurstOutputMode.SingleLibrary);
                            break;
                        case BurstGranularity.OnePerJob:
                            int assemblyIndex = 0;

                            //compile all exeternal assemblies into one big burst library
                            burstCompiler.Setup(arch.Key, externalAssemblies, assemblyIndex++, "externalassemblies",
                                burstSettings.EnvironmentVariables, BurstCompiler.BurstOutputMode.SingleLibrary);

                            //and the local ones, in one burst library per assembly.
                            foreach (var localAssembly in localAssemblies)
                            {
                                var producedLibraries = burstCompiler.Setup(arch.Key, new[] { localAssembly }, assemblyIndex,
                                    localAssembly.Path.FileNameWithoutExtension, burstSettings.EnvironmentVariables,
                                    BurstCompiler.BurstOutputMode.LibraryPerJob);
                                if (producedLibraries.Length > 0)
                                {
                                    assemblyIndex++;
                                }
                            }

                            break;
                    }
                }
            }

            var copiedPrebuiltAssemblies = SetupCopyPrebuiltAssemblies(classicContext,
                GetDestinationDirectoryForManagedAssemblies(classicContext, context.UsesIL2CPP()), assemblyGraph, context.UsesIL2CPP()).ToList();
            context.SetValue(new Il2CppInputAssemblies()
            { prebuiltAssemblies = copiedPrebuiltAssemblies, processedAssemblies = postProcessedPlayerAssemblies });
        }

        private static bool IsExternal(Assembly unityAssembly)
        {
            var packageInfo = PackageInfo.FindForAssetPath(unityAssembly.sourceFiles.First());
            return packageInfo != null && packageInfo.source == PackageSource.Registry;
        }


        static Assembly TransplantFromEditorGraphIntoPlayerGraph(Assembly assembly, List<Assembly> assemblies)
        {
            var haystack = new Dictionary<string, string>();
            foreach (var a in assemblies)
            {
                foreach (var ca in a.compiledAssemblyReferences)
                {
                    var f = Path.GetFileName(ca);
                    haystack.TryAdd(f, ca);
                }
            }

            var result = new Assembly(
                assembly.name,
                assembly.outputPath,
                assembly.sourceFiles,
                assembly.defines,
                assembly.assemblyReferences.Select(a => assemblies.FirstOrDefault(p => p.name == a.name) ?? throw new Exception($"cannot find {a.name}")).ToArray(),
                assembly.compiledAssemblyReferences.Select(r =>
                {
                    if (haystack.TryGetValue(Path.GetFileName(r), out var foundIt)) return foundIt;
                    return r;
                }).ToArray(),
                assembly.flags,
                assembly.compilerOptions);
            assemblies.Add(result);
            return result;
        }

        static NPath GetDestinationDirectoryForManagedAssemblies(IncrementalClassicSharedData classicContext, bool useIl2Cpp)
        {
            return useIl2Cpp
                ? Configuration.RootArtifactsPath.Combine("managedassemblies")
                : classicContext.DataDeployDirectory.Combine("Managed");

        }

        private static Dictionary<Assembly, (CSharpProgram program, DotNetAssembly assembly)> SetupCSharpProgramsFor(BuildContext context, List<Assembly> assembliesToCompile)
        {
            var classicSharedData = context.GetValue<ClassicSharedData>();
            var extraScriptingDefines = classicSharedData.Customizers.SelectMany(c => c.ProvidePlayerScriptingDefines()).ToArray();

            var unityAssemblyToCSharpProgramAndBuiltAssembly = new Dictionary<Assembly, (CSharpProgram program, DotNetAssembly assembly)>();
            foreach (var assemblyToCompile in assembliesToCompile)
            {
                var referencedPrebuiltAssemblies = assemblyToCompile.compiledAssemblyReferences
                    //we're going to provide .net bcl ourselves
                    .Where(f => !IsBclAssembly(f))

                    //in the compilationgraph, for some reason assemblies like Assembly-CSharp.dll get returned as if they reference all editor assemblies. very annoying.
                    //fortunattely all these references follow a similar pattern, in that they are always referenced from Library/ScriptAssemblies.  we're going to filter these out too.
                    .Where(f => !f.Contains("Library/ScriptAssemblies/"))
                    .Select(Path.GetFullPath)
                    .ToNPaths();


                var mydefines = assemblyToCompile.defines.ToList();
                //todo: we shouldd respect the project's target framework settings
                mydefines.Remove("NET_4_6");
                var csharpProgram = new CSharpProgram()
                {
                    FileName = assemblyToCompile.name + ".dll",
                    Sources = { assemblyToCompile.sourceFiles.Select(Path.GetFullPath).ToNPaths() },
                    Defines = { mydefines, "ENABLE_INCREMENTAL_PIPELINE", extraScriptingDefines },
                    CopyReferencesNextToTarget = false,
                    References =
                    {
                        referencedPrebuiltAssemblies,
                        assemblyToCompile.assemblyReferences.Select(r => unityAssemblyToCSharpProgramAndBuiltAssembly[r].program)
                    },
                    LanguageVersion = "7.3",
                    IgnoredWarnings = { 169 },
                    WarningsAsErrors = false,
                    Framework = { Framework.NetStandard20 },
                    Unsafe = true,
                };

                var builtAssembly = csharpProgram.SetupDefault();
                unityAssemblyToCSharpProgramAndBuiltAssembly.Add(assemblyToCompile, (csharpProgram, builtAssembly));
            }

            return unityAssemblyToCSharpProgramAndBuiltAssembly;
        }

        private static void SetupTypeResourcesFor(DotNetAssembly[] playerAssemblies,
            NPath runtimeInitializeOnLoadOutputDir, NPath typeDBOutputDirectory)
        {
            var typeResources = new TypeResources(typeDBOutputDirectory);

            var runtimeInitializeOnLoadFiles = playerAssemblies
                .Select(a => typeResources.SetupTypeDBAndRuntimeInitializeOnLoadFileFor(a).runtimeInitializeOnLoadFile)
                .ToArray();

            typeResources.SetupConcatenateRuntimeInitOnLoad(runtimeInitializeOnLoadFiles, runtimeInitializeOnLoadOutputDir);
        }

        static bool IsEditorAssembly(Assembly assembly) => (assembly.flags & AssemblyFlags.EditorAssembly) != 0;

        static List<(DotNetAssembly dotNetAssembly, Assembly unityAssembly)> SetupPostProcessPlayerAssembliesFor(
            List<Assembly> assembliesToCompile, IncrementalClassicSharedData classicContext,
            List<Assembly> codeGenAssemblies,
            Dictionary<Assembly, (CSharpProgram program, DotNetAssembly dotNetAssembly)>
                unityAssemblyToCSharpProgramAndBuiltAssembly, bool useIl2Cpp)
        {
            var builtProcessors = codeGenAssemblies.Select(a => unityAssemblyToCSharpProgramAndBuiltAssembly[a].dotNetAssembly)
                .ToArray();
            var postProcessorOutputDirectory = useIl2Cpp
                ? Configuration.RootArtifactsPath.Combine("PostProcessorOutput")
                : GetDestinationDirectoryForManagedAssemblies(classicContext, useIl2Cpp);

            var postProcessor = new PostProcessor(builtProcessors, postProcessorOutputDirectory);

            List<(Assembly unityAssembly, DotNetAssembly dotNetAssembly)> compiledPlayerAssemblies = assembliesToCompile
                .Where(a => !a.IsCodeGenAssembly())
                .Select(a => (a, unityAssemblyToCSharpProgramAndBuiltAssembly[a].dotNetAssembly))
                .ToList();

            List<(DotNetAssembly, Assembly unityAssembly)> postProcessedPlayerAssemblies;
            using (new ProfilerMarker(nameof(postProcessor.SetupPostProcessorInvocation)).Auto())
                postProcessedPlayerAssemblies = compiledPlayerAssemblies
                    .Select(a => (postProcessor.SetupPostProcessorInvocation(a.dotNetAssembly), a.unityAssembly)).ToList();
            return postProcessedPlayerAssemblies;
        }

        static IEnumerable<NPath> SetupCopyPrebuiltAssemblies(IncrementalClassicSharedData context, NPath destinationDirectoryForManagedAssemblies, List<Assembly> assemblyGraph, bool useIl2Cpp)
        {
            var classLibraryProfile = useIl2Cpp ? "unityaot" : "unityjit";
            NPath bclDir = $"{EditorApplication.applicationContentsPath}/MonoBleedingEdge/lib/mono/{classLibraryProfile}";

            //there are four types of assemblies that need to go into the Managed folder.
            //1) The assemblies we manually compiled and postprocessed just now,
            //2) the .net class libraries that we're going to run on. Note this needs to be not the reference assemblies we compile against, but the assemblies for the runtime that we'll be running on.
            //3) the unity engine assemlies that we're going to be running on
            //4) any precompiled assemblies that live in the assets folder
            //
            //these different assemblies have to come from different places, and it's a bit annoying that the ICompiledAssembly interface doesn't give us much help to figure out for a given assembly's reference
            //which kind it is.  Let's start by taking all files from the correct .net framework profile (=2) plus the correct unityengine assemblies (=3)
            using (new ProfilerMarker("CopyManagedAssemblies").Auto())
            {
                var bclFiles = bclDir.Files(true);
                var unityEngineFiles = context.UnityEngineAssembliesDirectory.Files(true);
                var filesToCopy = bclFiles.Concat(unityEngineFiles).ToList();

                foreach (var file in filesToCopy)
                    yield return CopyTool.Instance().Setup(destinationDirectoryForManagedAssemblies.Combine(file.FileName), file);

                var copiedFilenames = new HashSet<string>(filesToCopy.Select(f => f.FileName));
                //now let's scan all other referenced assemblies. If we find one that has a filename that we didn't find in the bcl directory or the unityengine assemblies, then it must be a precompield assembly, in which
                //case we're going to copy that one too.
                foreach (var referencedPrecompiledAssembly in assemblyGraph
                    .Where(p => !p.IsCodeGenAssembly())
                    .SelectMany(a => a.compiledAssemblyReferences)
                    .Distinct()
                    .Where(f => !copiedFilenames.Contains(Path.GetFileName(f)))
                    .Where(f => !f.ToString().StartsWith("Library/"))
                    .ToNPaths()
                )
                {
                    yield return CopyTool.Instance()
                        .Setup(destinationDirectoryForManagedAssemblies.Combine(referencedPrecompiledAssembly.FileName),
                            referencedPrecompiledAssembly.MakeAbsolute());
                }
            }
        }

        private static bool IsBclAssembly(NPath f)
        {
            if (f.IsChildOf(s_monoBleedingEdgeLib))
                return true;
            if (f.IsChildOf(s_netStandardDir))
                return true;
            return false;
        }
    }

    public class BurstSettings
    {
        public BurstGranularity BurstGranularity { set; get; } = BurstGranularity.OnePerJob;

        public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();
    }

    public enum BurstGranularity
    {
        OnePerJob,
        One
    }

    public class GraphSetupCodeGenerationStep : BuildStepBase
    {
        public override BuildResult Run(BuildContext context)
        {
            new GraphSetupCodeGeneration().SetupCodegeneration(context, GraphSetupCodeGeneration.Mode.Full);
            return context.Success();
        }
    }

    static class AssemblyExtensions
    {
        public static bool IsCodeGenAssembly(this Assembly a) => a.name.EndsWith(".CodeGen");

        public static IEnumerable<Assembly> OrderByDependencies(this IEnumerable<Assembly> modules)
        {
            var processed = new List<Assembly>();
            var unprocessed = new List<Assembly>(modules);
            while (unprocessed.Any())
            {
                var clone = new List<Assembly>(unprocessed);
                bool processedOne = false;
                foreach (var module in clone)
                {
                    if (!module.assemblyReferences.All(d => processed.Contains(d)))
                        continue;
                    processedOne = true;
                    processed.Add(module);
                    unprocessed.Remove(module);
                }
                if (!processedOne)
                    throw new ArgumentException("OrderByDependencies: One or several of these assemblies have a cyclic dependency: " + unprocessed.Select(x => x.name).SeparateWithSpace());
            }

            return processed;
        }
    }
}
#endif
