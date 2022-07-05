/*using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.iOS;
using BuildPipeline = UnityEditor.BuildPipeline;

public class BuildAssetBundlesEditor
{
	private static string assetBundleDirectory = "Assets/AssetBundles/";
	
	[MenuItem("Tools/Bundles/Build - ODR (iOS)")]
	private static void Build_iOS()
	{
		BuildTarget buildTarget = BuildTarget.iOS;
		BuildAssetBundleOptions options = BuildAssetBundleOptions.None;
		
		assetBundleDirectory = "Assets/AssetBundles/Build/ODR/";
		
#if ENABLE_IOS_ON_DEMAND_RESOURCES
		if (PlayerSettings.iOS.useOnDemandResources)
		{
			options |= BuildAssetBundleOptions.UncompressedAssetBundle;
		}
#endif
		
#if ENABLE_IOS_APP_SLICING
		options |= BuildAssetBundleOptions.UncompressedAssetBundle;
#endif
		
		if (!Directory.Exists(assetBundleDirectory))
		{
			Directory.CreateDirectory(assetBundleDirectory);
		}
		
		BuildPipeline.BuildAssetBundles(assetBundleDirectory, options, buildTarget);
	}

	[MenuItem("Tools/Bundles/Build Asset Bundles")]
	private static void Build()
	{
		BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
		assetBundleDirectory = "Assets/AssetBundles/Build/";
		BuildAssetBundleOptions options = BuildAssetBundleOptions.None;
		if (!Directory.Exists(assetBundleDirectory))
		{
			Directory.CreateDirectory(assetBundleDirectory);
		}

		BuildPipeline.BuildAssetBundles(assetBundleDirectory, options, buildTarget);
	}

	[InitializeOnLoadMethod]
	private static void SetupResourcesBuild()
	{
		UnityEditor.iOS.BuildPipeline.collectResources += CollectResources;
	}

	private static Dictionary<string, string> resources = new Dictionary<string, string>() 
	{
		{"odrpack", ""},
	};
	
	private static Resource[] CollectResources()
	{
		List<Resource> result = new List<Resource>();

		foreach (KeyValuePair<string,string> pair in resources)
		{
			result.Add(new Resource(pair.Key, assetBundleDirectory + pair.Key).AddOnDemandResourceTags(pair.Key));
			//result.Add(new Resource(pair.Key, assetBundleDirectory + pair.Key + ".unity3d").AddOnDemandResourceTags(pair.Key)); - not work
		}

		return result.ToArray();
	}
}*/