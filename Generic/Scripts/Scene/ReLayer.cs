using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReLayer : MonoBehaviour {
    [Layer]
    public int m_layer;

    [Layer]
    public int m_ignoredLayer;

    void Start() {
        foreach (Transform t in GetComponentsInChildren<Transform>(true)) {
            if (t.gameObject.layer != m_ignoredLayer) {
                t.gameObject.layer = m_layer;
            }
        }

        foreach (Light l in GetComponentsInChildren<Light>(true)) {
            l.cullingMask = 1 << m_layer;
        }
    }
}
