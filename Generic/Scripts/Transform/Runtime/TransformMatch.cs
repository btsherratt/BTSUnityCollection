using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformMatch : MonoBehaviour {
    public Transform m_matchTransform;
    public Transform m_relativeRoot;

    void LateUpdate() {
        if (m_matchTransform != null) {
            Transform offsetRoot = m_relativeRoot != null ? m_relativeRoot : transform;

            Matrix4x4 inverse = offsetRoot.localToWorldMatrix * transform.worldToLocalMatrix;

            Vector3 position = m_matchTransform.position - transform.InverseTransformPoint(offsetRoot.position);
            Quaternion rotation = m_matchTransform.rotation;

            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
