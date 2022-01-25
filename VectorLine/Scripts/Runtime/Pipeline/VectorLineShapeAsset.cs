using UnityEngine;

public abstract class VectorLineShapeAsset : ScriptableObject {
    public abstract int VertexCount { get; }

    public abstract void GetVertices(IVectorLineVertexArray vertexArray);
}

public class VectorLineShapeAsset<T> : VectorLineShapeAsset where T : IVectorLineVertexProviding, new() {
    [Flatten]
    public T m_shape;

    public override int VertexCount => m_shape.VertexCount;

    public override void GetVertices(IVectorLineVertexArray vertexArray) {
        m_shape.GetVertices(vertexArray);
    }
}
