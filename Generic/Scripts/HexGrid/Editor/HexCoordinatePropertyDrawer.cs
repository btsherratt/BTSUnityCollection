using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HexCoordinate))]
public class HexCoordinatePropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        position = EditorGUI.PrefixLabel(position, label);

        position.width /= 3;
        float offset = position.width;
        position.width -= 5.0f;

        EditorGUIUtility.labelWidth = 20.0f;

        SerializedProperty q = property.FindPropertyRelative("q");
        q.intValue = EditorGUI.IntField(position, "Q", q.intValue);
        position.x += offset;

        SerializedProperty r = property.FindPropertyRelative("r");
        r.intValue = EditorGUI.IntField(position, "R", r.intValue);
        position.x += offset;

        SerializedProperty s = property.FindPropertyRelative("s");
        s.intValue = EditorGUI.IntField(position, "S", s.intValue);
    }
}
