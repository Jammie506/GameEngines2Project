#if ENABLE_EXPERIMENTAL_INCREMENTAL_PIPELINE
using NiceIO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Player;
using UnityEngine;

namespace Unity.Build.Classic.Private.IncrementalClassicPipeline
{
    public static class SlimBuildPipeline
    {
        internal static GUID k_UnityBuiltinResources = new GUID("0000000000000000e000000000000000");
        internal static GUID k_UnityBuiltinExtraResources = new GUID("0000000000000000f000000000000000");

        public static NPath TempDataPath = new NPath("Temp/SlimBuilds").MakeAbsolute().CreateDirectory();
        public static NPath TempStreamingAssetsPath = new NPath("Temp/SlimBuildsStreamingAssets").MakeAbsolute().CreateDirectory();

        static NPath[] GetResourcesAssetPaths()
        {
            var resourcesFolders = new List<NPath>();
            var assetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (var path in assetPaths)
            {
                if (!AssetDatabase.IsValidFolder(path))
                    continue;
                var lowerPath = path.ToLower();
                var dirName = new NPath(lowerPath).FileName;
                if (dirName != "resources")
                    continue;
                if (lowerPath.Contains(".bundle/") || lowerPath.Contains(".app/"))
                    continue;
                if (lowerPath.Contains("/editor/"))
                    continue;
                resourcesFolders.Add(path);
            }

            var resourcesAssets = new HashSet<NPath>();
            foreach (var folder in resourcesFolders)
            {
                var files = folder.Contents("*.*", true);
                foreach (var file in files)
                {
                    if (file.Extension == "meta" || AssetDatabase.IsValidFolder(file.ToString()))
                        continue;
                    var mainType = AssetDatabase.GetMainAssetTypeAtPath(file.ToString());
                    if (mainType == typeof(DefaultAsset))
                        continue;
                    resourcesAssets.Add(file);
                }
            }
            return resourcesAssets.ToArray();
        }

        public static NPath BuildSlimGameManagers(BuildTarget target, string[] scenes, bool development, string typeDBDirectory)
        {
            NPath tempPath;

            WriteManagerParameters mParams;
            WriteParameters wParams;
            WriteCommand[] writeCommands;
            PreloadInfo preloadInfo;

            // Setup
            {
                tempPath = TempDataPath;
                tempPath.CreateDirectory();
                tempPath.Combine("Resources").MakeAbsolute().CreateDirectory();

                //work around bug in unity's TypeDBHelper where it assumes Library/Type will exist
                new NPath("Library/Type").EnsureDirectoryExists();

                mParams = new WriteManagerParameters
                {
                    settings = new UnityEditor.Build.Content.BuildSettings
                    {
                        target = target,
                        group = UnityEditor.BuildPipeline.GetBuildTargetGroup(target),
                        buildFlags = development ? ContentBuildFlags.DevelopmentBuild : ContentBuildFlags.None,
                        typeDB = TypeDbHelper.GetForPlayer(typeDBDirectory)
                    },
                    globalUsage = ContentBuildInterface.GetGlobalUsageFromGraphicsSettings(),
                    referenceMap = new BuildReferenceMap()
                };

                wParams = new WriteParameters
                {
                    settings = mParams.settings,
                    globalUsage = mParams.globalUsage,
                    referenceMap = mParams.referenceMap,
                    usageSet = new BuildUsageTagSet()
                };

                writeCommands = new[]
                {
                    new WriteCommand
                    {
                        fileName = "unity_builtin_extra",
                        internalName = "Resources/unity_builtin_extra",
                        serializeObjects = new List<SerializationInfo>()
                    },
                    new WriteCommand
                    {
                        fileName = "globalgamemanagers.assets",
                        internalName = "globalgamemanagers.assets",
                        serializeObjects = new List<SerializationInfo>()
                    }
                };

                preloadInfo = new PreloadInfo
                {
                    preloadObjects = new List<ObjectIdentifier>()
                };
            }

            // Dependency Calculation
            {
                var dependencyResults = ContentBuildInterface.CalculatePlayerDependenciesForGameManagers(mParams.settings, mParams.globalUsage, wParams.usageSet);

                var referencedObjects = dependencyResults.referencedObjects.ToArray();
                var types = ContentBuildInterface.GetTypeForObjects(referencedObjects);

                for (int i = 0; i < referencedObjects.Length; i++)
                {
                    if (referencedObjects[i].guid == k_UnityBuiltinResources)
                    {
                        // unity default resources contain scripts that need to be preloaded
                        if (types[i] == typeof(MonoScript))
                            preloadInfo.preloadObjects.Add(referencedObjects[i]);

                        // Prebuild player specific default resources file, don't remap local identifiers
                        mParams.referenceMap.AddMapping("Library/unity default resources", referencedObjects[i].localIdentifierInFile, referencedObjects[i]);
                    }
                    else if (referencedObjects[i].guid == k_UnityBuiltinExtraResources)
                    {
                        if (types[i] == typeof(Shader))
                        {
                            var command = writeCommands[0];
                            // Resources/unity_builtin_extra
                            // Don't remap local identifiers
                            var info = new SerializationInfo
                            {
                                serializationObject = referencedObjects[i],
                                serializationIndex = referencedObjects[i].localIdentifierInFile
                            };

                            command.serializeObjects.Add(info);
                            mParams.referenceMap.AddMapping(command.internalName, info.serializationIndex, info.serializationObject);
                        }
                        else
                        {
                            var command = writeCommands[1];
                            // globalgamemanagers.assets
                            // Squash / Remap local identifiers, starting at 2 (PreloadData 1)
                            var info = new SerializationInfo
                            {
                                serializationObject = referencedObjects[i],
                                serializationIndex = command.serializeObjects.Count + 2
                            };

                            command.serializeObjects.Add(info);
                            mParams.referenceMap.AddMapping(command.internalName, info.serializationIndex, info.serializationObject);
                        }
                    }
                    else if (types[i] == typeof(MonoScript))
                    {
                        //globalgamemanagers.assets
                        //error because we can't support all the ggm features in slim builds
                    }
                    else
                    {
                        //globalgamemanagers.assets
                        //error because we can't support all the ggm features in slim builds
                    }
                }
            }

            // Writing globalgamemanagers
            {
                var writeResults = ContentBuildInterface.WriteGameManagersSerializedFile(tempPath.ToString(), mParams);
                EditorUtility.ClearProgressBar();
                //if (writeResults.serializedObjects.Count == 0) return "FAIL";
            }

            // Writing globalgamemanagers.assets
            {
                wParams.writeCommand = writeCommands[1];
                wParams.preloadInfo = preloadInfo;
                var writeResults = ContentBuildInterface.WriteSerializedFile(tempPath.ToString(), wParams);
                EditorUtility.ClearProgressBar();
                //if (writeResults.serializedObjects.Count == 0) return "FAIL";
            }

            // Writing unity_builtin_extra"
            {
                wParams.writeCommand = writeCommands[0];
                wParams.preloadInfo = null;

                // unity_builtin_extras requires absolutepath writing, so fixup the internalName and referenceMap for this
                wParams.writeCommand.internalName = tempPath.Combine(wParams.writeCommand.internalName).MakeAbsolute().ToString();
                wParams.referenceMap.AddMappings(wParams.writeCommand.internalName, wParams.writeCommand.serializeObjects.ToArray(), true);

                var writeResults = ContentBuildInterface.WriteSerializedFile(tempPath.Combine("Resources").ToString(), wParams);
                EditorUtility.ClearProgressBar();
                //if (writeResults.serializedObjects.Count == 0) return "FAIL";
            }

            {
                var parameters = new BundleBuildParameters(mParams.settings.target, mParams.settings.group, TempStreamingAssetsPath.ToString());
                parameters.BundleCompression = UnityEngine.BuildCompression.Uncompressed;
                parameters.ScriptInfo = mParams.settings.typeDB;

                // Writing scenes.bundle
                {
                    WriteSceneBundles(parameters, scenes);
                    //if (???) return "FAIL";
                }

                // Writing resources.bundle
                {
                    WriteResourcesBundles(parameters);
                    //if (???) return "FAIL";
                }

                {
                    WriteRenderPipelineBundles(parameters);
                    //if (???) return "FAIL";
                }
            }

            return tempPath;
        }

