#if UNITY_IOS
using System.IO;
using UnityEngine.iOS;

namespace AssetBundles.AppleOnDemandResources.ResourceHandlers
{
    public abstract class AssetPackAssetBundleResourceHandlerBase : AssetBundleResourceHandlerBase
    {
        protected OnDemandResourcesRequest playAssetPackRequest;

        protected override bool IsValidPath(string path)
        {
            return true; // TODO: check path
            // Only handle local bundles
            // if (!path.StartsWith(Addressables.RuntimePath) || !path.EndsWith(".bundle"))
            // {
            //     return false;
            // }
            // return AddressablesAssetDelivery.IsPack(Path.GetFileNameWithoutExtension(path));
        }

        protected override float PercentComplete()
        {
            return ((playAssetPackRequest?.progress ?? .0f) + (m_RequestOperation?.progress ?? .0f)) * .5f;
        }

        protected override void BeginOperation(string path)
        {
            BeginOperationImpl(Path.GetFileNameWithoutExtension(path));
        }

        protected abstract void BeginOperationImpl(string assetPackName);
        
        public override void Unload()
        {
            base.Unload();
            if (playAssetPackRequest != null)
            {
                //playAssetPackRequest.Dispose(); // TODO: .AttemptCancel()
                playAssetPackRequest = null;
            }
        }
    }
}
#endif