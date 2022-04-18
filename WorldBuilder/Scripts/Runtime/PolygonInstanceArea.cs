using UnityEngine;

namespace SKFX.WorldBuilder {
    [ExecuteInEditMode]
    public class PolygonInstanceArea : TrianglesInstanceArea {
        public Vector3[] m_points;

        private new void OnValidate() {
            MarkTrianglesDirty();
            base.OnValidate();
        }

        protected override Triangle[] GenerateTriangles() {
            Triangle[] triangles = new Triangle[m_points.Length];

            Vector3 center = Vector3.zero;
            foreach (Vector3 point in m_points) {
                center += point;
            }
            center /= m_points.Length;

            for (int i = 0; i < m_points.Length; ++i) {
                Vector3 pointA = transform.TransformPoint(m_points[i]);
                Vector3 pointB = transform.TransformPoint(m_points[(i + 1) % m_points.Length]);
                Vector3 pointC = transform.TransformPoint(center);
                triangles[i] = new Triangle(pointA, pointB, pointC);
            }

            return triangles;
        }
    }
}