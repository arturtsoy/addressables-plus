using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class Demo : MonoBehaviour
{
	public static Demo instance;
	
	public Text progressText;
	public bool downloadDependencies = true;
	public List<string> labels = new List<string> {"preload", "default"};
	private IEnumerator Start()
	{
		instance = this;
	    if (downloadDependencies)
	    {
		    progressText.text = $"Assets Loading..";

		    foreach (string label in labels)
		    {
			    yield return LoadAsset(label);
		    }
		    progressText.text = $"Assets Loaded!";
	    }
    }
    
    private IEnumerator LoadAsset(string key)
    {
	    AsyncOperationHandle<long> getDownloadSize = Addressables.GetDownloadSizeAsync(key);
	    yield return getDownloadSize;
	    
	    if (getDownloadSize.Result > 0)
	    {
		    Debug.Log($"[Preloader-Assets] LoadAssets '{key}'");
		    string prefix = $"LoadAssets with key: {key}";
	    
		    AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(key);
           
		    while (downloadHandle.Status == AsyncOperationStatus.None)
		    {
			    progressText.text = $"{prefix} download: {downloadHandle.PercentComplete:P2}";
			    yield return null;
		    }
	    
		    yield return downloadHandle;
	    
		    Addressables.Release(downloadHandle);

		    progressText.text = $"{prefix} download complete!";
		    Debug.Log(progressText.text);
	    }
	    else
	    {
		    Debug.Log($"[Preloader-Assets] LoadAssets '{key}' ZERO size!");
	    }
	    
	    Addressables.Release(getDownloadSize);
    }

    private IEnumerator LoadAsset2(string key)
    {
	    Debug.Log($"[Preloader-Assets] LoadAssets '{key}'");
	    
	    var  downloadHandle = Addressables.DownloadDependenciesAsync(key, false);
	    float progress = 0;

	    while (downloadHandle.Status == AsyncOperationStatus.None)
	    {
		    float percentageComplete = downloadHandle.GetDownloadStatus().Percent;
		    if (percentageComplete > progress * 1.1) // Report at most every 10% or so
		    {
			    progress = percentageComplete;
		    }
		    yield return null;
	    }

	    Addressables.Release(downloadHandle); //Release the operation handle
	    
	    Debug.Log($"[Preloader-Assets] LoadAssets '{key}': 100%");
    }
}
