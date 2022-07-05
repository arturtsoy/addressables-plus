using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AssetBundles.AppleOnDemandResources.Editor
{
    [CustomEditor(typeof(CustomAssetPackSettings))]
    class CustomAssetPackSettingsInspector : UnityEditor.Editor
    {
        CustomAssetPackSettings m_Settings;
        // [SerializeField]
        // ReorderableList m_CustomAssetPacks;

        private void OnEnable()
        {
            m_Settings = target as CustomAssetPackSettings;
            if (m_Settings == null)
            {
	            return;
            }
        }

        public override void OnInspectorGUI()
        {
            //m_CustomAssetPacks.DoLayoutList();
        }

         private void DrawCustomAssetPackCallback(Rect rect, int index, bool isActive, bool isFocused)
         {
        //     float halfW = rect.width * 0.4f;
        //     CustomAssetPackEditorInfo currentAssetPack = m_Settings.CustomAssetPacks[index];
        //
        //     EditorGUI.BeginDisabledGroup(currentAssetPack.AssetPackName == CustomAssetPackSettings.kInstallTimePackName);
        //
        //     string oldName = currentAssetPack.AssetPackName;
        //     string newName = EditorGUI.DelayedTextField(new Rect(rect.x, rect.y, halfW, rect.height), oldName);
        //     
        //     if (newName != oldName)
        //     {
	       //      m_Settings.SetAssetPackName(index, newName);
        //     }
        //
        //     // DeliveryType oldType = currentAssetPack.DeliveryType;
        //     // DeliveryType newType = (DeliveryType)EditorGUI.EnumPopup(new Rect(rect.x + halfW, rect.y, rect.width - halfW, rect.height), new GUIContent(""), oldType, IsDeliveryTypeEnabled);
        //     //
        //     // if (oldType != newType)
        //     // {
	       //     //  m_Settings.SetDeliveryType(index, newType);
        //     // }
        //
        //     EditorGUI.EndDisabledGroup();
         }

        // private bool IsDeliveryTypeEnabled(Enum e)
        // {
        //     DeliveryType deliveryType = (DeliveryType)e;
        //     
        //     if (deliveryType != DeliveryType.InstallTime)
        //     {
	       //      return true;
        //     }
        //     return false;
        // }

        private void OnRemoveCustomAssetPack(ReorderableList list)
        {
	        // if (m_Settings.CustomAssetPacks[list.index].AssetPackName == CustomAssetPackSettings.kInstallTimePackName)
	        // {
		       //  Debug.LogError($"Cannot delete asset pack name '{CustomAssetPackSettings.kInstallTimePackName}'. It represents the generated asset packs which will contain all install-time content.");
	        // }
	        // else
	        // {
		       //  m_Settings.RemovePackAtIndex(list.index);
	        // }
        }
    }
}
