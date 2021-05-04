#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using Bee.Core;
using Bee.DotNet;
using NiceIO;
using System.Collections.Generic;
using Bee.CSharpSupport;
using Unity.Serialization.Json;
using UnityEditor.Compilation;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    class ScriptAssembliesJsonGenerator
    {
        class ScriptAssemblies
        {
            public ScriptAssemblies(int size)
            {
                names = new List<string>(size);
                types = new List<int>(size);
            }

            public List<string> names;
            public List<int> types;
        }

        private static readonly NPath jsonFileName = "ScriptingAssemblies.json";
        private const int k_UnityAssemblyType = 2;
        private const int k_UserAssemblyType = 16;

        public static void Setup(Dictionary<Assembly, (CSharpProgram program, DotNetAssembly dotNetAssembly)> unityAssemblyToCSharpProgramAndBuiltAssembly, NPath buildDataPath)
        {
            var unityAssemblyPaths = CompilationPipeline.GetPrecompiledAssemblyPaths(CompilationPipeline.PrecompiledAssemblySources.UnityEngine | CompilationPipeline.PrecompiledAssemblySources.UnityAssembly);
            var userAssemblyPaths = CompilationPipeline.GetPrecompiledAssemblyPaths(CompilationPipeline.PrecompiledAssemblySources.UserAssembly);

            var scriptAssemblies = new ScriptAssemblies(unityAssemblyPaths.Length + userAssemblyPaths.Length + unityAssemblyToCSharpProgramAndBuiltAssembly.Count);

            foreach (NPath assemblyPath in unityAssemblyPaths)
            {
                scriptAssemblies.names.Add(assemblyPath.FileName);
                scriptAssemblies.types.Add(k_UnityAssemblyType);
            }

            foreach (NPath assemblyPath in userAssemblyPaths)
            {
                scriptAssemblies.names.Add(assemblyPath.FileName);
                scriptAssemblies.types.Add(k_UserAssemblyType);
            }

            foreach (var assembly in unityAssemblyToCSharpProgramAndBuiltAssembly.Keys)
            {
                NPath assemblyPath = assembly.outputPath;
                scriptAssemblies.names.Add(assemblyPath.FileName);
                scriptAssemblies.types.Add(k_UserAssemblyType);
            }

            var serialize = JsonSerialization.ToJson(scriptAssemblies, new JsonSerializationParameters
            {
                DisableRootAdapters = true,
                SerializedType = typeof(ScriptAssemblies)
            });
            Backend.Current.AddWriteTextAction(buildDataPath.Combine(jsonFileName), serialize, "ScriptingAssembliesJson");
        }
    }
}
#endif
