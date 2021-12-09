using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SortingOrderTagExtensions {
    public static void SetRelativeSortingOrder(this Component component, int relativeSortingOrder) {
        component.gameObject.SetRelativeSortingOrder(relativeSortingOrder);
    }

    public static void SetRelativeSortingOrder(this GameObject gameObject, int relativeSortingOrder) {
        SortingOrderTag tag = gameObject.GetComponent<SortingOrderTag>();
        if (tag == null) {
            tag = gameObject.gameObject.AddComponent<SortingOrderTag>();
        }
        tag.RelativeSortingOrder = relativeSortingOrder;
    }
}
