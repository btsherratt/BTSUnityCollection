using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReparentTransform : MonoBehaviour {
    public Transform m_newParent;
    public bool m_preservePosition = true;

    private void OnEnable() {
        transform.SetParent(m_newParent, m_preservePosition);
    }
}
