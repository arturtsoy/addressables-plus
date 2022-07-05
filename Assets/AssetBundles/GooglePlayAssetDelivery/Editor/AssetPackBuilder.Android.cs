#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Android.AppBundle.Editor;
using Google.Android.AppBundle.Editor.AssetPacks;
using Google.Android.AppBundle.Editor.Internal;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace AssetBundles.GooglePlayAssetDelivery.Editor
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
	    
	    /**
	     * Create the Play Asset Delivery configuration.
	     */
	    public static AssetPackConfig CreateAssetPacks(TextureCompressionFormat textureCompressionFormat, string buildPath = null)
		{
			if (string.IsNullOrEmpty(buildPath))
			{
				buildPath = GetLocalBuildPath();
			}
			Debug.LogFormat("[PAD] {0}.{1} path={2}", nameof(AssetPackBuilder), nameof(CreateAssetPacks), buildPath);
			AssetPackConfig assetPackConfig = new AssetPackConfig
			{
				DefaultTextureCompressionFormat = textureCompressionFormat
			};
			if (!Directory.Exists(buildPath))
			{
				return null;
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
				Directory.CreateDirectory(targetPath);
				string bundlePath = Path.Combine(targetPath, Path.GetFileNameWithoutExtension(bundle.Bundle));
				File.Copy(bundle.Bundle, bundlePath);
				assetPackConfig.AssetPacks.Add(bundle.Name, bundle.CreateAssetPack(textureCompressionFormat, bundlePath));
			}
			
			WriteAssetPackConfig(bundles);
			AssetPackConfigSerializer.SaveConfig(assetPackConfig);
			return assetPackConfig;
		}

	    public static bool BuildBundleWithAssetPacks(BuildPlayerOptions buildPlayerOptions, MobileTextureSubtarget mobileTextureSubtarget, string buildPath)
	    {
		    TextureCompressionFormat textureCompressionFormat = ToTextureCompressionFormat(mobileTextureSubtarget); 
		    return BuildBundleWithAssetPacks(buildPlayerOptions, textureCompressionFormat, buildPath);
	    }

	    public static TextureCompressionFormat ToTextureCompressionFormat(MobileTextureSubtarget mobileTextureSubtarget)
	    {
		    switch (mobileTextureSubtarget)
		    {
			    case MobileTextureSubtarget.Generic:
				    return TextureCompressionFormat.Default;
			    case MobileTextureSubtarget.DXT:
				    return TextureCompressionFormat.Dxt1;
			    case MobileTextureSubtarget.PVRTC:
				    return TextureCompressionFormat.Pvrtc;
			    case MobileTextureSubtarget.ETC:
				    return TextureCompressionFormat.Etc1;
			    case MobileTextureSubtarget.ETC2:
				    return TextureCompressionFormat.Etc2;
			    case MobileTextureSubtarget.ASTC:
				    return TextureCompressionFormat.Astc;
			    default:
				    throw new ArgumentOutOfRangeException(nameof(mobileTextureSubtarget), mobileTextureSubtarget, null);
		    }
	    }
	    
	    public static bool BuildBundleWithAssetPacks(BuildPlayerOptions buildPlayerOptions, TextureCompressionFormat textureCompressionFormat, string buildPath)
	    {
		    if (buildPlayerOptions.target != BuildTarget.Android)
		    {
			    return false;
		    }
		    return AppBundlePublisher.Build(buildPlayerOptions, CreateAssetPacks(textureCompressionFormat, buildPath), true);
	    }

	    internal static AssetPackBundle[] GetBundles(string path, SearchOption searchOption = SearchOption.TopDirectoryOnly)
	    {
		    return Directory.GetFiles(path, "*.bundle", searchOption)
			    .Select(file => new AssetPackBundle(file, GetAssetPackGroupSchema(file)))
			    .Where(pack => pack.IsValid)
			    .ToArray();
	    }

	    /**
	     * Determine the AssetPackGroupSchema associated with this asset bundle.
	     * 
	     * @param string path of the bundle
	     */
		private static AssetPackGroupSchema GetAssetPackGroupSchema(string bundle)
		{
			return AddressableAssetSettingsDefaultObject.Settings.groups
				.Where(group => group.HasSchema<AssetPackGroupSchema>())
				.Where(group => Path.GetFileName(bundle).StartsWith(FormatGroupName(group)))
				.Select(group => group.GetSchema<AssetPackGroupSchema>())
				.FirstOrDefault();
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

		public static void ClearConfig()
		{
			AssetDatabase.DeleteAsset(AssetPackBundleConfig.PATH);
		}
    }
}
#endif