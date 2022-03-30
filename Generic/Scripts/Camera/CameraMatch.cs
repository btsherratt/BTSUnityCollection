using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMatch : MonoBehaviour {
    public Camera m_camera;
    public Camera m_matchCamera;

    void Start() {
        if (m_camera == null) {
            m_camera = GetComponent<Camera>();
        }
        if (m_matchCamera == null) {
            m_matchCamera = Camera.main;
        }
    }

    void Update() {
        m_camera.fieldOfView = m_matchCamera.fieldOfView;
    }
}
