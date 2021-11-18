using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRendererColorGroup : MonoBehaviour {
    public Color m_color = Color.white;

    public SpriteRenderer[] m_childSpriteRenderers;

    void Start() {
        ApplyColor(findChildRenderers: true);
    }

    void OnTransformChildrenChanged() {
        ApplyColor(findChildRenderers: true);
    }

    void Update() {
        ApplyColor();
    }

    void ApplyColor(bool findChildRenderers = false) {
        if (m_childSpriteRenderers == null || findChildRenderers) {
            m_childSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
        foreach (SpriteRenderer spriteRenderer in m_childSpriteRenderers) {
            spriteRenderer.color = m_color;
        }
    }
}
