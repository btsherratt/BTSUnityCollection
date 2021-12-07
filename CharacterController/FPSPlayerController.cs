using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSPlayerController : MonoBehaviour {
    public Transform m_head;

    public float m_moveSpeed = 10.0f;
    public float m_lookSensitivity = 360.0f;

    public bool m_preventFalls;

    CharacterController m_characterController;
    Vector2 m_headRotation;

    void Start() {
        m_characterController = GetComponent<CharacterController>();
        m_headRotation.x = m_head.localRotation.x;
        m_headRotation.y = m_head.localRotation.y;
    }

    void FixedUpdate()
    {
        Vector3 direction = Vector3.zero;

        Vector3 forwardVector = m_head.forward;
        forwardVector.y = 0.0f;
        Quaternion correctedForwardQuaternion = Quaternion.LookRotation(forwardVector);

        direction += Input.GetKey(KeyCode.W) ? Vector3.forward : Vector3.zero;
        direction += Input.GetKey(KeyCode.S) ? Vector3.back : Vector3.zero;
        direction += Input.GetKey(KeyCode.A) ? Vector3.left : Vector3.zero;
        direction += Input.GetKey(KeyCode.D) ? Vector3.right : Vector3.zero;

        Vector3 previousPosition = transform.position;
        m_characterController.Move(correctedForwardQuaternion * direction.normalized * m_moveSpeed * Time.fixedDeltaTime);

        if (m_preventFalls && m_characterController.isGrounded == false) {
            m_characterController.enabled = false;
            transform.position = previousPosition;
            m_characterController.enabled = true;
        }

        // mouselook
        m_headRotation.y += Input.GetAxis("Mouse X") * m_lookSensitivity * Time.fixedDeltaTime;
        m_headRotation.x -= Input.GetAxis("Mouse Y") * m_lookSensitivity * Time.fixedDeltaTime;
        m_headRotation.x = Mathf.Clamp(m_headRotation.x, -90, 90);
        m_head.localRotation = Quaternion.Euler(m_headRotation.x, m_headRotation.y, 0f);
    }
}
