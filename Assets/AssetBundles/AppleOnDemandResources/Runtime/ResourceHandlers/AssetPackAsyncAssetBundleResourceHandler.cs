#if UNITY_IOS
using UnityEngine;
using UnityEngine.iOS;

namespace AssetBundles.AppleOnDemandResources.ResourceHandlers
{
    public class AssetPackAsyncAssetBundleResourceHandler : AssetPackAssetBundleResourceHandlerBase
    {
        protected override void BeginOperationImpl(string assetPackName)
        {
            Debug.Log($"[ODR] AssetPackAsyncAssetBundleResourceHandler.BeginOperation assetPackName: {assetPackName}");
            playAssetPackRequest = OnDemandResources.PreloadAsync(new[] { assetPackName });
            
            if (playAssetPackRequest.isDone)
            {
                Debug.Log($"[ODR] AssetPackAsyncAssetBundleResourceHandler.BeginOperation PreloadAsync isDone assetPackName: {assetPackName}");
                OnPlayAssetPackRequestCompleted(assetPackName, playAssetPackRequest);
                return;
            }
            playAssetPackRequest.completed += request =>
            {
                Debug.Log($"[ODR] AssetPackAsyncAssetBundleResourceHandler.BeginOperation PreloadAsync completed assetPackName: {assetPackName}");
                OnPlayAssetPackRequestCompleted(assetPackName, request as OnDemandResourcesRequest);
            };
        }

        private void OnPlayAssetPackRequestCompleted(string assetPackName, OnDemandResourcesRequest request)
        {
            Debug.Log($"[ODR] AssetPackAsyncAssetBundleResourceHandler.OnPlayAssetPackRequestCompleted assetPackName: {assetPackName}");

            if (!string.IsNullOrEmpty(request.error))
            {
                Debug.Log($"[ODR] AssetPackAsyncAssetBundleResourceHandler.OnPlayAssetPackRequestCompleted assetPackName: {assetPackName} Error: {request.error}");

                CompleteOperation(this, $"Error downloading error pack: {request.error}");
                return;
            }
            // if (request.Status != AssetDeliveryStatus.Available)
            // {
            //     CompleteOperation(this, $"Error downloading status: {request.Status}");
            //     return;
            // }
            //var assetLocation = request.GetAssetLocation(assetPackName);
            //AddressablesAssetDelivery.AssociateAssetPackNameWithAssetBundlePath(assetPackName, assetLocation.Path);
            //m_RequestOperation = AssetBundle.LoadFromFileAsync(assetLocation.Path, /* crc= */ 0, assetLocation.Offset);
            string path = "res://" + assetPackName;
            Debug.Log($"[ODR] AssetPackAsyncAssetBundleResourceHandler.OnPlayAssetPackRequestCompleted AssetBundle.LoadFromFileAsync: {path}");
            m_RequestOperation = AssetBundle.LoadFromFileAsync(path);
            
            if (m_RequestOperation.isDone)
            {
                Debug.Log($"[ODR] AssetPackAsyncAssetBundleResourceHandler.OnPlayAssetPackRequestCompleted AssetBundle.LoadFromFileAsync isDone: {path}");

                LocalMRequestOperationCompleted(m_RequestOperation);
                return;
            }
            m_RequestOperation.completed += LocalMRequestOperationCompleted;
        }
        
        private void LocalMRequestOperationCompleted(AsyncOperation op)
        {
            AssetBundle assetBundle = (op as AssetBundleCreateRequest)?.assetBundle;
            Debug.Log($"[ODR] AssetPackAsyncAssetBundleResourceHandler.LocalMRequestOperationCompleted AssetBundle.LoadFromFileAsync assetBundle: {assetBundle}");

            CompleteOperation(this, assetBundle);
        }
    }
}
#endif