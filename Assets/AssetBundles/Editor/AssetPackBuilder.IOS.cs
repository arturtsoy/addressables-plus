#if UNITY_IOS

using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetBundles.AppleOnDemandResources;
using AssetBundles.AppleOnDemandResources.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AssetBundles.Editor
{
    public partial class AssetPackBuilder
    {
	    public static string BuildPath => "Library/com.carxtech.sr/StreamingAssetsCopy/";
	    
	    public static string GetLocalBuildPath()
	    {
		    AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
		    AddressableAssetProfileSettings profileSettings = settings.profileSettings;
		    string profileId = settings.activeProfileId;
		    string value = profileSettings.GetValueByName(profileId, AddressableAssetSettings.kLocalBuildPath);

		    return profileSettings.EvaluateString(profileId, value);
	    }
	    
		private static OnDemandResourcesSchema GetAssetPackGroupSchema(string bundle)
		{
			return AddressableAssetSettingsDefaultObject.Settings.groups
				.Where(group => group.HasSchema<OnDemandResourcesSchema>())
				.Where(group => Path.GetFileName(bundle).StartsWith(FormatGroupName(group)))
				.Select(group => group.GetSchema<OnDemandResourcesSchema>())
				.FirstOrDefault();
		}
		
		public static void CreateAssetPacks(string buildPath = null)
		{
			if (string.IsNullOrEmpty(buildPath))
			{
				buildPath = GetLocalBuildPath();
			}
			Debug.LogFormat("[AssetBundles] {0}.{1} path={2}", nameof(AssetPackBuilder), nameof(CreateAssetPacks), buildPath);

			if (!Directory.Exists(buildPath))
			{
				return;
			}
			AssetPackBundle[] bundles = GetBundles(buildPath); // buildPath = Library/com.unity.addressables/aa/Android/Android

			if (Directory.Exists(BuildPath))
			{
				Directory.Delete(BuildPath, true);
			}
			Directory.CreateDirectory(BuildPath);

			foreach (AssetPackBundle bundle in bundles) // bundle = //Library/com.unity.addressables/aa/iOS/iOS/fastfollow_assets_all_2384a0162231cfa4bb37d5bf38510764.bundle
			{
				string targetPath = Path.Combine(BuildPath, bundle.Name);  // bundle.Name = fastfollow_assets_all_2384a0162231cfa4bb37d5bf38510764
				//Directory.CreateDirectory(targetPath);
				//string bundlePath = Path.Combine(targetPath, Path.GetFileNameWithoutExtension(bundle.Bundle));
				File.Copy(bundle.Bundle, targetPath);
				//assetPackConfig.AssetPacks.Add(bundle.Name, bundle.CreateAssetPack(textureCompressionFormat, bundlePath));
			}
			
			WriteAssetPackConfig(bundles);
			// AssetPackConfigSerializer.SaveConfig(assetPackConfig);
			// return assetPackConfig;
			Debug.LogFormat("[AssetBundles] {0}.{1} Complete", nameof(AssetPackBuilder), nameof(CreateAssetPacks));
		}

		private static AssetPackBundle[] GetBundles(string path, SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			return Directory.GetFiles(path, "*.bundle", searchOption)
				.Select(file => new AssetPackBundle(file, GetAssetPackGroupSchema(file)))
				.Where(pack => pack.IsValid)
				.ToArray();
		}

		private static string FormatGroupName(AddressableAssetGroup assetGroup)
		{
			// Keep in sync with Library/PackageCache/com.unity.addressables@x.x.x/Editor/Build/DataBuilders/BuildScriptPackedMode.cs#819
			return assetGroup.Name.Replace(" ", "").Replace('\\', '/').Replace("//", "/").ToLower();
		}
		
		private static void WriteAssetPackConfig(IEnumerable<AssetPackBundle> packBundles)
		{
			AssetPackBundleConfig config = GetOrCreateConfig();
			config.packs = packBundles.Select(pack => pack.Name).ToArray();
			Debug.LogFormat("[{0}.{1}] path={2}", nameof(AssetPackBuilder), nameof(WriteAssetPackConfig), AssetPackBundleConfig.PATH);
			EditorUtility.SetDirty(config);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
		
		private static AssetPackBundleConfig GetOrCreateConfig()
		{
			AssetPackBundleConfig config = AssetDatabase.LoadAssetAtPath<AssetPackBundleConfig>(AssetPackBundleConfig.PATH);
			if (config == null)
			{
				config = ScriptableObject.CreateInstance<AssetPackBundleConfig>();
				var basePath = Path.GetDirectoryName(AssetPackBundleConfig.PATH);
				if (!Directory.Exists(basePath))
				{
					Directory.CreateDirectory(basePath);
				}
				AssetDatabase.CreateAsset(config, AssetPackBundleConfig.PATH);
			}
			return config;
		}
    }

    internal struct AssetPackBundle
    {
	    private const string BUNDLE_SUFFIX = ".bundle";
	    private const string CATALOG_BUNDLE = "catalog.bundle";
        
	    public string Name { get; }
	    public string Bundle { get; }
	    public OnDemandResourcesSchema Schema { get; }

	    public bool IsValid => Schema != null && Bundle.EndsWith(BUNDLE_SUFFIX) && !Bundle.EndsWith(CATALOG_BUNDLE);

	    public AssetPackBundle(string bundle, OnDemandResourcesSchema schema)
	    {
		    Name = Path.GetFileName(bundle);
		    Bundle = bundle;
		    Schema = schema;
	    }

	    public void DeleteFile()
	    {
		    File.Delete(Bundle);
	    }
    }
}
#endif