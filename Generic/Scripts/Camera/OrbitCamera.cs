using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCamera : MonoBehaviour, ICameraPositionProviding {
    public Transform m_followTarget;
    public Transform m_lookTarget;

    public float m_distance = 3.0f;
    public float m_anglePerSecond = 360.0f;
    public float m_autoAnglePerSecond = 10.0f;

    float m_angle = 0;

    PlayerInput m_playerInput;
    InputAction m_lookAction;

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
        m_playerInput = GameObject.FindGameObjectWithTag("GameController").GetComponent<PlayerInput>();
        m_lookAction = m_playerInput.actions.FindAction("Look");

        m_position = m_followTarget.position + Quaternion.Euler(0, m_angle, 0) * (Vector3.forward * m_distance);
    }

    void ICameraPositionProviding.UpdateCameraValues() {
        Vector2 lookDelta = m_lookAction.ReadValue<Vector2>();
        if (Mathf.Abs(lookDelta.x) >= float.Epsilon) {
            m_angle += lookDelta.x * m_anglePerSecond * Time.deltaTime;
        } else {
            //m_angle = Mathf.MoveTowardsAngle(m_angle, m_followTarget.transform.eulerAngles.y + 180, m_autoAnglePerSecond * Time.deltaTime);
        }

        m_position = m_followTarget.position + Quaternion.Euler(0, m_angle, 0) * (Vector3.forward * m_distance);


        //
    }
}
