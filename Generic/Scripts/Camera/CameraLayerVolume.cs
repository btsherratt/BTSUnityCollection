using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLayerVolume : MonoBehaviour {
    [Layer]
    public int m_renderingLayer;

    public TriggerDelegate m_trigger;

    private void Start() {
        m_trigger = this.GetOrAddComponent<TriggerDelegate>(m_trigger);
        m_trigger.m_triggerEnterDelegate += OnCameraEnter;
    }

    void OnCameraEnter(Collider trigger, Collider other) {
        Camera camera = other.GetComponentInChildren<Camera>();
        if (camera != null) {
            camera.cullingMask = 1 << m_renderingLayer;
        }
    }
}
