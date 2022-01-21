using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(DirtyField<>), true)]
public class DirtyFieldPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        SerializedProperty valueProperty = property.FindPropertyRelative("m_value");
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(position, valueProperty, label);
        if (EditorGUI.EndChangeCheck()) {
            IDirtyFieldPropertyDrawerExtensions dirtyField = fieldInfo.GetValue(property.serializedObject.targetObject) as IDirtyFieldPropertyDrawerExtensions;
            fieldInfo.SetValue(property.serializedObject.targetObject, dirtyField.EditedField());
        }
    }
}
