using UnityEngine;

public static class ComponentExtensions {
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
