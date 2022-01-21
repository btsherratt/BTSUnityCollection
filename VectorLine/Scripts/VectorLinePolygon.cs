using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorLinePolygon : VectorLineDrawable {
    public VectorLinePolygonAsset m_polygonAsset;

    public override int VertexCount => m_polygonAsset.m_vertices.Length;

#if UNITY_EDITOR
    public override bool IsDirty => UnityEditor.EditorUtility.IsDirty(m_polygonAsset);
#else
    public override bool IsDirty => false;
#endif

    public override int DrawableID => m_polygonAsset.GetInstanceID();

    public override IEnumerable<VectorLineVertex> GetVertices() {
        foreach (VectorLineVertex v in m_polygonAsset.m_vertices) {
            yield return v;
        }
    }
}
