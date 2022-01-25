using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FlattenAttribute))]
public class FlattenPropertyDrawer : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        float height = 0.0f;
        foreach (SerializedProperty childProperty in GetChildProperties(property)) {
            height += EditorGUI.GetPropertyHeight(childProperty, label, true);
            height += EditorGUIUtility.standardVerticalSpacing;
        }
        return Mathf.Max(height - EditorGUIUtility.standardVerticalSpacing, 0.0f);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        float currentHeight = position.y;

        foreach (SerializedProperty childProperty in GetChildProperties(property)) {
            EditorGUI.BeginChangeCheck();

            Rect childPosition = position;
            childPosition.height = EditorGUI.GetPropertyHeight(childProperty, label);
            childPosition.y = currentHeight;
            EditorGUI.PropertyField(childPosition, childProperty, true);
            currentHeight += childPosition.height + EditorGUIUtility.standardVerticalSpacing;

            if (EditorGUI.EndChangeCheck()) {
                property.serializedObject.ApplyModifiedProperties();
                break;
            }
        }

        EditorGUI.EndProperty();
    }

    IEnumerable<SerializedProperty> GetChildProperties(SerializedProperty property) {
        SerializedProperty childProperty = property.Copy();
        if (childProperty.NextVisible(true)) {
            do {
                yield return childProperty.Copy();
            } while (childProperty.NextVisible(false));
        }
    }
}
