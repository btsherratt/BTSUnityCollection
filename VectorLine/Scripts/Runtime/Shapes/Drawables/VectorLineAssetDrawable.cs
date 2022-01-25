using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorLineAssetDrawable : VectorLineDrawable {
    public VectorLineShapeAsset Asset {
        get => m_asset;
        set {
            if (m_asset != value) {
                m_asset = value;
                m_clean = false;
            }
        }
    }

    public override bool IsDirty => !m_clean
#if UNITY_EDITOR
        || (m_asset != null && UnityEditor.EditorUtility.IsDirty(m_asset))
#endif
        ;

    public override int VertexCount => m_asset != null ? m_asset.VertexCount : 0;

    [SerializeField]
    VectorLineShapeAsset m_asset;

    bool m_clean;

    public override void GetVertices(IVectorLineVertexArray vertexArray) {
        m_clean = true;
        m_asset.GetVertices(vertexArray);
    }

#if UNITY_EDITOR
    void OnValidate() {
        m_clean = false;
    }
#endif
}
