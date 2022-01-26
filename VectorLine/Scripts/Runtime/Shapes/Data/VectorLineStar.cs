using UnityEngine;

[System.Serializable]
public partial struct VectorLineStar {
    public int points;

    public float primaryRadius;
    public float secondaryRadius;

    public float startAngle;
    public float secondaryAngleRelativeOffset;

    public bool starburst;

    public Color color;

    public override int GetHashCode() {
        int h = 17;
        h = (h * 31) ^ points.GetHashCode();
        h = (h * 31) ^ primaryRadius.GetHashCode();
        h = (h * 31) ^ secondaryRadius.GetHashCode();
        h = (h * 31) ^ startAngle.GetHashCode();
        h = (h * 31) ^ secondaryAngleRelativeOffset.GetHashCode();
        h = (h * 31) ^ starburst.GetHashCode();
        h = (h * 31) ^ color.GetHashCode();
        return h;
    }
}

public partial struct VectorLineStar : IVectorLineVertexProviding {
    int IVectorLineVertexProviding.VertexCount => starburst ? points * 2 : points * 4;

    void IVectorLineVertexProviding.GetVertices(IVectorLineVertexArray vertexArray) {
        for (int i = 0; i < points; ++i) {
            Vector3 innerOffset = Vector3.up * secondaryRadius;
            Vector3 outerOffset = Vector3.up * primaryRadius;

            float relativePointA = i / (float)points;
            float relativePointC = ((i + 1) % points) / (float)points;
            Quaternion rotationA = Quaternion.Euler(0, 0, Mathf.Lerp(0.0f, 360.0f, relativePointA) + startAngle);
            Quaternion rotationC = Quaternion.Euler(0, 0, Mathf.Lerp(0.0f, 360.0f, relativePointC) + startAngle);
            Quaternion rotationB = Quaternion.Lerp(rotationA, rotationC, secondaryAngleRelativeOffset);
            
            Vector3 previousOuterPosition = rotationA * outerOffset;
            Vector3 innerPosition = rotationB * innerOffset;
            Vector3 outerPosition = rotationC * outerOffset;

            if (starburst == false) {
                vertexArray.AddVertex(new VectorLineVertex(previousOuterPosition, color));
                vertexArray.AddVertex(new VectorLineVertex(innerPosition, color));
            }

            vertexArray.AddVertex(new VectorLineVertex(innerPosition, color));
            vertexArray.AddVertex(new VectorLineVertex(outerPosition, color));
        }
    }
}
