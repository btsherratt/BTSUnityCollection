using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class AngularTransform : MonoBehaviour {
    public enum Direction {
        Clockwise,
        AntiClockwise,
    }

    public float m_angle;
    public float m_offset;

    public Direction m_direction;
    public Transform m_centerTransform;

    void Update() {
        
        
        Quaternion rotation = m_direction == Direction.Clockwise ? Quaternion.Euler(0, 0, -m_angle) : Quaternion.Euler(0, 0, m_angle);
        Vector3 rotatedOffset = rotation * Vector3.up * m_offset;

        if (m_centerTransform != null) {
            rotatedOffset = m_centerTransform.TransformPoint(rotatedOffset);
            rotation = m_centerTransform.rotation * rotation;
        }

        transform.position = rotatedOffset;
        transform.rotation = rotation;
    }
}
