using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(QuickCollisionLayerAttribute))]
public class QuickCollisionLayerPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        Rect controlRect = EditorGUI.PrefixLabel(position, label);
        Rect togglePosition = controlRect;
        togglePosition.width = togglePosition.height;

        int inputValue = property.intValue;
        int outputValue = 0;

        for (int maskBit = 0; maskBit < 8; ++maskBit) {
            togglePosition.x = controlRect.x + togglePosition.width * maskBit;
            bool enabled = EditorGUI.Toggle(togglePosition, (inputValue & 1 << maskBit) > 0);
            if (enabled) {
                outputValue |= 1 << maskBit;
            }
        }

        property.intValue = outputValue;

        EditorGUI.EndProperty();
    }
}
