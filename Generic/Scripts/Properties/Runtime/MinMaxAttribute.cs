using UnityEngine;

public class MinMaxAttribute : PropertyAttribute {
    public float Min { get; private set; }
    public float Max { get; private set; }

    public MinMaxAttribute(float min, float max) {
        Min = min;
        Max = max;
    }
}
