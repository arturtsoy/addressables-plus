#if UNITY_ANDROID
using System.Linq;
using UnityEngine;

namespace AssetBundles.GooglePlayAssetDelivery
{
    public class AssetPackBundleConfig : ScriptableObject
    {
        public const string FILENAME = "AssetPacks";
        public const string PATH = "Assets/Resources/AssetPacks.asset";

        public string[] packs;
        
        public bool IsPack(string assetPackName)
        {
            return (packs?.Contains(assetPackName)).GetValueOrDefault(false);
        }
    }
}
#endif