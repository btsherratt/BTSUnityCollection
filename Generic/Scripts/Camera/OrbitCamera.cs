using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitCamera : MonoBehaviour, ICameraPositionProviding {
    public Transform m_followTarget;
    public Transform m_lookTarget;

    public float m_distance = 3.0f;

    float m_angle = 0;

    Vector3 m_position;
    Vector3 ICameraPositionProviding.CameraPosition => m_position;

    Vector3 ICameraPositionProviding.CameraLookTarget => m_lookTarget.position;

    void Start() {
        if (m_followTarget == null) {
            m_followTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }
        if (m_lookTarget == null) {
            m_lookTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void ICameraPositionProviding.UpdateCameraValues() {
        m_position = m_followTarget.position + Quaternion.Euler(0, m_angle, 0) * (Vector3.forward * m_distance);


        //
    }
}
