using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICameraPositionProviding {
    Vector3 CameraPosition { get; }
    Vector3 CameraLookTarget { get; }

    void UpdateCameraValues();
}

public class CameraMovementBlender : MonoBehaviour {
    public Transform m_cameraTransform;

    ICameraPositionProviding[] m_cameraPositionProviders;

    void Start() {
        if (m_cameraTransform == null) {
            m_cameraTransform = transform;// GetComponent<Camera>();
        }
        //if (m_camera == null) {
        //    m_camera = Camera.main;
        //}

        m_cameraPositionProviders = GetComponentsInChildren<ICameraPositionProviding>();
    }

    void LateUpdate() {
        Vector3 position = Vector3.zero;
        Vector3 target = Vector3.zero;
        foreach (ICameraPositionProviding cameraPositionProvider in m_cameraPositionProviders) {
            cameraPositionProvider.UpdateCameraValues();
            position += cameraPositionProvider.CameraPosition;
            target += cameraPositionProvider.CameraLookTarget;
        }
        position /= m_cameraPositionProviders.Length;
        target /= m_cameraPositionProviders.Length;

        m_cameraTransform.position = Vector3.MoveTowards(m_cameraTransform.position, position, 10.0f*Time.deltaTime);
        m_cameraTransform.rotation = Quaternion.RotateTowards(m_cameraTransform.rotation, Quaternion.LookRotation(target - m_cameraTransform.position, Vector3.up), 100.0f * Time.deltaTime);
    }
}
