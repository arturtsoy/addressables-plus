using System;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using AsyncOperation = UnityEngine.AsyncOperation;

namespace AssetBundles.AppleOnDemandResources.ResourceProviders
{
    [DisplayName("Apple Asset Bundle Provider")]
    public class AppleAssetBundleProvider : AssetBundleProvider
    {
        #if UNITY_IOS
        private ProvideHandle m_provideHandle;
        private AssetBundle m_assetBundle;
        private UnityEngine.iOS.OnDemandResourcesRequest m_request;

        public override void Provide(ProvideHandle provideHandle)
        {
            m_provideHandle = provideHandle;
            
            string bundleName = Path.GetFileNameWithoutExtension(m_provideHandle.Location.InternalId);
            
            if (!OnDemandResourcesRuntimeData.Instance.BundleNameToAssetPack.ContainsKey(bundleName))
            {
                // Bundle is either assigned to the generated asset packs, or not assigned to any asset pack
                base.Provide(m_provideHandle);
            }
            else
            {
                // string path = m_provideHandle.ResourceManager.TransformInternalId(m_provideHandle.Location);
                // string assetPackName = Path.GetFileNameWithoutExtension(path);
                
                // Bundle is assigned to a custom fast-follow or on-demand asset pack
                string assetPackName = OnDemandResourcesRuntimeData.Instance.BundleNameToAssetPack[bundleName].AssetPackName;
                if (OnDemandResourcesRuntimeData.Instance.AssetPackNameToDownloadPath.ContainsKey(assetPackName))
                {
                    // Asset pack is already downloaded
                    base.Provide(m_provideHandle);
                }
                else
                {
                    // Download the asset pack
                    
                    string tag = "odr"; // TODO: Replace tag

                    m_provideHandle.SetProgressCallback(PercentComplete);
                    m_request = UnityEngine.iOS.OnDemandResources.PreloadAsync(new[] { tag });
                    m_request.completed += request => OnRequestCompleted(tag, request);
                }
            }
        }

        private float PercentComplete()
        {
            return m_request?.progress ?? 0.0f;
        }
        
        private void OnRequestCompleted(string tag, AsyncOperation asyncOperation)
        {
            if (m_request.error != null)
            { 
                Debug.LogError("[Assets] LoadAsset Error: " + m_request.error);
                m_provideHandle.Complete(this, false, new Exception("ODR request failed: " + m_request.error));
                return;
            }
            
            AssetBundleCreateRequest requestOperation = AssetBundle.LoadFromFileAsync("res://" + tag);
            if (requestOperation.isDone)
            {
                AssetBundle assetBundle = (asyncOperation as AssetBundleCreateRequest)?.assetBundle;
                m_provideHandle.Complete(this, assetBundle != null, null);
                return;
            }
            
            requestOperation.completed += operation =>
            {
                AssetBundle assetBundle = (operation as AssetBundleCreateRequest)?.assetBundle;
                m_provideHandle.Complete(this, assetBundle != null, null);
            };
        }

        public override void Release(IResourceLocation location, object asset)
        {
            base.Release(location, asset);
            m_provideHandle = default;
            
            if (m_request != null)
            {
                m_request.Dispose();
                m_request = null;
            }
        }
#endif
    }
}