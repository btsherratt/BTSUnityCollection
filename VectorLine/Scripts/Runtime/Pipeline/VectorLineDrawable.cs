using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVectorLineVertexArray {
    void AddVertex(VectorLineVertex vertex);
}

public interface IVectorLineVertexProviding {
    public int VertexCount { get; }
    public void GetVertices(IVectorLineVertexArray vertexArray);
}

public abstract class VectorLineDrawable : TrackedMonoBehaviour<VectorLineDrawable> {
    public const int INVALID_ID = 0;

    public abstract bool IsDirty { get; }

    public virtual int DrawableID => GetInstanceID();

    public abstract int VertexCount { get; }

    public abstract void GetVertices(IVectorLineVertexArray vertexArray);

    private void OnEnable() {
        // We just want the enable box to show up... Thanks... :)
    }
}
