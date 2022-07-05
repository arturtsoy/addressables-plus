using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AddressablesPlayAssetDelivery
{
    public class ButtonInteractionLink : MonoBehaviour
    {
        public AssetReference reference;
        public Transform parent;

        private bool m_isLoading = false;
        private GameObject m_obj = null;

        public void OnButtonClicked()
        {
	        if (m_isLoading)
	        {
		        Debug.LogError("Loading operation currently in progress.");
	        }
            else if (!m_isLoading)
            {
                if (m_obj == null)
                {
                    // Load the object
                    StartCoroutine(Instantiate());
                }
                else
                {
                    // Unload the object
                    Addressables.ReleaseInstance(m_obj);
                    m_obj = null;
                }
            }
        }

        private IEnumerator Instantiate()
        {
            m_isLoading = true;
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(reference, parent);
            yield return handle;
            m_obj = handle.Result;
            m_isLoading = false;
        }
    }
}
