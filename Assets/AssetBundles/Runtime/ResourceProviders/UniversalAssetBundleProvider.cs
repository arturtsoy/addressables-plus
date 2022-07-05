using System;
using System.ComponentModel;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UDebug = UnityEngine.Debug;

namespace AssetBundles.ResourceProviders
{
	[DisplayName("Universal Asset Bundle Provider")]
	public class UniversalAssetBundleProvider : ResourceProviderBase
	{
#if UNITY_ANDROID
		private AssetBundles.GooglePlayAssetDelivery.ResourceProviders.GoogleAssetBundleAsyncProvider m_provider = new AssetBundles.GooglePlayAssetDelivery.ResourceProviders.GoogleAssetBundleAsyncProvider();
#elif UNITY_IOS
		private AssetBundles.AppleOnDemandResources.ResourceProviders.AppleAssetBundleProvider m_provider = new AssetBundles.AppleOnDemandResources.ResourceProviders.AppleAssetBundleProvider();
#else
        private ResourceProviderBase m_provider = null;
#endif

		public override bool Initialize(string id, string data)
		{
			UDebug.Log($"[AssetBundles] UniversalResourceProvider.Initialize: {id}, data: {data}");
			return base.Initialize(id, data);
		}
		
		public override void Provide(ProvideHandle provideHandle)
		{
			UDebug.Log($"[AssetBundles] UniversalResourceProvider.Provide: {m_ProviderId}");
			m_provider?.Provide(provideHandle);
		}

	    public override Type GetDefaultType(IResourceLocation location)
	    {
		    return m_provider?.GetDefaultType(location);
	    }

	    public override void Release(IResourceLocation location, object asset)
	    {
		    m_provider?.Release(location, asset);
	    }
    }
}