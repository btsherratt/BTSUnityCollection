using UnityEngine;

[System.Serializable]
public partial struct VectorLineStar {
    public int points;

    public float primaryRadius;
    public float secondaryRadius;

    public float startAngle;
    public float secondaryAngleRelativeOffset;

    public Color color;

    public bool starburst;

    public override int GetHashCode() {
        return points.GetHashCode()
            ^ primaryRadius.GetHashCode()
            ^ secondaryRadius.GetHashCode()
            ^ startAngle.GetHashCode()
            ^ secondaryAngleRelativeOffset.GetHashCode()
            ^ color.GetHashCode()
            ^ starburst.GetHashCode();
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
