using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(XTagAttribute))]
public class XTagPropertyDrawer : PropertyDrawer {
    /*public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.dro
        property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
        EditorGUI.EndProperty();
    }*/
}
