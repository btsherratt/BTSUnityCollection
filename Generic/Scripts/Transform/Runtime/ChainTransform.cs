using UnityEngine;

public class ChainTransform : MonoBehaviour {
    [Header("The transform to chain to")]
    public ChainTransform m_chainedTransform;

    [Header("The point at which you chain to your chained transform")]
    public Vector3 m_chainPoint;

    [Header("The point at which a chained transform will chain to you")]
    public Vector3 m_attachPoint;

    ChainTransform m_nextChainedTransform;

    private void OnEnable() {
        ForceUpdateChain();
    }

    void LateUpdate() {
        if (m_chainedTransform == null) {
            ForceUpdateChain();
        }
    }

    public void ForceUpdateChain() {
        ChainTransform currentChainTransform = this;

        while (currentChainTransform.m_chainedTransform != null) {
            currentChainTransform.m_chainedTransform.m_nextChainedTransform = currentChainTransform;
            currentChainTransform = currentChainTransform.m_chainedTransform;
        }

        ChainTransform parentChainTransform = currentChainTransform;
        currentChainTransform = currentChainTransform.m_nextChainedTransform;

        while (currentChainTransform != null) {
            Vector3 targetPosition = parentChainTransform.transform.TransformPoint(parentChainTransform.m_attachPoint);
            Vector3 currentPosition = currentChainTransform.transform.TransformPoint(currentChainTransform.m_chainPoint);
            Vector3 currentAnglePosition = currentChainTransform.transform.TransformPoint(currentChainTransform.m_attachPoint);

            Vector3 targetPositionDelta = targetPosition - currentPosition;
            Vector3 targetAngleDelta = targetPosition - currentAnglePosition;
            if (targetPositionDelta.sqrMagnitude > 0) {
                targetAngleDelta.Normalize();
                Quaternion targetRotation = Quaternion.Euler(0, 0, Mathf.Atan2(targetAngleDelta.y, targetAngleDelta.x) * Mathf.Rad2Deg - 90.0f);

                currentChainTransform.transform.rotation = targetRotation;

                currentChainTransform.transform.position = targetPosition - currentChainTransform.transform.TransformVector(currentChainTransform.m_chainPoint);
            }

            parentChainTransform = currentChainTransform;
            currentChainTransform = currentChainTransform.m_nextChainedTransform;
        }
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(transform.TransformPoint(m_chainPoint), 0.1f);
        Gizmos.DrawWireSphere(transform.TransformPoint(m_attachPoint), 0.1f);
    }
}
