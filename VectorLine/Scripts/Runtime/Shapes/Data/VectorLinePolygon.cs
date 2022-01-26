using UnityEngine;

[System.Serializable]
public partial struct VectorLinePolygon {
    public bool loop;
    public VectorLineVertex[] vertices;

    public override int GetHashCode() {
        int h = 17;
        h = (h * 31) ^ loop.GetHashCode();
        h = (h * 31) ^ vertices.GetHashCode();
        return h;
    }
}

public partial struct VectorLinePolygon : IVectorLineVertexProviding {
    int IVectorLineVertexProviding.VertexCount => vertices != null ? (loop ? vertices.Length * 2 : vertices.Length) : 0;

    void IVectorLineVertexProviding.GetVertices(IVectorLineVertexArray vertexArray) {
        for (int i = 0; i < vertices.Length; ++i) {
            vertexArray.AddVertex(vertices[i]);

            if (loop) {
                vertexArray.AddVertex(vertices[(i + 1) % vertices.Length]);
            }
        }
    }
}
