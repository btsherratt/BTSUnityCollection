using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtIK : MonoBehaviour {
    public Transform m_target;

    Animator m_animator;

    private void OnEnable() {
        m_animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex) {
        if (m_target != null) {
            m_animator.SetLookAtPosition(m_target.position);
            m_animator.SetLookAtWeight(1.0f);//, 0.0f, 1.0f, 1.0f, 0.5f);
        } else {
            m_animator.SetLookAtWeight(0.0f);
        }
    }
}
