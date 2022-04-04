using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationalMovement : MonoBehaviour {
    public Vector3 m_degreesPerSecond;

    public bool m_randomOffset = true;
    public float m_fluctuation = 0.2f;

    float m_rndOffset;

    void Start() {
        m_rndOffset = Random.value;
        if (m_randomOffset) {
            Quaternion rotation = transform.rotation;
            rotation *= Quaternion.Euler(m_degreesPerSecond * Random.value * 360.0f);
            transform.rotation = rotation;
        }
    }

    void Update() {
        Quaternion rotation = transform.rotation;
        rotation *= Quaternion.Euler(m_degreesPerSecond * (Time.deltaTime + Mathf.Sin((m_rndOffset + Time.time) / 1000.0f) * Random.Range(0.0f, m_fluctuation)));
        transform.rotation = rotation;
    }
}
