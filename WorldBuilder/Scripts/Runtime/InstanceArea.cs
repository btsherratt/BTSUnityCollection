using System.Collections.Generic;
using UnityEngine;

namespace SKFX.WorldBuilder {
    public abstract class InstanceArea : MonoBehaviour {
        const int kInstancesPerUnit = 10;

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
        }

        class TriangleTransformDetailsProvider : ITransformDetailsProviding {
            List<Triangle> m_triangleList;
            float m_totalArea;
            long m_instanceCount;
            int m_seed;

            public long DetailsCount => m_instanceCount;

            public TriangleTransformDetailsProvider(IEnumerable<InstanceArea> additiveAreas, float density, int seed) {
                m_triangleList = new List<Triangle>();
                foreach (InstanceArea area in additiveAreas) {
                    m_triangleList.AddRange(area.GetTriangles());
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

                    TransformDetails details = new TransformDetails();
                    details.position = selectedTriangle.RandomPoint();
                    details.rotation = Quaternion.LookRotation(selectedTriangle.forward, selectedTriangle.normal);
                    details.uniformScale = 1.0f;

                    Random.State currentState = Random.state;
                    Random.state = oldState;

                    yield return details;

                    oldState = Random.state;
                    Random.state = currentState;
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
                        snappedDetails.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.right, hit.normal), hit.normal) * Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0); // FIXME, seed
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

        public delegate void ChangeEvent(InstanceArea instanceArea);
        public static event ChangeEvent ms_changeEvent;

        public static ITransformDetailsProviding TransformDetailsProvider(IEnumerable<InstanceArea> additiveAreas, float density, int seed) {
            return new TriangleTransformDetailsProvider(additiveAreas, density, seed);
        }

        protected void OnValidate() {
            if (ms_changeEvent != null) {
                ms_changeEvent(this);
            }
        }

        protected abstract IEnumerable<Triangle> GetTriangles();
    }
}
