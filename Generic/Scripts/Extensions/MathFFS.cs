using UnityEngine;

public static class MathFFS {
    public static float InverseLerpUnclamped(float a, float b, float v) {
        float t = (v - a) / (b - a);
        return t;
    }
}
