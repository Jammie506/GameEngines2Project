#if ENABLE_INCREMENTAL_PIPELINE
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.Build.SlimPlayerRuntime
{
    static class SlimRuntimeStartup
    {
        public static AssetBundle ScenesBundle;
        public static AssetBundle RenderPipelineBundle;

        public static string[] Scenes { get; internal set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void InitializeFirstSceneCallback()
        {
            ScenesBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/scenes.bundle");
            Scenes = ScenesBundle.GetAllScenePaths();

            RenderPipelineBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/renderpipeline.bundle");
            if (RenderPipelineBundle != null)
                UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline = RenderPipelineBundle.LoadAsset("DefaultRenderPipeline") as UnityEngine.Rendering.RenderPipelineAsset;

            SceneManagerApiOverriding.LoadFirstSceneFunc = LoadFirstScene;
            SceneManagerApiOverriding.SceneCountFunc = SceneCount;
            SceneManagerApiOverriding.IndexToSceneNameFunc = IndexToSceneName;
        }

        public static AsyncOperation LoadFirstScene(bool async)
        {
            if (async)
                return SceneManager.LoadSceneAsync(Scenes[0]);
            SceneManager.LoadScene(Scenes[0]);
            return null;
        }

        public static int SceneCount()
        {
            return Scenes.Length;
        }

        public static string IndexToSceneName(int buildIndex)
        {
            if (buildIndex > 0 && buildIndex < Scenes.Length)
                return Scenes[buildIndex];
            return "";
        }

        // ------------------------------------------------------------

        public static AssetBundle ResourcesBundle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void InitializeResourcesCallback()
        {
            ResourcesBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/resources.bundle");
            ResourcesApiOverriding.LoadFunc = Load;
            ResourcesApiOverriding.LoadAllFunc = LoadAll;
            ResourcesApiOverriding.LoadAsyncFunc = LoadAsync;
            ResourcesApiOverriding.FindObjectsOfTypeAllFunc = FindObjectsOfTypeAll;
            ResourcesApiOverriding.UnloadFunc = Unload;
        }

        private static Object Load(string path, System.Type systemTypeInstance)
        {
            return ResourcesBundle.LoadAsset(path.ToLower(), systemTypeInstance);
        }

        private static Object[] LoadAll(string path, System.Type systemTypeInstance)
        {
            if (string.IsNullOrEmpty(path))
                return ResourcesBundle.LoadAllAssets(systemTypeInstance);
            return ResourcesBundle.LoadAssetWithSubAssets(path.ToLower(), systemTypeInstance);
        }

        private static ResourceRequest LoadAsync(string path, System.Type systemTypeInstance)
        {
            return ResourcesBundle.LoadAssetAsync(path.ToLower(), systemTypeInstance);
        }

        private static Object[] FindObjectsOfTypeAll(System.Type systemTypeInstance)
        {
            return ResourcesBundle.LoadAllAssets(systemTypeInstance);
        }

        private static void Unload(Object assetToUnload)
        {
            // Do nothing
        }
    }
}
#endif
