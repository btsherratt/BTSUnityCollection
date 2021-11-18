using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class SetTransformClip : PlayableAsset {
    public ExposedReference<Transform> m_parentTransform;

    public Vector3 m_localPositionStart = Vector3.zero;
    public Vector3 m_localPositionEnd = Vector3.zero;

    public Vector3 m_localRotationStart = Vector3.zero;
    public Vector3 m_localRotationEnd = Vector3.zero;

    public Vector3 m_localScaleStart = Vector3.one;
    public Vector3 m_localScaleEnd = Vector3.one;

    public AnimationCurve m_positionCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve m_rotationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve m_scaleCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public bool m_setParent;
    public bool m_ignorePosition;
    public bool m_ignoreRotation;
    public bool m_ignoreScale;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
        ScriptPlayable<SetTransformPlayable> playable = ScriptPlayable<SetTransformPlayable>.Create(graph);
        SetTransformPlayable setTransformPlayable = playable.GetBehaviour();

        setTransformPlayable.m_parentTransform = m_parentTransform.Resolve(graph.GetResolver());

        setTransformPlayable.m_localPositionStart = m_localPositionStart;
        setTransformPlayable.m_localPositionEnd = m_localPositionEnd;
        setTransformPlayable.m_localRotationStart = m_localRotationStart;
        setTransformPlayable.m_localRotationEnd = m_localRotationEnd;
        setTransformPlayable.m_localScaleStart = m_localScaleStart;
        setTransformPlayable.m_localScaleEnd = m_localScaleEnd;

        setTransformPlayable.m_positionCurve = m_positionCurve;
        setTransformPlayable.m_rotationCurve = m_rotationCurve;
        setTransformPlayable.m_scaleCurve = m_scaleCurve;

        setTransformPlayable.m_setParent = m_setParent;
        setTransformPlayable.m_ignorePosition = m_ignorePosition;
        setTransformPlayable.m_ignoreRotation = m_ignoreRotation;
        setTransformPlayable.m_ignoreScale = m_ignoreScale;

        return playable;
    }
}