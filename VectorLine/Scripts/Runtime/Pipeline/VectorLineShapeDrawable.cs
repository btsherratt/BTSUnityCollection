using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorLineShapeDrawable<T> : VectorLineDrawable where T : IVectorLineVertexProviding, new() {
    [Flatten]
    public T m_shape;

    int m_cleanShapeHash;
    public override bool IsDirty => m_shape.GetHashCode() != m_cleanShapeHash;

    public override int VertexCount => m_shape.VertexCount;

    public override void GetVertices(IVectorLineVertexArray vertexArray) {
        m_cleanShapeHash = m_shape.GetHashCode();
        m_shape.GetVertices(vertexArray);
    }
}
