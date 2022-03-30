using System.Collections.Generic;
using UnityEngine;

namespace SKFX.WorldBuilder {
    [ExecuteInEditMode]
    public class PolygonInstanceArea : InstanceArea {
        public Vector3[] m_points;

        protected override IEnumerable<Triangle> GetTriangles() {
            Vector3 center = Vector3.zero;
            foreach (Vector3 point in m_points) {
                center += point;
            }
            center /= m_points.Length;

            for (int i = 0; i < m_points.Length; ++i) {
                Vector3 pointA = transform.TransformPoint(m_points[i]);
                Vector3 pointB = transform.TransformPoint(m_points[(i + 1) % m_points.Length]);
                Vector3 pointC = transform.TransformPoint(center);

                yield return new Triangle(pointA, pointB, pointC);
            }
        }
    }
}