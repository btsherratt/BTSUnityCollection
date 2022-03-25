using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PolygonArea))]
public class PolygonAreaEditor : Editor {
    [SerializeField]
    bool m_editing;

    [SerializeField]
    bool m_showPoints;

    PolygonArea targetArea => target as PolygonArea;

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (GUILayout.Button("Re-center")) {
            Vector3 worldPosition = Vector3.zero;
            List<Vector3> points = new List<Vector3>();
            foreach (Vector3 point in targetArea.m_points) {
                Vector3 worldPoint = targetArea.transform.TransformPoint(point);
                points.Add(worldPoint);
                worldPosition += worldPoint;
            }
            worldPosition /= targetArea.m_points.Length;

            RaycastHit hit;
            if (Physics.Raycast(worldPosition + Vector3.up * 3000, Vector3.down, out hit, float.MaxValue, 1 << targetArea.m_snapLayer)) {
                worldPosition = hit.point;
            }

            targetArea.transform.position = worldPosition;
            for (int i = 0; i < targetArea.m_points.Length; ++i) {
                targetArea.m_points[i] = targetArea.transform.InverseTransformPoint(points[i]);
            }
        }
    }
    
    private void OnSceneGUI() {
        if (targetArea.m_points?.Length >= 3) {
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
        }

        if (m_editing) {

            SerializedProperty pointsProperty = serializedObject.FindProperty("m_points");

            for (int i = 0; i < pointsProperty.arraySize; ++i) {
                SerializedProperty pointProperty = pointsProperty.GetArrayElementAtIndex(i);
                Vector3 point = pointProperty.vector3Value;
                Vector3 worldPoint = targetArea.transform.TransformPoint(point);

                EditorGUI.BeginChangeCheck();
                
                Vector3 newWorldPoint = Handles.PositionHandle(worldPoint, Quaternion.identity);

                if (EditorGUI.EndChangeCheck()) {
                    RaycastHit hit;
                    if (Physics.Raycast(newWorldPoint + Vector3.up * 3000, Vector3.down, out hit, float.MaxValue, 1 << targetArea.m_snapLayer)) {
                        newWorldPoint = hit.point;
                    }

                    Vector3 newPoint = targetArea.transform.InverseTransformPoint(newWorldPoint);

                    pointProperty.vector3Value = newPoint;
                    pointProperty.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        Handles.BeginGUI();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        m_editing = GUILayout.Toggle(m_editing, "Edit Area", "Button");
        m_showPoints = GUILayout.Toggle(m_showPoints, "Debug Points", "Button");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        Handles.EndGUI();

        if (m_editing) {
            SerializedProperty pointsProperty = serializedObject.FindProperty("m_points");

            // Override being able to click anywhere else...
            HandleUtility.AddDefaultControl(0);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, float.MaxValue, 1 << targetArea.m_snapLayer)) {
                    int idx = pointsProperty.arraySize;
                    pointsProperty.InsertArrayElementAtIndex(idx);
                    SerializedProperty pointProperty = pointsProperty.GetArrayElementAtIndex(idx);
                    pointProperty.vector3Value = targetArea.transform.InverseTransformPoint(hit.point);
                    pointProperty.serializedObject.ApplyModifiedProperties();
                }

                Event.current.Use();
            }
        }

        if (m_showPoints) {
            foreach (PolygonArea.Point point in targetArea.FillPoints()) {
                Vector3 worldPoint = point.position;// targetArea.transform.TransformPoint(point);

                Handles.color = Color.white;
                Handles.DrawLine(worldPoint + point.rotation * Vector3.forward * point.uniformScale, worldPoint + point.rotation * Vector3.back * point.uniformScale);
                Handles.DrawLine(worldPoint + point.rotation * Vector3.left * point.uniformScale, worldPoint + point.rotation * Vector3.right * point.uniformScale);

                Handles.color = Color.red;
                Handles.DrawLine(worldPoint, worldPoint + (point.rotation * Vector3.up * point.uniformScale));
            }
        }
    }
}
