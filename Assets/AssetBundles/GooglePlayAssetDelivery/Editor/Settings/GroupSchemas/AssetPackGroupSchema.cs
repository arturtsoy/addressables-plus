using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Google.Android.AppBundle.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace AssetBundles.GooglePlayAssetDelivery.Editor
{
    [DisplayName("Play Asset Delivery")]
    public class AssetPackGroupSchema : AddressableAssetGroupSchema
    {
        [SerializeField]
        AssetPackDeliveryMode m_DeliveryMode;

        public AssetPackDeliveryMode mDeliveryMode
        {
            get { return m_DeliveryMode; }
            set { m_DeliveryMode = value; }
        }

        public AssetPack CreateAssetPack(string bundle, TextureCompressionFormat textureCompressionFormat)
        {
            return new AssetPack
            {
                DeliveryMode = m_DeliveryMode,
                CompressionFormatToAssetPackDirectoryPath = new Dictionary<TextureCompressionFormat, string>()
                {
                    {textureCompressionFormat, Path.GetDirectoryName(bundle)}
                }
            };
        }
        
        /// <inheritdoc/>
        public override void OnGUIMultiple(List<AddressableAssetGroupSchema> otherSchemas)
        {
            var so = new SerializedObject(this);
            var prop = so.FindProperty("m_DeliveryMode");

            // Type/Static Content
            ShowMixedValue(prop, otherSchemas, typeof(bool), "m_DeliveryMode");
            EditorGUI.BeginChangeCheck();
            m_DeliveryMode = (AssetPackDeliveryMode)EditorGUILayout.EnumPopup("Delivery Mode", m_DeliveryMode);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var s in otherSchemas)
                    ((AssetPackGroupSchema) s).m_DeliveryMode = m_DeliveryMode;
            }
            EditorGUI.showMixedValue = false;

            so.ApplyModifiedProperties();
        }
    }
}