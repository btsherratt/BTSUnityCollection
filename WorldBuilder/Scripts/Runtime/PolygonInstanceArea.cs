using System.Collections.Generic;
using UnityEngine;

namespace SKFX.WorldBuilder {
    [ExecuteInEditMode]
    public class PolygonInstanceArea : InstanceArea {
        public Vector3[] m_points;

        Triangle[] m_cachedTriangles;
        float m_cachedArea;
        Bounds m_cachedBounds;

        protected override float Area {
            get {
                RegenerateTriangles();
                return m_cachedArea;
            }
        }

        private new void OnValidate() {
            m_cachedTriangles = null;
            base.OnValidate();
        }

        void RegenerateTriangles() {
            if (m_cachedTriangles == null) {
                m_cachedTriangles = new Triangle[m_points.Length];

                Vector3 center = Vector3.zero;
                foreach (Vector3 point in m_points) {
                    center += point;
                }
                center /= m_points.Length;

                m_cachedBounds = new Bounds(center, Vector3.zero);

                m_cachedArea = 0.0f;
                for (int i = 0; i < m_points.Length; ++i) {
                    Vector3 pointA = transform.TransformPoint(m_points[i]);
                    Vector3 pointB = transform.TransformPoint(m_points[(i + 1) % m_points.Length]);
                    Vector3 pointC = transform.TransformPoint(center);

                    m_cachedTriangles[i] = new Triangle(pointA, pointB, pointC);
                    m_cachedArea += m_cachedTriangles[i].area;
                    m_cachedBounds.Encapsulate(pointA);
                    m_cachedBounds.Encapsulate(pointB);
                    m_cachedBounds.Encapsulate(pointC);
                }
            }
        }

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

        protected override bool TestPointInArea(Vector3 point) {
            if (m_cachedBounds.Contains(point)) {
                return true; // FIXME
            } else {
                return false;
            }
        }

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

            public bool ContainsPoint(Vector3 point) {
                Vector2 a = deltaA.XZ();
                Vector2 b = deltaB.XZ();
                Vector2 c = (p1 - p0).XZ();
                Vector2 test1 = (point - p2).XZ();
                Vector2 test2 = (point - p0).XZ();
                float da = Vector2.Dot(test1, a);
                float db = Vector2.Dot(test1, b);
                float dc = Vector2.Dot(test2, c);
                return da >= 0 && db >= 0 && dc >= 0 && da <= a.sqrMagnitude && db <= b.sqrMagnitude && dc <= c.sqrMagnitude; // FIXME, slow
            }
        }

#if false
        class TriangleTransformDetailsProvider : ITransformDetailsProviding {
            List<Triangle> m_triangleList;
            List<Triangle> m_subtractiveTriangleList;
            float m_totalArea;
            long m_instanceCount;
            int m_seed;

            public long DetailsCount => m_instanceCount;

            public TriangleTransformDetailsProvider(IEnumerable<InstanceArea> areas, float density, int seed) {
                m_triangleList = new List<Triangle>();
                m_subtractiveTriangleList = new List<Triangle>();
                foreach (InstanceArea area in areas) {
                    if (area.m_operation == Operation.Subtractive) {
                        m_subtractiveTriangleList.AddRange(area.GetTriangles());
                    }
                }

                m_triangleList = new List<Triangle>();
                foreach (InstanceArea area in areas) {
                    if (area.m_operation == Operation.Additive) {
                        //foreach (Triangle subtractiveTriangle in m_subtractiveTriangleList) {
                        foreach (Triangle t in area.GetTriangles()) {
                            //if ((subtractiveTriangle.ContainsPoint(t.p0) || subtractiveTriangle.ContainsPoint(t.p1) || subtractiveTriangle.ContainsPoint(t.p2)) == false) {
                            m_triangleList.Add(t);
                            //}
                        }
                        //}
                    }
                }

                m_totalArea = 0;
                foreach (Triangle t in m_triangleList) {
                    m_totalArea += t.area;
                }

                m_instanceCount = Mathf.FloorToInt(Mathf.Lerp(0, kInstancesPerUnit * Mathf.Sqrt(m_totalArea), density));
                m_seed = seed;
            }

            public IEnumerable<TransformDetails> GenerateDetails() {
                Random.State oldState = Random.state;
                Random.InitState(m_seed);

                for (int i = 0; i < m_instanceCount; ++i) {
                    float random = Random.Range(0, m_totalArea);

                    Triangle selectedTriangle = new Triangle();
                    foreach (Triangle t in m_triangleList) {
                        selectedTriangle = t;
                        random -= t.area;
                        if (random <= 0) {
                            break;
                        }
                    }




                    /*
                    Vector3 point;
                    bool bad = false;
                    int attempt = 0;

                    do {
                        point = selectedTriangle.RandomPoint();
                        
                        bad = false;
                        foreach (Triangle t in m_subtractiveTriangleList) {
                            bad = t.ContainsPoint(point);
                            if (bad) break;
                        }
                    } while (bad && attempt++ < 100);
                    */

                    Vector3 point = selectedTriangle.RandomPoint();



                    TransformDetails details = new TransformDetails();
                    details.position = point;
                    details.rotation = Quaternion.LookRotation(selectedTriangle.forward, selectedTriangle.normal);
                    details.uniformScale = 1.0f;

                    //                  Random.State currentState = Random.state;
                    //                  Random.state = oldState;

                    yield return details;

                    //                 oldState = Random.state;
                    //                  Random.state = currentState;  fixme, is this safe really?? (no)
                }

                Random.state = oldState;
            }

            public IEnumerable<TransformDetails> GenerateSnappedDetails(int snapLayerMask) {
                foreach (TransformDetails details in GenerateDetails()) {
                    TransformDetails snappedDetails = new TransformDetails();
                    snappedDetails.position = details.position;
                    snappedDetails.rotation = details.rotation;
                    snappedDetails.uniformScale = details.uniformScale;

                    Vector3 normal = details.rotation * Vector3.up;
                    RaycastHit hit;

                    if (Physics.Raycast(details.position + Vector3.up * 3000, Vector3.down, out hit, float.MaxValue, snapLayerMask)) {
                        snappedDetails.position = hit.point;
                        snappedDetails.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.right, hit.normal), hit.normal);
                    }

                    /*if (Physics.Raycast(details.position + normal * 3000, -normal, out hit, float.MaxValue, snapLayerMask)) {
                        snappedDetails.position = hit.point;
                        snappedDetails.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.down, hit.normal), hit.normal);
                    } else if (Physics.Raycast(details.position - normal * 3000, normal, out hit, float.MaxValue, snapLayerMask)) {
                        snappedDetails.position = hit.point;
                        snappedDetails.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.down, -hit.normal), -hit.normal);
                    }*/

                    yield return snappedDetails;
                }
            }
        }
#endif

    }
}