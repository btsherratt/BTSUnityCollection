using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SplineMesh : MonoBehaviour {
    public Material m_material;

    public Mesh m_generatedMesh { get; private set; }

    bool m_dirty;

    private void OnEnable() {
        m_dirty = true;
    }

    private void OnValidate() {
        m_dirty = true;
    }

    private void Update() {
        if (m_dirty) {
            m_dirty = false;
            GenerateMesh();
            UpdatePreview();
        }
    }

    void GenerateMesh() {
        Spline spline = GetComponent<Spline>();

        /*float zSize = m_baseMesh.bounds.size.z;
        int repeatTimes = Mathf.FloorToInt(spline.Length / zSize);

        Debug.Log($"r: {spline.Length} {zSize} {repeatTimes}");

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> tangents = new List<Vector3>();
        List<int> indices = new List<int>();

        for (int i = 0; i < repeatTimes; ++i) {
            int indexOffset = vertices.Count;

            for (int j = 0; j < m_baseMesh.vertexCount; ++j) {
                Vector3 vertex = m_baseMesh.vertices[j];
                Vector3 tangent = m_baseMesh.tangents[j];

                float zOffset = zSize * i;
                float z = zOffset + vertex.z;

                Vector3 forward;
                Vector3 basePosition = spline.Lerp(out forward, z, Spline.Units.World);
                Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

                Vector3 correctedVertex = basePosition + (rotation * Vector3.left * vertex.x) + (rotation * Vector3.up * vertex.y);
                vertices.Add(correctedVertex);

                Vector3 correctedTangent = rotation * tangent;
                tangents.Add(correctedTangent);
            }

            foreach (int index in m_baseMesh.GetIndices(0)) {
                indices.Add(indexOffset + index);
            }
        }

        Mesh generatedMesh = new Mesh();
        generatedMesh.name = "Generated Mesh";
        generatedMesh.SetVertices(vertices);
        generatedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        generatedMesh.SetIndices(indices, m_baseMesh.GetTopology(0), 0);

        m_generatedMesh = generatedMesh;

        Debug.Log("OK");*/
    }

    void UpdatePreview() {
        Transform previewTransform = transform.Find("__SplineModel");
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        if (previewTransform == null) {
            GameObject previewObject = new GameObject("__SplineModel");
            previewObject.hideFlags = HideFlags.HideAndDontSave;
            previewTransform = previewObject.transform;
            previewTransform.SetParent(transform, false);
            meshFilter = previewObject.AddComponent<MeshFilter>();
            meshRenderer = previewObject.AddComponent<MeshRenderer>();
        } else {
            meshFilter = previewTransform.GetComponent<MeshFilter>();
            meshRenderer = previewTransform.GetComponent<MeshRenderer>();
        }
        previewTransform.gameObject.hideFlags = HideFlags.DontSave;
        meshFilter.sharedMesh = m_generatedMesh;
        meshRenderer.sharedMaterial = m_material;
    }
}
