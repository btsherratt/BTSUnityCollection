using System.Collections.Generic;
using UnityEngine;

using Unity.Collections;
using Unity.Jobs;

namespace SKFX.WorldBuilder {
    public abstract class InstanceArea : MonoBehaviour {
        protected interface IJobContainer : System.IDisposable {
            JobHandle Schedule();
        }

        public delegate void ChangeEvent(InstanceArea instanceArea);
        public static event ChangeEvent ms_changeEvent;

        class InstanceAreaTransformDetailsProvider : ITransformDetailsProviding {
            List<InstanceArea> m_additiveAreas;
            List<InstanceArea> m_subtractiveAreas;
            float m_totalArea;
            long m_instanceCount;
            uint m_seed;

            public long DetailsCount => m_instanceCount;

            public InstanceAreaTransformDetailsProvider(IEnumerable<InstanceArea> areas, float density, uint seed, int instancesPerUnit) {
                m_additiveAreas = new List<InstanceArea>();
                m_subtractiveAreas = new List<InstanceArea>();
                foreach (InstanceArea area in areas) {
                    if (area.isActiveAndEnabled) {
                        if (area.m_operation == Operation.Additive) {
                            m_additiveAreas.Add(area);
                        } else {
                            m_subtractiveAreas.Add(area);
                        }
                    }
                }

                m_totalArea = 0;
                foreach (InstanceArea instanceArea in m_additiveAreas) {
                    m_totalArea += instanceArea.Area;
                }

                m_instanceCount = Mathf.FloorToInt(Mathf.Lerp(0, instancesPerUnit * Mathf.Sqrt(m_totalArea), density));
                m_seed = seed;
            }



            /*public struct MyJob : IJob {
                [ReadOnly]
                public InstanceArea instanceArea;

                public NativeArray<TransformDetails> result;

                public void Execute() {
                    TransformDetails details = new TransformDetails();
                    details.position = instanceArea.RandomPointInArea();
                    details.rotation = Quaternion.identity;// LookRotation(selectedTriangle.forward, selectedTriangle.normal);
                    details.uniformScale = 1.0f;

                    result[0] = details;
                }
            }*/





            public long GenerateDetails(TransformDetails[] transformDetailsOut, long startIndex, int snapLayerMask = 0) {
                NativeArray<TransformDetails>[] results = new NativeArray<TransformDetails>[m_additiveAreas.Count];
                IJobContainer[] jobs = new IJobContainer[m_additiveAreas.Count];
                NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(m_additiveAreas.Count, Allocator.Temp);

                for (int i = 0; i < m_additiveAreas.Count; ++i) {
                    InstanceArea instanceArea = m_additiveAreas[i];
                    float areaFraction = instanceArea.Area / m_totalArea;
                    long instances = Mathf.FloorToInt(m_instanceCount * areaFraction);
                    results[i] = new NativeArray<TransformDetails>((int)instances, Allocator.TempJob);
                    jobs[i] = instanceArea.ScheduleTransformDetailsGeneratorJob(results[i], instances, m_seed); // FIXME, rotate the seed?
                    jobHandles[i] = jobs[i].Schedule();
                }

                JobHandle.CompleteAll(jobHandles);

                long currentStartIndex = startIndex;
                for (int i = 0; i < m_additiveAreas.Count; ++i) {
                    jobs[i].Dispose();

                    for (int j = 0; j < results[i].Length; ++j) {
                        transformDetailsOut[currentStartIndex + j] = results[i][j];

                        if (snapLayerMask > 0) {
                            ref TransformDetails details = ref transformDetailsOut[currentStartIndex + j];

                            Vector3 normal = details.rotation * Vector3.up;
                            RaycastHit hit;

                            if (Physics.Raycast(details.position + Vector3.up * 3000, Vector3.down, out hit, float.MaxValue, snapLayerMask)) {
                                details.position = hit.point;
                                details.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.right, hit.normal), hit.normal);
                            }
                        }
                    }

                    currentStartIndex += results[i].Length;
                    results[i].Dispose();
                }

                return currentStartIndex;
            }
        }

        public static ITransformDetailsProviding TransformDetailsProvider(IEnumerable<InstanceArea> additiveAreas, float density, uint seed, int instancesPerUnit) {
            return new InstanceAreaTransformDetailsProvider(additiveAreas, density, seed, instancesPerUnit);
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

        protected abstract IJobContainer ScheduleTransformDetailsGeneratorJob(NativeArray<TransformDetails> details, long instanceCount, uint randomSeed);
        //protected abstract IJobContainer ScheduleTransformDetailsFilterJob(NativeArray<TransformDetails> details);


        //        protected abstract bool TestPointInArea(Vector3 point);

        public abstract bool TestPointInArea(Vector3 point);
        public abstract bool TestPointInAreaXZ(Vector3 point);

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