        static ReturnCode WriteSceneBundles(BundleBuildParameters parameters, string[] scenes)
        {
            var bundles = new AssetBundleBuild[1];
            bundles[0].assetBundleName = "scenes.bundle";
            bundles[0].assetNames = scenes;

            var content = new BundleBuildContent(bundles);

            return ContentPipeline.BuildAssetBundles(parameters, content, out var result);
        }

        static ReturnCode WriteRenderPipelineBundles(BundleBuildParameters parameters)
        {
            var srp = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
            if (srp == null)
                return ReturnCode.SuccessNotRun;

            var bundles = new AssetBundleBuild[1];
            bundles[0].assetBundleName = "renderpipeline.bundle";
            bundles[0].assetNames = new[] { AssetDatabase.GetAssetPath(UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline) };
            bundles[0].addressableNames = new[] { "DefaultRenderPipeline" };

            var content = new BundleBuildContent(bundles);

            return ContentPipeline.BuildAssetBundles(parameters, content, out var result);
        }

        static ReturnCode WriteResourcesBundles(BundleBuildParameters parameters)
        {
            // Resources loading path handling has a few rules:
            // - Paths are relative to resources folder
            // - Paths are lowercase
            // - Paths have extensions removed
            // So "C:\Project\Assets\MyGame\Resources\SO\SomeAsset.asset" becomes "so\someAsset"
            // On the loading side, all paths passed in have ToLower() run on them.
            var assetPaths = GetResourcesAssetPaths();
            var bundles = new AssetBundleBuild[]
            {
                    new AssetBundleBuild()
                    {
                        assetBundleName = "resources.bundle",
                        assetNames = assetPaths.Select(x => x.ToString()).ToArray(),
                        addressableNames = assetPaths.Select(x =>
                        {
                            const string k_Folder = "/resources/";
                            const int k_FolderLength = 11;

                            // Hack for PostProcessing stack
                            var shader = AssetDatabase.LoadAssetAtPath<Shader>(x.ToString());
                            if (shader != null)
                                return shader.name.ToLower();

                            var path = x.ChangeExtension("").ToString().ToLower();
                            var index = path.ToLower().LastIndexOf(k_Folder);
                            if (index > -1)
                                path = path.Substring(index + k_FolderLength);
                            return path;
                        }).ToArray()
                    }
            };

            var content = new BundleBuildContent(bundles);

            return ContentPipeline.BuildAssetBundles(parameters, content, out var result);
        }
    }
}
#endif
