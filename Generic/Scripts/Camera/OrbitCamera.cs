using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCamera : MonoBehaviour, CameraController.ISourceUpdating, CameraController.IPositionSource, CameraController.IRotationSource {
    public Transform m_followTarget;
    public Transform m_lookTarget;

    public float m_distance = 3.0f;
    public float m_anglePerSecond = 360.0f;
    public float m_autoAnglePerSecond = 10.0f;

    public bool m_invertedXControls = true;

    [Layer]
    public int m_physicsLayer;

    float m_angle = 0;

    PlayerInput m_playerInput;
    InputAction m_lookAction;

    Vector3 m_position;
    
    public Vector3 GetCameraPosition(Camera camera) {
        return m_position;
    }

    public Quaternion GetCameraRotation(Camera camera) {
        return Quaternion.LookRotation(m_lookTarget.position - m_position, Vector3.up);
        throw new System.NotImplementedException();
    }

    private void OnEnable() {
        if (m_followTarget == null) {
            m_followTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }
        if (m_lookTarget == null) {
            m_lookTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }
        m_playerInput = GameObject.FindGameObjectWithTag("GameController").GetComponent<PlayerInput>();
        m_lookAction = m_playerInput.actions.FindAction("Look");

        m_position = m_followTarget.position + Quaternion.Euler(0, m_angle, 0) * (Vector3.forward * m_distance);

        Camera.main.GetComponent<CameraController>().PushControlSource(this);
    }

    public void UpdateForCamera(Camera camera) {
        Vector2 lookDelta = m_lookAction.ReadValue<Vector2>();
        if (Mathf.Abs(lookDelta.x) >= float.Epsilon) {
            m_angle += (m_invertedXControls ? -lookDelta.x : lookDelta.x) * m_anglePerSecond * Time.deltaTime;
        } else {
            //m_angle = Mathf.MoveTowardsAngle(m_angle, m_followTarget.transform.eulerAngles.y + 180, m_autoAnglePerSecond * Time.deltaTime);
        }

        Vector3 position = m_followTarget.position + Quaternion.Euler(0, m_angle, 0) * (Vector3.forward * m_distance);

        float distance = Mathf.Abs(m_followTarget.localPosition.y);

        RaycastHit raycastHit;
        if (Physics.Raycast(position + Vector3.up * 100, Vector3.down, out raycastHit, distance + 100, 1 << m_physicsLayer)) {
            position = raycastHit.point + Vector3.up * distance;
        }

        m_position = position;
    }
}
