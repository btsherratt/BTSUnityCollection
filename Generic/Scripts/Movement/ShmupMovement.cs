using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShmupMovement : MonoBehaviour {
    public ShmupControlScheme m_controlScheme;

    public float m_speed = 1.0f;
    public float m_maxTurning = 10.0f;

    void Update() {
        Vector3 targetVector = Vector3.zero;

        if (Input.GetKey(m_controlScheme.m_up)) {
            targetVector += Vector3.up;
        }

        if (Input.GetKey(m_controlScheme.m_down)) {
            targetVector += Vector3.down;
        }

        if (Input.GetKey(m_controlScheme.m_left)) {
            targetVector += Vector3.left;
        }

        if (Input.GetKey(m_controlScheme.m_right)) {
            targetVector += Vector3.right;
        }

        targetVector.Normalize();

        if (targetVector.sqrMagnitude > 0) {
            transform.position += targetVector * m_speed * Time.deltaTime;

            Quaternion targetRotation = Quaternion.Euler(0, 0, Mathf.Atan2(targetVector.y, targetVector.x) * Mathf.Rad2Deg - 90.0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, m_maxTurning * Time.deltaTime);
        }
    }
}
