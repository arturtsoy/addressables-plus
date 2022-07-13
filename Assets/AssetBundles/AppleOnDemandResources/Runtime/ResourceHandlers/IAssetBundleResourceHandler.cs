﻿using System;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace AssetBundles.AppleOnDemandResources.ResourceHandlers
{
    public interface IAssetBundleResourceHandler
    {
        bool TryBeginOperation(ProvideHandle provideHandle, AssetBundleRequestOptions options, Action<IAssetBundleResourceHandler, AssetBundle, Exception> OnCompleted);
        void Unload();
    }
}