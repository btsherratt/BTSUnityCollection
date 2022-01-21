using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorLineRect : VectorLineDrawable {
    public override bool IsDirty => m_center.Dirty || m_size.Dirty || m_color.Dirty;

    public DirtyField<Vector2> m_center = Vector2.zero;
    public DirtyField<Vector2> m_size = Vector2.one;

    public DirtyField<Color> m_color = Color.white;

    public override int VertexCount => 8;

    public override IEnumerable<VectorLineVertex> GetVertices() {
        m_center.MarkClean();
        m_size.MarkClean();
        m_color.MarkClean();

        Vector2 p1 = m_center.Value - m_size.Value * 0.5f;
        Vector2 p3 = m_center.Value + m_size.Value * 0.5f;
        Vector2 p2 = new Vector2(p3.x, p1.y);
        Vector2 p4 = new Vector2(p1.x, p3.y);

        yield return new VectorLineVertex(p1, m_color);
        yield return new VectorLineVertex(p2, m_color);

        yield return new VectorLineVertex(p2, m_color);
        yield return new VectorLineVertex(p3, m_color);

        yield return new VectorLineVertex(p3, m_color);
        yield return new VectorLineVertex(p4, m_color);

        yield return new VectorLineVertex(p4, m_color);
        yield return new VectorLineVertex(p1, m_color);
    }
}
