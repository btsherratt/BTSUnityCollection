using UnityEngine;

public class SplineModel : MonoBehaviour
{
    public Material material;

    public float width = 1.0f;
    public float offset = 0.01f;

    //private void OnValidate()
    //{
    //    Generate();
    //}

    public void Generate(Spline.SplinePoint[] points)
    {
        Spline s = GetComponent<Spline>();
        SplineDesigner sd = GetComponent<SplineDesigner>();

        Vector3[] vertices = new Vector3[(points.Length - 1) * 4];
        Vector3[] normals = new Vector3[(points.Length - 1) * 4];
        int[] triangles = new int[(points.Length - 1) * 6];

        int vtx = 0, tri = 0;
        for (int i = 1; i < points.Length - 1; ++i)
        {
            Vector3 P0 = points[i - 1].position;
            Vector3 P1 = points[  i  ].position;
            Vector3 P2 = points[i + 1].position;

            vertices[vtx] = transform.InverseTransformPoint(sd.ProjectToCollider(P2 + Vector3.Cross((P2 - P1).normalized, Vector3.up) * width / 2.0f) + Vector3.up * offset); //transform.InverseTransformPoint(P0);
            vertices[vtx + 1] = transform.InverseTransformPoint(sd.ProjectToCollider(P2 - Vector3.Cross((P2 - P1).normalized, Vector3.up) * width / 2.0f) + Vector3.up * offset); //transform.InverseTransformPoint(P1);
            vertices[vtx + 2] = transform.InverseTransformPoint(sd.ProjectToCollider(P1 + Vector3.Cross((P1 - P0).normalized, Vector3.up) * width / 2.0f) + Vector3.up * offset);
            vertices[vtx + 3] = transform.InverseTransformPoint(sd.ProjectToCollider(P1 - Vector3.Cross((P1 - P0).normalized, Vector3.up) * width / 2.0f) + Vector3.up * offset);

            normals[vtx] = Vector3.Cross(vertices[vtx], vertices[vtx + 1]);
            normals[vtx + 1] = Vector3.Cross(vertices[vtx + 1], vertices[vtx + 2]);
            normals[vtx + 2] = Vector3.Cross(vertices[vtx + 2], vertices[vtx + 1]);
            normals[vtx + 3] = Vector3.Cross(vertices[vtx + 3], vertices[vtx + 2]);

            triangles[tri] = vtx;
            triangles[tri + 1] = vtx + 1;
            triangles[tri + 2] = vtx + 2;

            triangles[tri + 3] = vtx+ 1;
            triangles[tri + 4] = vtx + 3;
            triangles[tri + 5] = vtx + 2;

            vtx += 4;
            tri += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        //mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        MeshFilter mf = GetComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }
}
