#if UNITY_IOS

using System;
using System.IO;
using AssetBundles.AppleOnDemandResources;
using AssetBundles.AppleOnDemandResources.Editor;
using Google.Android.AppBundle.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
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
            //     
            // if (assetPackConfig == null)
            // {
            //     EditorUtility.DisplayDialog("Create config for Addressables Groups", "Unable to create AssetPack config. Make sure Addressables asset bundles have been build!", "Ok");
            // }
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

            //string buildPath = GetLocalBuildPath(); // buildPath = Library/com.unity.addressables/aa/iOS/iOS

            string buildPath = AssetPackBuilder.BuildPath;
            
            Debug.LogFormat($"[ODR] Collect from path: {buildPath}");
			
            if (!Directory.Exists(buildPath))
            {
                Debug.LogError($"[ODR] Directory not exist: {buildPath}");
                result.ToArray();
            }
			
            string[] bundlesPaths = Directory.GetFiles(buildPath, "*.bundle", SearchOption.TopDirectoryOnly);
            //string[] bundlesPaths = GetBundles(buildPath);

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
		    var settings = AddressableAssetSettingsDefaultObject.Settings;
		    var profileSettings = settings.profileSettings;
		    var profileId = settings.activeProfileId;
		    var value = profileSettings.GetValueByName(profileId, AddressableAssetSettings.kLocalBuildPath);

		    return profileSettings.EvaluateString(profileId, value);
	    }

        private static string[] GetBundles(string path, SearchOption searchOption = SearchOption.TopDirectoryOnly)
	    {
		    return Directory.GetFiles(path, "*.bundle", searchOption);
	    }
        
        private static UnityEditor.iOS.Resource[] CollectResources2() 
        {
            //var res = new Resource("pair.Key", "assetBundleDirectory + pair.Key").AddOnDemandResourceTags("pair.Key");
            Debug.Log("[ODR] CollectResources");
	        
            System.Collections.Generic.List<UnityEditor.iOS.Resource> result = new System.Collections.Generic.List<UnityEditor.iOS.Resource>();

            try
            {
                AssetDatabase.StartAssetEditing();
                
                if (File.Exists(CustomAssetPackUtility.BuildProcessorDataPath)) // "Assets/AssetBundles/AppleOnDemandResources/Build\BuildProcessorData.json"
                {
                    string contents = File.ReadAllText(CustomAssetPackUtility.BuildProcessorDataPath);
                    BuildProcessorData data =  JsonUtility.FromJson<BuildProcessorData>(contents);

                    foreach (BuildProcessorDataEntry entry in data.Entries)
                    {
                        if (File.Exists(entry.BundleBuildPath)) // "Library/com.unity.addressables/aa/iOS/iOS/odr-first_assets_all_a3147d48508f6a6d539436e1cee0ebb7.bundle"
                        {
                            string tag = Path.GetFileNameWithoutExtension(entry.BundleBuildPath);
                            result.Add(new UnityEditor.iOS.Resource(tag, entry.BundleBuildPath).AddOnDemandResourceTags(tag));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception occured when moving data for an app bundle build: {e.Message}.");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            
            return result.ToArray();
        }

#endif
    }
}
#endif