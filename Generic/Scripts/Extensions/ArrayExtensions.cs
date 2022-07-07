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

    public static T[] CopyAdding<T>(this T[] array, T value) {
        T[] copy = new T[array.Length + 1];
        array.CopyTo(copy, 1);
        copy[0] = value;
        return copy;
    }
}
