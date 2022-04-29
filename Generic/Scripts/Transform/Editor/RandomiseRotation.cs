using UnityEditor;
using UnityEngine;

public static class RandomiseRotation {
    [MenuItem("Tools/BTS/Transform/Randomise Rotation/Local X")]
    public static void RandomiseRotationX() {
        SetRandomRotation(new Vector3(360, 0, 0));
    }

    [MenuItem("Tools/BTS/Transform/Randomise Rotation/Local Y")]
    public static void RandomiseRotationY() {
        SetRandomRotation(new Vector3(0, 360, 0));
    }

    [MenuItem("Tools/BTS/Transform/Randomise Rotation/Local Z")]
    public static void RandomiseRotationZ() {
        SetRandomRotation(new Vector3(0, 0, 360));
    }

    static void SetRandomRotation(Vector3 eulerAmounts) {
        foreach (Transform transform in Selection.transforms) {
            Quaternion rotation = transform.localRotation;
            Quaternion randomRotation = Quaternion.Euler(eulerAmounts * Random.value);
            rotation = randomRotation * rotation;
            transform.localRotation = rotation;
        }
    }
}
