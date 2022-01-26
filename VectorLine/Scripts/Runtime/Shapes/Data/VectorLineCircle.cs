using UnityEngine;

[System.Serializable]
public partial struct VectorLineCircle {
    public float radius;

    public Color color;

    public override int GetHashCode() {
        int h = 17;
        h = (h * 31) ^ radius.GetHashCode();
        h = (h * 31) ^ color.GetHashCode();
        return h;
    }
}

public partial struct VectorLineCircle : IVectorLineVertexProviding {
    int IVectorLineVertexProviding.VertexCount => GetCornerCount() * 2;

    int GetCornerCount() {
        if (radius < 0.1f) {
            return 10;
        } if (radius < 1.0f) {
            return 20;
        } else if (radius < 10.0f) {
            return 30;
        } else {
            return 50;
        }
    }

    void IVectorLineVertexProviding.GetVertices(IVectorLineVertexArray vertexArray) {
        int corners = GetCornerCount();

        Vector3 offset = Vector3.up * radius;

        Vector3 initialPosition = offset;
        Vector3 previousPosition = initialPosition;

        for (int i = 1; i < corners; ++i) {
            float angle = Mathf.Lerp(0.0f, 360.0f, i / (float)corners);
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
