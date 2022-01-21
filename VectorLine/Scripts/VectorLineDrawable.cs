using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VectorLineDrawable : MonoBehaviour {
    public abstract bool IsDirty { get; }

    public virtual int DrawableID => GetInstanceID();

    public abstract int VertexCount { get; }

    public abstract IEnumerable<VectorLineVertex> GetVertices();
}
