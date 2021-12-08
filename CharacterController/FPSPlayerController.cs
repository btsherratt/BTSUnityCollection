using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSPlayerController : MonoBehaviour {
    public Transform m_head;

    public float m_moveSpeed = 10.0f;
    public float m_lookSensitivity = 360.0f;

    public bool m_preventFalls;

	public bool m_captureMouse;
	public bool m_allowEscape;

	CharacterController m_characterController;
    Vector2 m_headRotation;

	bool m_mouseCaptured;
    Vector2 m_mousePreviousValues;

    void Start() {
        m_characterController = GetComponent<CharacterController>();
        m_headRotation.x = m_head.localRotation.x;
        m_headRotation.y = m_head.localRotation.y;

		if (m_captureMouse) {
			//CaptureMouse();
        }
    }

    void Update() {
        if (m_mouseCaptured == false && m_captureMouse && Input.GetMouseButtonDown(0)) {
            CaptureMouse();
        } else if (m_mouseCaptured && Input.GetKeyDown(KeyCode.Escape)) {
            ReleaseMouse();
        }

        //m_headRotation.x = m_head.localRotation.x;
        //m_headRotation.y = transform.localRotation.y;

        if (m_captureMouse == false || m_mouseCaptured) {
            Vector2 currentMouseValues = MouseValues();
            Vector2 delta = currentMouseValues - m_mousePreviousValues;
            //if (delta.magnitude > 0.1f) {
                m_headRotation.y += delta.x * m_lookSensitivity * Time.deltaTime;
                m_headRotation.x -= delta.y * m_lookSensitivity * Time.deltaTime;
                m_headRotation.x = Mathf.Clamp(m_headRotation.x, -90, 90);
                m_head.localRotation = Quaternion.Euler(m_headRotation.x, m_headRotation.y, 0f);
                //m_mousePreviousValues = currentMouseValues;
            //}
        }

        Vector3 direction = Vector3.zero;

        Vector3 forwardVector = m_head.forward;
        forwardVector.y = 0.0f;

        if (forwardVector.magnitude < float.Epsilon) {
            forwardVector = Vector3.forward;
        }

        Quaternion correctedForwardQuaternion = Quaternion.LookRotation(forwardVector);

        direction += Input.GetKey(KeyCode.W) ? Vector3.forward : Vector3.zero;
        direction += Input.GetKey(KeyCode.S) ? Vector3.back : Vector3.zero;
        direction += Input.GetKey(KeyCode.A) ? Vector3.left : Vector3.zero;
        direction += Input.GetKey(KeyCode.D) ? Vector3.right : Vector3.zero;

        Vector3 previousPosition = transform.position;
        m_characterController.Move(correctedForwardQuaternion * direction.normalized * m_moveSpeed * Time.deltaTime);

        if (m_preventFalls && m_characterController.isGrounded == false) {
            m_characterController.enabled = false;
            transform.position = previousPosition;
            m_characterController.enabled = true;
        }
    }

	void CaptureMouse() {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
        m_mouseCaptured = true;
        m_mousePreviousValues = MouseValues();
    }

    void ReleaseMouse() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        m_mouseCaptured = false;
    }

    Vector2 MouseValues() {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }
}
