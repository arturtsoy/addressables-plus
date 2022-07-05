#if UNITY_IOS

using System.IO;
using System.Linq;
using AssetBundles.AppleOnDemandResources.Editor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using Debug = UnityEngine.Debug;

namespace AssetBundles.Editor
{
    public partial class AssetPackBuilder
    {
	    public static string BuildPath => "Library/com.carx-tech.sr/StreamingAssetsCopy/";
	    
	    public static string GetLocalBuildPath()
	    {
		    var settings = AddressableAssetSettingsDefaultObject.Settings;
		    var profileSettings = settings.profileSettings;
		    var profileId = settings.activeProfileId;
		    var value = profileSettings.GetValueByName(profileId, AddressableAssetSettings.kLocalBuildPath);

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
			var bundles = GetBundles(buildPath); // buildPath = Library/com.unity.addressables/aa/Android/Android

			if (Directory.Exists(BuildPath))
			{
				Directory.Delete(BuildPath, true);
			}
			Directory.CreateDirectory(BuildPath);

			foreach (var bundle in bundles) // bundle = //Library/com.unity.addressables/aa/Android/Android\fastfollow_assets_all_2384a0162231cfa4bb37d5bf38510764.bundle
			{
				string targetPath = Path.Combine(BuildPath, bundle.Name);  // bundle.Name = fastfollow_assets_all_2384a0162231cfa4bb37d5bf38510764
				//Directory.CreateDirectory(targetPath);
				//string bundlePath = Path.Combine(targetPath, Path.GetFileNameWithoutExtension(bundle.Bundle));
				File.Copy(bundle.Bundle, targetPath);
				//assetPackConfig.AssetPacks.Add(bundle.Name, bundle.CreateAssetPack(textureCompressionFormat, bundlePath));
			}
			
			//WriteAssetPackConfig(bundles);
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