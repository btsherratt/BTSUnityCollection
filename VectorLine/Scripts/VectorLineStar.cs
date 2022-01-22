using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorLineStar : VectorLineDrawable {
    public override bool IsDirty => m_points.Dirty || m_innerRadius.Dirty || m_outerRadius.Dirty || m_angleOffset.Dirty || m_innerRelativeOffset.Dirty || m_innerColor.Dirty || m_outerColor.Dirty || m_starburst.Dirty;

    public DirtyField<int> m_points = 5;

    public DirtyField<float> m_innerRadius = 0.3f;
    public DirtyField<float> m_outerRadius = 1.0f;

    public DirtyField<float> m_angleOffset = 0.0f;
    public DirtyField<float> m_innerRelativeOffset = 0.5f;

    public DirtyField<Color> m_innerColor = Color.white;
    public DirtyField<Color> m_outerColor = Color.white;

    public DirtyField<bool> m_starburst = false;

    public override int VertexCount => m_points * 2 * (m_starburst ? 1 : 2);

    public override IEnumerable<VectorLineVertex> GetVertices() {
        m_points.MarkClean();
        m_innerRadius.MarkClean();
        m_outerRadius.MarkClean();
        m_innerRelativeOffset.MarkClean();
        m_innerColor.MarkClean();
        m_outerColor.MarkClean();

        //Vector3 initialPosition = Vector3.up * m_outerRadius;
        //yield return new VectorLineVertex(initialPosition, m_color);

        for (int i = 0; i < m_points; ++i) {
            Vector3 innerOffset = Vector3.up * m_innerRadius;
            Vector3 outerOffset = Vector3.up * m_outerRadius;

            Quaternion rotationA = Quaternion.Euler(0, 0, Mathf.Lerp(0.0f, 360.0f, Mathf.Repeat(MathFFS.InverseLerpUnclamped(0, m_points, i), 1.0f)) + m_angleOffset);
            Quaternion rotationB = Quaternion.Euler(0, 0, Mathf.Lerp(0.0f, 360.0f, Mathf.Repeat(Mathf.Lerp(MathFFS.InverseLerpUnclamped(0, m_points, i), MathFFS.InverseLerpUnclamped(0, m_points, i + 1), m_innerRelativeOffset), 1.0f)) + m_angleOffset);
            Quaternion rotationC = Quaternion.Euler(0, 0, Mathf.Lerp(0.0f, 360.0f, Mathf.Repeat(MathFFS.InverseLerpUnclamped(0, m_points, i + 1), 1.0f)) + m_angleOffset);

            Vector3 previousOuterPosition = rotationA * outerOffset;
            Vector3 innerPosition = rotationB * innerOffset;
            Vector3 outerPosition = rotationC * outerOffset;

            if (m_starburst == false) {
                yield return new VectorLineVertex(previousOuterPosition, m_innerColor);
                yield return new VectorLineVertex(innerPosition, m_outerColor);
            }

            yield return new VectorLineVertex(innerPosition, m_innerColor);
            yield return new VectorLineVertex(outerPosition, m_outerColor);
        }
    }
}
