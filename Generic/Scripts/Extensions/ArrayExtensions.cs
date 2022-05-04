using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayExtensions {
    public static bool Contains<T>(this T[] array, T value) {
        for (int i = 0; i < array.Length; ++i) {
            if (array[i].Equals(value)) {
                return true;
            }
        }
        return false;
    }
}
