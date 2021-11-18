using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(HexTransform))]
public class HexTransformEditor : Editor {
    SerializedProperty m_coordinateProperty;

    void OnEnable() {
        m_coordinateProperty = serializedObject.FindProperty("m_coordinate");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_coordinateProperty, new GUIContent("Coordinate"));

        serializedObject.ApplyModifiedProperties();
    }
}