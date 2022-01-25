using UnityEngine;

[System.Serializable]
public partial struct VectorLineRegularPolygon {
    public int corners;
    public float radius;
    public float startAngle;

    public Color color;

    public override int GetHashCode() {
        return corners.GetHashCode()
            ^ radius.GetHashCode()
            ^ startAngle.GetHashCode()
            ^ color.GetHashCode();
    }
}

public partial struct VectorLineRegularPolygon : IVectorLineVertexProviding {
    int IVectorLineVertexProviding.VertexCount => corners * 2;

    void IVectorLineVertexProviding.GetVertices(IVectorLineVertexArray vertexArray) {
        Vector3 offset = Vector3.up * radius;

        Quaternion initialRotation = Quaternion.Euler(0, 0, startAngle);
        Vector3 initialPosition = initialRotation * offset;
        Vector3 previousPosition = initialPosition;

        for (int i = 1; i < corners; ++i) {
            float angle = startAngle + Mathf.Lerp(0.0f, 360.0f, i / (float)corners);
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Vector3 position = rotation * offset;
            vertexArray.AddVertex(new VectorLineVertex(previousPosition, color));
            vertexArray.AddVertex(new VectorLineVertex(position, color));
            previousPosition = position;
        }

        vertexArray.AddVertex(new VectorLineVertex(previousPosition, color));
        vertexArray.AddVertex(new VectorLineVertex(initialPosition, color));
    }
}
