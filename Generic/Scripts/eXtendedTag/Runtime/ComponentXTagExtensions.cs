using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComponentXTagExtensions {
    public static bool IsXTagged(this Component component, string tag) {
        XTag xTag;
        if (component.TryGetComponent(out xTag)) {
            return xTag.m_tag == tag;
        } else {
            return false;
        }
    }
}
