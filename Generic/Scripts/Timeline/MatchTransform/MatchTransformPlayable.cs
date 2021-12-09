using UnityEngine;
using UnityEngine.Playables;

public class MatchTransformPlayable : PlayableBehaviour {
    public Transform m_matchTransform;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
        Transform trackBinding = playerData as Transform;
        trackBinding.Match(m_matchTransform);
    }
}
