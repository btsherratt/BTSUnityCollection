using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBasedFOV : MonoBehaviour, CameraController.IFOVSource {
    public interface ISpeedProviding {
        float CurrentSpeed { get; }
        float MaxSpeed { get; }
    }

    public GameObject m_trackingObject;

    public float m_minFOV = 60.0f;
    public float m_maxFOV = 70.0f;
    public AnimationCurve m_fovCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public float m_FOVPS = 3.0f;

    ISpeedProviding m_speedProvider;
    float m_currentFOV;

    public void SetupForCamera(Camera camera, bool transition) {
        m_currentFOV = m_minFOV;

        if (m_trackingObject == null) {
            m_trackingObject = GameObject.FindGameObjectWithTag("Player");
        }
        if (m_speedProvider == null) {
            m_speedProvider = m_trackingObject.GetComponent<ISpeedProviding>();
        }

        if (transition) {
            m_currentFOV = camera.fieldOfView;
        }
    }

    public float GetCameraFOV(Camera camera) {
        float max = m_speedProvider.MaxSpeed;
        float current = m_speedProvider.CurrentSpeed;
        float t = Mathf.InverseLerp(0, max, current);
        float fovT = m_fovCurve.Evaluate(t);
        float fov = Mathf.Lerp(m_minFOV, m_maxFOV, fovT);
        m_currentFOV = Mathf.MoveTowards(m_currentFOV, fov, m_FOVPS * Time.deltaTime);
        return m_currentFOV;
    }
}
