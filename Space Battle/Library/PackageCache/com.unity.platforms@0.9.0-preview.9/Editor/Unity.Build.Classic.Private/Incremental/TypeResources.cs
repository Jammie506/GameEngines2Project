#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using Bee.Core;
using Bee.DotNet;
using NiceIO;
using System.Linq;
using Bee.Tools;
using UnityEditor;


namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    /// <summary>
    /// In order for the data build to support scenarios where the user has #ifdef around monobehaviour fields,  we need to know which fields exist in the assemblies-compiled-for-player
    /// and how they differ from the data we have in the editor. This information about which fields exist in unity code is often referred to as the TypeDB. We've added support
    /// to unity to construct its typeDB for the serialization process from a directory containing a bunch of json files.  This buildcode is responsible for adding the nodes
    /// to the bee graph responsible for generating these typedb .json files.   After they are generated, we will do the data build.
    ///
    /// Kind of by accident we have another process that also requires scanning all assemblies: determining which types have an [RuntimeInitializeOnLoad] attribute. We used
    /// to store this in a globalgamemanager data file, but that was poorly suited to the incremental buildpipeline that would like the globalgamemanager data file not to have
    /// to change on any c# change.  So this infomration moved to a json file as well. The same external tool that we add to the graph here that is responsible for generating the
    /// typeDB files will also generate a RuntimeInitializeOnLoad.json file, since it's processing the assembly data anyway.
    ///
    /// For the runtime initialize on load .json files, in the build process, we generate one per assembly that we compile. After all of those have been generated
    /// we run the same tool in a different mode, to concatenate the inidividual json files into one big one, so that the runtime player code doesn't have to do that itself.
    ///
    /// The tool that we add to the graph to generate these files is a tool that ships with the unity editor, since the non incremental pipeline uses this technique as well.
    ///
    /// A pecurliar part of this setup is that at the time of this writing, we do not generate individual .json files for untiy's own assemblies (UnityEngine.dll), nor for the
    /// prebuilt assemblies that a user has in their project. We rely on the fact that the unity editor generates .json files for these already in the Library/Type directory
    /// of the project.
    /// </summary>

    class TypeResources
    {
        public NPath TypeDBOutputDirectory { get; }
        private DotNetAssembly _processorRunner;
        private DotNetAssembly[] _builtProcessorsDependenciesAndSelf;
        private readonly DotNetRunnableProgram _typeGeneratorExecutable;

        private const string FinalConcatenatedRuntimeInitializeOnLoadFileName = "RuntimeInitializeOnLoads.json";

        private static NPath EditorCreatedRuntimeFilesForUnityAssemblies => new NPath("Library/Type/RuntimeInitOnLoad-buildAssemblies.json").MakeAbsolute();
        private static NPath EditorCreatedRuntimeFilesForUserPrecompiledAssemblies => new NPath("Library/Type/RuntimeInitOnLoad-userPrecompiledAssemblies.json").MakeAbsolute();

        public TypeResources(NPath typeDbOutputDirectory)
        {
            TypeDBOutputDirectory = typeDbOutputDirectory;
            var typeResources = new DotNetAssembly($"{EditorApplication.applicationContentsPath}/Tools/TypeGenerator/TypeGenerator.exe", Framework.Framework471);
            _typeGeneratorExecutable = new DotNetRunnableProgram(typeResources, DotNetRuntime.FindFor(typeResources));
        }

        public NPath SetupConcatenateRuntimeInitOnLoad(NPath[] perAssemblyRuntimeInitOnLoads, NPath dataDeployDir)
        {
            var concatenatedOutputFile = dataDeployDir.Combine(FinalConcatenatedRuntimeInitializeOnLoadFileName);
            var inputFiles = perAssemblyRuntimeInitOnLoads.Append(EditorCreatedRuntimeFilesForUnityAssemblies, EditorCreatedRuntimeFilesForUserPrecompiledAssemblies).ToArray();
            var args = new []
            {
                "-c", inputFiles.InQuotes().SeparateWithComma(),
                "-o", concatenatedOutputFile.InQuotes()
            };

            Backend.Current.AddAction($"ConcatenateRuntimeInitOnLoadJsons",
                new[] { concatenatedOutputFile},
                inputFiles,
                executableStringFor: _typeGeneratorExecutable.InvocationString,
                args,
                supportResponseFile: true
            );

            return concatenatedOutputFile;
        }

        public (NPath runtimeInitializeOnLoadFile, NPath typeDBfile) SetupTypeDBAndRuntimeInitializeOnLoadFileFor(DotNetAssembly inputAsm)
        {
            var searchPathsOneLine = inputAsm.RecursiveRuntimeDependenciesIncludingSelf.Select(x => x.Path.Parent).Distinct().InQuotes().SeparateWithComma();
            var inputAssemblyFileName = inputAsm.Path.FileNameWithoutExtension;

            var args = new[]
            {
                "-a", inputAsm.Path.InQuotes(),
                "-s", searchPathsOneLine,
                "-o", TypeDBOutputDirectory.InQuotes(),
                "-n", inputAssemblyFileName.InQuotes(),
                "-r", "True"
            };

            var runtimeInitializeOnLoadFile = TypeDBOutputDirectory.Combine($"RuntimeInitOnLoad-{inputAssemblyFileName}.json");
            var typeDBfile = TypeDBOutputDirectory.Combine($"TypeDb-{inputAssemblyFileName}.json");

            Backend.Current.AddAction($"TypeGenerator",
                new[]
                {
                    typeDBfile,
                    runtimeInitializeOnLoadFile,
                },
                inputAsm.Paths,
                executableStringFor: _typeGeneratorExecutable.InvocationString,
                args,
                supportResponseFile: true
            );

            return (runtimeInitializeOnLoadFile: runtimeInitializeOnLoadFile, typeDBfile: typeDBfile);
        }
    }
}
#endif
