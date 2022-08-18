using UnityEngine;
using UnityEngine.InputSystem;

public class FollowCamera : MonoBehaviour, CameraController.ISourceUpdating, CameraController.IPositionSource, CameraController.IRotationSource {
    public float m_followSpeed = 1.0f;
    public float m_followRadius = 1.0f;
    public float m_followRadiusGive = 0.1f;
    public AnimationCurve m_followCurve;

    public Transform m_followTarget;

    public float m_lookSpeed = 360.0f;
    public float m_lookAngleGive = 5.0f;
    public AnimationCurve m_lookCurve;
    public Transform m_lookTarget;

    [Layer]
    public int m_physicsLayer;

    public float m_anglePerSecond = 360.0f;
    public bool m_invertedXControls = true;

    Vector3 m_position;
    Quaternion m_rotation;

    InputAction m_lookAction;

    public Vector3 GetCameraPosition(Camera camera) {
        return m_position;
    }

    public Quaternion GetCameraRotation(Camera camera) {
        return m_rotation;
    }

    private void OnEnable() {
        if (m_followTarget == null) {
            m_followTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }
        if (m_lookTarget == null) {
            m_lookTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }
        PlayerInput playerInput = GameObject.FindGameObjectWithTag("GameController").GetComponent<PlayerInput>();
        m_lookAction = playerInput.actions.FindAction("Look");
    }

    public void SetupForCamera(Camera camera, bool transition) {
        if (transition) {
            m_position = camera.transform.position;
            m_rotation = camera.transform.rotation;
        } else {
            m_position = m_followTarget.position + -m_lookTarget.forward * m_followRadius;
            m_rotation = Quaternion.LookRotation(m_followTarget.position - m_position, Vector3.up);
        }

    }

    public void UpdateForCamera(Camera camera) {
        Vector3 targetPosition = m_followTarget.position;

        Vector3 delta = m_position - targetPosition;
        delta.y = 0;

        Vector3 targetFollowPosition = targetPosition + delta.normalized * m_followRadius;
        RaycastHit raycastHit;
        if (Physics.Raycast(targetFollowPosition + Vector3.up * 100, Vector3.down, out raycastHit, 200, 1 << m_physicsLayer)) {
            targetFollowPosition = raycastHit.point;
            targetFollowPosition.y = Mathf.Max(m_followTarget.position.y, raycastHit.point.y + m_followTarget.localPosition.y);
        }

        float correctionDistance = Vector3.Distance(m_position, targetFollowPosition) ;// Mathf.Abs(distance - m_followRadius);

        // NB: This is a failsafe for warping.
        if (correctionDistance < 100.0f) {
            float correction = Mathf.InverseLerp(0, m_followRadiusGive, correctionDistance);
            float correctionAmount = m_followCurve.Evaluate(correction);
            m_position = Vector3.MoveTowards(m_position, targetFollowPosition, correctionAmount * m_followSpeed * Time.deltaTime);
        } else {
            m_position = targetFollowPosition;
        }

        Quaternion targetFollowRotation = Quaternion.LookRotation(m_lookTarget.position - m_position, Vector3.up);

        if (correctionDistance < 100.0f) {
            float angle = Quaternion.Angle(m_rotation, targetFollowRotation);
            float correction = Mathf.InverseLerp(0, m_lookAngleGive, angle);
            float correctionAmount = m_lookCurve.Evaluate(correction);
            m_rotation = Quaternion.RotateTowards(m_rotation, targetFollowRotation, correctionAmount * m_lookSpeed * Time.deltaTime);
        } else {
            m_rotation = targetFollowRotation;
        }


        Vector2 lookDelta = m_lookAction.ReadValue<Vector2>();
        if (Mathf.Abs(lookDelta.x) >= float.Epsilon) {
            Quaternion cameraRotation = Quaternion.Euler(0, (m_invertedXControls ? -lookDelta.x : lookDelta.x) * m_anglePerSecond * Time.deltaTime, 0);
            m_position = transform.TransformPoint(cameraRotation * transform.InverseTransformPoint(m_position));
            m_rotation = cameraRotation * m_rotation;
        }
    }

    private void OnDrawGizmos() {
        if (m_followTarget != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_followTarget.position, m_followRadius);
        }
    }
}
