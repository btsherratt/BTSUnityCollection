using UnityEngine;

namespace SKFX.WorldBuilder {
    [ExecuteInEditMode]
    public class MeshInstanceArea : TrianglesInstanceArea {
        public Mesh m_mesh;

        private new void OnValidate() {
            MarkTrianglesDirty();
            base.OnValidate();
        }

        protected override Triangle[] GenerateTriangles() {
            int numTrianges = (int)(m_mesh.triangles.LongLength / 3);
            Triangle[] triangles = new Triangle[numTrianges];

            for (int i = 0; i < numTrianges; ++i) {
                int vertexIdx1 = m_mesh.triangles[i * 3 + 0];
                int vertexIdx2 = m_mesh.triangles[i * 3 + 1];
                int vertexIdx3 = m_mesh.triangles[i * 3 + 2];

                Vector3 pointA = transform.TransformPoint(m_mesh.vertices[vertexIdx1]);
                Vector3 pointB = transform.TransformPoint(m_mesh.vertices[vertexIdx2]);
                Vector3 pointC = transform.TransformPoint(m_mesh.vertices[vertexIdx3]);

                triangles[i] = new Triangle(pointA, pointB, pointC);
            }

            return triangles;
        }
    }
}