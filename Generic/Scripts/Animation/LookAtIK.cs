using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtIK : MonoBehaviour {
    public Transform m_target;

    Animator m_animator;
    float m_lookAtWeight;

    private void OnEnable() {
        m_animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex) {
        float targetWeight = 0.0f;
        if (m_target != null) {
            float dot = Vector3.Dot(transform.InverseTransformPoint(m_target.position).normalized, Vector3.forward);
            if (dot > 0.0f) {
                m_animator.SetLookAtPosition(m_target.position);
                targetWeight = 1.0f;//, 0.0f, 1.0f, 1.0f, 0.5f);
            }
        }

        m_lookAtWeight = Mathf.MoveTowards(m_lookAtWeight, targetWeight, Time.deltaTime);
        m_animator.SetLookAtWeight(m_lookAtWeight);
    }
}
