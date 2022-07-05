#if UNITY_IOS
using UnityEngine;
using UnityEngine.iOS;
using System;
using System.Collections;
using UnityEngine.AddressableAssets;

public class LoadBundle : MonoBehaviour
{
	public AssetBundle     TextureBundle;


	void Start( )
	{
		StartCoroutine( LoadAsset( "odrpack", "odrpack" ) );
	}

	public IEnumerator LoadAsset( string resourceName, string odrTag )
	{
		// Create the request
		OnDemandResourcesRequest request = OnDemandResources.PreloadAsync( new string[] { odrTag } );

		// Wait until request is completed
		yield return request;

		// Check for errors
		if (request.error != null)
		{
			throw new Exception( "ODR request failed: " + request.error );
		}
		
		TextureBundle = AssetBundle.LoadFromFile( "res://" + resourceName );

		request.Dispose( );
	}
}
#endif