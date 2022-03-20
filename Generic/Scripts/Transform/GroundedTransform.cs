using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GroundedTransform : MonoBehaviour {
    [Layer]
    public int m_groundLayer;

    public Vector3 m_groundingDirection = Vector3.down;

    public bool m_setRotation = false;

    private void Update() {
        if (transform.hasChanged) {
            RaycastHit hit;
            if (Physics.Raycast(transform.position - m_groundingDirection * 100.0f, m_groundingDirection, out hit, float.PositiveInfinity, 1 << m_groundLayer)) {
                transform.position = hit.point;
                if (m_setRotation) {
                    transform.up = hit.normal;
                }
            }
        }
    }
}
