using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions {
    public static Bounds CalculateBounds(this GameObject gameObject) {
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0) {
            return RendererExtensions.CalculateBounds(renderers);
        } else {
            return new Bounds(gameObject.transform.position, Vector3.zero);
        }
    }
}
