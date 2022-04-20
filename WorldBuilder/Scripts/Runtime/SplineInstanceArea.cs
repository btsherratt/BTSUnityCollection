using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SKFX.WorldBuilder {
    [ExecuteInEditMode]
    public class SplineInstanceArea : InstanceArea {
        struct JobContainer : IJobContainer {
            TransformDetailsGeneratorJob job;

            public JobContainer(TransformDetailsGeneratorJob job) {
                this.job = job;
            }

            public void Dispose() {
                job.spline.Dispose();
            }

            public JobHandle Schedule() {
                return job.Schedule();
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        struct TransformDetailsGeneratorJob : IJob {
            [ReadOnly]
            public Spline.SplineImpl spline;

            [ReadOnly]
            public float width;

            [ReadOnly]
            public Matrix4x4 matrix;

            [ReadOnly]
            public uint randomSeed;

            [WriteOnly]
            public NativeArray<TransformDetails> Output;

            public void Execute() {
                Unity.Mathematics.Random rnd = new Unity.Mathematics.Random(randomSeed);

                for (int i = 0; i < Output.Length; ++i) {
                    TransformDetails details = new TransformDetails();

                    Vector3 forward;
                    Vector3 point = spline.Lerp(out forward, rnd.NextFloat(0.0f, spline.Length), Spline.Units.World);
                    //point = transform.TransformPoint(point);
                    // FIXME, offset
                    //point = m_spline.transform.TransformPoint(point);
                    point += Vector3.Cross(forward, Vector3.up) * rnd.NextFloat(-width / 2.0f, width / 2.0f);
                    point = matrix * new Vector4(point.x, point.y, point.z, 1.0f);

                    details.position = point;
                    Output[i] = details;
                }
            }
        }

        public Spline m_spline;
        public float m_width = 1.0f;

        protected override float Area => m_spline != null ? m_spline.Length * m_width : 0.0f;

        void OnEnable() {
            Spline.ms_updateEvent -= OnSplineUpdateEvent;
            Spline.ms_updateEvent += OnSplineUpdateEvent;
        }

        private void OnDisable() {
            Spline.ms_updateEvent -= OnSplineUpdateEvent;
        }

        private void OnSplineUpdateEvent(Spline spline) {
            if (spline == m_spline || spline.gameObject == gameObject) {
                SendChangeEvent();
            }
        }

#if false
        protected override IEnumerable<Triangle> GetTriangles() {
            Spline s = m_spline ?? GetComponentInChildren<Spline>();
            Spline.SplinePoint[] points = s.GenerateSpline();

            for (int i = 1; i < points.Length - 1; ++i) {
                Vector3 P0 = points[i - 1].position;
                Vector3 P1 = points[i].position;
                Vector3 P2 = points[i + 1].position;

                Vector3 pointA = transform.TransformPoint(P2 + Vector3.Cross((P2 - P1).normalized, Vector3.up) * m_width / 2.0f);
                Vector3 pointB = transform.TransformPoint(P2 - Vector3.Cross((P2 - P1).normalized, Vector3.up) * m_width / 2.0f);
                Vector3 pointC = transform.TransformPoint(P1 + Vector3.Cross((P1 - P0).normalized, Vector3.up) * m_width / 2.0f);
                Vector3 pointD = transform.TransformPoint(P1 - Vector3.Cross((P1 - P0).normalized, Vector3.up) * m_width / 2.0f);

                yield return new Triangle(pointA, pointB, pointC);
                yield return new Triangle(pointB, pointC, pointD);
            }

            /*for (int i = 0; i < kSubdivisions; ++i) {
                float angle1 = Mathf.Lerp(0, 360, i / (float)kSubdivisions);
                float angle2 = Mathf.Lerp(0, 360, (i + 1) / (float)kSubdivisions);

                Vector3 pointA = transform.TransformPoint(m_center + Quaternion.Euler(0, angle1, 0) * Vector3.forward * m_radius);
                Vector3 pointB = transform.TransformPoint(m_center + Quaternion.Euler(0, angle2, 0) * Vector3.forward * m_radius);
                Vector3 pointC = transform.TransformPoint(m_center);

                yield return new Triangle(pointA, pointB, pointC);
            }*/
        }
#endif




        protected override IJobContainer ScheduleTransformDetailsGeneratorJob(NativeArray<TransformDetails> details, long instanceCount, uint randomSeed) {
            TransformDetailsGeneratorJob job = new TransformDetailsGeneratorJob();
            job.randomSeed = randomSeed;
            job.spline = m_spline.GetSplineImpl(Allocator.TempJob);
            job.width = m_width;
            job.matrix = m_spline.transform.localToWorldMatrix;
            job.Output = details;

            return new JobContainer(job);
        }






        protected override Vector3 RandomPointInArea() {
            Vector3 forward;
            Vector3 point = m_spline.Lerp(out forward, Random.Range(0.0f, m_spline.Length), Spline.Units.World);
            //point = transform.TransformPoint(point);
            // FIXME, offset
            point = m_spline.transform.TransformPoint(point);
            point += Vector3.Cross(forward, Vector3.up) * Random.Range(-m_width / 2.0f, m_width / 2.0f);
            return point;
        }


        public override bool TestPointInArea(Vector3 point) {
            point = m_spline.transform.InverseTransformPoint(point);
            return m_spline.TestDistanceToSpline(point, m_width);
        }

        public override bool TestPointInAreaXZ(Vector3 point) {
            point = m_spline.transform.InverseTransformPoint(point);
            return m_spline.TestDistanceToSpline(point, m_width, new Vector3(1, 0, 1));
        }
    }
}