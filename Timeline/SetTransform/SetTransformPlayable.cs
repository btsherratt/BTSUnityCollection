using UnityEngine;
using UnityEngine.Playables;

public class SetTransformPlayable : PlayableBehaviour {
    public Transform m_parentTransform;

    public Vector3 m_localPositionStart;
    public Vector3 m_localPositionEnd;

    public Vector3 m_localRotationStart;
    public Vector3 m_localRotationEnd;

    public Vector3 m_localScaleStart;
    public Vector3 m_localScaleEnd;

    public AnimationCurve m_positionCurve;
    public AnimationCurve m_rotationCurve;
    public AnimationCurve m_scaleCurve;

    public bool m_setParent;
    public bool m_ignorePosition;
    public bool m_ignoreRotation;
    public bool m_ignoreScale;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
        float t = (float)(playable.GetTime() / playable.GetDuration());

        Transform trackBinding = playerData as Transform;

        if (m_setParent && trackBinding.parent != m_parentTransform) {
            trackBinding.SetParent(m_parentTransform, true);
        }

        if (m_ignorePosition == false) {
            float pt = m_positionCurve.Evaluate(t);
            trackBinding.localPosition = Vector3.Lerp(m_localPositionStart, m_localPositionEnd, pt);
        }

        if (m_ignoreRotation == false) {
            float rt = m_rotationCurve.Evaluate(t);
            trackBinding.localRotation = Quaternion.Lerp(Quaternion.Euler(m_localRotationStart), Quaternion.Euler(m_localRotationEnd), rt);
        }

        if (m_ignoreScale == false) {
            float st = m_scaleCurve.Evaluate(t);
            trackBinding.localScale = Vector3.Lerp(m_localScaleStart, m_localScaleEnd, st);
        }
    }
}
