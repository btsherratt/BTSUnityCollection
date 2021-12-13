using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReLayer : MonoBehaviour {
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/BTS/Relayer")]
    static void ExecuteAll() {
        ReLayer[] items = FindObjectsOfType<ReLayer>();
        foreach (ReLayer item in items) {
            item.DoRelayer();
        }
    }
#endif

    [Layer]
    public int m_layer;

    [Layer]
    public int m_ignoredLayer;

    void Start() {
        if (gameObject.layer != m_layer) {
            DoRelayer();
        }
    }

    void DoRelayer() {
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
