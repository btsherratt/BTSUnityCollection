using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RendererExtensions {
    public static Bounds CalculateBounds(IList<Renderer> renderers) {
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers) {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }
}
