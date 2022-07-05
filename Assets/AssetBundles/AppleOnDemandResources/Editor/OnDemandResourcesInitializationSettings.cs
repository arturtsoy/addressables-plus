using UnityEditor;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;

namespace AssetBundles.AppleOnDemandResources.Editor
{
    /// <summary>
    /// Asset container for 
    /// </summary>
    [CreateAssetMenu(fileName = "AppleOnDemandResourcesInitializationSettings.asset", menuName = "Addressables/Initialization/Apple On-Demand Resources Initialization Settings")]
    public class OnDemandResourcesInitializationSettings : ScriptableObject, IObjectInitializationDataProvider
    {  
        /// <summary>
        /// Display name used in GUI for this object.
        /// </summary>
        public string Name { get { return "Apple On-Demand Resources Initialization Settings"; } }
        
        [SerializeField]
        OnDemandResourcesInitializationData m_Data = new OnDemandResourcesInitializationData();
        /// <summary>
        /// The cache initialization data that will be serialized and applied during Addressables initialization.
        /// </summary>
        public OnDemandResourcesInitializationData Data
        {
            get
            {
                return m_Data;
            }
            set
            {
                m_Data = value;
            }
        }

        /// <summary>
        /// Create initialization data to be serialized into the Addressables runtime data.
        /// </summary>
        /// <returns>The serialized data for the initialization class and the data.</returns>
        public ObjectInitializationData CreateObjectInitializationData()
        {
            return ObjectInitializationData.CreateSerializedInitializationData<OnDemandResourcesInitialization>(typeof(OnDemandResourcesInitialization).Name, m_Data);
        }
    }

    [CustomPropertyDrawer(typeof(OnDemandResourcesInitializationSettings), true)]
    class OnDemandResourcesInitializationSettingsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty prop = property.FindPropertyRelative("m_LogWarnings");
            prop.boolValue = EditorGUI.Toggle(position, new GUIContent("Log Warnings", "Show warnings that occur when configuring Addressables."), prop.boolValue);
            EditorGUI.EndProperty();
        }
    }
}
