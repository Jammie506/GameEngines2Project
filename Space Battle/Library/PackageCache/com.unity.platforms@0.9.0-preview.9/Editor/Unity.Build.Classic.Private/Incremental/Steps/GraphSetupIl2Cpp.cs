#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using Bee.DotNet;
using NiceIO;
using System;
using System.Collections.Generic;
using System.Linq;
using Bee.Core;
using Bee.NativeProgramSupport;
using UnityEditor;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    internal class Il2CppInputAssemblies
    {
        internal List<NPath> prebuiltAssemblies { set; get; }
        internal List<(DotNetAssembly dotNetAssembly, UnityEditor.Compilation.Assembly unityAssembly)> processedAssemblies { set; get; }
    }

    public class GraphSetupIl2Cpp : BuildStepBase
    {
        private CodeGen ToCodeGen(Il2CppCompilerConfiguration il2CppCompilerConfiguration)
        {
            switch(il2CppCompilerConfiguration)
            {
                case Il2CppCompilerConfiguration.Debug: return CodeGen.Debug;
                case Il2CppCompilerConfiguration.Release: return CodeGen.Release;
                case Il2CppCompilerConfiguration.Master: return CodeGen.Master;
                default:
                    throw new ArgumentException(nameof(il2CppCompilerConfiguration));
            }
        }
        public override BuildResult Run(BuildContext context)
        {
            // TODO: Move to IsEnabled
            if (!context.UsesIL2CPP())
                return context.Success();

            var sharedData = context.GetValue<IncrementalClassicSharedData>();
            var input = context.GetValue<Il2CppInputAssemblies>();
    
            if (!context.TryGetComponent(out ClassicScriptingSettings scriptingSettings))
                throw new ArgumentException("IL2CPP Compiler Configuration was not set on BuildContext");

            var workingDirectory = context.GetValue<ClassicSharedData>().WorkingDirectory;

            var il2CppBeeSuport = new IL2CPPBeeSupport(sharedData);

            var il2CppFiles = il2CppBeeSuport.SetupIl2CppOutputFiles(
                sharedData.BuildTarget,
                input.prebuiltAssemblies,
                input.processedAssemblies.Select(p => p.dotNetAssembly).ToList(),
                sharedData.IL2CPPDataDirectory,
                workingDirectory);

            var platformSupport = context.GetValue<IL2CPPPlatformBeeSupport>();
            foreach (var a in sharedData.Architectures.Values)
            {
                var toolChain = a.ToolChain ?? throw new ArgumentException("ToolChain was not set on BuildContext");
                var format = a.NativeProgramFormat;

                var npc = new NativeProgramConfiguration(ToCodeGen(scriptingSettings.Il2CppCompilerConfiguration), toolChain, false);
                var nativeProgramForIl2CppOutput = il2CppBeeSuport.NativeProgramForIL2CPPOutputFor(
                    npc.ToolChain.Platform is AndroidPlatform ? "libil2cpp" : "GameAssembly",
                    platformSupport,
                    il2CppFiles);

                nativeProgramForIl2CppOutput.RTTI.Set(toolChain.EnablingExceptionsRequiresRTTI);

                var builtNativeProgram = nativeProgramForIl2CppOutput.SetupSpecificConfiguration(npc, format);

                builtNativeProgram.DeployTo(a.IL2CPPLibraryDirectory);
            }

            return context.Success();
        }
    }
}
#endif
