using UnityEngine;

[System.Serializable]
public partial struct VectorLineRect {
    public Vector2 center;
    public Vector2 size;

    public Color color;

    public override int GetHashCode() {
        return center.GetHashCode()
            ^ size.GetHashCode()
            ^ color.GetHashCode();
    }
}

public partial struct VectorLineRect : IVectorLineVertexProviding {
    int IVectorLineVertexProviding.VertexCount => 8;

    void IVectorLineVertexProviding.GetVertices(IVectorLineVertexArray vertexArray) {
        Vector2 p1 = center - size * 0.5f;
        Vector2 p3 = center + size * 0.5f;
        Vector2 p2 = new Vector2(p3.x, p1.y);
        Vector2 p4 = new Vector2(p1.x, p3.y);

        vertexArray.AddVertex(new VectorLineVertex(p1, color));
        vertexArray.AddVertex(new VectorLineVertex(p2, color));

        vertexArray.AddVertex(new VectorLineVertex(p2, color));
        vertexArray.AddVertex(new VectorLineVertex(p3, color));

        vertexArray.AddVertex(new VectorLineVertex(p3, color));
        vertexArray.AddVertex(new VectorLineVertex(p4, color));

        vertexArray.AddVertex(new VectorLineVertex(p4, color));
        vertexArray.AddVertex(new VectorLineVertex(p1, color));
    }
}
