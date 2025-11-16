using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(CollectableUpgradeSO))]
public class CollectableUpgradeSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        
        // Get reference to the target object
        CollectableUpgradeSO character = (CollectableUpgradeSO)target;

        if (character.ItemIcon != null)
        {
            GUILayout.Label("Loot Preview", EditorStyles.boldLabel);

            // Add padding around the image using a vertical layout
            GUILayout.BeginVertical("box");
            GUILayout.Space(10);

            float maxPreviewSize = 32f;
            Rect rect = GUILayoutUtility.GetRect(maxPreviewSize, maxPreviewSize, GUILayout.ExpandWidth(false));
            EditorGUI.DrawPreviewTexture(rect, character.ItemIcon.texture, null, ScaleMode.ScaleToFit);

            GUILayout.Space(10);
            GUILayout.EndVertical();
        }

        //GUILayout.Space(15); // Additional space after the image

            // Draw default inspector
        DrawDefaultInspector();
    }
}

