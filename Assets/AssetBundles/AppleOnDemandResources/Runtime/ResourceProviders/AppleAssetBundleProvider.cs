using System;
using System.ComponentModel;
using System.IO;
using UnityEngine;
using AssetBundles.ResourceProviders;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using AsyncOperation = UnityEngine.AsyncOperation;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace AssetBundles.AppleOnDemandResources.ResourceProviders
{
    [DisplayName("Apple Asset Bundle Provider")]
    public class AppleAssetBundleProvider : AssetBundleProvider
    {
        private ProvideHandle m_provideHandle;
        private AssetBundle m_assetBundle;

		public AssetBundle GetAssetBundle()
        {
            /*if (m_assetBundle == null)
            {
                if (m_downloadHandler != null)
                {
                    m_assetBundle = m_downloadHandler.assetBundle;
                    m_downloadHandler.Dispose();
                    m_downloadHandler = null;
                }
                else if (m_RequestOperation is AssetBundleCreateRequest)
                {
                    m_assetBundle = (m_RequestOperation as AssetBundleCreateRequest).assetBundle;
                }
            }*/
            return m_assetBundle;
        }

        //#if UNITY_IOS
        private OnDemandResourcesRequest m_request;
        private UniversalAssetBundleProvider m_provider;

        public void Initialize(UniversalAssetBundleProvider provider)
        {
            m_provider = provider;
        }
        public override void Provide(ProvideHandle provideHandle)
        {
            m_provideHandle = provideHandle;
            
            string bundleName = Path.GetFileNameWithoutExtension(m_provideHandle.Location.InternalId);
            
            Debug.Log($"[ODR] AppleAssetBundleProvider.Provide bundleName: {bundleName}");

            string tag = bundleName;

            m_provideHandle.SetProgressCallback(PercentComplete);
            m_request = UnityEngine.iOS.OnDemandResources.PreloadAsync(new[] { tag });
            
            m_request.completed += request =>
            {
                OnRequestCompleted(tag, request);
            };
        }
		
		public override Type GetDefaultType(IResourceLocation location)
        {
            return typeof(IAssetBundleResource);
        }

        private float PercentComplete()
        {
            return m_request?.progress ?? 0.0f;
        }
        
        private void OnRequestCompleted(string tag, AsyncOperation asyncOperation)
        {
            OnDemandResourcesRequest request = (asyncOperation as OnDemandResourcesRequest);
            
        	Debug.Log($"[ODR] AppleAssetBundleProvider.OnRequestCompleted BEGIN for tag: '{tag}'");

            if (!string.IsNullOrEmpty(request.error))
            { 
                Debug.LogError($"[ODR] AppleAssetBundleProvider.OnRequestCompleted Error for tag: '{tag}': {request.error}");
                m_provideHandle.Complete(m_provider, false, new Exception("ODR request failed: " + request.error));
                return;
            }

            Debug.Log($"[ODR] AppleAssetBundleProvider.OnRequestCompleted Success for tag: '{tag}'");

            // #if UNITY_IOS && !UNITY_EDITOR
            // AssetBundleCreateRequest requestOperation = AssetBundle.LoadFromFileAsync("res://" + tag);
            // #else
            //var assetBundle = AssetBundle.LoadFromFile(tag);
            m_assetBundle = AssetBundle.LoadFromFile( "res://" + tag );
            // #endif
            Debug.Log($"[ODR] AssetBundle.LoadFromFile Tag: '{tag}': res: {m_assetBundle}");

            //if (requestOperation.isDone)
            {
            	Debug.Log($"[ODR] AppleAssetBundleProvider.OnRequestCompleted.LoadFromFileAsync isDone Success for tag: '{tag}'");
                // [ODR] AppleAssetBundleProvider.OnRequestCompleted.LoadFromFileAsync isDone Success for tag: 'installtime_assets_all_5f3da29bb446a276d2d86ba4cf0f0906'
                
                m_provideHandle.Complete(this, m_assetBundle != null, null); 
                // ОШИБКА ТУТ
                //после [ODR] AppleAssetBundleProvider.OnRequestCompleted END for tag: 'ondemand_assets_all_92575f50c24d12c5e76b01215767ccc1'
                
                // ЗАТЕМ после Exception: The ProvideHandle is invalid. After the handle has been completed, it can no longer be used
                //     at UnityEngine.ResourceManagement.ResourceProviders.ProvideHandle.get_InternalOp () [0x00000] in <00000000000000000000000000000000>:0 
                // at UnityEngine.ResourceManagement.ResourceProviders.ProvideHandle.Complete[T] (T result, System.Boolean status, System.Exception exception) [0x00000] in <00000000000000000000000000000000>:0 
                // at AssetBundles.AppleOnDemandResources.ResourceProviders.AppleAssetBundleProvider.OnRequestCompleted (System.String tag, UnityEngine.AsyncOperation asyncOperation) [0x00000] in <00000000000000000000000000000000>:0 
                // at AssetBundles.AppleOnDemandResources.ResourceProviders.AppleAssetBundleProvider+<>c__DisplayClass6_0.<Provide>b__0 (UnityEngine.AsyncOperation request) [0x00000] in <00000000000000000000000000000000>:0 
                // at UnityEngine.AsyncOperation.InvokeCompletionEvent () [0x00000] in <00000000000000000000000000000000>:0 
                request.Dispose();
                Debug.Log($"[ODR] AppleAssetBundleProvider.OnRequestCompleted END for tag: '{tag}'");
                return;
            }

            // requestOperation.completed += operation =>
            // {
            //     Debug.Log($"[ODR] AppleAssetBundleProvider.OnRequestCompleted.LoadFromFileAsync Completed for tag: '{tag}'");
            //
            //     AssetBundle assetBundle = (operation as AssetBundleCreateRequest)?.assetBundle;
            //     m_provideHandle.Complete(m_provider, assetBundle != null, null);
            //     string path = m_request.GetResourcePath(tag);
            //     Debug.Log($"[ODR] AppleAssetBundleProvider.OnRequestCompleted.GetResourcePath for tag: '{tag}': '{path}'");
            //     // Call Dispose() when resource is no longer needed.
            //     m_request.Dispose();
            // };

            // Get path to the resource and use it
            Debug.Log($"[ODR] AppleAssetBundleProvider.OnRequestCompleted END for tag: '{tag}'");
        }

        public override void Release(IResourceLocation location, object asset)
        {
            base.Release(location, asset);
            m_provideHandle = default;
			m_assetBundle = null;
            
            if (m_request != null)
            {
                m_request.Dispose();
                m_request = null;
            }
        }
//#endif
    }
}