using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(DefaultListValueAttribute))]
public class DefaultListValuePropertyDrawer : PropertyDrawer {
    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        VisualElement element = base.CreatePropertyGUI(property);

        return element;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.PropertyField(position, property, label, true);
        //base.OnGUI(position, property, label);
    }

    /*public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        property.

        property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        EditorGUI.EndProperty();

        EditorGUI.BeginProperty(position, label, property);
        property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        EditorGUI.EndProperty();
    }*/
}
