using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReLayer : MonoBehaviour {
    [Layer]
    public int m_layer;

    void Start() {
        foreach (Transform t in GetComponentsInChildren<Transform>()) {
            t.gameObject.layer = m_layer;
        }

        foreach (Light l in GetComponentsInChildren<Light>()) {
            l.cullingMask = 1 << m_layer;
        }
    }
}
