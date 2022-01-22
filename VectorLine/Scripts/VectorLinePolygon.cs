using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorLinePolygon : VectorLineDrawable {
    public VectorLinePolygonAsset m_polygonAsset;

    public override int VertexCount => m_polygonAsset.m_vertices != null ? m_polygonAsset.m_vertices.Length * (m_polygonAsset.m_lineLoop ? 2 : 1) : 0;

#if UNITY_EDITOR
    public override bool IsDirty => UnityEditor.EditorUtility.IsDirty(m_polygonAsset);
#else
    public override bool IsDirty => false;
#endif

    public override int DrawableID => m_polygonAsset != null ? m_polygonAsset.GetInstanceID() : VectorLineDrawable.INVALID_ID;

    public override IEnumerable<VectorLineVertex> GetVertices() {
        VectorLineVertex initialVertex = m_polygonAsset.m_vertices[0];
        VectorLineVertex previousVertex = initialVertex;
        for (int i = 1; i < m_polygonAsset.m_vertices.Length; ++i) {
            VectorLineVertex vertex = m_polygonAsset.m_vertices[i];

            yield return previousVertex;

            if (m_polygonAsset.m_lineLoop) {
                yield return vertex;
            }

            previousVertex = vertex;
        }
        
        yield return previousVertex;

        if (m_polygonAsset.m_lineLoop) {
            yield return initialVertex;
        }
    }
}
