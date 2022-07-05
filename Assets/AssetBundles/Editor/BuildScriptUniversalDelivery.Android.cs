#if UNITY_ANDROID

using AssetBundles.GooglePlayAssetDelivery.Editor;
using Google.Android.AppBundle.Editor;
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
            Debug.Log($"[PAD] DoBuild Begin");
            // Build AssetBundles
            TResult result = base.DoBuild<TResult>(builderInput, aaContext);

            // Don't prepare content for asset packs if the build target isn't set to Android
            if (builderInput.Target != BuildTarget.Android)
            {
                Addressables.LogWarning("Build target is not set to Android. No custom asset pack config files will be created.");
                return result;
            }
            
            Debug.Log($"[PAD] DoBuild End");

            return result;
        }
        
        [InitializeOnLoadMethod]
        private static void RegisterScriptableBuildCallbacks()
        {
            BuildScript.buildCompleted += BuildCompleted;
        }
        
        private static void BuildCompleted(AddressableAssetBuildResult result)
        {
            Debug.Log($"[PAD] BuildCompleted");
            AssetPackConfig assetPackConfig = AssetPackBuilder.CreateAssetPacks(TextureCompressionFormat.Default);
                
            if (assetPackConfig == null)
            {
                EditorUtility.DisplayDialog("Create config for Addressables Groups", "Unable to create AssetPack config. Make sure Addressables asset bundles have been build!", "Ok");
            }
        }
    }
}
#endif