using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SKFX.WorldBuilder {
    [ExecuteInEditMode]
    public class DiscInstanceArea : InstanceArea {
        [BurstCompile(CompileSynchronously = true)]
        struct TransformDetailsGeneratorJob : IJob, IDisposable {
            [ReadOnly]
            public float objectRadius;

            [ReadOnly]
            public uint randomSeed;

            [ReadOnly]
            public Vector3 center;

            [ReadOnly]
            public float radius;

            [ReadOnly]
            public Matrix4x4 matrix;

            [WriteOnly]
            public NativeArray<ObjectDetails> Output;

            public void Execute() {
                Unity.Mathematics.Random rnd = new Unity.Mathematics.Random(randomSeed);
                for (int i = 0; i < Output.Length; ++i) {
                    Vector2 point = rnd.NextFloat2Direction() * rnd.NextFloat(radius);

                    ObjectDetails details = new ObjectDetails();
                    details.transformDetails.position = matrix * new Vector4(point.x, 0, point.y, 1);
                    details.transformDetails.rotation = Quaternion.identity;
                    details.transformDetails.uniformScale = 1.0f;
                    details.radius = objectRadius;

                    Output[i] = details;
                }
            }

            public void Dispose() {
                // Don't need to do anything
            }
        }

        [BurstCompile(CompileSynchronously = true)]
        struct TransformDetailsFilterJob : IJob, IDisposable {
            [ReadOnly]
            public Vector3 center;

            [ReadOnly]
            public float radius;

            [ReadOnly]
            public Matrix4x4 matrix;

            [ReadOnly]
            public NativeArray<ObjectDetails> Input;

            [WriteOnly]
            public NativeArray<bool> Output;

            public void Execute() {
                for (int i = 0; i < Output.Length; ++i) {
                    Vector3 position = Input[i].transformDetails.position;
                    Vector3 point = matrix * new Vector4(position.x, position.y, position.z, 1.0f);
                    Vector3 delta = point.XZ() - center.XZ();
                    bool test = delta.sqrMagnitude <= radius * radius;
                    Output[i] = test;
                }
            }

            public void Dispose() {
                // Don't need to do anything
            }
        }

        public Vector3 m_center = Vector3.zero;
        public float m_radius = 1.0f;

        protected override float Area => Mathf.PI * m_radius * m_radius;

        protected override IJobContainer CreateTransformDetailsGeneratorJob(NativeArray<ObjectDetails> details, long instanceCount, float objectRadius, uint randomSeed) {
            TransformDetailsGeneratorJob job = new TransformDetailsGeneratorJob();
            job.objectRadius = objectRadius;
            job.randomSeed = randomSeed;
            job.center = m_center;
            job.radius = m_radius;
            job.matrix = transform.localToWorldMatrix;
            job.Output = details;

            return new JobContainer<TransformDetailsGeneratorJob>(job);
        }

        protected override IJobContainer CreateTransformDetailsFilterJob(NativeArray<ObjectDetails> details, NativeArray<bool> overlap) {
            TransformDetailsFilterJob job = new TransformDetailsFilterJob();
            job.center = m_center;
            job.radius = m_radius;
            job.matrix = transform.worldToLocalMatrix;
            Debug.Log(job.matrix);
            job.Input = details;
            job.Output = overlap;

            return new JobContainer<TransformDetailsFilterJob>(job);
        }

        protected override Vector3 RandomPointInArea() {
            Vector2 point = UnityEngine.Random.insideUnitCircle * m_radius;
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