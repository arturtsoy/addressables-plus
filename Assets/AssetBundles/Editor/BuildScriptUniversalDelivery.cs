using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEngine;

namespace AssetBundles.Editor
{
    [CreateAssetMenu(fileName = "BuildScriptUniversalDelivery.asset", menuName = "Addressables/Custom Build/Universal Delivery")]
    public partial class BuildScriptUniversalDelivery : BuildScriptPackedMode
    {
        public override string Name => "Universal Delivery";
    }
}