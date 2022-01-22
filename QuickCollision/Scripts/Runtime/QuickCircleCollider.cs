using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickCircleCollider : QuickCollider {
    public Vector3 m_center = Vector3.zero;

    public float m_radius = 1.0f;

    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.TransformPoint(m_center), m_radius);
    }
}
