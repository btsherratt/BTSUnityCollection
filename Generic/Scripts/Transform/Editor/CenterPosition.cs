using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class CenterPosition {
    [MenuItem("Tools/BTS/Transform/Center Position")]
    public static void CenterTransformPosition() {
        foreach (Transform transform in Selection.transforms) {
            CenterTransformPositionFromChildren(transform);
        }
    }

    static void CenterTransformPositionFromChildren(Transform transform) {
        List<Transform> childTransforms = new List<Transform>();
        
        if (transform.childCount > 0) {
            Vector3 position = Vector3.zero;
            foreach (Transform childTransform in transform) {
                childTransforms.Add(childTransform);
                position += childTransform.position;
            }
            position /= transform.childCount;

            foreach (Transform childTransform in childTransforms) {
                childTransform.SetParent(null, true);
            }

            transform.position = position;

            foreach (Transform childTransform in childTransforms) {
                childTransform.SetParent(transform, true);
            }
        }
    }
}
