using UnityEditor;
using UnityEngine;

public static class ApplyDynamicTag {
    [MenuItem("Tools/BTS/Transform/Apply Dynamic Tag")]
    public static void DoApplyDynamicTag() {
        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Dynamic")) {
            gameObject.isStatic = false;
        }
    }
}
