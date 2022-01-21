using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(DirtyField<>), true)]
public class DirtyFieldPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        SerializedProperty valueProperty = property.FindPropertyRelative("m_value");

        EditorGUI.BeginChangeCheck();

        //cur.serializedObject.Update();
        EditorGUI.PropertyField(position, valueProperty, label);
        //cur.serializedObject.ApplyModifiedProperties();
        
        if (EditorGUI.EndChangeCheck() && property.serializedObject.hasModifiedProperties) {
            IDirtyFieldPropertyDrawerExtensions dirtyField = fieldInfo.GetValue(property.serializedObject.targetObject) as IDirtyFieldPropertyDrawerExtensions;
            fieldInfo.SetValue(property.serializedObject.targetObject, dirtyField.EditedField());
        }
    }
}
