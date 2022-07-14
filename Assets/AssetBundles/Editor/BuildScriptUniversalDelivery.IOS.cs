#if UNITY_IOS

using System.IO;
using AssetBundles.AppleOnDemandResources.Editor;
using AssetBundles.GooglePlayAssetDelivery.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AssetBundles.Editor
{
    public partial class BuildScriptUniversalDelivery : BuildScriptPackedMode
    {
        // public override bool CanBuildData<T>()
        // {
        //     return typeof(T).IsAssignableFrom(typeof(AddressablesPlayerBuildResult));
        // }
        protected override string ProcessAllGroups(AddressableAssetsBuildContext aaContext)
        {
            Debug.Log($"[ODR] ProcessAllGroups");
            return base.ProcessAllGroups(aaContext);
        }

        protected override string ProcessBundledAssetSchema(
            BundledAssetGroupSchema schema,
            AddressableAssetGroup assetGroup,
            AddressableAssetsBuildContext aaContext)
        {
            Debug.Log($"[ODR] ProcessBundledAssetSchema");

            PlatformSchema platformSchema = assetGroup.GetSchema<PlatformSchema>();

            if (platformSchema != null)
            {
#if UNITY_IOS
                schema.IncludeInBuild = platformSchema.IsSupport(PlatformSchema.Platform.iOS);
#elif UNITY_ANDROID
                schema.IncludeInBuild = platformSchema.IsSupport(PlatformSchema.Platform.Android);
#endif
            }
            else
            {
                bool platformSchemaIOS = assetGroup.GetSchema<OnDemandResourcesSchema>() != null;
                bool platformSchemaAndroid = assetGroup.GetSchema<AssetPackGroupSchema>() != null;

                if (platformSchemaIOS && platformSchemaAndroid || 
                    !platformSchemaIOS && !platformSchemaAndroid)
                {
                    schema.IncludeInBuild = true;
                }
#if UNITY_IOS
                else if (platformSchemaIOS)
#elif UNITY_ANDROID
                else if (platformSchemaAndroid)
#endif
                {
                    schema.IncludeInBuild = true;
                }
            }

            return base.ProcessBundledAssetSchema(schema, assetGroup, aaContext);
        }

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