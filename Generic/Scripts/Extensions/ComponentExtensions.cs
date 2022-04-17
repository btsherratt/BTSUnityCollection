using UnityEngine;
using System.Collections.Generic;

public static class ComponentExtensions {
    public delegate bool FilterDelegate<T>(T component) where T : Component;

    public static T Nearest<T>(IEnumerable<T> components, Vector3 position, FilterDelegate<T> filter = null) where T : Component {
        T nearest = null;
        float nearestDistanceSq = float.MaxValue;
        foreach (T component in components) {
            if (filter == null || filter(component)) {
                float distanceSq = (component.transform.position - position).sqrMagnitude;
                if (distanceSq < nearestDistanceSq) {
                    nearestDistanceSq = distanceSq;
                    nearest = component;
                }
            }
        }
        return nearest;
    }

    public static T GetOrAddComponent<T>(this Component component, T input = null) where T : Component {
        T output = input;

        if (output == null) {
            output = component.GetComponent<T>();
            if (output == null) {
                output = component.gameObject.AddComponent<T>();
            }
        }

        return output;
    }
}
