/*
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.iOS;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.ResourceManagement.Util;

namespace AssetBundles.AppleOnDemandResources.Editor
{
    /// <summary>
    /// In addition to the Default Build Script behavior (building AssetBundles), this script assigns Android bundled content to "install-time" or "on-demand" custom asset packs
    /// specified in <see cref="CustomAssetPackSettings"/>.
    ///
    /// We will create the config files necessary for creating an asset pack (see https://docs.unity3d.com/Manual/play-asset-delivery.html#custom-asset-packs).
    /// The files are:
    /// * An {asset pack name}.androidpack folder located in 'Assets/AssetBundles/AppleOnDemandResources/Build/CustomAssetPackContent'
    /// * A 'build.gradle' file for each .androidpack folder. If this file is missing, Unity will assume that the asset pack uses "on-demand" delivery.
    ///
    /// Additionally we generate some files to store build and runtime data that are located in in 'Assets/OnDemandResources/Build':
    /// * Create a 'BuildProcessorData.json' file to store the build paths and .androidpack paths for bundles that should be assigned to custom asset packs.
    /// At build time this will be used by the <see cref="OnDemandResourcesBuildProcessor"/> to relocate bundles to their corresponding .androidpack paths.
    /// * Create a 'CustomAssetPacksData.json' file to store custom asset pack information to be used at runtime. See <see cref="OnDemandResourcesInitialization"/>.
    ///
    /// We assign any content marked for "install-time" delivery to the generated asset packs. In most cases the asset pack containing streaming assets will use "install-time" delivery,
    /// but in large projects it may use "fast-follow" delivery instead. For more information see https://docs.unity3d.com/Manual/play-asset-delivery.html#generated-asset-packs.
    ///
    /// Because <see cref="AddressablesPlayerBuildProcessor"/> moves all Addressables.BuildPath content to the streaming assets path, any content in that directory
    /// will be included in the generated asset packs even if they are not marked for "install-time" delivery.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildScriptAppleOnDemandResources.asset", menuName = "Addressables/Custom Build/Apple On-Demand Resources")]
    public class BuildScriptOnDemandResources : BuildScriptPackedMode
    {
        /// <inheritdoc/>
        public override string Name
        {
            get { return "Apple On-Demand Resources Z"; }
        }
        
        protected override TResult DoBuild<TResult>(AddressablesDataBuilderInput builderInput, AddressableAssetsBuildContext aaContext)
        {
            Debug.Log("[ODR] BuildScriptOnDemandResources.DoBuild Begin");
            
            TResult result = base.DoBuild<TResult>(builderInput, aaContext);

            if ( builderInput.Target != BuildTarget.iOS)
            {
                Addressables.LogWarning("Build target is not set to Android. No custom asset pack config files will be created.");
                return result;
            }
            
            Debug.Log("[ODR] BuildScriptOnDemandResources.DoBuild End");

            return result;
        }
        
        [InitializeOnLoadMethod]
        private static void RegisterScriptableBuildCallbacks()
        {
            BuildScript.buildCompleted += BuildCompleted;
        }
        
        private static void BuildCompleted(AddressableAssetBuildResult result)
        {
            Debug.Log($"[ODR] BuildScriptOnDemandResources.BuildCompleted");
        }
        
#if ENABLE_IOS_ON_DEMAND_RESOURCES

        [InitializeOnLoadMethod]
        private static void SetupResourcesBuild()
        {
	        UnityEditor.iOS.BuildPipeline.collectResources += CollectResources;
        }

        private static UnityEditor.iOS.Resource[] CollectResources() 
        {
            //var res = new Resource("pair.Key", "assetBundleDirectory + pair.Key").AddOnDemandResourceTags("pair.Key");
            Debug.Log("[ODR] BuildScriptOnDemandResources.CollectResources");
	        
	        System.Collections.Generic.List<UnityEditor.iOS.Resource> result = new System.Collections.Generic.List<UnityEditor.iOS.Resource>();

	        // foreach (string tag in resources)
	        // {
		       //  result.Add(new UnityEditor.iOS.Resource(tag, assetBundleDirectoryIOS + tag).AddOnDemandResourceTags(tag));
	        // }
	        
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

        /// <inheritdoc/>
        public override void ClearCachedData()
        {
            base.ClearCachedData();
            try
            {
                ClearJsonFiles();
                ClearBundlesInAssetsFolder();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void ClearBundlesInAssetsFolder()
        {
            if (AssetDatabase.IsValidFolder(CustomAssetPackUtility.PackContentRootDirectory))
            {
                // Delete all bundle files in 'Assets/OnDemandResources/Build/CustomAssetPackContent'
                List<string> bundleFiles = Directory.EnumerateFiles(CustomAssetPackUtility.PackContentRootDirectory, "*.bundle", SearchOption.AllDirectories).ToList();
                
                foreach (string file in bundleFiles)
                {
	                AssetDatabase.DeleteAsset(file);
                }
            }
        }

        private void ClearJsonFiles()
        {
            // Delete "CustomAssetPacksData.json"
            if (File.Exists(CustomAssetPackUtility.CustomAssetPacksDataEditorPath))
            {
	            AssetDatabase.DeleteAsset(CustomAssetPackUtility.CustomAssetPacksDataEditorPath);
            }
            
            if (File.Exists(CustomAssetPackUtility.CustomAssetPacksDataRuntimePath))
            {
                File.Delete(CustomAssetPackUtility.CustomAssetPacksDataRuntimePath);
                File.Delete(CustomAssetPackUtility.CustomAssetPacksDataRuntimePath + ".meta");
                CustomAssetPackUtility.DeleteDirectory(Application.streamingAssetsPath, true);
            }

            // Delete "BuildProcessorData.json"
            if (File.Exists(CustomAssetPackUtility.BuildProcessorDataPath))
            {
	            AssetDatabase.DeleteAsset(CustomAssetPackUtility.BuildProcessorDataPath);
            }
        }

        private void CreateCustomAssetPacks(AddressableAssetSettings settings, CustomAssetPackSettings customAssetPackSettings, bool resetAssetPackSchemaData)
        {
            List<CustomAssetPackEditorInfo> customAssetPacks = customAssetPackSettings.CustomAssetPacks;
            Dictionary<string, CustomAssetPackDataEntry> assetPackToDataEntry = new Dictionary<string, CustomAssetPackDataEntry>();
            Dictionary<string, BuildProcessorDataEntry> bundleIdToEditorDataEntry = new Dictionary<string, BuildProcessorDataEntry>();

            CreateBuildOutputFolders();

            foreach (AddressableAssetGroup group in settings.groups)
            {
                if (HasRequiredSchemas(settings, group))
                {
                    OnDemandResourcesSchema assetPackSchema = group.GetSchema<OnDemandResourcesSchema>();
                    // Reset schema data to match Custom Asset Pack Settings. This can occur when the CustomAssetPackSettings was deleted but the schema properties still use the old settings data.
                    if (resetAssetPackSchemaData || assetPackSchema.AssetPackIndex >= customAssetPacks.Count)
                    {
	                    assetPackSchema.ResetAssetPackIndex();
                    }

                    CustomAssetPackEditorInfo assetPack = customAssetPacks[assetPackSchema.AssetPackIndex];
                    if (IsAssignedToCustomAssetPack(settings, group, assetPackSchema, assetPack))
                    {
	                    CreateConfigFiles(group, assetPack.AssetPackName, assetPackToDataEntry, bundleIdToEditorDataEntry);
                    }
                    
                    BundledAssetGroupSchema bundledSchema = group.GetSchema<BundledAssetGroupSchema>();
                    string buildPath = bundledSchema.BuildPath.GetValue(settings);
                    //bundledSchema.m_AssetBundleProviderType = new SerializedType();
                }
            }

            // Create the bundleIdToEditorDataEntry. It contains information for relocating custom asset pack bundles when building a player.
            SerializeBuildProcessorData(bundleIdToEditorDataEntry.Values.ToList());

            // Create the CustomAssetPacksData.json file. It contains all custom asset pack information that can be used at runtime.
            SerializeCustomAssetPacksData(assetPackToDataEntry.Values.ToList());
        }

        private void CreateBuildOutputFolders()
        {
            // Create the 'Assets/AssetBundles/AppleOnDemandResources/Build' directory
            if (!AssetDatabase.IsValidFolder(CustomAssetPackUtility.BuildRootDirectory))
            {
	            AssetDatabase.CreateFolder(CustomAssetPackUtility.RootDirectory, CustomAssetPackUtility.kBuildFolderName);
            }
            else
            {
	            ClearJsonFiles();
            }

            // Create the 'Assets/OnDemandResources/Build/CustomAssetPackContent' directory
            if (!AssetDatabase.IsValidFolder(CustomAssetPackUtility.PackContentRootDirectory))
            {
	            AssetDatabase.CreateFolder(CustomAssetPackUtility.BuildRootDirectory, CustomAssetPackUtility.kPackContentFolderName);
            }
            else
            {
	            ClearBundlesInAssetsFolder();
            }
        }

        private bool BuildPathIncludedInStreamingAssets(string buildPath)
        {
            return buildPath.StartsWith(Addressables.BuildPath) || buildPath.StartsWith(Application.streamingAssetsPath);
        }

        private string ConstructAssetPackDirectoryName(string assetPackName)
        {
            return $"{assetPackName}.androidpack";
        }

        private string CreateAssetPackDirectory(string assetPackName)
        {
            string folderName = ConstructAssetPackDirectoryName(assetPackName);
            string path = Path.Combine(CustomAssetPackUtility.PackContentRootDirectory, folderName).Replace("\\", "/");

            if (!AssetDatabase.IsValidFolder(path))
            {
	            AssetDatabase.CreateFolder(CustomAssetPackUtility.PackContentRootDirectory, folderName);
            }
            return path;
        }

        private bool HasRequiredSchemas(AddressableAssetSettings settings, AddressableAssetGroup group)
        {
            bool hasBundledSchema = group.HasSchema<BundledAssetGroupSchema>();
            bool hasPADSchema = group.HasSchema<OnDemandResourcesSchema>();

            if (!hasBundledSchema && !hasPADSchema)
            {
	            return false;
            }
            
            if (!hasBundledSchema && hasPADSchema)
            {
                Addressables.LogWarning($"Group '{group.name}' has a '{typeof(OnDemandResourcesSchema).Name}' but not a '{typeof(BundledAssetGroupSchema).Name}'. " +
                    $"It does not contain any bundled content to be assigned to an asset pack.");
                return false;
            }
            
            if (hasBundledSchema && !hasPADSchema)
            {
                BundledAssetGroupSchema bundledSchema = group.GetSchema<BundledAssetGroupSchema>();
                string buildPath = bundledSchema.BuildPath.GetValue(settings);
                
                if (BuildPathIncludedInStreamingAssets(buildPath))
                {
                    Addressables.Log($"Group '{group.name}' does not have a '{typeof(OnDemandResourcesSchema).Name}' but its build path '{buildPath}' will be included in StreamingAssets at build time. " +
                        $"The group will be assigned to the generated asset packs unless its build path is changed.");
                }
                return false;
            }
            return true;
        }

        private bool IsAssignedToCustomAssetPack(AddressableAssetSettings settings, AddressableAssetGroup group, OnDemandResourcesSchema schema, CustomAssetPackEditorInfo assetPack)
        {
            if (!schema.IncludeInAssetPack)
            {
                BundledAssetGroupSchema bundledSchema = group.GetSchema<BundledAssetGroupSchema>();
                string buildPath = bundledSchema.BuildPath.GetValue(settings);
                
                if (BuildPathIncludedInStreamingAssets(buildPath))
                {
                    Addressables.LogWarning($"Group '{group.name}' has 'Include In Asset Pack' disabled, but its build path '{buildPath}' will be included in StreamingAssets at build time. " +
                        $"The group will be assigned to the streaming assets pack.");
                }
                return false;
            }

            return true;
        }

        private void CreateConfigFiles(AddressableAssetGroup group, string assetPackName, Dictionary<string, CustomAssetPackDataEntry> assetPackToDataEntry, Dictionary<string, BuildProcessorDataEntry> bundleIdToEditorDataEntry)
        {
            foreach (AddressableAssetEntry entry in group.entries)
            {
	            if (bundleIdToEditorDataEntry.ContainsKey(entry.BundleFileId))
	            {
		            continue;
	            }

                string bundleBuildPath = AddressablesRuntimeProperties.EvaluateString(entry.BundleFileId); // {UnityEngine.AddressableAssets.Addressables.RuntimePath}/iOS/odr-first_assets_all_a3147d48508f6a6d539436e1cee0ebb7.bundle
                string bundleName = Path.GetFileNameWithoutExtension(bundleBuildPath); // Library/com.unity.addressables/aa/iOS/iOS/odr-first_assets_all_a3147d48508f6a6d539436e1cee0ebb7.bundle

                if (!assetPackToDataEntry.ContainsKey(assetPackName))
                {
                    // Create .androidpack directory and gradle file for the asset pack
                    assetPackToDataEntry[assetPackName] = new CustomAssetPackDataEntry(assetPackName, new List<string>() { bundleName }); //bundleName = odr-first_assets_all_a3147d48508f6a6d539436e1cee0ebb7
                    //string androidPackDir = CreateAssetPackDirectory(assetPackName);
                    //CreateOrEditGradleFile(androidPackDir, assetPackName);
                }
                else
                {
                    // Otherwise just save the bundle to asset pack data
                    assetPackToDataEntry[assetPackName].AssetBundles.Add(bundleName);
                }

                // Store the bundle's build path and its corresponding .androidpack folder location
                string bundlePackDir = ConstructAssetPackDirectoryName(assetPackName); // bundlePackDir = InstallTimeContent.androidpack
                string assetsFolderPath = Path.Combine(bundlePackDir, Path.GetFileName(bundleBuildPath)); //assetsFolderPath = InstallTimeContent.androidpack\odr-first_assets_all_a3147d48508f6a6d539436e1cee0ebb7.bundle
                bundleIdToEditorDataEntry.Add(entry.BundleFileId, new BuildProcessorDataEntry(bundleBuildPath, assetsFolderPath));
            }
        }

        private void SerializeBuildProcessorData(List<BuildProcessorDataEntry> entries)
        {
            BuildProcessorData customPackEditorData = new BuildProcessorData(entries);
            string contents = JsonUtility.ToJson(customPackEditorData); // CustomAssetPackUtility.BuildProcessorDataPath = Assets/AssetBundles/AppleOnDemandResources/Build\BuildProcessorData.json
            File.WriteAllText(CustomAssetPackUtility.BuildProcessorDataPath, contents); // contents = {"Entries":[{"BundleBuildPath":"Library/com.unity.addressables/aa/iOS/iOS/odr-first_assets_all_a3147d48508f6a6d539436e1cee0ebb7.bundle","AssetsSubfolderPath":"InstallTimeContent.androidpack\\odr-first_assets_all_a3147d48508f6a6d539436e1cee0ebb7.bundle"}]}
        }

        private void SerializeCustomAssetPacksData(List<CustomAssetPackDataEntry> entries)
        {
            CustomAssetPackData customPackData = new CustomAssetPackData(entries);
            string contents = JsonUtility.ToJson(customPackData); // CustomAssetPackUtility.CustomAssetPacksDataEditorPath = Assets/AssetBundles/AppleOnDemandResources/Build\CustomAssetPacksData.json
            File.WriteAllText(CustomAssetPackUtility.CustomAssetPacksDataEditorPath, contents); // contents = {"Entries":[{"AssetPackName":"InstallTimeContent","AssetBundles":["odr-first_assets_all_a3147d48508f6a6d539436e1cee0ebb7"]}]}
        }
    }
}
*/
