using System;
using System.Collections.Generic;

namespace AssetBundles.AppleOnDemandResources
{
    [Serializable]
    public class CustomAssetPackDataEntry
    {
        public string AssetPackName;
        public List<string> AssetBundles;

        public CustomAssetPackDataEntry(string assetPackName, IEnumerable<string> assetBundles)
        {
            AssetPackName = assetPackName;
            AssetBundles = new List<string>(assetBundles);
        }
    }

    [Serializable]
    public class CustomAssetPackData
    {
        public List<CustomAssetPackDataEntry> Entries;

        public CustomAssetPackData(List<CustomAssetPackDataEntry> entries)
        {
            Entries = new List<CustomAssetPackDataEntry>(entries);
        }
    }
}
