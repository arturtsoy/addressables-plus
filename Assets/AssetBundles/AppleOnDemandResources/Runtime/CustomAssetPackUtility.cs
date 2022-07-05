using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AssetBundles.AppleOnDemandResources
{
	public class CustomAssetPackUtility
    {
        public const string kBuildFolderName = "Build";
        public const string kPackContentFolderName = "CustomAssetPackContent";

        public const string kBuildProcessorDataFilename = "BuildProcessorData.json";
        public const string kCustomAssetPackDataFilename = "CustomAssetPacksData.json";

        public static string RootDirectory
        {
            get { return $"Assets/AssetBundles/AppleOnDemandResources"; }
        }

        public static string BuildRootDirectory
        {
            get { return $"{RootDirectory}/{kBuildFolderName}"; }
        }

        public static string PackContentRootDirectory
        {
            get { return $"{BuildRootDirectory}/{kPackContentFolderName}"; }
        }

        public static string BuildProcessorDataPath
        {
            get { return Path.Combine(BuildRootDirectory, kBuildProcessorDataFilename); }
        }

        public static string CustomAssetPacksDataEditorPath
        {
            get { return Path.Combine(BuildRootDirectory, kCustomAssetPackDataFilename); }
        }

        public static string CustomAssetPacksDataRuntimePath
        {
            get { return Path.Combine(Application.streamingAssetsPath, kCustomAssetPackDataFilename); }
        }

#if UNITY_EDITOR
        public static void DeleteDirectory(string directoryPath, bool onlyIfEmpty)
        {
            bool isEmpty = !Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories).Any()
                && !Directory.EnumerateDirectories(directoryPath, "*", SearchOption.AllDirectories).Any();
            if (!onlyIfEmpty || isEmpty)
            {
                // check if the folder is valid in the AssetDatabase before deleting through standard file system
                string relativePath = directoryPath.Replace("\\", "/").Replace(Application.dataPath, "Assets");
                if (UnityEditor.AssetDatabase.IsValidFolder(relativePath))
                    UnityEditor.AssetDatabase.DeleteAsset(relativePath);
                else
                    Directory.Delete(directoryPath, true);
            }
        }
#endif
    }
}
