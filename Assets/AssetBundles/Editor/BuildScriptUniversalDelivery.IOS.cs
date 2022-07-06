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

            foreach (var bundlePath in bundlesPaths) // bundle = //Library/com.unity.addressables/aa/Android/Android\fastfollow_assets_all_2384a0162231cfa4bb37d5bf38510764.bundle
            {
                result.Add(new UnityEditor.iOS.Resource("odr", bundlePath).AddOnDemandResourceTags("odr"));
                Debug.LogFormat($"[ODR] AddOnDemandResourceTags for: {buildPath}");
            }
			
            Debug.Log("[ODR] CollectResources Complete!");
            return result.ToArray();
        }

        private static string GetLocalBuildPath()
	    {
            return AssetPackBuilder.BuildPath;
            
            /*//buildPath = Library/com.unity.addressables/aa/iOS/iOS
		    var settings =  UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
		    var profileSettings = settings.profileSettings;
		    var profileId = settings.activeProfileId;
		    var value = profileSettings.GetValueByName(profileId, UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.kLocalBuildPath);
		    
		    return profileSettings.EvaluateString(profileId, value);*/

	    }
#endif
    }
}
#endif