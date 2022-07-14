using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace AssetBundles.Editor
{
    [DisplayName("Platform")]
    public class PlatformSchema : AddressableAssetGroupSchema
    {
        public enum Platform
        {
            None = 0,
            iOS = 1,
            Android = 2,
            All = 100,
        }

        [SerializeField]
        protected Platform m_platform = Platform.All;

        public virtual Platform platform
        {
            get => m_platform;
            set => m_platform = value;
        }

        public bool IsSupport(Platform value)
        {
            return m_platform == Platform.All || m_platform == value;
        }
        
        /// <inheritdoc/>
        public override void OnGUIMultiple(List<AddressableAssetGroupSchema> otherSchemas)
        {
            var so = new SerializedObject(this);
            var prop = so.FindProperty("m_platform");

            // Type/Static Content
            ShowMixedValue(prop, otherSchemas, typeof(bool), "m_platform");
            EditorGUI.BeginChangeCheck();
            m_platform = (Platform)EditorGUILayout.EnumPopup("Platform", m_platform);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var s in otherSchemas)
                    ((PlatformSchema) s).m_platform = m_platform;
            }
            EditorGUI.showMixedValue = false;

            so.ApplyModifiedProperties();
        }
    }
}
