using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace AssetBundles.AppleOnDemandResources.Editor
{
    [DisplayName("On Demand Resources")]
    public class OnDemandResourcesSchema : AddressableAssetGroupSchema
    {
	    [SerializeField]
	    int m_AssetPackIndex = 0;
	    /// <summary>
	    /// Represents the asset pack that will contain this group's bundled content. Note that 'InstallTimeContent' is representative of the generated asset packs.
	    /// </summary>
	    public int AssetPackIndex
	    {
		    get
		    {
			    return m_AssetPackIndex;
		    }
		    set
		    {
			    if (m_AssetPackIndex != value)
			    {
				    m_AssetPackIndex = value;
				    SetDirty(true);
			    }
		    }
	    }
	    
	    [SerializeField]
        bool m_IncludeInAssetPack = true;
        /// <summary>
        /// Controls whether to include content in the specified asset pack.
        /// We use <see cref="BuildScriptOnDemandResources"/> to assign content to custom asset packs.
        /// </summary>
        public bool IncludeInAssetPack
        {
            get { return m_IncludeInAssetPack; }
            set
            {
                m_IncludeInAssetPack = value;
                SetDirty(true);
            }
        }

        [SerializeField]
        CustomAssetPackSettings m_Settings;
        /// <summary>
        /// Object that stores all custom asset pack information.
        /// </summary>
        public CustomAssetPackSettings Settings
        {
            get
            {
	            if (!CustomAssetPackSettings.SettingsExists)
	            {
		            ResetAssetPackIndex();
	            }
	            
                if (m_Settings == null)
                {
                    m_Settings = CustomAssetPackSettings.GetSettings(true);
                }
                return m_Settings;
            }
            set
            {
	            if (m_Settings != value)
	            {
		            m_Settings = value;
	            }
            }
        }

        GUIContent m_AssetPackGUI = new GUIContent("Asset Pack", "Asset pack that will contain this group's bundled content.");

        GUIContent m_IncludeInAssetPackGUI = new GUIContent("Include In Asset Pack", "Controls whether to include this group's bundled content in the specified custom asset pack when using the 'Play Asset Delivery' build script.");

        public void ResetAssetPackIndex()
        {
            m_Settings = null;
        }

        private void ShowAssetPacks(SerializedObject so, List<AddressableAssetGroupSchema> otherSchemas = null)
        {
            SerializedProperty prop = so.FindProperty("m_AssetPackIndex");
            if (otherSchemas != null)
            {
	            ShowMixedValue(prop, otherSchemas, typeof(int), "m_AssetPackIndex");
            }

            prop = so.FindProperty("m_IncludeInAssetPack");
            
            if (otherSchemas != null)
            {
	            ShowMixedValue(prop, otherSchemas, typeof(bool), "m_IncludeInAssetPack");
            }
            
            EditorGUI.BeginChangeCheck();
            bool newIncludeInAssetPack = EditorGUILayout.Toggle(m_IncludeInAssetPackGUI, IncludeInAssetPack);
            
            if (EditorGUI.EndChangeCheck())
            {
                IncludeInAssetPack = newIncludeInAssetPack;
                if (otherSchemas != null)
                {
                    foreach (AddressableAssetGroupSchema s in otherSchemas)
                    {
                        OnDemandResourcesSchema padSchema = s as OnDemandResourcesSchema;
                        padSchema.IncludeInAssetPack = newIncludeInAssetPack;
                    }
                }
            }

            if (otherSchemas != null)
            {
	            EditorGUI.showMixedValue = false;
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Manage Asset Packs", "Minibutton"))
                {
                    EditorGUIUtility.PingObject(Settings);
                    Selection.activeObject = Settings;
                }
            }
        }

        /// <inheritdoc/>
        public override void OnGUI()
        {
            SerializedObject so = new SerializedObject(this);
            ShowAssetPacks(so);
            so.ApplyModifiedProperties();
        }

        /// <inheritdoc/>
        public override void OnGUIMultiple(List<AddressableAssetGroupSchema> otherSchemas)
        {
            SerializedObject so = new SerializedObject(this);
            ShowAssetPacks(so, otherSchemas);
            so.ApplyModifiedProperties();
        }
    }
}
