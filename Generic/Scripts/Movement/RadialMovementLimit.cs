using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMovementLimit : MonoBehaviour {
    public Transform m_limitCentreTransform;
    public float m_limitRadius = 1.0f;

    void Start() {
        if (m_limitCentreTransform == null) {
            m_limitCentreTransform = transform.parent;
        }
    }

    void LateUpdate() {
        Vector3 centrePosition = m_limitCentreTransform != null ? m_limitCentreTransform.position : Vector3.zero;
        Vector3 delta = transform.position - centrePosition;

        if (delta.sqrMagnitude > m_limitRadius * m_limitRadius) {
            transform.position = centrePosition + delta.normalized * m_limitRadius;
        } 
    }
}
