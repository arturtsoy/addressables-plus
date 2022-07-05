using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressablesPlayAssetDelivery
{
    public class ButtonInteraction : MonoBehaviour
    {
	    public string key = "Prefabs/cube"; 
        public Transform parent;

        public bool loadAsync = true;
        public bool waitingOneFrame = false;
        
        private bool m_isLoading = false;
        private GameObject m_obj = null;


        private AsyncOperationHandle<GameObject> m_handle;
        
        public void OnButtonClicked()
        {
	        if (m_isLoading)
	        {
		        Debug.LogError("Loading operation currently in progress.");
	        }
            else if (!m_isLoading)
            {
                if (!m_handle.IsValid())
                {
                    // Load the object
                    StartCoroutine(Instantiate());
                }
                else
                {
                    // Unload the object
                    Addressables.ReleaseInstance(m_handle);
                    m_obj = null;
                }
            }
        }

        private IEnumerator Instantiate()
        {
            m_isLoading = true;
            string prefix = $"[Assets] Load<GameObject> with key: {key}, async: {loadAsync}";
            Debug.Log(prefix);
            
            m_handle = Addressables.InstantiateAsync(key, parent);
            
            if (loadAsync)
            {
	            Debug.Log("Frame handle Before:" + Time.frameCount);
				yield return m_handle;
				m_obj = m_handle.Result;
				Debug.Log("Frame handle After:" + Time.frameCount);
            }
            else
            {
	            //m_handle = Addressables.Instantiate(key, parent);
	            
	            Debug.Log("Frame WaitForCompletion Before:" + Time.frameCount);
	            
	            if (waitingOneFrame)
	            {
		            yield return null;
	            }
	            m_obj = m_handle.WaitForCompletion();
	            Debug.Log("Frame WaitForCompletion After:" + Time.frameCount);
            }

            Debug.Log($"{prefix} Handle State: {m_handle.Status}. m_obj: {m_obj}");
            m_isLoading = false;
            yield return null;
        }
    }
}
