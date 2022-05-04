using System;
using System.Collections.Generic;
using UnityEngine;

using Unity.Collections;
using Unity.Jobs;

namespace SKFX.WorldBuilder {
    public abstract class InstanceArea : TrackedMonoBehaviour<InstanceArea> {
        protected interface IJobContainer : IDisposable {
            JobHandle Schedule();
        }

        protected struct JobContainer<T> : IJobContainer where T : struct, IJob, IDisposable {
            T job;

            public JobContainer(T job) {
                this.job = job;
            }

            public void Dispose() {
                job.Dispose();
            }

            public JobHandle Schedule() {
                return job.Schedule();
            }
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

            public InstanceAreaTransformDetailsProvider(IEnumerable<InstanceArea> additiveAreas, IEnumerable<InstanceArea> subtractiveAreas, float density, uint seed, int instancesPerUnit) {
                m_additiveAreas = new List<InstanceArea>(additiveAreas);
                m_subtractiveAreas = new List<InstanceArea>(subtractiveAreas);

                m_totalArea = 0;
                foreach (InstanceArea instanceArea in m_additiveAreas) {
                    m_totalArea += instanceArea.Area;
                }

                m_instanceCount = Mathf.FloorToInt(Mathf.Lerp(0, instancesPerUnit * Mathf.Sqrt(m_totalArea), density));
                m_seed = seed;
            }

            public long GenerateDetails(TransformDetails[] transformDetailsOut, long startIndex, float objectRadius, float groundAngleMultiplier, float maxAngle, int snapLayerMask = 0) {
                List<NativeArray<ObjectDetails>> additiveResults = new List<NativeArray<ObjectDetails>>();
                List<IJobContainer> additiveJobs = new List<IJobContainer>();
                NativeArray<JobHandle> additiveJobHandles = new NativeArray<JobHandle>(m_additiveAreas.Count, Allocator.Temp);

                long totalNumInstances = 0;
                for (int i = 0; i < m_additiveAreas.Count; ++i) {
                    InstanceArea instanceArea = m_additiveAreas[i];
                    float areaFraction = instanceArea.Area / m_totalArea;
                    long instances = Mathf.FloorToInt(m_instanceCount * areaFraction);
                    if (instances > 0 && instances < int.MaxValue) {
                        additiveResults.Add(new NativeArray<ObjectDetails>((int)instances, Allocator.TempJob));
                        additiveJobs.Add(instanceArea.CreateTransformDetailsGeneratorJob(additiveResults[i], instances, objectRadius, m_seed)); // FIXME, rotate the seed?
                        additiveJobHandles[i] = additiveJobs[i].Schedule();
                    } else if (instances > 0) {
                        Debug.LogError("Instance is exceeding the max instance count... :(");
                    }
                    totalNumInstances += instances;
                }

                // Urm... weird.
                if (totalNumInstances >= int.MaxValue) {
                    Debug.LogError("We are exceeding the max instance count... (or zero) :(");
                    return startIndex;
                }
                if (totalNumInstances < 0) {
                    return startIndex;
                }

                JobHandle.CompleteAll(additiveJobHandles);
                additiveJobHandles.Dispose();

                NativeArray<ObjectDetails> combinedResults = new NativeArray<ObjectDetails>((int)totalNumInstances, Allocator.TempJob);

                int currentIdx = 0;
                for (int i = 0; i < additiveResults.Count; ++i) {
                    if (additiveResults[i] != null) {
                        for (int j = 0; j < additiveResults[i].Length; ++j) {
                            combinedResults[currentIdx++] = additiveResults[i][j];
                        }

                        additiveJobs[i].Dispose();
                        additiveResults[i].Dispose();
                    }
                }

                NativeArray<bool>[] subtractiveResults = new NativeArray<bool>[m_subtractiveAreas.Count];
                IJobContainer[] subtractiveJobs = new IJobContainer[m_subtractiveAreas.Count];
                NativeArray<JobHandle> subtractiveJobHandles = new NativeArray<JobHandle>(m_subtractiveAreas.Count, Allocator.Temp);

                for (int i = 0; i < m_subtractiveAreas.Count; ++i) {
                    InstanceArea instanceArea = m_subtractiveAreas[i];
                    subtractiveResults[i] = new NativeArray<bool>(combinedResults.Length, Allocator.TempJob);
                    subtractiveJobs[i] = instanceArea.CreateTransformDetailsFilterJob(combinedResults, subtractiveResults[i]);
                    subtractiveJobHandles[i] = subtractiveJobs[i].Schedule();
                }

                JobHandle.CompleteAll(subtractiveJobHandles);
                subtractiveJobHandles.Dispose();

                long currentOutIndex = startIndex;
                for (int i = 0; i < combinedResults.Length; ++i) {
                    bool pass = true;
                    for (int j = 0; j < m_subtractiveAreas.Count; ++j) {
                        if (subtractiveResults[j][i] == true) {
                            pass = false;
                            break;
                        }
                    }

                    if (pass) {
                        transformDetailsOut[currentOutIndex] = combinedResults[i].transformDetails;
                        ref TransformDetails details = ref transformDetailsOut[currentOutIndex];

                        if (snapLayerMask > 0) {
                            Vector3 normal = details.rotation * Vector3.up;
                            RaycastHit hit;

                            if (Physics.Raycast(details.position + Vector3.up * 3000, Vector3.down, out hit, float.MaxValue, snapLayerMask)) {
                                details.position = hit.point;
                                details.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.right, hit.normal), hit.normal);
                            } else {
                                pass = false;
                            }
                        }

                        if (pass) {
                            float angle = Vector3.Angle(details.rotation * Vector3.up, Vector3.up);
                            pass = angle <= maxAngle;
                        }

                        if (pass) {
                            details.rotation = Quaternion.SlerpUnclamped(Quaternion.identity, details.rotation, groundAngleMultiplier);
                            ++currentOutIndex;
                        }
                    }
                }

                for (int i = 0; i < m_subtractiveAreas.Count; ++i) {
                    subtractiveResults[i].Dispose();
                    subtractiveJobs[i].Dispose();
                }

                combinedResults.Dispose();

                return currentOutIndex;
            }
        }

        public static ITransformDetailsProviding TransformDetailsProvider(IEnumerable<InstanceArea> additiveAreas, IEnumerable<InstanceArea> subtractiveAreas, float density, uint seed, int instancesPerUnit) {
            return new InstanceAreaTransformDetailsProvider(additiveAreas, subtractiveAreas, density, seed, instancesPerUnit);
        }

        public enum Operation {
            Additive,
            Subtractive,
            Cutout,
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

        protected abstract IJobContainer CreateTransformDetailsGeneratorJob(NativeArray<ObjectDetails> details, long instanceCount, float objectRadius, uint randomSeed);
        protected abstract IJobContainer CreateTransformDetailsFilterJob(NativeArray<ObjectDetails> details, NativeArray<bool> overlap);


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
