using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour {
    public float m_followSpeed = 1.0f;
    //public float m_followAcceleration = 1.0f;
    public float m_followRadius = 1.0f;

    public Transform m_followTargetTransform;

    float m_zPosition;
    //Vector3 m_velocity;

    void Start() {
        if (m_followTargetTransform == null) {
            m_followTargetTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        m_zPosition = transform.position.z;

        Vector3 position = m_followTargetTransform.position;
        position.z = m_zPosition;
        transform.position = position;
    }

    void LateUpdate() {
        Vector3 targetPosition = m_followTargetTransform.position;
        targetPosition.z = m_zPosition;

        Vector3 delta = targetPosition - transform.position;

        float distance = delta.magnitude;
        if (distance > m_followRadius) {
            Vector3 positionDelta = delta.normalized * m_followSpeed * Time.deltaTime;
            transform.position += positionDelta;
        }




#if false
        if (distance > 0.0f) {
            float rampedSpeed = m_followSpeed * (distance / m_followRadius);
            float clippedSpeed = Mathf.Min(rampedSpeed, m_followSpeed);
            Vector3 desiredVelocity = (clippedSpeed / distance) * delta;
            Vector3 steeringAcceleration = desiredVelocity - m_velocity;

            m_velocity += steeringAcceleration * Time.deltaTime;

            //transform.position += delta.normalized * m_followSpeed * Time.deltaTime;
            /*if (delta.sqrMagnitude > m_followRadius) {
                m_velocity += delta.normalized * Mathf.Min(m_followAcceleration, delta.magnitude) * Time.deltaTime;
                //m_followAcceleration += 
            } else {
                m_velocity *= 0.9f;
            }*/
            transform.position += m_velocity * Time.deltaTime;

            //m_velocity *= 0.8f * Time.deltaTime;
        }
#endif
    }

    private void OnDrawGizmos() {
        if (m_followTargetTransform != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_followTargetTransform.position, m_followRadius);
        }
    }
}
