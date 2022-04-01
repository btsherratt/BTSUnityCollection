using System.Collections.Generic;
using UnityEngine;

namespace SKFX.WorldBuilder {
    [ExecuteInEditMode]
    public class DiscInstanceArea : InstanceArea {
        public Vector3 m_center = Vector3.zero;
        public float m_radius = 1.0f;

        protected override float Area => Mathf.PI * m_radius * m_radius;

        protected override Vector3 RandomPointInArea() {
            Vector2 point = Random.insideUnitCircle * m_radius;
            Vector3 position = transform.position + new Vector3(point.x, 0, point.y);
            return position;
        }

        protected override bool TestPointInArea(Vector3 point) {
            Vector3 delta = point - transform.position;
            bool test = delta.sqrMagnitude <= m_radius * m_radius;
            return test;
        }

        /*protected override IEnumerable<Triangle> GetTriangles() {
            for (int i = 0; i < kSubdivisions; ++i) {
                float angle1 = Mathf.Lerp(0, 360, i / (float)kSubdivisions);
                float angle2 = Mathf.Lerp(0, 360, (i + 1) / (float)kSubdivisions);

                Vector3 pointA = transform.TransformPoint(m_center + Quaternion.Euler(0, angle1, 0) * Vector3.forward * m_radius);
                Vector3 pointB = transform.TransformPoint(m_center + Quaternion.Euler(0, angle2, 0) * Vector3.forward * m_radius);
                Vector3 pointC = transform.TransformPoint(m_center);

                yield return new Triangle(pointA, pointB, pointC);
            }
        }*/
    }
}