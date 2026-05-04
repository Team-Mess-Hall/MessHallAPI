using MelonLoader.Utils;
using UnityEngine;
using static MessHallAPI.Debugger.Logging;

namespace MessHallAPI.Managers
{
    /// <summary>
    /// Contains Two RPC's RequestLoadAssetBundle and RequestLoadSceneBundle
    /// </summary>
    public class AssetBundleLoader
    {
        /// <summary>
        /// Dictionary containing loaded Assets Bundles
        /// </summary>
        public static Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();
        /// <summary>
        /// Dictionary containing loaded Scene Bundles
        /// </summary>
        public static Dictionary<string, AssetBundle> sceneBundles = new Dictionary<string, AssetBundle>();

        /// <summary>
        /// Requests an Asset Bundle to be loaded
        /// </summary>
        /// <param name="bundlename">the name of the bundle file with out the .assetbundle</param>
        /// <param name="ModName">name of the mod folder in UserData so for example "MessHallAPI" would be UserData/MessHallAPI</param>
        public static void RequestLoadAssetBundle(string bundlename, string ModName)
        {
            string basePath = Path.Combine(MelonEnvironment.UserDataDirectory, ModName);
            string assetsPath = Path.Combine(basePath, bundlename);
            assetBundles[bundlename] = AssetBundle.LoadFromFile(assetsPath);
            if (assetBundles[bundlename] == null)
            {
                Error($"Failed to load {bundlename}");
                return;
            }
        }

        /// <summary>
        /// Requests a Scene Bundle to be loaded (an asset bundle that contains only scenes)
        /// </summary>
        /// <param name="sceneBundlename">the name of the bundle file with out the .assetbundle</param>
        /// <param name="Modname">name of the mod folder in UserData so for example "MessHallAPI" would be UserData/MessHallAPI</param>
        public static void RequestLoadSceneBundle(string sceneBundlename, string Modname)
        {
            string basePath = Path.Combine(MelonEnvironment.UserDataDirectory, Modname);
            string scenebundlepath = Path.Combine(basePath, sceneBundlename);
            sceneBundles[sceneBundlename] = AssetBundle.LoadFromFile(scenebundlepath);
            if (sceneBundles[sceneBundlename] == null)
            {
                Error($"Failed to load {sceneBundlename}");
                return;
            }
        }
    }
}
