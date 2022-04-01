using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Spline))]
public class SplineEditor : Editor {
    [SerializeField]
    bool m_editing;

    Spline targetSpline => target as Spline;

    void OnSceneGUI() {
        /*if (targetSpline.controlPoints?.Length >= 3) {
            List<Vector3> points = new List<Vector3>();
            foreach (Vector3 point in targetArea.m_points) {
                points.Add(targetArea.transform.TransformPoint(point));
            }

            points.Add(targetArea.transform.TransformPoint(targetArea.m_points[0]));

            Handles.color = Color.magenta;
            Handles.DrawPolyLine(points.ToArray());
        }

        Handles.color = Color.yellow;
        foreach (Vector3 point in targetArea.m_points) {
            Vector3 worldPoint = targetArea.transform.TransformPoint(point);
            Handles.DrawLine(worldPoint + Vector3.forward, worldPoint + Vector3.back);
            Handles.DrawLine(worldPoint + Vector3.left, worldPoint + Vector3.right);
        }*/

        if (m_editing) {

            SerializedProperty pointsProperty = serializedObject.FindProperty("m_controlPoints");

            for (int i = 0; i < pointsProperty.arraySize; ++i) {
                SerializedProperty pointProperty = pointsProperty.GetArrayElementAtIndex(i);
                Vector3 point = pointProperty.vector3Value;
                Vector3 worldPoint = targetSpline.transform.TransformPoint(point);

                EditorGUI.BeginChangeCheck();

                Vector3 newWorldPoint = Handles.PositionHandle(worldPoint, Quaternion.identity);

                if (EditorGUI.EndChangeCheck()) {
                    RaycastHit hit;
                    if (Physics.Raycast(newWorldPoint + Vector3.up * 3000, Vector3.down, out hit, float.MaxValue)) {
                        newWorldPoint = hit.point;
                    }

                    Vector3 newPoint = targetSpline.transform.InverseTransformPoint(newWorldPoint);

                    pointProperty.vector3Value = newPoint;
                    pointProperty.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        Handles.BeginGUI();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        m_editing = GUILayout.Toggle(m_editing, "Edit Points", "Button");
        //m_showPoints = GUILayout.Toggle(m_showPoints, "Debug Points", "Button");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        Handles.EndGUI();

        if (m_editing) {
            SerializedProperty pointsProperty = serializedObject.FindProperty("m_controlPoints");

            // Override being able to click anywhere else...
            HandleUtility.AddDefaultControl(0);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, float.MaxValue)) {
                    int idx = pointsProperty.arraySize;
                    pointsProperty.InsertArrayElementAtIndex(idx);
                    SerializedProperty pointProperty = pointsProperty.GetArrayElementAtIndex(idx);
                    pointProperty.vector3Value = targetSpline.transform.InverseTransformPoint(hit.point);
                    pointProperty.serializedObject.ApplyModifiedProperties();
                }

                Event.current.Use();
            }
        }
    }
}
