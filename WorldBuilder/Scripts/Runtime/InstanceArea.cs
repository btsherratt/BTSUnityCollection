using System.Collections.Generic;
using UnityEngine;

namespace SKFX.WorldBuilder {
    public abstract class InstanceArea : MonoBehaviour {
        const int kInstancesPerUnit = 10;

        public delegate void ChangeEvent(InstanceArea instanceArea);
        public static event ChangeEvent ms_changeEvent;








        class InstanceAreaTransformDetailsProvider : ITransformDetailsProviding {
            List<InstanceArea> m_additiveAreas;
            List<InstanceArea> m_subtractiveAreas;
            float m_totalArea;
            long m_instanceCount;
            int m_seed;

            public long DetailsCount => m_instanceCount;

            public InstanceAreaTransformDetailsProvider(IEnumerable<InstanceArea> areas, float density, int seed) {
                m_additiveAreas = new List<InstanceArea>();
                m_subtractiveAreas = new List<InstanceArea>();
                foreach (InstanceArea area in areas) {
                    if (area.m_operation == Operation.Additive) {
                        m_additiveAreas.Add(area);
                    } else {
                        m_subtractiveAreas.Add(area);
                    }
                }

                m_totalArea = 0;
                foreach (InstanceArea instanceArea in m_additiveAreas) {
                    m_totalArea += instanceArea.Area;
                }

                m_instanceCount = Mathf.FloorToInt(Mathf.Lerp(0, kInstancesPerUnit * Mathf.Sqrt(m_totalArea), density));
                m_seed = seed;
            }

            public IEnumerable<TransformDetails> GenerateDetails() {
                Random.State oldState = Random.state;
                Random.InitState(m_seed);

                for (int i = 0; i < m_instanceCount; ++i) {
                    float random = Random.Range(0, m_totalArea);

                    InstanceArea selectedArea = null;
                    foreach (InstanceArea instanceArea in m_additiveAreas) {
                        selectedArea = instanceArea;
                        random -= instanceArea.Area;
                        if (random <= 0) {
                            break;
                        }
                    }

                    Vector3 point;
                    bool badPosition;
                    do {
                        point = selectedArea.RandomPointInArea();

                        badPosition = false;
                        foreach (InstanceArea instanceArea in m_subtractiveAreas) {
                            if (instanceArea.TestPointInArea(point)) {
                                badPosition = true;
                                break;
                            }
                        }
                    } while (badPosition);

                    TransformDetails details = new TransformDetails();
                    details.position = point;
                    details.rotation = Quaternion.identity;// LookRotation(selectedTriangle.forward, selectedTriangle.normal);
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








        public static ITransformDetailsProviding TransformDetailsProvider(IEnumerable<InstanceArea> additiveAreas, float density, int seed) {
            return new InstanceAreaTransformDetailsProvider(additiveAreas, density, seed);
        }

        public enum Operation {
            Additive,
            Subtractive
        }

        public Operation m_operation;

        protected void OnValidate() {
            SendChangeEvent();
        }

        protected void SendChangeEvent() {
            if (ms_changeEvent != null) {
                ms_changeEvent(this);
            }
        }

        protected abstract float Area { get; }
        protected abstract Vector3 RandomPointInArea();
        protected abstract bool TestPointInArea(Vector3 point);

        /*void OnDrawGizmos() {
            foreach (Triangle triangle in GetTriangles()) {
                Gizmos.DrawLine(triangle.p0, triangle.p1);
                Gizmos.DrawLine(triangle.p1, triangle.p2);
                Gizmos.DrawLine(triangle.p2, triangle.p0);
            }
        }*/

        //protected abstract IEnumerable<Triangle> GetTriangles();
    }
}
