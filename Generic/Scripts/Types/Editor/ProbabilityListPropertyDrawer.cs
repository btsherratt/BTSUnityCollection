using UnityEngine;
using UnityEditor;

/*[CustomPropertyDrawer(typeof(ProbabilityList<>), true)]
public class ProbabilityListPropertyDrawer : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        SerializedProperty valueProperty = property.FindPropertyRelative("m_value");
        return EditorGUI.GetPropertyHeight(valueProperty, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        SerializedProperty valueProperty = property.FindPropertyRelative("m_value");

        EditorGUI.BeginChangeCheck();

        //cur.serializedObject.Update();
        EditorGUI.PropertyField(position, valueProperty, label, true);
        //cur.serializedObject.ApplyModifiedProperties();

        if ((EditorGUI.EndChangeCheck() && property.serializedObject.hasModifiedProperties)) {
            IDirtyFieldPropertyDrawerExtensions dirtyField = fieldInfo.GetValue(property.serializedObject.targetObject) as IDirtyFieldPropertyDrawerExtensions;
            fieldInfo.SetValue(property.serializedObject.targetObject, dirtyField.EditedField());

            //Undo.RecordObject(property.serializedObject.targetObject, $"Modified property {property.name}");
            //property.serializedObject.ApplyModifiedProperties();
        }
    }
}*/
