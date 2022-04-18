using System.Collections.Generic;
using UnityEngine;

namespace SKFX.WorldBuilder {
    public abstract class TrianglesInstanceArea : InstanceArea {
        protected struct Triangle {
            public Vector3 p0;
            public Vector3 p1;
            public Vector3 p2;

            public Vector3 normal { get; private set; }
            public Vector3 forward { get; private set; }

            public float area { get; private set; }

            Vector3 deltaA;
            Vector3 deltaB;

            public Triangle(Vector3 p0, Vector3 p1, Vector3 p2) {
                this.p0 = p0;
                this.p1 = p1;
                this.p2 = p2;

                deltaA = p0 - p2;
                deltaB = p1 - p2;

                Vector3 cross = Vector3.Cross(deltaA, deltaB);
                normal = cross.normalized;
                forward = deltaA.normalized;

                area = cross.magnitude / 2.0f;
            }

            public Vector3 RandomPoint() {
                float rndA = Random.value;
                float rndB = Random.value;

                Vector3 position;
                if (rndA + rndB <= 1.0f) {
                    Vector3 rnd = deltaA * rndA + deltaB * rndB;
                    position = p2 + rnd;
                } else {
                    Vector3 rnd = deltaA * rndA + deltaB * rndB;
                    position = p2 + deltaA + deltaB - rnd;
                }

                return position;
            }

            // https://blackpawn.com/texts/pointinpoly/
            public bool ContainsPoint(Vector3 point) {
                Vector3 deltaPoint = point - p2;

                Vector2 a = deltaA.XZ();
                Vector2 b = deltaB.XZ();
                Vector2 c = deltaPoint.XZ();

                float dot00 = Vector2.Dot(a, a);
                float dot01 = Vector2.Dot(a, b);
                float dot02 = Vector2.Dot(a, c);

                float dot11 = Vector2.Dot(b, b);
                float dot12 = Vector2.Dot(b, c);

                float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
                float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
                float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

                return (u >= 0) && (v >= 0) && (u + v < 1);
            }
        }

        Triangle[] m_cachedTriangles;
        float m_cachedArea;
        Bounds m_cachedBounds;

        protected override float Area {
            get {
                RegenerateTriangles();
                return m_cachedArea;
            }
        }

        protected void MarkTrianglesDirty() {
            m_cachedTriangles = null;
        }

        void RegenerateTriangles() {
            if (m_cachedTriangles == null) {
                m_cachedTriangles = GenerateTriangles();

                m_cachedBounds.size = Vector3.zero;
                m_cachedArea = 0.0f;

                if (m_cachedTriangles.Length > 0) {
                    m_cachedBounds.center = m_cachedTriangles[0].p0;

                    for (int i = 0; i < m_cachedTriangles.Length; ++i) {
                        Triangle t = m_cachedTriangles[i];
                        m_cachedArea += t.area;
                        m_cachedBounds.Encapsulate(t.p0);
                        m_cachedBounds.Encapsulate(t.p1);
                        m_cachedBounds.Encapsulate(t.p2);
                    }
                }
            }
        }

        protected abstract Triangle[] GenerateTriangles();

        protected override Vector3 RandomPointInArea() {
            RegenerateTriangles();

            float random = Random.Range(0, m_cachedArea);

            Triangle selectedTriangle = new Triangle();
            foreach (Triangle t in m_cachedTriangles) {
                selectedTriangle = t;
                random -= t.area;
                if (random <= 0) {
                    break;
                }
            }

            Vector3 point = selectedTriangle.RandomPoint();
            return point;
        }

        public override bool TestPointInArea(Vector3 point) {
            RegenerateTriangles();

            if (m_cachedBounds.SqrDistance(point) <= float.Epsilon) {
                //Vector3 localPoint = transform.InverseTransformPoint(point);
                foreach (Triangle t in m_cachedTriangles) {
                    if (t.ContainsPoint(point)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool TestPointInAreaXZ(Vector3 point) {
            RegenerateTriangles();

            point.y = m_cachedBounds.center.y;
            if (m_cachedBounds.SqrDistance(point) <= float.Epsilon) {
                //Vector3 localPoint = transform.InverseTransformPoint(point);
                foreach (Triangle t in m_cachedTriangles) {
                    if (t.ContainsPoint(point)) {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}