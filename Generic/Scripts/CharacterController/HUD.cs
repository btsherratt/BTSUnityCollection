using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

public class HUD : ImmediateModeShapeDrawer {
    public Color m_defaultColor = Color.white;
    public Color m_interactiveColor = Color.white;

    public InteractivePicker m_interactivePicker;

    void Start() {
        if (m_interactivePicker == null) {
            m_interactivePicker = GetComponent<InteractivePicker>();
        }
    }

    public override void DrawShapes(Camera cam) {
        if (cam == Camera.main) {
            using (Draw.Command(cam)) {
                Draw.RadiusSpace = ThicknessSpace.Noots;
                Draw.ThicknessSpace = ThicknessSpace.Noots;

                Draw.ZTest = UnityEngine.Rendering.CompareFunction.Always;
                Draw.Matrix = transform.localToWorldMatrix;
                Draw.BlendMode = ShapesBlendMode.ColorDodge;
                Draw.LineGeometry = LineGeometry.Flat2D;

                Color color = m_interactivePicker.m_interactionAvailable ? m_interactiveColor : m_defaultColor;

                Draw.Disc(Vector3.zero, 1.0f, color);

                if (m_interactivePicker.m_interactionAvailable) {
                    Draw.RegularPolygonBorder(Vector3.zero, sideCount: 4, radius: 2.3f, thickness: 0.2f, color: m_interactiveColor);
                }
            }
        }
    }
}