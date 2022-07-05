using UnityEngine;
using UnityEditor;

public class MarkChanged {
    [MenuItem("Tools/BTS/Transform/Mark Selected Changed")]
    public static void MarkSelectedChanged() {
        foreach (Transform transform in Selection.transforms) {
            transform.hasChanged = true;
        }
    }

    [MenuItem("Tools/BTS/Transform/Mark All Changed")]
    public static void MarkAllChanged() {
        foreach (Transform transform in GameObject.FindObjectsOfType<Transform>(true)) {
            transform.hasChanged = true;
        }
    }
}
