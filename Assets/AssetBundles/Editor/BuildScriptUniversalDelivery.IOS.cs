#if UNITY_IOS

using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AssetBundles.Editor
{
    public partial class BuildScriptUniversalDelivery : BuildScriptPackedMode
    {
        protected override TResult DoBuild<TResult>(AddressablesDataBuilderInput builderInput, AddressableAssetsBuildContext aaContext)
        {
            Debug.Log($"[ODR] BuildScriptUniversalDelivery.IOS.DoBuild Begin");
            
            TResult result = base.DoBuild<TResult>(builderInput, aaContext);

            if ( builderInput.Target != BuildTarget.iOS)
            {
                Addressables.LogWarning("Build target is not set to Android. No custom asset pack config files will be created.");
                return result;
            }
            
            Debug.Log($"[ODR] BuildScriptUniversalDelivery.IOS.DoBuild End");

            return result;
        }
        
        [InitializeOnLoadMethod]
        private static void RegisterScriptableBuildCallbacks()
        {
            BuildScript.buildCompleted += BuildCompleted;
        }
        
        private static void BuildCompleted(AddressableAssetBuildResult result)
        {
            Debug.Log($"[ODR] BuildCompleted");
            AssetPackBuilder.CreateAssetPacks();
        }
        
#if ENABLE_IOS_ON_DEMAND_RESOURCES

        [InitializeOnLoadMethod]
        private static void SetupResourcesBuild()
        {
	        UnityEditor.iOS.BuildPipeline.collectResources += CollectResources;
        }

        private static UnityEditor.iOS.Resource[] CollectResources()
        {
            Debug.Log("[ODR] CollectResources Begin");
	        
            System.Collections.Generic.List<UnityEditor.iOS.Resource> result = new System.Collections.Generic.List<UnityEditor.iOS.Resource>();
            string buildPath = GetLocalBuildPath();
            
            Debug.LogFormat($"[ODR] Collect from path: {buildPath}");
			
            if (!Directory.Exists(buildPath))
            {
                Debug.LogError($"[ODR] Directory not exist: {buildPath}");
                result.ToArray();
            }
			
            string[] bundlesPaths = Directory.GetFiles(buildPath, "*.bundle", SearchOption.TopDirectoryOnly);

            foreach (string bundlePath in bundlesPaths)
            {
                string bundleName = Path.GetFileNameWithoutExtension(bundlePath); // bundle = Library/com.carxtech.sr/StreamingAssetsCopy/world2_assets_all_c271ef7acf3a528b089eeb7ae296ed6b.bundle
                Debug.LogFormat($"[ODR] AddOnDemandResourceTags for: name: '{bundleName}' with path '{bundlePath}'");
                result.Add(new UnityEditor.iOS.Resource(bundleName, bundlePath).AddOnDemandResourceTags(bundleName));
            }
			
            Debug.Log("[ODR] CollectResources Complete!");
            return result.ToArray();
        }

        private static string GetLocalBuildPath()
	    {
            return AssetPackBuilder.BuildPath;
        }
#endif
    }
}
#endif