using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MinMaxAttribute))]
public class MinMaxPropertyDrawer : PropertyDrawer {
    const float kSpacing = 4.0f;

    MinMaxAttribute minMaxAttribute => attribute as MinMaxAttribute;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        Vector2 value = property.vector2Value;
        float min = value.x;
        float max = value.y;

        EditorGUI.BeginChangeCheck();

        position.width -= 2 * (EditorGUIUtility.fieldWidth + kSpacing);
        EditorGUI.MinMaxSlider(position, label, ref min, ref max, minMaxAttribute.Min, minMaxAttribute.Max);

        position.x += position.width + kSpacing;
        position.width = EditorGUIUtility.fieldWidth;
        min = EditorGUI.FloatField(position, min);

        position.x += position.width + kSpacing;
        position.width = EditorGUIUtility.fieldWidth;
        max = EditorGUI.FloatField(position, max);

        if (EditorGUI.EndChangeCheck()) {
            Vector2 newValue = new Vector2(min, max);
            property.vector2Value = newValue;
            property.serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.EndProperty();
    }
}
