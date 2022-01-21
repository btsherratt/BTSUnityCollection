using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorLineRegularPolygon : VectorLineDrawable {
    public override bool IsDirty => m_corners.Dirty || m_radius.Dirty || m_color.Dirty;

    public DirtyField<int> m_corners = 3;

    public DirtyField<float> m_radius = 1.0f;

    public DirtyField<Color> m_color = Color.white;

    public override int VertexCount => (m_corners + 1) * 2;

    public override IEnumerable<VectorLineVertex> GetVertices() {
        m_corners.MarkClean();
        m_radius.MarkClean();
        m_color.MarkClean();

        Vector3 initialPosition = Vector3.up * m_radius;
        Vector3 previousPosition = initialPosition;

        for (int i = 1; i < m_corners; ++i) {
            Vector3 offset = Vector3.up * m_radius;
            Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0.0f, 360.0f, Mathf.InverseLerp(0, m_corners, i)));
            Vector3 position = rotation * offset;
            yield return new VectorLineVertex(previousPosition, m_color);
            yield return new VectorLineVertex(position, m_color);
            previousPosition = position;
        }

        yield return new VectorLineVertex(previousPosition, m_color);
        yield return new VectorLineVertex(initialPosition, m_color);
    }
}
