using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformAnimation : MonoBehaviour {

    public Vector3 m_angularSpeed;

    float m_startTime;
    Vector3 m_startAngles;

    void OnEnable () {
        m_startTime = Time.time;
        m_startAngles = transform.localRotation.eulerAngles;
    }

    void Update() {
        float deltaTime = Time.time - m_startTime;
        Vector3 angles = m_startAngles + m_angularSpeed * deltaTime;
        transform.localRotation = Quaternion.Euler(angles);
    }
}
