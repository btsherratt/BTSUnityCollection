using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


namespace SKFX.WorldBuilder {
    [ExecuteInEditMode]
    public class DiscInstanceArea : InstanceArea {
        struct JobContainer : IJobContainer {
            TransformDetailsGeneratorJob job;

            public JobContainer(TransformDetailsGeneratorJob job) {
                this.job = job;
            }

            public void Dispose() {
                // Don't need to do anything
            }

            public JobHandle Schedule() {
                return job.Schedule();
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        struct TransformDetailsGeneratorJob : IJob {
            [ReadOnly]
            public uint randomSeed;

            [ReadOnly]
            public Vector3 center;

            [ReadOnly]
            public float radius;

            [ReadOnly]
            public Matrix4x4 matrix;

            [WriteOnly]
            public NativeArray<TransformDetails> Output;

            public void Execute() {
                Unity.Mathematics.Random rnd = new Unity.Mathematics.Random(randomSeed);
                for (int i = 0; i < Output.Length; ++i) {
                    TransformDetails details = new TransformDetails();
                    Vector2 point = rnd.NextFloat2Direction() * rnd.NextFloat(radius);
                    details.position = matrix * new Vector4(point.x, 0, point.y, 1);
                    Output[i] = details;
                }
            }
        }

        public Vector3 m_center = Vector3.zero;
        public float m_radius = 1.0f;

        protected override float Area => Mathf.PI * m_radius * m_radius;

        protected override IJobContainer ScheduleTransformDetailsGeneratorJob(NativeArray<TransformDetails> details, long instanceCount, uint randomSeed) {
            TransformDetailsGeneratorJob job = new TransformDetailsGeneratorJob();
            job.randomSeed = randomSeed;
            job.center = m_center;
            job.radius = m_radius;
            job.matrix = transform.localToWorldMatrix;
            job.Output = details;

            return new JobContainer(job);
        }

        protected override Vector3 RandomPointInArea() {
            Vector2 point = Random.insideUnitCircle * m_radius;
            Vector3 position = transform.position + new Vector3(point.x, 0, point.y);
            return position;
        }

        public override bool TestPointInArea(Vector3 point) {
            return TestPointInArea(point, transform.position);
        }

        public override bool TestPointInAreaXZ(Vector3 point) {
            return TestPointInArea(point.XZ(), transform.position.XZ());
        }

        bool TestPointInArea(Vector3 point, Vector3 position) {
            Vector3 delta = point - position;
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