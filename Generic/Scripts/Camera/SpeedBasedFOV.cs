using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBasedFOV : MonoBehaviour {
    public interface ISpeedProviding {
        float CurrentSpeed { get; }
        float MaxSpeed { get; }
    }

    public GameObject m_trackingObject;
    public Camera m_camera;

    public float m_minFOV = 60.0f;
    public float m_maxFOV = 70.0f;
    public AnimationCurve m_fovCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public float m_FOVPS = 3.0f;

    ISpeedProviding m_speedProvider;
    float m_currentFOV;

    void Start() {
        if (m_trackingObject == null) {
            m_trackingObject = GameObject.FindGameObjectWithTag("Player");
        }
        m_speedProvider = m_trackingObject.GetComponent<ISpeedProviding>();

        if (m_camera == null) {
            m_camera = GetComponent<Camera>();
        }
        if (m_camera == null) {
            m_camera = Camera.main;
        }
        m_currentFOV = m_minFOV;
    }

    void LateUpdate() {
        float max = m_speedProvider.MaxSpeed;
        float current = m_speedProvider.CurrentSpeed;
        float t = Mathf.InverseLerp(0, max, current);
        float fovT = m_fovCurve.Evaluate(t);
        float fov = Mathf.Lerp(m_minFOV, m_maxFOV, fovT);
        m_currentFOV = Mathf.MoveTowards(m_currentFOV, fov, m_FOVPS * Time.deltaTime);
        m_camera.fieldOfView = m_currentFOV;
    }
}
